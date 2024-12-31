using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.TileSway;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Tiles;

[DrawOrder(DrawOrderAttribute.Layer.NonSolid)]
public class BaobabPod : ModTile, ISwayInWind
{
	private static readonly Dictionary<Point16, float> hitData = []; //Stores the modified rotation of the tile at these coordinates

	private const int numStages = 3;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new(0, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(121, 92, 19));
		DustType = DustID.WoodFurniture;
		HitSound = SoundID.Dig;
	}

	public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
	{
		void DropCoins(int amount)
		{
			int[] split = Utils.CoinsSplit(amount);

			for (int c = 0; c < split.Length; c++)
			{
				if (split[c] == 0)
					continue;

				int type = c switch
				{
					3 => ItemID.PlatinumCoin,
					2 => ItemID.GoldCoin,
					1 => ItemID.SilverCoin,
					_ => ItemID.CopperCoin,
				};

				int numStacks = Math.Min(Main.rand.Next(3) + 1, split[c]);
				for (int r = 0; r < numStacks; r++)
					DropItem(i, j, type, split[c] / numStacks);
				//Split the stack across a number of items randomly
			}
		}

		if (!ProgressStage(i, j, out int stage))
			return;

		fail = true;
		TileExtensions.GetTopLeft(ref i, ref j);

		//Add hitData
		float random = Main.rand.NextFloat(-1f, 1f) * .5f;
		var key = new Point16(i, j);
		if (!hitData.TryAdd(key, random))
			hitData[key] = random;

		SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCHit/HardNaturalHit") with { Pitch = stage - 1 }, new Vector2(i + 1, j + 1) * 16);
		for (int d = 0; d < 10; d++)
			Dust.NewDustDirect(new Vector2(i, j) * 16, 32, 32, DustType, Scale: Main.rand.NextFloat())
				.velocity = (Vector2.UnitY * -Main.rand.NextFloat(2f)).RotatedByRandom(MathHelper.Pi);

		if (stage == numStages - 1)
		{
			var source = new EntitySource_TileBreak(i, j);

			SoundEngine.PlaySound(SoundID.NPCHit7 with { Pitch = -1 }, new Vector2(i + 1, j + 1) * 16);
			for (int g = 1; g < 4; g++)
			{
				Gore.NewGore(source, Main.rand.NextVector2FromRectangle(new Rectangle(i * 16, j * 16, 32, 16)),
					(Vector2.UnitY * -Main.rand.NextFloat(1f, 4f)).RotatedByRandom(1.5f), Mod.Find<ModGore>("BaobabPod" + g).Type);
			}

			for (int g = 1; g < 4; g++)
			{
				var gore = Gore.NewGoreDirect(source, new Vector2(i + 1, j + 1) * 16,
					Vector2.Zero, GoreID.Smoke1);

				gore.velocity = Vector2.UnitX * Main.rand.NextFloat(-1f, 1f);
				gore.alpha = 200;
				gore.position -= new Vector2(gore.Width, gore.Height) / 2;
			}

			DropItem(i, j, ModContent.ItemType<Items.Tools.LivingBaobabLeafWand>());
			DropItem(i, j, ModContent.ItemType<Items.Tools.LivingBaobabWand>());
			DropItem(i, j, ItemID.Waterleaf, Main.rand.Next(2) + 1);
			DropItem(i, j, ModContent.ItemType<Items.SavannaGrassSeeds>(), Main.rand.Next(3) + 1);
			DropItem(i, j, ModContent.ItemType<Items.BaobabFruit.BaobabFruit>());

			if (Main.rand.NextBool(3))
				DropItem(i, j, ItemID.Vine);

			DropCoins(Main.rand.Next(500, 800));
		} //Break open
		else
			DropCoins(Main.rand.Next(150, 200));
	}

	private static bool ProgressStage(int i, int j, out int stage)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(tile);

		if (tile.TileFrameX / data.CoordinateFullWidth >= numStages - 1)
		{
			stage = numStages;
			return false;
		}

		TileExtensions.GetTopLeft(ref i, ref j);

		for (int frameX = 0; frameX < data.Width; frameX++)
			for (int frameY = 0; frameY < data.Width; frameY++)
				Framing.GetTileSafely(i + frameX, j + frameY).TileFrameX += (short)data.CoordinateFullWidth;

		stage = tile.TileFrameX / data.CoordinateFullWidth;
		return true;
	}

	private static void DropItem(int i, int j, int type, int stack = 1)
	{
		var source = new EntitySource_TileBreak(i, j);

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			int id = Item.NewItem(source, new Rectangle(i * 16, j * 16, 32, 16), type, stack, true);
			Main.item[id].velocity = (Vector2.UnitY * -Main.rand.NextFloat(1f, 4f)).RotatedByRandom(1.5f);
			Main.item[id].noGrabDelay = 100;

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendData(MessageID.SyncItem, number: id);
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		var key = new Point16(i, j);
		hitData.Remove(key);
	} //Remove our hitdata

	public void DrawInWind(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		static float GetRotation(int i, int j)
		{
			TileExtensions.GetTopLeft(ref i, ref j);

			var key = new Point16(i, j);
			if (hitData.TryGetValue(key, out float rotation))
				return rotation;

			return 0;
		}

		var tile = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(tile);

		var texture = TextureAssets.Tile[Type].Value;
		Vector2 position = new Vector2(i, j) * 16 - Main.screenPosition;
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);

		spriteBatch.Draw(texture, position + origin, source, Lighting.GetColor(i, j), GetRotation(i, j), origin, 1, SpriteEffects.None, 0);

		if (tile.TileFrameY > 0)
			DrawGrassOverlay(i, j, spriteBatch, offset, rotation, origin);

		//Update hitData
		TileExtensions.GetTopLeft(ref i, ref j);

		var key = new Point16(i, j);
		if (hitData.TryGetValue(key, out float hitRot))
			hitData[key] = MathHelper.Lerp(hitRot, 0, .1f);
	}

	private void DrawGrassOverlay(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);

		var texture = TextureAssets.Tile[Type].Value;
		var data = TileObjectData.GetTileData(tile);
		int frameX = tile.TileFrameX % data.CoordinateFullWidth;

		Vector2 position = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2((frameX == 0) ? -2 : 0, 0);
		var source = new Rectangle(18 * 6, frameX, 18, 18);

		spriteBatch.Draw(texture, position + offset, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
	}
}
