using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.TileSway;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna.Items.BaobabFruit;

[DrawOrder(DrawOrderAttribute.Layer.NonSolid)]
public class BaobabFruitTile : ModTile, ISwayInWind
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileMergeDirt[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileCut[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.Origin = new(0, 0);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Tiles.LivingBaobabLeaf>()];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 2;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(140, 140, 100));
		DustType = DustID.t_PearlWood;
		HitSound = SoundID.Grass;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		var position = new Vector2(i, j) * 16 + new Vector2(8, 22);
		Projectile.NewProjectile(new EntitySource_TileBreak(i, j), position, Vector2.Zero, ModContent.ProjectileType<BaobabFruitProj>(), 10, 0f, -1, frameX / 18);
	}

	public void DrawInWind(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin)
	{
		var tile = Framing.GetTileSafely(i, j);

		var drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y);
		var source = new Rectangle((tile.TileFrameY == 0) ? 18 : tile.TileFrameX, tile.TileFrameY, 16, 16);
		var effects = (i / 18 % 2 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		spriteBatch.Draw(TextureAssets.Tile[Type].Value, drawPos + offset - new Vector2(0, 2), source, Lighting.GetColor(i, j), rotation, origin, 1, effects, 0f);
	}
}
