using SpiritReforged.Common.ItemCommon;

namespace SpiritReforged.Content.Savanna.Items.Food;

public class CustardApple : FoodItem
{
	internal override Point Size => new(26, 30);
	public override void StaticDefaults() => SetFruitType();
}

