using SpiritReforged.Common.TileCommon.FurnitureTiles;
using SpiritReforged.Content.Jungle.Bamboo.Items;

namespace SpiritReforged.Content.Jungle.Bamboo.Tiles;

public class BambooBench : SofaTile
{
	public override int CoreMaterial => ModContent.ItemType<StrippedBamboo>();
}