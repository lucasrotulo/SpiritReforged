using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Ocean.Items.Reefhunter.Particles;
using Terraria.Audio;

public class PoolNoodleGNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public bool bubbled;

	public override void ResetEffects(NPC npc) => bubbled = false;

	public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
		if (bubbled && projectile.minion && Main.rand.NextBool(5))
		{
			ParticleHandler.SpawnParticle(new BubblePop(npc.Center, .6f, 0.9f, 35, Main.rand.NextFloat(-5f, 5f)));

			SoundEngine.PlaySound(SoundID.Item54, npc.Center);
			SoundEngine.PlaySound(SoundID.Item86, npc.Center);
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/Impact_LightPop") with { PitchVariance = 0.4f, Pitch = .5f, MaxInstances = 10 }, npc.Center);
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/Explosion_Balloon") with { PitchVariance = 0.2f, MaxInstances = 10 }, npc.Center);
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/Explosion_Liquid") with { Volume = .75f, PitchVariance = 0.2f, MaxInstances = 10 }, npc.Center);

			int radius = 40;
			Player p = Main.player[projectile.owner];

			for (int i = 0; i < Main.maxNPCs; ++i)
			{
				NPC n = Main.npc[i];
				if (n.CanBeChasedBy() && n.DistanceSQ(projectile.Center) < radius * radius)
					n.SimpleStrikeNPC((int)(damageDone * 1.5f), p.Center.X < n.Center.X ? -1 : 1, false, 4f);
			}
		}
	}
}