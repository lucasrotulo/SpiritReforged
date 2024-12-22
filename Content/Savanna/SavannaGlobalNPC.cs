using SpiritReforged.Common.Misc;
using SpiritReforged.Content.Savanna.NPCs.Sparrow;
using SpiritReforged.Content.Savanna.Tiles.Paintings;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Savanna;

public class SavannaGlobalNPC : GlobalNPC
{
	internal static HashSet<int> savannaFaunaTypes = [];

	public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
	{
		if (npc.type == NPCID.Dryad && Main.LocalPlayer.InModBiome<Biome.SavannaBiome>())
		{
			var grassSeeds = items.FirstOrDefault(x => x != null && x.type == ItemID.GrassSeeds);

			if (grassSeeds != default)
				grassSeeds.SetDefaults(ModContent.ItemType<Items.SavannaGrassSeeds>());
		}
	}

	public override void ModifyShop(NPCShop shop)
	{
		if (shop.NpcType == NPCID.Painter)
		{
			shop.Add(Mod.Find<ModItem>(nameof(DustyFields) + "Item").Type, Condition.MoonPhaseFull, SpiritConditions.InSavanna);
			shop.Add(Mod.Find<ModItem>(nameof(DustyFields) + "Item").Type, Condition.MoonPhaseWaningGibbous, SpiritConditions.InSavanna);

			shop.Add(Mod.Find<ModItem>(nameof(OrangeSkies) + "Item").Type, Condition.MoonPhaseThirdQuarter, SpiritConditions.InSavanna);
			shop.Add(Mod.Find<ModItem>(nameof(OrangeSkies) + "Item").Type, Condition.MoonPhaseWaningCrescent, SpiritConditions.InSavanna);

			shop.Add(Mod.Find<ModItem>(nameof(StrongWinds) + "Item").Type, Condition.MoonPhaseNew, SpiritConditions.InSavanna);
			shop.Add(Mod.Find<ModItem>(nameof(StrongWinds) + "Item").Type, Condition.MoonPhaseWaxingCrescent, SpiritConditions.InSavanna);

			shop.Add(Mod.Find<ModItem>(nameof(WaningSun) + "Item").Type, Condition.MoonPhaseFirstQuarter, SpiritConditions.InSavanna);
			shop.Add(Mod.Find<ModItem>(nameof(WaningSun) + "Item").Type, Condition.MoonPhaseWaxingGibbous, SpiritConditions.InSavanna);

		}
	}

	public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
	{
		if (spawnInfo.Player.InModBiome<Biome.SavannaBiome>())
		{
			pool.Remove(0);

			if (!Main.dayTime)
				pool[NPCID.DoctorBones] = 0.005f;

			float odds = spawnInfo.Player.GetModPlayer<DustStorm.DustStormPlayer>().ZoneDustStorm ? .22f : .1f;
			pool[NPCID.Vulture] = odds;

			float critterOdds = spawnInfo.Player.GetModPlayer<DustStorm.DustStormPlayer>().ZoneDustStorm ? 0f : .05f;
			if (Main.dayTime)
			{
				pool[NPCID.Bird] = critterOdds;
				pool[ModContent.NPCType<Sparrow>()] = critterOdds * 2;
			}
		}
	}

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if ((npc.type == NPCID.Vulture || npc.type == NPCID.Bird || npc.type == ModContent.NPCType<Sparrow>()) && source is EntitySource_SpawnNPC)
		{
			//Move to an acacia treetop within 40 tiles when naturally spawned
			var nearby = Tiles.AcaciaTree.AcaciaTree.Platforms.Where(x => x.Distance(npc.Center) < 16 * 40).OrderBy(x => x.Distance(npc.Center)).FirstOrDefault();
			if (nearby != default)
			{
				npc.Center = nearby.Hitbox.ClosestPointInRect(npc.Center);
				npc.netUpdate = true;
			}
		}
	}
}
