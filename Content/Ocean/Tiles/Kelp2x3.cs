﻿using Terraria.DataStructures;

namespace SpiritReforged.Content.Ocean.Tiles;

public class Kelp2x3 : ModTile
{
	private static Texture2D glowmask;

    public Kelp2x3() => glowmask = ModContent.Request<Texture2D>("SpiritReforged/Content/Ocean/Tiles/Kelp2x3_Glow", AssetRequestMode.ImmediateLoad).Value;
    public override void Unload() => glowmask = null;

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileCut[Type] = false;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Width = 2;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 0);
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.AnchorValidTiles = [TileID.Sand, TileID.Crimsand, TileID.Ebonsand];
		TileObjectData.newTile.RandomStyleRange = 1;
		TileObjectData.addTile(Type);

		TileID.Sets.DisableSmartCursor[Type] = true;
		DustType = DustID.Grass;

		AddMapEntry(new Color(24, 105, 25), Language.GetText("Mods.SpiritReforged.Tiles.KelpMapEntry"));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (Framing.GetTileSafely(i, j).TileFrameY == 18) 
		{
			r = 0.3f * 1.5f;
			g = 0.3f * 1.5f;
			b = 0;
		}
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile t = Framing.GetTileSafely(i, j);
		Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
		spriteBatch.Draw(glowmask, new Vector2(i * 16, j * 16) - Main.screenPosition + zero, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16), Color.LightYellow);
	}
}
