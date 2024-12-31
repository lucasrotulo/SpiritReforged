using SpiritReforged.Common.Particle;
using SpiritReforged.Common.TileCommon;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Ocean.Items.Pearl;

[DrawOrder(DrawOrderAttribute.Layer.Solid)] //Draw over sand
public class PearlStringTile : ModTile
{
	private static Asset<Texture2D> glowTexture;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoFail[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
		TileObjectData.newTile.CoordinateHeights = [18];
		TileObjectData.newTile.Origin = new(1, 0);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
		TileObjectData.newTile.AnchorValidTiles = [TileID.Sand, TileID.Ebonsand, TileID.Crimsand, TileID.Pearlsand];
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(100, 100, 120));
		RegisterItemDrop(ModContent.ItemType<PearlString>());

		DustType = -1; //No dust

		glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight)
	{
		int type = Framing.GetTileSafely(i, j + 1).TileType;

		switch (type)
		{
			case TileID.Sand:
				Framing.GetTileSafely(i, j).TileFrameY = 0;
				break;

			case TileID.Ebonsand:
				Framing.GetTileSafely(i, j).TileFrameY = 18;
				break;

			case TileID.Crimsand:
				Framing.GetTileSafely(i, j).TileFrameY = 36;
				break;

			case TileID.Pearlsand:
				Framing.GetTileSafely(i, j).TileFrameY = 54;
				break;
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		var tile = Framing.GetTileSafely(i, j);
		var texture = TextureAssets.Tile[tile.TileType].Value;

		var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
		var position = new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(0, 14);

		spriteBatch.Draw(texture, position, source, Lighting.GetColor(i, j), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		spriteBatch.Draw(glowTexture.Value, position, source, Lighting.GetColor(i, j) * 2, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

		var rect = new Rectangle(i * 16, j * 16 + 14, 16, 16);
		if (!Main.gamePaused && Main.rand.NextBool(50) && Main.LocalPlayer.Distance(rect.Center()) < 100) //Nearby dust effects
		{
			var dustPos = Main.rand.NextVector2FromRectangle(rect);

			var dust = Dust.NewDustPerfect(dustPos, DustID.SilverCoin, Scale: .2f);
			dust.noGravity = true;
			dust.velocity = Vector2.Zero;
			dust.noLightEmittence = true;

			ParticleHandler.SpawnParticle(new Particles.GlowParticle(dustPos, Vector2.Zero, Main.DiscoColor * .8f, Color.Black, .75f, 50));
		}

		return false;
	}
}