using SpiritReforged.Common.Misc;
using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Ocean.Items.Reefhunter.Particles;

namespace SpiritReforged.Content.Ocean.Items.PoolNoodle;

public class PoolNoodleBubbleBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		Main.pvpBuff[Type] = false;
		Main.buffNoSave[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.GetGlobalNPC<PoolNoodleGNPC>().bubbled = true;

		if (Main.rand.NextBool(35))
			ParticleHandler.SpawnParticle(new BubbleParticle(npc.Center, new Vector2(0, Main.rand.NextFloat(-1.5f, 0.5f)), Main.rand.NextFloat(0.05f, 0.1f), 40));
	}
}