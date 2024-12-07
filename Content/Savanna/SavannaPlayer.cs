using SpiritReforged.Content.Savanna.Biome;
using SpiritReforged.Content.Savanna.Items.Fishing;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna;

public class SavannaPlayer : ModPlayer
{
	public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
	{
		if (Player.InModBiome<SavannaBiome>() && attempt.crate)
			itemDrop = Main.hardMode ? ModContent.ItemType<HardmodeSavannaCrate>() : ModContent.ItemType<SavannaCrate>();
	}
}