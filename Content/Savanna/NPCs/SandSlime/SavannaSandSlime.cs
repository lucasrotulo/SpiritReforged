using SpiritReforged.Content.Savanna.Biome;
using SpiritReforged.Content.Savanna.DustStorm;
using Terraria.GameContent.Bestiary;

namespace SpiritReforged.Content.Savanna.NPCs.SandSlime;

public class SavannaSandSlime : ModNPC
{
	public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 3;

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.SandSlime);

		AIType = NPCID.SandSlime;
		AnimationType = NPCID.SandSlime;
		Banner = Item.NPCtoBanner(NPCID.SandSlime);
		BannerItem = Item.BannerToItem(Banner);
		SpawnModBiomes = [ModContent.GetInstance<SavannaBiome>().Type];
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Sandstorm");

	public override void HitEffect(NPC.HitInfo hit)
	{
		for (int k = 0; k < 20; k++)
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sand, 2.5f * hit.HitDirection, -2.5f, 0, default, 0.78f);
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.AddCommon(ItemID.Gel, 1, 1, 3);
		npcLoot.AddCommon(ItemID.SlimeStaff, 10000);
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		var player = spawnInfo.Player;
		return player.InModBiome<SavannaBiome>() && player.GetModPlayer<DustStormPlayer>().ZoneDustStorm ? 0.3f : 0;
	}
}