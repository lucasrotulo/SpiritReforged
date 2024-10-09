using SpiritReforged.Content.Vanilla.Items.Food;

namespace SpiritReforged.Content.Savanna.NPCs.Gar;

[AutoloadCritter]
public class GoldGar : Gar
{
	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter += 0.22f;
		NPC.frameCounter %= Main.npcFrameCount[NPC.type];
		int frame = (int)NPC.frameCounter;
		NPC.frame.Y = frame * frameHeight;
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		for (int i = 0; i < 13; i++)
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Sunflower, 2f * hit.HitDirection, -2f, 0, default, Main.rand.NextFloat(0.75f, 0.95f));

		if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
		{
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GarGore5").Type, 1f);
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("GarGore6").Type, Main.rand.NextFloat(.5f, .7f));
		}
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.AddCommon<RawFish>(2);
}