using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.TileSway;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Tiles;

[DrawOrder(DrawOrderAttribute.Layer.NonSolid, DrawOrderAttribute.Layer.OverPlayers)]
public class ElephantGrass : ModTile, ISwayInWind
{
	public static bool IsElephantGrass(int i, int j)
	{
		int type = Framing.GetTileSafely(i, j).TileType;
		return TileLoader.GetTile(type) is ElephantGrass;
	}

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
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrass>()];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 5;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(104, 156, 70));
		DustType = DustID.JunglePlants;
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
		var data = TileObjectData.GetTileData(tile);

		int clusterOffX = -2 + (tile.TileFrameX / 18 + i) * 2 % 4;
		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);
		var effects = (i * (tile.TileFrameX / 18) % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset + new Vector2(clusterOffX, 0), source, Lighting.GetColor(i, j), rotation, origin, 1, effects, 0f);
	}

	public virtual void DrawBack(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(tile);

		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);

		for (int x = 0; x < (tile.TileFrameX / 18 + i) % 3 + 1; x++)
		{
			int clusterOffX = -2 + x * 2 % 4;
			float rotationOff = ((x + i + tile.TileFrameX / 18) % 5f - 2.5f) * .1f;

			var source = new Rectangle((tile.TileFrameX / 18 + i + x) % data.RandomStyleRange * 18, tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);
			var effects = (i + x * (tile.TileFrameX / 18) % 2 == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			var color = Color.Lerp(Color.White, Color.Goldenrod, .9f + (float)Math.Sin(i / 10f) * .1f);

			spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset + new Vector2(clusterOffX, 0), source, Lighting.GetColor(i, j).MultiplyRGB(color), rotation * .5f + rotationOff, origin, 1, effects, 0f);
		}
	}

	public void DrawInWind(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		if (DrawOrderHandler.order == DrawOrderAttribute.Layer.OverPlayers)
			DrawFront(i, j, spriteBatch, offset, rotation, origin);
		else
			DrawBack(i, j, spriteBatch, offset, rotation, origin);
	}

	public void ModifyRotation(int i, int j, ref float rotation) => rotation *= 1.9f;

	public float SetWindSway(Point16 topLeft)
	{
		var data = TileObjectData.GetTileData(Framing.GetTileSafely(topLeft));
		float rotation = Main.instance.TilesRenderer.GetWindCycle(topLeft.X, topLeft.Y, TileSwaySystem.Instance.GrassWindCounter * 2.25f);

		if (!WorldGen.InAPlaceWithWind(topLeft.X, topLeft.Y, data.Width, data.Height))
			rotation = 0f;

		return rotation + TileSwayHelper.GetHighestWindGridPushComplex(topLeft.X, topLeft.Y, data.Width, data.Height, 20, 3f, 1, true);
	}
}

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
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrass>()];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(104, 156, 70));
		DustType = DustID.JunglePlants;
		HitSound = SoundID.Grass;
	}

	public override void RandomUpdate(int i, int j)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(Type, 0);

		if (tile.TileFrameY != (data.Height - 1) * 18)
			return;

		if (Main.rand.NextBool(2)) //Grow up
		{
			if (!Framing.GetTileSafely(i, j - 2).HasTile && IsElephantGrass(i - 1, j) && IsElephantGrass(i + 1, j))
			{
				for (int y = 0; y < 2; y++)
					Framing.GetTileSafely(i, j - 1).HasTile = false; //Deactivate all tiles in this multitile

				NetMessage.SendTileSquare(-1, i, j - 1, 1, 2);

				int type = ModContent.TileType<ElephantGrass>();
				int style = Main.rand.Next(data.RandomStyleRange);

				WorldGen.PlaceObject(i, j, type, true, style);
				NetMessage.SendObjectPlacement(-1, i, j, type, style, 0, -1, -1);
			}
		}

		if (Main.rand.NextBool(8)) //Spread
		{
			int[] anchors = data.AnchorValidTiles;
			
			for (int d = 0; d < 2; d++)
			{
				int dir = (d == 0) ? -1 : 1; //Check directly left and right

				if (anchors.Contains(Framing.GetTileSafely(i + dir, j + 1).TileType) && Framing.GetTileSafely(i + dir, j - 1).LiquidAmount < 80 && 
					!Framing.GetTileSafely(i + dir, j + 1).TopSlope && !Framing.GetTileSafely(i + dir, j - 1).HasTile && !Framing.GetTileSafely(i + dir, j - 2).HasTile)
				{
					int style = Main.rand.Next(data.RandomStyleRange);
					WorldGen.PlaceObject(i, j, Type, true, style);
					NetMessage.SendObjectPlacement(-1, i, j, Type, style, 0, -1, -1);

					break;
				}
			}
		}
	}

	public override void DrawBack(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(tile);

		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);

		int clusterOffX = -2 + (tile.TileFrameX / 18 + i + 1) * 2 % 4;
		var source = new Rectangle((tile.TileFrameX + 18) % (data.RandomStyleRange * 18), tile.TileFrameY, 16, data.CoordinateHeights[tile.TileFrameY / 18]);
		var effects = (i * (tile.TileFrameX / 18) % 2 == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset + new Vector2(clusterOffX, 0), source, Lighting.GetColor(i, j).MultiplyRGB(Color.Goldenrod), rotation * .5f, origin, 1, effects, 0f);
	}
}
