using SpiritReforged.Common.TileCommon.FurnitureTiles;

namespace SpiritReforged.Content.Savanna.Tiles.Furniture;

public class DrywoodChandelier : ChandelierTile
{
	public override int CoreMaterial => ModContent.ItemType<Items.Drywood.Drywood>();
	public override bool BlurGlowmask => false;
}
