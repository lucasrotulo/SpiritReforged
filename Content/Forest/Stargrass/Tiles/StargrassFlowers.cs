using SpiritReforged.Common.Visuals.Glowmasks;
using Terraria.GameContent.Metadata;

namespace SpiritReforged.Content.Forest.Stargrass.Tiles;

[AutoloadGlowmask("Method:Content.Forest.Stargrass.Tiles.StargrassFlowers Glow")]
public class StargrassFlowers : ModTile
{
	public static Color Glow(object obj)
	{
		const float MinBrightness = 0.4f;
		const float MaxDist = 140 * 140;

		var pos = (Point)obj;
		float dist = Main.player[Player.FindClosest(new Vector2(pos.X, pos.Y) * 16, 16, 16)].DistanceSQ(new Vector2(pos.X, pos.Y) * 16 + new Vector2(8));
		float strength = MinBrightness;

		if (dist < MaxDist)
			strength = MathHelper.Lerp(MinBrightness, 1f, 1 - dist / MaxDist);

		return StargrassTile.Glow(pos) * strength;
	}

	public override void SetStaticDefaults()
	{
		const int TileHeight = 24;

		Main.tileSolid[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoFail[Type] = true;
		Main.tileCut[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.SwaysInWindBasic[Type] = true;
		TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.newTile.WaterDeath = false;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinateHeights = [TileHeight];
		TileObjectData.newTile.DrawYOffset = -(TileHeight - 18);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 27;
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<StargrassTile>()];
		TileObjectData.newTile.AnchorAlternateTiles = [TileID.ClayPot, TileID.PlanterBox];
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(20, 190, 130));
		DustType = DustID.Grass;
		HitSound = SoundID.Grass;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 2;

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		int frame = Main.tile[i, j].TileFrameX / 18;
		if (frame >= 6)
			(r, g, b) = (0.025f, 0.1f, 0.25f);
	}
}