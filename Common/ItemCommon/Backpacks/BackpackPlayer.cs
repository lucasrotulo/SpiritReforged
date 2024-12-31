using Terraria.ModLoader.IO;

namespace SpiritReforged.Common.ItemCommon.Backpacks;

internal class BackpackPlayer : ModPlayer
{
	public Item backpack = new();
	public Item vanityBackpack = new();
	public bool packVisible = true;

	private int _lastSelectedEquipPage = 0;
	private bool _hadBackpack = false;

	internal static bool TryGetBackpack(Player player, out BackpackItem backpack)
	{
		if (player.GetModPlayer<BackpackPlayer>().backpack.ModItem is BackpackItem tryBackpack)
		{
			backpack = tryBackpack;
			return true;
		}

		backpack = null;
		return false;
	}

	public override void UpdateEquips()
	{
		if (Player.HeldItem.ModItem is BackpackItem) //Open the equip menu when a backpack is picked up
		{
			if (!_hadBackpack)
				_lastSelectedEquipPage = Main.EquipPageSelected;

			Main.EquipPageSelected = 2;
		}
		else if (_hadBackpack)
		{
			Main.EquipPageSelected = _lastSelectedEquipPage;
		}

		_hadBackpack = Player.HeldItem.ModItem is BackpackItem;
	}

	public override void FrameEffects() //This way, players can be seen wearing backpacks in the selection screen
	{
		if (vanityBackpack != null && !vanityBackpack.IsAir)
			ApplyEquip(vanityBackpack);
		else if (backpack != null && !backpack.IsAir && packVisible)
			ApplyEquip(backpack);
	}

	private void ApplyEquip(Item backpack)
	{
		Player.back = EquipLoader.GetEquipSlot(Mod, backpack.ModItem.Name, EquipType.Back);
		Player.front = EquipLoader.GetEquipSlot(Mod, backpack.ModItem.Name, EquipType.Front);
	}

	public override void SaveData(TagCompound tag)
	{
		if (backpack is not null)
			tag.Add("backpack", ItemIO.Save(backpack));

		if (vanityBackpack is not null)
			tag.Add("vanity", ItemIO.Save(vanityBackpack));

		tag.Add(nameof(packVisible), packVisible);
	}

	public override void LoadData(TagCompound tag)
	{
		if (tag.TryGet("backpack", out TagCompound item))
			backpack = ItemIO.Load(item);

		if (tag.TryGet("vanity", out TagCompound vanity))
			vanityBackpack = ItemIO.Load(vanity);

		packVisible = tag.Get<bool>(nameof(packVisible));
	}
}
