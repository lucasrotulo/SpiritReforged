﻿using SpiritReforged.Content.Cloudstalk.Buffs;

namespace SpiritReforged.Content.Cloudstalk.Items;

public class DoubleJumpPotion : ModItem
{
	public override void SetStaticDefaults() => Item.ResearchUnlockCount = 20;

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 34;
		Item.rare = ItemRarityID.Blue;
		Item.maxStack = Item.CommonMaxStack;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;
		Item.useTime = Item.useAnimation = 20;
		Item.consumable = true;
		Item.autoReuse = false;
		Item.buffType = ModContent.BuffType<DoubleJumpPotionBuff>();
		Item.buffTime = 10800;
		Item.UseSound = SoundID.Item3;
	}

	public override void AddRecipes()
	{
		Recipe recipe = CreateRecipe();
		recipe.AddIngredient(ItemID.BottledWater);
		recipe.AddIngredient(ModContent.ItemType<Cloudstalk>());
		recipe.AddIngredient(ItemID.Cloud, 5);
		recipe.AddIngredient(ItemID.Feather);
		recipe.AddTile(TileID.Bottles);
		recipe.Register();
	}
}
