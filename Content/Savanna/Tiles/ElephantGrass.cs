﻿using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.Corruption;
using SpiritReforged.Common.TileCommon.TileSway;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Tiles;

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrass : ModTile, IConvertibleTile
{
	protected virtual Color SubColor => Color.Goldenrod;

	public static bool IsShortgrass(int i, int j) => TileObjectData.GetTileStyle(Main.tile[i, j]) > 4;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoFail[Type] = true;
		Main.tileCut[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrass>()];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 5;

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.RandomStyleRange = 3;

		PreAddObjectData();
		TileObjectData.addAlternate(5);
		TileObjectData.addTile(Type);

		HitSound = SoundID.Grass;
	}

	public virtual void PreAddObjectData()
	{
		AddMapEntry(new(104, 156, 70));
		DustType = DustID.JungleGrass;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;

	public override void NearbyEffects(int i, int j, bool closer)
	{
		//Play sounds when walking inside grass patches
		if (Main.LocalPlayer.velocity.Length() > 2f && Main.LocalPlayer.miscCounter % 3 == 0 && new Rectangle(i * 16, j * 16, 16, 16).Intersects(Main.LocalPlayer.getRect()))
			SoundEngine.PlaySound(SoundID.Grass with { Volume = .5f, PitchVariance = 1 }, Main.LocalPlayer.Center);
	}

	public override void RandomUpdate(int i, int j) //Grow up; spreading happens in SavannaGrass.RandomUpdate
	{
		if (!Main.rand.NextBool() || !IsShortgrass(i, j))
			return;

		if (GrassSurrounding())
		{
			Main.tile[i, j].TileFrameX = (short)(Main.rand.Next(5) * 18);
			NetMessage.SendTileSquare(-1, i, j, 1, 1);
		}

		bool GrassSurrounding() => Framing.GetTileSafely(i - 1, j).TileType == Type && Framing.GetTileSafely(i + 1, j).TileType == Type;
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) //Imitates wind sway drawing but with unique dimensions
	{
		const int height = 3; //Pseudo tile height
		float physics = Physics(new Point16(i, j - (height - 1)));

		for (int y = 0; y < 3; y++)
		{
			float swing = 1f - (float)(y + 1) / height + .5f;
			float rotation = physics * swing * .1f;

			var rotationOffset = new Vector2(0, Math.Abs(rotation) * 20f);
			var drawOrigin = new Vector2(8, (height - y) * 16);

			var frame = new Point(Main.tile[i, j].TileFrameX, Main.tile[i, j].TileFrameY + y * 18);
			var offset = drawOrigin + rotationOffset + new Vector2(0, y * 16 - 32);

			if (DrawOrderSystem.Order == DrawOrderAttribute.Layer.OverPlayers)
				DrawFront(i, j, spriteBatch, offset, rotation, drawOrigin, frame);
			else
				DrawBack(i, j, spriteBatch, offset, rotation, drawOrigin, frame);
		}

		return false;

		static float Physics(Point16 topLeft)
		{
			float rotation = Main.instance.TilesRenderer.GetWindCycle(topLeft.X, topLeft.Y, ModContent.GetInstance<TileSwaySystem>().GrassWindCounter * 2.25f);
			if (!WorldGen.InAPlaceWithWind(topLeft.X, topLeft.Y, 1, height))
				rotation = 0f;

			return (rotation + TileSwayHelper.GetHighestWindGridPushComplex(topLeft.X, topLeft.Y, 1, height, 20, 3f, 1, true)) * 1.9f;
		}
	}

	public virtual void DrawFront(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin, Point frame)
	{
		float seed = (frame.X / 18f + i) * .8f % 1f;

		offset += new Vector2(MathHelper.Lerp(-2, 2, seed), 2);
		var source = new Rectangle(frame.X, frame.Y, 16, 16);
		var effects = ((int)(seed * 2) % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, new Vector2(i, j) * 16 - Main.screenPosition + offset, source, Lighting.GetColor(i, j), rotation, origin, 1, effects, 0f);
	}

	public virtual void DrawBack(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin, Point frame)
	{
		var data = Main.tile[i, j].SafelyGetData();
		int amount = i % 2 + 1;

		for (int x = 0; x < amount; x++)
		{
			float seed = (frame.X / 18f + i + x * .25f) * 1.3f % 1f;

			rotation += seed * .25f;
			offset += new Vector2(MathHelper.Lerp(-2, 2, seed), 2);

			int baseOffset = IsShortgrass(i, j) ? 5 : 0;
			int frameX = (baseOffset + (int)MathHelper.Lerp(0, data.RandomStyleRange, seed)) * 18;

			var source = new Rectangle(frameX, frame.Y, 16, 16);
			var effects = ((int)(seed * 2) % 2 == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			var color = Color.Lerp(Color.White, SubColor, .9f + (float)Math.Sin(i / 8f) * .3f);

			spriteBatch.Draw(TextureAssets.Tile[Type].Value, new Vector2(i, j) * 16 - Main.screenPosition + offset, source, Lighting.GetColor(i, j).MultiplyRGB(color), rotation * .4f, origin, 1, effects, 0f);
		}
	}

	public virtual bool Convert(IEntitySource source, ConversionType type, int i, int j)
	{
		if (source is EntitySource_Parent { Entity: Projectile })
			return false;

		var tile = Main.tile[i, j];
		int id = tile.TileType;

		if (TileCorruptor.GetConversionType<ElephantGrass, ElephantGrassCorrupt, ElephantGrassCrimson, ElephantGrassHallow>(id, type, out int conversionType))
			tile.TileType = (ushort)conversionType;

		return true;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassCorrupt : ElephantGrass
{
	protected override Color SubColor => Color.Gray;

	public override void PreAddObjectData()
	{
		TileID.Sets.AddCorruptionTile(Type);
		TileID.Sets.Corrupt[Type] = true;

		AddMapEntry(new(109, 106, 174));

		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrassCorrupt>()];
		DustType = DustID.Corruption;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassCrimson : ElephantGrass
{
	protected override Color SubColor => new(190, 165, 0);

	public override void PreAddObjectData()
	{
		TileID.Sets.AddCrimsonTile(Type);
		TileID.Sets.Crimson[Type] = true;

		AddMapEntry(new(183, 69, 68));

		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrassCrimson>()];
		DustType = DustID.CrimsonPlants;
	}
}

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrassHallow : ElephantGrass
{
	protected override Color SubColor => Color.LightPink;

	public override void PreAddObjectData()
	{
		TileID.Sets.Hallow[Type] = true;
		TileID.Sets.HallowBiome[Type] = 1;

		AddMapEntry(new(78, 193, 227));

		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrassHallow>()];
		DustType = DustID.HallowedPlants;
	}
}
