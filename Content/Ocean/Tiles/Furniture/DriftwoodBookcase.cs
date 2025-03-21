using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Common.TileCommon.PresetTiles;

namespace SpiritReforged.Content.Ocean.Tiles.Furniture;

public class DriftwoodBookcase : BookcaseTile
{
	public override int CoreMaterial => ItemMethods.AutoItemType<Driftwood>();
}
