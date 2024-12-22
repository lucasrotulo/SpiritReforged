using SpiritReforged.Content.Forest.Safekeeper;
using SpiritReforged.Content.Ocean.Items.Blunderbuss;

namespace SpiritReforged.Common.NPCCommon;

internal class DiscoveryTravelShop : GlobalNPC
{
	public override void SetupTravelShop(int[] shop, ref int nextSlot)
	{
		if (!Main.rand.NextBool(2))
			return;

		int[] types = [ModContent.ItemType<Blunderbuss>(), ModContent.ItemType<SafekeeperRing>()];

		shop[nextSlot] = types[Main.rand.Next(types.Length)]; //Select one discovery item
		nextSlot++;
	}
}
