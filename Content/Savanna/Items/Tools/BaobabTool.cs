using SpiritReforged.Content.Savanna.Ecotone;

namespace SpiritReforged.Content.Savanna.Items.Tools;

internal class BaobabTool : ModItem
{
	public override void SetStaticDefaults() => ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.CactusSword);
	}

	public override bool? UseItem(Player player)
	{
		Point point = Main.MouseWorld.ToTileCoordinates();
		BaobabGen.GenerateBaobab(point.X, point.Y);
		return true;
	}
}