using SpiritReforged.Common.Misc;
using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Particles;
using Terraria.Audio;

namespace SpiritReforged.Content.Forest.MarksmanArmor;

internal class MarksmanPlayer : ModPlayer
{
	public bool Concentrated => concentratedCooldown <= 0;
	private int concentratedCooldown = 360;

	public bool active = false;

	public override void ResetEffects() => active = false;

	public override void PostUpdateEquips()
	{
		var newCol = Color.Lerp(Color.LightGoldenrodYellow, Color.Goldenrod, Main.rand.NextFloat());

		if (active)
		{
			if (concentratedCooldown == 0)
			{
				SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Pitch = 1.5f }, Player.Center);

				for (int i = 0; i < 12; i++)
					ParticleHandler.SpawnParticle(new GlowParticle(Player.Center, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1f, 3f), newCol, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(30, 50), 12, delegate (Particle p) { p.Velocity *= 0.9f; }));

			}

			concentratedCooldown -= (Player.velocity.X == 0f) ? 2 : 1;
		}
		else
		{
			concentratedCooldown = 420;
		}

		if (Concentrated)
		{
			if (Main.rand.NextBool(12))
			{
				var rect = Player.getRect();
				var headRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height / 3);
				var position = Main.rand.NextVector2FromRectangle(headRect);

				ParticleHandler.SpawnParticle(new GlowParticle(position, Vector2.UnitY * -Main.rand.NextFloat(.5f), newCol, Main.rand.NextFloat(.2f, .3f), 80, 12));
			}
		}
	}

	public override void OnHurt(Player.HurtInfo info)
	{
		if (active)
		{
			if (Concentrated)
			{
				SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { PitchVariance = .5f }, Player.Center);
				SoundEngine.PlaySound(SoundID.Item98 with { Volume = .5f }, Player.Center);
			}

			concentratedCooldown = 360;
		}
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) 
	{
		if (!Concentrated)
			return;

		SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundMiss with { PitchVariance = .5f }, target.Center);
		SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact with { Pitch = .5f }, target.Center);
		SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Pitch = -.5f, Volume = .8f }, target.Center);

		var newCol = Color.Lerp(Color.LightGoldenrodYellow, Color.Goldenrod, Main.rand.NextFloat());

		for (int i = 0; i < 12; i++)
			ParticleHandler.SpawnParticle(new GlowParticle(target.Center, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1f, 3f), newCol, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(30, 50), 12, delegate (Particle p) { p.Velocity *= 0.96f; }));

		ParticleHandler.SpawnParticle(new DissipatingImage(target.Center, newCol.Additive(), Main.rand.NextFloat(MathHelper.TwoPi), 0.095f, Main.rand.NextFloat(-0.5f, 0.5f), "Fire", new(0.4f, 0.4f), new(5, 1), 25));

		modifiers.FinalDamage *= 1.2f;
		modifiers.SetCrit();
		concentratedCooldown = 300;
	}
}
