using SpiritReforged.Common.SimpleEntity;
using SpiritReforged.Common.TileCommon.CustomTree;
using SpiritReforged.Common.TileCommon.TileSway;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpiritReforged.Content.Savanna.Tiles.AcaciaTree;

public class AcaciaTree : CustomTree
{
	private const int numStyles = 4;

	/// <summary> How much acacia tree tops sway in the wind. Used by the client for drawing and platform logic. </summary>
	public static float GetSway(int i, int j, double factor = 0)
	{
		if (factor == 0)
			factor = TileSwaySystem.Instance.TreeWindCounter;

		return Main.instance.TilesRenderer.GetWindCycle(i, j, factor) * .4f;
	}
	public static IEnumerable<TreetopPlatform> Platforms => SimpleEntitySystem.entities.Where(x => x is TreetopPlatform).Cast<TreetopPlatform>();

	public override int TreeHeight => WorldGen.genRand.Next(8, 16);

	public override void PostSetStaticDefaults()
	{
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrass>()];

		AddMapEntry(new Color(120, 80, 75));
		RegisterItemDrop(ModContent.ItemType<Items.Drywood.Drywood>());
		DustType = DustID.WoodFurniture;
	}

	public override void NearbyEffects(int i, int j, bool closer) //Spawn platforms
	{
		var pt = new Point16(i, j);
		if (IsTreeTop(i, j, true) && !Platforms.Where(x => x.TreePosition == pt).Any())
		{
			int type = SimpleEntitySystem.types[typeof(TreetopPlatform)];
			//Spawn our entity at direct tile coordinates where it can reposition itself after updating
			SimpleEntitySystem.NewEntity(type, pt.ToVector2());
		}
	}

	protected override void OnShakeTree(int i, int j)
	{
		var drop = new WeightedRandom<int>();

		drop.Add(ItemID.None, .8f);
		drop.Add(ModContent.ItemType<Items.Food.Caryocar>(), .2f);
		drop.Add(ModContent.ItemType<Items.Food.CustardApple>(), .2f);
		drop.Add(ModContent.ItemType<Items.BaobabFruit.BaobabFruit>(), .2f);
		drop.Add(ModContent.ItemType<Items.Drywood.Drywood>(), .8f);
		drop.Add(ItemID.Acorn, .7f);

		var position = new Vector2(i, j - 3) * 16;
		int dropType = (int)drop;
		if (dropType > ItemID.None)
			Item.NewItem(null, new Rectangle((int)position.X, (int)position.Y, 16, 16), dropType);

		GrowEffects(i, j, true);
	}

	public override void DrawTreeFoliage(int i, int j, SpriteBatch spriteBatch)
	{
		var position = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(10, 0) + TreeHelper.GetPalmTreeOffset(i, j);
		float rotation = GetSway(i, j) * .08f;

		if (Framing.GetTileSafely(i, j).TileType == Type && Framing.GetTileSafely(i, j - 1).TileType != Type) //Draw treetops
		{
			const int framesY = 2;

			int frameY = Framing.GetTileSafely(i, j).TileFrameX / frameSize % framesY;

			var source = topsTexture.Frame(1, framesY, 0, frameY, sizeOffsetY: -2);
			var origin = new Vector2(source.Width / 2, source.Height) - new Vector2(0, 2);

			spriteBatch.Draw(topsTexture.Value, position, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
		}
		else if (branchesTexture != null) //Draw branches
		{
			const int framesX = 2;
			const int framesY = 3;

			int frameX = (TileObjectData.GetTileStyle(Framing.GetTileSafely(i, j)) / numStyles == framesX) ? 1 : 0;
			int frameY = Framing.GetTileSafely(i, j).TileFrameX / frameSize % framesY;

			var source = branchesTexture.Frame(framesX, framesY, frameX, frameY, -2, -2);
			var origin = new Vector2(frameX == 0 ? source.Width : 0, 44);

			position.X += 6 * (frameX == 0 ? -1 : 1); //Directional offset

			spriteBatch.Draw(branchesTexture.Value, position, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
		}
	}

	public override void AddDrawPoints(int i, int j, SpriteBatch spriteBatch)
	{
		if (IsTreeTop(i, j) && TileObjectData.GetTileStyle(Framing.GetTileSafely(i, j)) % numStyles < 2)
			treeDrawPoints.Add(new Point16(i, j));
		else if (TileObjectData.GetTileStyle(Framing.GetTileSafely(i, j)) / numStyles > 0)
			treeDrawPoints.Add(new Point16(i, j));
	}

	protected override void OnGrowEffects(int i, int j, int height)
	{
		for (int h = 0; h < height; h++)
		{
			var center = new Vector2(i, j + h) * 16f + new Vector2(8);
			int range = 20;
			int loops = 3;

			if (h == 0)
			{
				center.Y -= 16 * 3;
				range = 80;
				loops = 20;
			}

			for (int g = 0; g < loops; g++)
				Gore.NewGorePerfect(new EntitySource_TileBreak(i, j), center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(range), 
					Main.rand.NextVector2Unit(), GoreID.TreeLeaf_Palm, .7f + Main.rand.NextFloat() * .6f);
		}
	}

	protected override void GenerateTree(int i, int j, int height)
	{
		int variance = WorldGen.genRand.Next(-8, 9) * 2;
		short xOff = 0;

		for (int h = 0; h < height; h++)
		{
			int style = 0;

			if (WorldGen.genRand.NextBool(6)) //Select rare segments
				style = 1;

			if (h > 2 && WorldGen.genRand.NextBool(5)) //Select branched segments by exceding the normal style limit
			{
				if (WorldGen.genRand.NextBool())
					style += numStyles; //Left branch
				else
					style += numStyles * 2; //Right branch
			}

			WorldGen.PlaceTile(i, j - h, Type, true);
			Framing.GetTileSafely(i, j - h).TileFrameX = (short)(style * frameSize * 3 + WorldGen.genRand.Next(3) * frameSize);
			Framing.GetTileSafely(i, j - h).TileFrameY = TreeHelper.GetPalmOffset(j, variance, height, ref xOff);
		}

		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, i, j + 1 - height, 1, height, TileChangeType.None);
	}
}
