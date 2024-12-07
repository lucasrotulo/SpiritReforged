using SpiritReforged.Common.BuffCommon;

namespace SpiritReforged.Content.Savanna.Items.Gar;

public class QuenchPotion : ModItem
{
	public override void Load()
	{
		On_Player.QuickBuff += FocusQuenchPotion;
		BuffHooks.ModifyBuffTime += QuenchifyBuff;
	}

	private void FocusQuenchPotion(On_Player.orig_QuickBuff orig, Player self)
	{
		if (!self.cursed && !self.CCed && !self.dead && !self.HasBuff<QuenchPotion_Buff>() && self.CountBuffs() < Player.MaxBuffs)
		{
			int itemIndex = self.FindItemInInventoryOrOpenVoidBag(Type, out bool inVoidBag);
			var item = inVoidBag ? self.bank4.item[itemIndex] : self.inventory[itemIndex];

			ItemLoader.UseItem(item, self);
			self.AddBuff(item.buffType, item.buffTime);

			if (item.consumable && ItemLoader.ConsumeItem(item, self) && --item.stack <= 0)
				item.TurnToAir();
		}

		orig(self);
	}

	private void QuenchifyBuff(int buffType, ref int buffTime, Player player, bool quickBuff)
	{
		if (!Main.debuff[buffType] && buffType != ModContent.BuffType<QuenchPotion_Buff>() && player.HasBuff<QuenchPotion_Buff>())
			buffTime = (int)(buffTime * 1.25f);
	}

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 30;
		Item.rare = ItemRarityID.Blue;
		Item.maxStack = Item.CommonMaxStack;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;
		Item.useTime = Item.useAnimation = 20;
		Item.consumable = true;
		Item.autoReuse = false;
		Item.buffType = ModContent.BuffType<QuenchPotion_Buff>();
		Item.buffTime = 60 * 45;
		Item.value = 200;
		Item.UseSound = SoundID.Item3;
	}

	public override void AddRecipes() => CreateRecipe()
			.AddIngredient(Mod.Find<ModItem>("GarItem").Type, 1)
			.AddIngredient(ItemID.Blinkroot, 1).AddIngredient(ItemID.Moonglow, 1)
			.AddIngredient(ItemID.Waterleaf, 1)
			.AddIngredient(ItemID.BottledWater, 1)
			.AddTile(TileID.Bottles)
			.Register();
}

public class QuenchPotion_Buff : ModBuff { }