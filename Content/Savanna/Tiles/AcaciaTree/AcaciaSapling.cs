using SpiritReforged.Common.TileCommon.CustomTree;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;

namespace SpiritReforged.Content.Savanna.Tiles.AcaciaTree;

public class AcaciaSapling : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.Width = 1;
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SavannaGrass>()];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.DrawFlipHorizontal = true;
		TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.StyleMultiplier = 3;
		TileObjectData.addTile(Type);

		//TileID.Sets.TreeSapling[Type] = true; //Will break on tile update if this is true
		TileID.Sets.CommonSapling[Type] = true;
		TileID.Sets.SwaysInWindBasic[Type] = true;
		TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

		AddMapEntry(new Color(170, 120, 100), Language.GetText("MapObject.Sapling"));

		DustType = DustID.WoodFurniture;
		AdjTiles = [TileID.Saplings];

		SaplingHandler.RegisterSapling(Type);
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	public override void RandomUpdate(int i, int j)
	{
		if (WorldGen.genRand.NextBool(20))
			CustomTree.GrowTree<AcaciaTree>(i, j);
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
	{
		if (i % 2 == 0)
			effects = SpriteEffects.FlipHorizontally;
	}
}