using SpiritReforged.Common.TileCommon;
using SpiritReforged.Common.TileCommon.FurnitureTiles;

namespace SpiritReforged.Content.Ocean.Tiles.Driftwood;

public class DriftwoodToilet : ToiletTile, IAutoloadTileItem
{
	public void SetItemDefaults(ModItem item) => item.Item.value = Item.sellPrice(copper: 30);

	public void AddItemRecipes(ModItem item) => item.CreateRecipe()
		.AddIngredient<Items.Driftwood.DriftwoodTileItem>(6)
		.AddTile(TileID.Sawmill)
		.Register();
}
