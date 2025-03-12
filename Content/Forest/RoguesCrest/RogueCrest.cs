using SpiritReforged.Common.ItemCommon;
using Terraria.GameContent.ItemDropRules;

namespace SpiritReforged.Content.Forest.RoguesCrest;

[AutoloadEquip(EquipType.Neck)]
public class RogueCrest : MinionAccessory
{
	public override MinionAccessoryData Data => new MinionAccessoryData(ModContent.ProjectileType<RogueKnifeMinion>(), 6);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		CrateDatabase.AddCrateRule(ItemID.WoodenCrate, ItemDropRule.Common(Type, 4));
		CrateDatabase.AddCrateRule(ItemID.WoodenCrateHard, ItemDropRule.Common(Type, 4));
	}

	public override void SetDefaults()
	{
		Item.damage = 4;
		Item.DamageType = DamageClass.Summon;
		Item.knockBack = .5f;
		Item.width = 38;
		Item.height = 36;
		Item.value = Item.buyPrice(0, 3, 0, 0);
		Item.rare = ItemRarityID.Blue;
		Item.defense = 1;
		Item.accessory = true;
	}
}
