using SpiritReforged.Content.Savanna.Biome;
using SpiritReforged.Content.Savanna.DustStorm;
using Terraria.GameContent.Bestiary;

namespace SpiritReforged.Content.Savanna.NPCs.Sparrow;

[AutoloadCritter]
public class Sparrow : ModNPC
{
	public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 5;

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Bird);
		AIType = NPCID.Bird;
		AnimationType = NPCID.Bird;
		SpawnModBiomes = [ModContent.GetInstance<SavannaBiome>().Type];
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");

	public override void HitEffect(NPC.HitInfo hit)
	{
		for (int k = 0; k < 8; k++)
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2.5f * hit.HitDirection, -2.5f, 0, default, .9f);

		if (NPC.life <= 0 && !Main.dedServ)
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SparrowGore").Type, 1f);
	}
}