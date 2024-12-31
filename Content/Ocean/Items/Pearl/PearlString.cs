using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Common.PlayerCommon;

namespace SpiritReforged.Content.Ocean.Items.Pearl;

[AutoloadEquip(EquipType.Neck)]
public class PearlString : AccessoryItem
{
	public override void SetDefaults()
	{
		Item.width = Item.height = 20;
		Item.accessory = true;
		Item.value = Item.sellPrice(gold: 10);
		Item.rare = ItemRarityID.Orange;
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		base.UpdateAccessory(player, hideVisual);
		player.luck += .15f;
	}
}

internal class PearlStringNPC : GlobalNPC
{
	public override bool PreKill(NPC npc)
	{
		if (Main.player[npc.lastInteraction].HasAccessory<PearlString>())
			npc.value *= 1.1f; //+10% coin drops

		return true;
	}
}
