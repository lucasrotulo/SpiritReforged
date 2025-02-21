﻿using SpiritReforged.Common.SimpleEntity;
using SpiritReforged.Common.TileCommon.Corruption;
using SpiritReforged.Common.TileCommon.CustomTree;
using SpiritReforged.Common.TileCommon.TileSway;
using SpiritReforged.Content.Savanna.DustStorm;
using SpiritReforged.Content.Savanna.Items.Food;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace SpiritReforged.Content.Savanna.Tiles.AcaciaTree;

public class AcaciaTree : CustomTree, IConvertibleTile
{
	public static IEnumerable<TreetopPlatform> Platforms => SimpleEntitySystem.entities.Where(x => x is TreetopPlatform).Cast<TreetopPlatform>();

	public override int TreeHeight => WorldGen.genRand.Next(8, 16);
	protected virtual int ValidAnchor => ModContent.TileType<SavannaGrass>();

	/// <summary> How much acacia tree tops sway in the wind. Used by the client for drawing and platform logic. </summary>
	public static float GetSway(int i, int j, double factor = 0)
	{
		if (factor == 0)
			factor = TileSwaySystem.Instance.TreeWindCounter;

		return Main.instance.TilesRenderer.GetWindCycle(i, j, factor) * .4f;
	}
	public override void PreAddTileObjectData()
	{
		TileObjectData.newTile.AnchorValidTiles = [ValidAnchor];

		AddMapEntry(new Color(120, 80, 75));
		RegisterItemDrop(ModContent.ItemType<Items.Drywood.Drywood>());
		DustType = DustID.WoodFurniture;
	}

	public override bool IsTreeTop(int i, int j)
	{
		if (ModContent.GetModTile(Main.tile[i, j].TileType) is not AcaciaTree || ModContent.GetModTile(Main.tile[i, j - 1].TileType) is AcaciaTree)
			return false;

		return Main.tile[i, j].TileFrameX <= FrameSize * 5;
	}

	public override void NearbyEffects(int i, int j, bool closer) //Spawn platforms
	{
		var pt = new Point16(i, j);

		if (IsTreeTop(i, j) && !Platforms.Where(x => x.TreePosition == pt).Any())
		{
			int type = SimpleEntitySystem.types[typeof(TreetopPlatform)];
			//Spawn our entity at direct tile coordinates where it can reposition itself after updating
			SimpleEntitySystem.NewEntity(type, pt.ToVector2());
		}
	}

	public override IEnumerable<Item> GetItemDrops(int i, int j)
	{
		foreach (var item in base.GetItemDrops(i, j))
		{
			item.stack *= 2;
			yield return item;
		}
	}

	protected override void OnShakeTree(int i, int j)
	{
		var drop = new WeightedRandom<int>();

		drop.Add(ItemID.None, .8f);
		drop.Add(ModContent.ItemType<Items.Food.Caryocar>(), .2f);
		drop.Add(ModContent.ItemType<Items.Food.CustardApple>(), .2f);
		drop.Add(ModContent.ItemType<BaobabFruit>(), .2f);
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

		if (IsTreeTop(i, j)) //Draw treetops
		{
			const int framesY = 2;

			int frameY = Framing.GetTileSafely(i, j).TileFrameX / FrameSize % framesY;
			var source = TopTexture.Frame(1, framesY, 0, frameY, sizeOffsetY: -2);
			var origin = new Vector2(source.Width / 2, source.Height) - new Vector2(0, 2);
			
			TryDrawGodrays(position, rotation);

			spriteBatch.Draw(TopTexture.Value, position, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
		}
		else //Draw branches
		{
			const int framesX = 2;
			const int framesY = 3;

			int frameX = (Noise(new Vector2(i, j)) > 0) ? 1 : 0;
			int frameY = Framing.GetTileSafely(i, j).TileFrameX / FrameSize % framesY;
			var source = BranchTexture.Frame(framesX, framesY, frameX, frameY, -2, -2);
			var origin = new Vector2(frameX == 0 ? source.Width : 0, 44);

			position += new Vector2(6 * ((frameX == 0) ? -1 : 1), 8); //Directional offset

			spriteBatch.Draw(BranchTexture.Value, position, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
		}
	}

	protected static void TryDrawGodrays(Vector2 position, float rotation)
	{
		const float startTime = 9.50f;
		const float endTime = 15f;

		if (Main.raining || Main.LocalPlayer.GetModPlayer<DustStormPlayer>().ZoneDustStorm)
			return;

		float time = Utils.GetDayTimeAs24FloatStartingFromMidnight();

		if (time is < startTime or > endTime)
			return;

		float opacity = Math.Clamp((float)Math.Sin((time - startTime) / (endTime - startTime) * MathHelper.Pi) * 2, 0, 1);
		float x = MathHelper.Lerp(200, -140, (float)(Main.time / Main.dayLength));

		Vector3 topLeft = new Vector3(position, 0) + new Vector3(new Vector2(-160, 0).RotatedBy(rotation) - new Vector2(0, 106), 0);
		Vector3 topRight = new Vector3(position, 0) + new Vector3(new Vector2(150, 0).RotatedBy(rotation) - new Vector2(0, 106), 0);
		Vector3 botLeft = new Vector3(position, 0) + new Vector3(-160 + x, 40, 0);
		Color color = Color.White;

		short[] indices = [0, 1, 2, 1, 3, 2];

		VertexPositionColorTexture[] vertices =
		[
			new(topLeft, color, new Vector2(0, 0)),
            new(topRight, color, new Vector2(1, 0)),
            new(botLeft, color, new Vector2(0, 1)),
            new(botLeft + new Vector3(310, 0, 0), color, new Vector2(1, 1)),
		];

		Effect effect = AssetLoader.LoadedShaders["ShadowFade"];
		var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
		Matrix view = Main.GameViewMatrix.TransformationMatrix;
		Matrix renderMatrix = view * projection;

		foreach (EffectPass pass in effect.CurrentTechnique.Passes)
		{
			effect.Parameters["baseShadowColor"].SetValue(Color.Black.ToVector4() * 0.325f * opacity);
			effect.Parameters["adjustColor"].SetValue(Color.MidnightBlue.ToVector4() * 0.5f * opacity);
			effect.Parameters["noiseScroll"].SetValue(Main.GameUpdateCount * 0.0015f);
			effect.Parameters["noiseStretch"].SetValue(3);
			effect.Parameters["uWorldViewProjection"].SetValue(renderMatrix);
			effect.Parameters["noiseTexture"].SetValue(AssetLoader.LoadedTextures["vnoise"]);
			pass.Apply();

			Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 4, indices, 0, 2);
		}
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
			int style = WorldGen.genRand.NextBool(6) ? 1 : 0; //Rare segments

			WorldGen.PlaceTile(i, j - h, Type, true);
			var tile = Framing.GetTileSafely(i, j - h);

			if (tile.HasTile && tile.TileType == Type)
			{
				Framing.GetTileSafely(i, j - h).TileFrameX = (short)(style * FrameSize * 3 + WorldGen.genRand.Next(3) * FrameSize);
				Framing.GetTileSafely(i, j - h).TileFrameY = TreeHelper.GetPalmOffset(j, variance, height, ref xOff);
			}
		}

		if (Main.netMode != NetmodeID.SinglePlayer)
			NetMessage.SendTileSquare(-1, i, j + 1 - height, 1, height, TileChangeType.None);
	}

