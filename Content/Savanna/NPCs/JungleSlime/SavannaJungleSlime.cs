using SpiritReforged.Content.Savanna.Biome;
using SpiritReforged.Content.Savanna.DustStorm;
using SpiritReforged.Content.Vanilla.Items.Food;
using Terraria.GameContent.Bestiary;

namespace SpiritReforged.Content.Savanna.NPCs.JungleSlime;

public class SavannaJungleSlime : ModNPC
{
	public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 2;

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.SandSlime);
		NPC.color = Color.White * .8f;

		AIType = NPCID.JungleSlime;
		AnimationType = NPCID.BlueSlime;
		Banner = Item.NPCtoBanner(NPCID.JungleSlime);
		BannerItem = Item.BannerToItem(Banner);
		NPC.alpha = 60;
		SpawnModBiomes = [ModContent.GetInstance<SavannaBiome>().Type];
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Sandstorm");

	public override void HitEffect(NPC.HitInfo hit)
	{
		for (int k = 0; k < 20; k++)
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.JungleGrass, 2.5f * hit.HitDirection, -2.5f, 0, default, 0.78f);
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.AddCommon(ItemID.Gel, 1, 1, 3);
		npcLoot.AddCommon(ItemID.SlimeStaff, 10000);
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		var player = spawnInfo.Player;
		return player.InModBiome<SavannaBiome>() && player.ZoneJungle ? 0.1f : 0;
	}
}