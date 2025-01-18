﻿using SpiritReforged.Content.Underground.Railgun;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.WorldGeneration;
using SpiritReforged.Content.Forest.Misc;
using SpiritReforged.Content.Underground.ExplorerTreads;

namespace SpiritReforged.Content.Underground;

public class UndergroundChestLoot : ModSystem
{
	public override void PostWorldGen()
	{
		ChestPoolUtils.AddToVanillaChest(new ChestPoolUtils.ChestInfo(new int[] { ModContent.ItemType<ZiplineGun>(), ModContent.ItemType<ExplorerTreadsItem>() }, 1, 0.25f), (int)VanillaChestID.Gold, 1);
		ChestPoolUtils.AddToVanillaChest(new ChestPoolUtils.ChestInfo(new int[] { ModContent.ItemType<TornMapPiece>() }, 2, 0.1f), (int)VanillaChestID.Gold, Main.rand.Next(1, 4));
	}
}