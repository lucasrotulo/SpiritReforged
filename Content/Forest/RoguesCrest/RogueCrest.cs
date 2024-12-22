using SpiritReforged.Common.ItemCommon;

namespace SpiritReforged.Content.Forest.RoguesCrest;

[AutoloadEquip(EquipType.Neck)]
public class RogueCrest : MinionAccessory
{
	public override MinionAccessoryData Data => new MinionAccessoryData(ModContent.ProjectileType<RogueKnifeMinion>(), 5);

	public override void SetDefaults()
	{
		Item.damage = 5;
		Item.DamageType = DamageClass.Summon;
		Item.knockBack = .5f;
		Item.width = 48;
		Item.height = 49;
		Item.value = Item.buyPrice(0, 3, 0, 0);
		Item.rare = ItemRarityID.Green;
		Item.defense = 1;
		Item.accessory = true;
	}
}
