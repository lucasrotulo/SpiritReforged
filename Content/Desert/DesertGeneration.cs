using SpiritReforged.Content.Desert.GildedScarab;
using SpiritReforged.Common.Misc;

namespace SpiritReforged.Content.Desert;

public class DesertGeneration : ModSystem
{
	public override void PostWorldGen() => ChestPoolUtils.AddToVanillaChest(new ChestPoolUtils.ChestInfo(new int[] { ModContent.ItemType<GildedScarab.GildedScarab>() }, 1, 0.25f), ChestPoolUtils.sandstoneChests, 1, TileID.Containers2);
}