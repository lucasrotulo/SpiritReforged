namespace SpiritReforged.Content.Savanna;

public class SavannaRecipes : ModSystem
{
	public override void AddRecipes()
	{
		Recipe.Create(ItemID.HunterPotion, 1)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Blinkroot)
			.AddIngredient(Mod.Find<ModItem>("GarItem").Type)
			.AddTile(TileID.Bottles)
			.Register();

		Recipe.Create(ItemID.RoastedBird, 1)
			.AddIngredient(Mod.Find<ModItem>("SparrowItem").Type)
			.AddTile(TileID.CookingPots)
			.Register();
	}

	public override void AddRecipeGroups()
	{
		RecipeGroup birds = RecipeGroup.recipeGroups[RecipeGroup.recipeGroupIDs["Birds"]];
		birds.ValidItems.Add(Mod.Find<ModItem>("SparrowItem").Type);
	}
}
