using SpiritReforged.Common.TileCommon.PresetTiles;
using SpiritReforged.Content.Ocean.Items;

namespace SpiritReforged.Content.Ocean.Tiles;

public class PirateChest : ChestTile
{
	//public override bool IsLoadingEnabled(Mod mod) => SpiritClassic.Enabled; //IAutoloadTileItem expects this to always be loaded

	public override void StaticDefaults()
	{
		base.StaticDefaults();

		Main.tileShine2[Type] = true;
		Main.tileShine[Type] = 1200;

		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
		TileID.Sets.GeneralPlacementTiles[Type] = false;

		AddMapEntry(new Color(161, 115, 54), MapEntry, MapChestName);
		AddMapEntry(new Color(87, 64, 31), MapEntry, MapChestName);
		MakeLocked(ModContent.ItemType<PirateKey>());
	}

	public override ushort GetMapOption(int i, int j) => (ushort)(IsLockedChest(i, j) ? 1 : 0);
	public override bool IsLockedChest(int i, int j) => Main.tile[i, j] != null && Main.tile[i, j].TileFrameX > 18;
	public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
	{
		dustType = DustID.Gold;
		return true;
	}
}