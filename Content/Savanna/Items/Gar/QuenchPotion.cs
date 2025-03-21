using SpiritReforged.Common.BuffCommon;

namespace SpiritReforged.Content.Savanna.Items.Gar;

public class QuenchPotion : ModItem
{
	#region detours
	public override void Load()
	{
		On_Player.QuickBuff += FocusQuenchPotion;
		BuffPlayer.ModifyBuffTime += QuenchifyBuff;
	}

	/// <summary> Forces this potion to be used before all others with quick buff. </summary>
	private static void FocusQuenchPotion(On_Player.orig_QuickBuff orig, Player self)
	{
		if (!self.cursed && !self.CCed && !self.dead && !self.HasBuff<QuenchPotion_Buff>() && self.CountBuffs() < Player.MaxBuffs)
		{
			int itemIndex = self.FindItemInInventoryOrOpenVoidBag(ModContent.ItemType<QuenchPotion>(), out bool inVoidBag);

			if (itemIndex > 0)
			{
				var item = inVoidBag ? self.bank4.item[itemIndex] : self.inventory[itemIndex];

				ItemLoader.UseItem(item, self);
				self.AddBuff(item.buffType, item.buffTime);

				if (item.consumable && ItemLoader.ConsumeItem(item, self) && --item.stack <= 0)
					item.TurnToAir();
			}
		}

		orig(self);
	}

	/// <summary> Improves buff times with <see cref="QuenchPotion_Buff"/>. </summary>
	private static void QuenchifyBuff(int buffType, ref int buffTime, Player player, bool quickBuff)
	{
		if (!Main.debuff[buffType] && buffType != ModContent.BuffType<QuenchPotion_Buff>() && player.HasBuff<QuenchPotion_Buff>())
			buffTime = (int)(buffTime * 1.25f);
	}
	#endregion

	public override void SetStaticDefaults() => Item.ResearchUnlockCount = 20;

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

	public override void AddRecipes() => CreateRecipe().AddIngredient(Mod.Find<ModItem>("GarItem").Type).AddIngredient(ItemID.Blinkroot)
		.AddIngredient(ItemID.Moonglow).AddIngredient(ItemID.Waterleaf).AddIngredient(ItemID.BottledWater).AddTile(TileID.Bottles).Register();
}

public class QuenchPotion_Buff : ModBuff { }