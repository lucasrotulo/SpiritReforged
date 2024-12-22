namespace SpiritReforged.Content.Savanna;

public class SavannaGlobalItem : GlobalItem
{
	public override void SetDefaults(Item item)
	{
		if (item.type == Mod.Find<ModItem>("GoldGarItem").Type)
			item.value = Item.sellPrice(0, 10, 0, 0);

		if (item.type == Mod.Find<ModItem>("KillifishItem").Type)
			item.value = Item.sellPrice(0, 10, 0, 0);

		if (item.type == Mod.Find<ModItem>("KillifishBannerItem").Type)
			item.value = Item.sellPrice(0, 0, 2, 0);

		if (item.type == Mod.Find<ModItem>("GarBannerItem").Type)
			item.value = Item.sellPrice(0, 0, 2, 0);

		if (item.type == Mod.Find<ModItem>("GarItem").Type)
			item.value = Item.sellPrice(0, 0, 5, 37);

		if (item.type == Mod.Find<ModItem>("KillifishItem").Type)
			item.value = Item.sellPrice(0, 0, 3, 29);

		if (item.type == Mod.Find<ModItem>("TermiteItem").Type)
		{
			item.value = Item.sellPrice(0, 0, 0, 95);
			item.bait = 9;
		}

		if (item.type == Mod.Find<ModItem>("SparrowItem").Type)
			item.value = Item.sellPrice(0, 0, 5, 0);
	}

	public override void AddRecipes() => Recipe.Create(ItemID.HunterPotion, 1)
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient(ItemID.Blinkroot)
			.AddIngredient(Mod.Find<ModItem>("GarItem").Type)
			.AddTile(TileID.Bottles)
			.Register();
}