	public bool Convert(IEntitySource source, ConversionType type, int i, int j)
	{
		if (source is EntitySource_Parent { Entity: Projectile })
			return false; //Only rely on the anchor tile source (TileUpdate) for conversions

		int oldType = Main.tile[i, j].TileType;
		var tile = Main.tile[i, j];

		tile.TileType = (ushort)(type switch
		{
			ConversionType.Hallow => ModContent.TileType<AcaciaTreeHallow>(),
			ConversionType.Crimson => ModContent.TileType<AcaciaTreeCrimson>(),
			ConversionType.Corrupt => ModContent.TileType<AcaciaTreeCorrupt>(),
			_ => ModContent.TileType<AcaciaTree>(),
		});

		if (Main.tile[i, j - 1].TileType == oldType) //Convert the entire tree from the base
			TileCorruptor.Convert(new EntitySource_TileUpdate(i, j), type, i, j - 1);

		return true;
	}
}

public class AcaciaTreeCorrupt : AcaciaTree
{
	protected override int ValidAnchor => ModContent.TileType<SavannaGrassCorrupt>();

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		TileID.Sets.Corrupt[Type] = true;
	}
}

public class AcaciaTreeCrimson : AcaciaTree
{
	protected override int ValidAnchor => ModContent.TileType<SavannaGrassCrimson>();

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		TileID.Sets.Crimson[Type] = true;
	}
}

public class AcaciaTreeHallow : AcaciaTree
{
	protected override int ValidAnchor => ModContent.TileType<SavannaGrassHallow>();

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		TileID.Sets.Hallow[Type] = true;
	}

	public override void DrawTreeFoliage(int i, int j, SpriteBatch spriteBatch)
	{
		var position = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(10, 0) + TreeHelper.GetPalmTreeOffset(i, j);
		float rotation = GetSway(i, j) * .08f;

		if (IsTreeTop(i, j)) //Draw treetops
		{
			const int framesX = 8;
			const int framesY = 2;

			int frameX = i % 8;
			int frameY = Framing.GetTileSafely(i, j).TileFrameX / FrameSize % framesY;
			var source = TopTexture.Frame(framesX, framesY, frameX, frameY, -2, -2);
			var origin = new Vector2(source.Width / 2, source.Height) - new Vector2(0, 2);

			TryDrawGodrays(position, rotation);

			spriteBatch.Draw(TopTexture.Value, position, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
		}
		else //Draw branches
		{
			const int framesX = 16;
			const int framesY = 3;

			int frameX = ((Noise(new Vector2(i, j)) > 0) ? 1 : 0) + j % 8 * 2;
			int frameY = Framing.GetTileSafely(i, j).TileFrameX / FrameSize % framesY;
			bool flip = frameX % 2 == 0;
			var source = BranchTexture.Frame(framesX, framesY, frameX, frameY, -2, -2);
			var origin = new Vector2(flip ? source.Width : 0, 44);

			position += new Vector2(6 * (flip ? -1 : 1), 8); //Directional offset

			spriteBatch.Draw(BranchTexture.Value, position, source, Lighting.GetColor(i, j), rotation, origin, 1, SpriteEffects.None, 0);
		}
	}
}