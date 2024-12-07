namespace SpiritReforged.Common.ModCompat;

internal class MusicDisplayCompat : ModSystem
{
	public override void PostSetupContent()
	{
		if (!ModLoader.TryGetMod("MusicDisplay", out Mod display))
			return;

		LocalizedText modName = Language.GetText("Mods.SpiritReforged.MusicDisplay.ModName");

		void AddMusic(string name)
		{
			LocalizedText author = Language.GetText("Mods.SpiritReforged.MusicDisplay." + name + ".Author");
			LocalizedText displayName = Language.GetText("Mods.SpiritReforged.MusicDisplay." + name + ".DisplayName");
			display.Call("AddMusic", (short)MusicLoader.GetMusicSlot(SpiritReforgedMod.Instance, "Assets/Music/" + name), displayName, author, modName);
		}

		AddMusic("Duststorm");
		AddMusic("Savanna");
		AddMusic("DeepOcean");
	}
}
