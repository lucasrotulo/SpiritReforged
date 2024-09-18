using SpiritReforged.Common.TileCommon;

namespace SpiritReforged.Content.Savanna.Tiles;

public class SavannaDirt : ModTile, IAutoloadTileItem
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileMergeDirt[Type] = true;
		Main.tileBlockLight[Type] = true;

		Main.tileMerge[Type][TileID.WoodBlock] = true;
		Main.tileMerge[TileID.WoodBlock][Type] = true;

		Main.tileMerge[Type][TileID.Sand] = true;
		Main.tileMerge[TileID.Sand][Type] = true;

		AddMapEntry(new Color(138, 79, 45));
		MineResist = .5f;
	}

	public override bool CanExplode(int i, int j) => true;
}