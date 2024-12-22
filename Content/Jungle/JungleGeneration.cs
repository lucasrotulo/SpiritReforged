using SpiritReforged.Common.Misc;
using SpiritReforged.Content.Jungle.Toucane;

namespace SpiritReforged.Content.Jungle;

public class JungleGeneration : ModSystem
{
	public override void PostWorldGen() => ChestPoolUtils.AddToVanillaChest(new ChestPoolUtils.ChestInfo(new int[] { ModContent.ItemType<ToucaneItem>() }, 1, 0.25f), ChestPoolUtils.ivyChests, 1);
}