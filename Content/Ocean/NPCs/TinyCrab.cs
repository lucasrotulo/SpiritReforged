using Terraria.GameContent.Bestiary;

namespace SpiritReforged.Content.Ocean.NPCs;

[AutoloadCritter]
public class TinyCrab : ModNPC
{
	public override void SetStaticDefaults() => Main.npcFrameCount[NPC.type] = 4;

	public override void SetDefaults()
	{
		NPC.dontCountMe = true;
		NPC.width = 18;
		NPC.height = 18;
		NPC.damage = 0;
		NPC.defense = 0;
		NPC.lifeMax = 5;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = .45f;
		NPC.aiStyle = 67;
		NPC.npcSlots = 0;
		AIType = NPCID.Bunny;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Ocean");

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter += 0.15f;
		NPC.frameCounter %= Main.npcFrameCount[NPC.type];
		int frame = (int)NPC.frameCounter;
		NPC.frame.Y = frame * frameHeight;
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
		{
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TinyCrabGore").Type, 1f);
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TinyCrabGore").Type, Main.rand.NextFloat(.5f, .7f));
		}
	}
}