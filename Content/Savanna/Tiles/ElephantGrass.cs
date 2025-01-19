﻿using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.Corruption;
using SpiritReforged.Common.TileCommon.TileSway;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Tiles;

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrass : ModTile, ISwayTile, IConvertibleTile
{
	protected virtual int[] TileAnchors => [ModContent.TileType<SavannaGrass>()];
	protected virtual Color MapColor => new(104, 156, 70);
	protected virtual int Dust => DustID.JungleGrass;
	protected virtual Color LerpColor => Color.Goldenrod;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileCut[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.newTile.Origin = new(0, 2);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = TileAnchors;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 5;
		TileObjectData.addTile(Type);

		AddMapEntry(MapColor);
		DustType = Dust;
		HitSound = SoundID.Grass;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;

	public override void NearbyEffects(int i, int j, bool closer)
	{
		//Play sounds when walking inside grass patches
		if (Main.LocalPlayer.velocity.Length() > 2f && Main.LocalPlayer.miscCounter % 3 == 0 && new Rectangle(i * 16, j * 16, 16, 16).Intersects(Main.LocalPlayer.getRect()))
			SoundEngine.PlaySound(SoundID.Grass with { Volume = .5f, PitchVariance = 1 }, Main.LocalPlayer.Center);
	}

	public virtual void DrawFront(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = tile.SafelyGetData();

		int clusterOffX = -2 + (tile.TileFrameX / 18 + i) * 2 % 4;
		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);
		var effects = (i * (tile.TileFrameX / 18) % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset + new Vector2(clusterOffX, 0), source, Lighting.GetColor(i, j), rotation, origin, 1, effects, 0f);
	}

	public virtual void DrawBack(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = tile.SafelyGetData();

		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);

		for (int x = 0; x < (tile.TileFrameX / 18 + i) % 3 + 1; x++)
		{
			int clusterOffX = -2 + x * 2 % 4;
			float rotationOff = ((x + i + tile.TileFrameX / 18) % 5f - 2.5f) * .1f;

			var source = new Rectangle((tile.TileFrameX / 18 + i + x) % data.RandomStyleRange * 18, tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);
			var effects = (i + x * (tile.TileFrameX / 18) % 2 == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			var color = Color.Lerp(Color.White, LerpColor, .9f + (float)Math.Sin(i / 10f) * .1f);

			spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset + new Vector2(clusterOffX, 0), source, Lighting.GetColor(i, j).MultiplyRGB(color), rotation * .5f + rotationOff, origin, 1, effects, 0f);
		}
	}

	public void DrawSway(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		if (DrawOrderSystem.Order == DrawOrderAttribute.Layer.OverPlayers)
			DrawFront(i, j, spriteBatch, offset, rotation, origin);
		else
			DrawBack(i, j, spriteBatch, offset, rotation, origin);
	}

	public float Physics(Point16 topLeft)
	{
		var data = TileObjectData.GetTileData(Framing.GetTileSafely(topLeft));
		float rotation = Main.instance.TilesRenderer.GetWindCycle(topLeft.X, topLeft.Y, ModContent.GetInstance<TileSwaySystem>().GrassWindCounter * 2.25f);

		if (!WorldGen.InAPlaceWithWind(topLeft.X, topLeft.Y, data.Width, data.Height))
			rotation = 0f;

		return (rotation + TileSwayHelper.GetHighestWindGridPushComplex(topLeft.X, topLeft.Y, data.Width, data.Height, 20, 3f, 1, true)) * 1.9f;
	}

	public virtual bool Convert(IEntitySource source, ConversionType type, int i, int j)
	{
		if (source is EntitySource_Parent { Entity: Projectile })
			return false;

		j -= Main.tile[i, j].TileFrameY / 18;
		int id = Main.tile[i, j].TileType;

		if (TileCorruptor.GetConversionType<ElephantGrass, ElephantGrassCorrupt, ElephantGrassCrimson, ElephantGrassHallow>(id, type, out int conversionType))
		{
			for (int y = j; y < j + 3; ++y)
			{
				Tile tile = Main.tile[i, y];
				tile.TileType = (ushort)conversionType;
			}
		}

		if (Main.netMode == NetmodeID.Server)
			NetMessage.SendTileSquare(-1, i, j, 1, 3);

		return false;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassShort : ElephantGrass
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileCut[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new(0, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = TileAnchors;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.addTile(Type);

		AddMapEntry(MapColor);
		DustType = Dust;
		HitSound = SoundID.Grass;
	}

	public override void RandomUpdate(int i, int j) //Grow up; spreading happens in SavannaGrass.RandomUpdate
	{
		if (!(IsBase() && Main.rand.NextBool()))
			return;

		if (!Framing.GetTileSafely(i, j - 2).HasTile && GrassSurrounding())
		{
			for (int y = 0; y < 2; y++)
				Framing.GetTileSafely(i, j - y).ClearTile();

			if (WorldGen.PlaceTile(i, j, ModContent.TileType<ElephantGrass>(), true, style: Main.rand.Next(5)))
				NetMessage.SendTileSquare(-1, i, j - 2, 1, 3);
		}

		bool IsBase()
		{
			var tile = Main.tile[i, j];

			if (!tile.HasTile)
				return false;

			var tileData = TileObjectData.GetTileData(tile);
			if (tileData == null)
				return false;

			int partFrameX = tile.TileFrameX % tileData.CoordinateFullWidth;
			int partFrameY = tile.TileFrameY % tileData.CoordinateFullHeight;
			return partFrameX == 0 && partFrameY == 18;
		}

		bool GrassSurrounding()
		{
			HashSet<int> types = [ModContent.TileType<ElephantGrassShort>(), ModContent.TileType<ElephantGrass>()];
			return types.Contains(Framing.GetTileSafely(i - 1, j).TileType) && types.Contains(Framing.GetTileSafely(i + 1, j).TileType);
		}
	}

	public override void DrawBack(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = tile.SafelyGetData();

		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);

		int clusterOffX = -2 + (tile.TileFrameX / 18 + i + 1) * 2 % 4;
		var source = new Rectangle((tile.TileFrameX + 18) % (data.RandomStyleRange * 18), tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);
		var effects = (i * (tile.TileFrameX / 18) % 2 == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset + new Vector2(clusterOffX, 0), source, Lighting.GetColor(i, j).MultiplyRGB(LerpColor), 
			rotation * .5f, origin, 1, effects, 0f);
	}

	public override bool Convert(IEntitySource source, ConversionType type, int i, int j)
	{
		if (source is EntitySource_Parent { Entity: Projectile })
			return false;

		j -= Main.tile[i, j].TileFrameY / 18;
		int id = Main.tile[i, j].TileType;

		if (TileCorruptor.GetConversionType<ElephantGrassShort, ElephantGrassShortCorrupt, ElephantGrassShortCrimson, ElephantGrassShortHallow>(id, type, out int newId))
		{
			for (int y = j; y < j + 2; ++y)
			{
				Tile tile = Main.tile[i, y];
				tile.TileType = (ushort)newId;
			}
		}

		if (Main.netMode == NetmodeID.Server)
			NetMessage.SendTileSquare(-1, i, j, 1, 3);

		return false;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassCorrupt : ElephantGrass
{
	protected override int[] TileAnchors => [ModContent.TileType<SavannaGrassCorrupt>()];
	protected override Color MapColor => new(109, 106, 174);
	protected override int Dust => DustID.Corruption;
	protected override Color LerpColor => Color.Gray;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.AddCorruptionTile(Type);
		TileID.Sets.Corrupt[Type] = true;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassCrimson : ElephantGrass
{
	protected override int[] TileAnchors => [ModContent.TileType<SavannaGrassCrimson>()];
	protected override Color MapColor => new(183, 69, 68);
	protected override int Dust => DustID.CrimsonPlants;
	protected override Color LerpColor => Color.Gray;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.AddCrimsonTile(Type);
		TileID.Sets.Crimson[Type] = true;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassHallow : ElephantGrass
{
	protected override int[] TileAnchors => [ModContent.TileType<SavannaGrassHallow>()];
	protected override Color MapColor => new(78, 193, 227);
	protected override int Dust => DustID.HallowedPlants;
	protected override Color LerpColor => Color.Gray;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.Hallow[Type] = true;
		TileID.Sets.HallowBiome[Type] = 1;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassShortCorrupt : ElephantGrassShort
{
	protected override int[] TileAnchors => [ModContent.TileType<SavannaGrassCorrupt>()];
	protected override Color MapColor => new(109, 106, 174);
	protected override int Dust => DustID.Corruption;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.AddCorruptionTile(Type);
		TileID.Sets.Corrupt[Type] = true;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassShortCrimson : ElephantGrassShort
{
	protected override int[] TileAnchors => [ModContent.TileType<SavannaGrassCrimson>()];
	protected override Color MapColor => new(183, 69, 68);
	protected override int Dust => DustID.CrimsonPlants;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.AddCrimsonTile(Type);
		TileID.Sets.Crimson[Type] = true;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassShortHallow : ElephantGrassShort
{
	protected override int[] TileAnchors => [ModContent.TileType<SavannaGrassHallow>()];
	protected override Color MapColor => new(78, 193, 227);
	protected override int Dust => DustID.HallowedPlants;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		TileID.Sets.Hallow[Type] = true;
		TileID.Sets.HallowBiome[Type] = 1;
	}
}