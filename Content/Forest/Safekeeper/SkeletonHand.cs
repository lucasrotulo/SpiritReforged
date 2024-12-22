using SpiritReforged.Common.Particle;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Forest.Safekeeper;

public class SkeletonHand : ModTile
{
	private static Asset<Texture2D> glowTexture;
	private static readonly Point[] glowPoints = [new Point(9, 11), new Point(13, 5), new Point(7, 11)]; //Corresponds to different styles

	public override void SetStaticDefaults()
	{
		Main.tileNoFail[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;

		const int height = 30;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.CoordinateWidth = 26;
		TileObjectData.newTile.CoordinateHeights = [height];
		TileObjectData.newTile.DrawYOffset = -(height - 18);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(165, 165, 150));
		RegisterItemDrop(ModContent.ItemType<SafekeeperRing>());

		glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		var tile = Main.tile[i, j];

		var data = TileObjectData.GetTileData(tile);
		var zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, data.CoordinateWidth, data.CoordinateFullHeight);
		var position = new Vector2(i, j) * 16 - new Vector2((source.Width - 16) / 2, source.Height - 16 - 4);

		float mult = MathHelper.Clamp(1f - Main.LocalPlayer.Distance(new Vector2(i, j) * 16) / 100f, 0, 1);

		spriteBatch.Draw(glowTexture.Value, position - Main.screenPosition + zero, 
			source, Color.White * mult, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

		if (!Main.gamePaused && mult > 0 && Main.rand.NextBool(5))
		{
			int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
			var dustPos = position + glowPoints[style].ToVector2() + Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f);

			ParticleHandler.SpawnParticle(new Particles.GlowParticle(dustPos,
				Vector2.UnitY * -Main.rand.NextFloat(.5f), Color.White, Color.Orange, .15f, 30, 5));
		}
	}
}
