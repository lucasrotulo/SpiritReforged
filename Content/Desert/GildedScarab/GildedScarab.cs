using SpiritReforged.Common.ItemCommon;

namespace SpiritReforged.Content.Desert.GildedScarab;

internal class GildedScarab : AccessoryItem
{
	public override void SetDefaults()
	{
		Item.width = 34;
		Item.height = 32;
		Item.rare = ItemRarityID.Blue;
		Item.accessory = true;
		Item.value = Item.sellPrice(0, 1, 0, 0);
	}
}