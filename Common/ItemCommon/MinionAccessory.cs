using SpiritReforged.Common.PlayerCommon;

namespace SpiritReforged.Common.ItemCommon;

/// <summary> 
/// A class that handles boilerplate code that every minion accessory (accessories that spawn a guy) has. 
/// Needs each Minion Accessory to provide damage and a projectile type
/// Stores data using a record that can be referenced later, and associates data with the itemtype
/// </summary>
public record MinionAccessoryData(int ProjType, int Damage);

public abstract class MinionAccessory : AccessoryItem
{
	public abstract MinionAccessoryData Data { get; }

	public override void SetStaticDefaults()
	{
		MinionAccessoryPlayer.MinionDataByItemId.Add(Type, Data);

		base.SetStaticDefaults();
	}
}
