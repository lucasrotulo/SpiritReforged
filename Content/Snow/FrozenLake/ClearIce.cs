using SpiritReforged.Common.TileCommon;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Snow.FrozenLake;

public class ClearIce : ModTile, IReflective
{
    private static Asset<Texture2D> overlay;
    public override void Load() => overlay = ModContent.Request<Texture2D>(Texture + "_Overlay");

    public override void SetStaticDefaults()
    {
		Main.tileNoFail[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileMerge[TileID.SnowBlock][Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
		=> WorldGen.TileMergeAttempt(-2, TileID.SnowBlock, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Main.tile[i, j];

        var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
        var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
        var position = new Vector2(i, j) * 16 - Main.screenPosition + zero;

		//Draw water
		int waterStyle = WaterStyleID.Snow; //Main.waterStyle;
		float waterAlpha = Main.liquidAlpha[waterStyle]; 

		Lighting.GetCornerColors(i, j, out var vertices);
        vertices.BottomLeftColor *= waterAlpha;
        vertices.BottomRightColor *= waterAlpha;
        vertices.TopLeftColor *= waterAlpha;
        vertices.TopRightColor *= waterAlpha;

        if ((!Main.tile[i, j - 1].HasTile || Main.tile[i, j - 1].TileType != Type) && Main.tile[i, j - 1].LiquidAmount < 100)
        {
            vertices.TopLeftColor.A -= 60;
            vertices.TopRightColor.A -= 60;
        }

        Main.tileBatch.Draw(TextureAssets.Liquid[waterStyle].Value, position, source, vertices, Vector2.Zero, 1f, SpriteEffects.None);

		//Draw the potential item/NPC
		var pt = new Point16(i, j);
		if (TileEntity.ByPosition.TryGetValue(pt, out TileEntity value) && value is StoredTileEntity te)
			te.Draw();

        //Draw the tile
        const int alpha = 50;
        const float opacity = .7f;

        Lighting.GetCornerColors(i, j, out vertices);
        vertices.BottomLeftColor = (vertices.BottomLeftColor with { A = alpha }) * opacity;
        vertices.BottomRightColor = (vertices.BottomRightColor with { A = alpha }) * opacity;
        vertices.TopLeftColor = (vertices.TopLeftColor with { A = alpha }) * opacity;
        vertices.TopRightColor = (vertices.TopRightColor with { A = alpha }) * opacity;

        if (!Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].LiquidAmount == 255)
            vertices.BottomLeftColor = vertices.BottomRightColor = Color.Transparent; //Blend into water below

		Main.tileBatch.Draw(TextureAssets.Tile[Type].Value, position, source, vertices, Vector2.Zero, 1, SpriteEffects.None);
        spriteBatch.Draw(overlay.Value, position, source, Lighting.GetColor(i, j), 0, Vector2.Zero, 1, SpriteEffects.None, 0);

        return false;
    }
}
