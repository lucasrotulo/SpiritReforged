namespace SpiritReforged.Content.Desert.GildedScarab;

internal class GildedScarabBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.pvpBuff[Type] = true;
		Main.buffNoTimeDisplay[Type] = false;
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		=> tip = Language.GetText("Mods.SpiritReforged.Buffs.GildedScarabBuff.Description").WithFormatArgs(Main.LocalPlayer.GetModPlayer<GildedScarabPlayer>().ScarabDefense).Value;

	public override void Update(Player player, ref int buffIndex) => player.statDefense += player.GetModPlayer<GildedScarabPlayer>().ScarabDefense;
}