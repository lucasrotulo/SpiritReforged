using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Content.Ocean.Items;
using SpiritReforged.Content.Ocean.Tiles;

namespace SpiritReforged.Content.Vanilla.Items.Food;

public class SteamedMussels : FoodItem
{
	internal override Point Size => new(46, 22);

	public override void AddRecipes()
	{
		Recipe recipe1 = CreateRecipe(1);
		recipe1.AddIngredient(ModContent.ItemType<MusselItem>(), 3);
		recipe1.AddIngredient(ModContent.ItemType<Kelp>(), 1);
		recipe1.AddTile(TileID.CookingPots);
		recipe1.Register();
	}
}
