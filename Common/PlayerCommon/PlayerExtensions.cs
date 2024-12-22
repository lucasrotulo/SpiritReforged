using SpiritReforged.Common.ItemCommon;

namespace SpiritReforged.Common.PlayerCommon;

internal static class PlayerExtensions
{
	/// <summary>
	/// Gets the item timer for the given item on the given player.
	/// </summary>
	/// <param name="player">The associated player.</param>
	/// <param name="item">The item to check against</param>
	/// <param name="slot"></param>
	/// <returns>The timer value for the item.</returns>
	/// <exception cref="InvalidCastException"/>
	public static int ItemTimer(this Player player, ModItem item, int slot = -1) 
		=> item is ITimerItem tItem
			? player.GetModPlayer<MiscAccessoryPlayer>().timers[tItem.GetType().Name + (slot == -1 ? "" : slot.ToString())]
			: throw new InvalidCastException("Item timer not found or invalid.");

	/// <summary>
	/// Gets the item timer for the given item on the given player.
	/// </summary>
	/// <typeparam name="T">The ITimerItem type to check for.</typeparam>
	/// <param name="player">The associated player.</param>
	/// <param name="slot"></param>
	/// <returns>The timer value for the item.</returns>
	public static int ItemTimer<T>(this Player player, int slot = -1) where T : ModItem, ITimerItem 
		=> player.GetModPlayer<MiscAccessoryPlayer>().timers[typeof(T).Name + (slot == -1 ? "" : slot.ToString())];

	/// <summary>
	/// Sets the item timer for the given item on the given player.
	/// </summary>
	/// <typeparam name="T">The ITimerItem type to set.</typeparam>
	/// <param name="player">The associated player.</param>
	/// <param name="value">The value to set the timer to.</param>
	/// <param name="slot"></param>
	public static void SetItemTimer<T>(this Player player, int value, int slot = -1) where T : ModItem, ITimerItem 
		=> player.GetModPlayer<MiscAccessoryPlayer>().timers[ModContent.GetInstance<T>().GetType().Name + (slot == -1 ? "" : slot.ToString())] = value;

	public static bool HasAccessory(this Player player, Item item) => item.ModItem is AccessoryItem acc && player.GetModPlayer<MiscAccessoryPlayer>().accessory[acc.AccName];
	public static bool HasAccessory(this Player player, ModItem item) => item is AccessoryItem acc && player.GetModPlayer<MiscAccessoryPlayer>().accessory[acc.AccName];
	public static bool HasAccessory<TItem>(this Player player) where TItem : AccessoryItem => player.GetModPlayer<MiscAccessoryPlayer>().accessory[ModContent.GetInstance<TItem>().AccName];
	public static bool HasAccessory(this Player player, int itemId) => HasAccessory(player, ContentSamples.ItemsByType[itemId]);

}
