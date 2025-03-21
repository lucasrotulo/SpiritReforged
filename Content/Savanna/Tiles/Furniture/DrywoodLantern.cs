using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Common.TileCommon.PresetTiles;

namespace SpiritReforged.Content.Savanna.Tiles.Furniture;

public class DrywoodLantern : LanternTile
{
	public override int CoreMaterial => ItemMethods.AutoItemType<Drywood>();
	public override bool BlurGlowmask => false;
}
