using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Ocean.Items;
using SpiritReforged.Content.Particles;
using Terraria.Audio;

namespace SpiritReforged.Content.Ocean.Hydrothermal;

public class HydrothermalVentPlume : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	public override void SetDefaults()
	{
		Projectile.ignoreWater = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = Tiles.HydrothermalVent.eruptDuration;
	}

	public override void AI()
	{
		if (Projectile.timeLeft % 20 == 0) //Spawn slag pickups
		{
			SoundEngine.PlaySound(SoundID.Drown with { Pitch = -.5f, PitchVariance = .25f, Volume = 1.5f }, Projectile.Center);

			if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(4))
				Item.NewItem(Projectile.GetSource_FromAI(), Projectile.Center, ModContent.ItemType<MineralSlagPickup>());
		}

		if (Main.rand.NextBool(10))
			ForegroundEmber();

		if (Main.rand.NextBool(12)) //Small embers
			ParticleHandler.SpawnParticle(new GlowParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(-1f, 1f) * 4, 0),
				(Projectile.velocity * Main.rand.NextFloat(.25f)).RotatedByRandom(.4f), Color.OrangeRed, Main.rand.NextFloat(.1f, .4f), 190, 8, delegate (Particle p)
				{
					p.Velocity = p.Velocity.RotatedByRandom(.05f);
				}));

		for (int i = 0; i < 2; i++)
		{
			var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, .1f + Main.rand.Next(5) * .1f);
			dust.fadeIn = 1.5f + Main.rand.Next(5) * 0.1f;
			dust.noGravity = true;
			dust.position = Projectile.Center + new Vector2(0f, -Projectile.height / 2f).RotatedBy(Projectile.rotation, default) * 1.1f;
		}

		float speedY = -2.5f;
		var dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Utils.SelectRandom(Main.rand, 6, 259, 31), 0f, speedY, 200, default, Main.rand.NextFloat() + .5f);

		dust2.velocity *= new Vector2(0.3f, 2f);
		dust2.velocity.Y -= 2;
		dust2.position = new Vector2(Projectile.Center.X, Projectile.Center.Y + Projectile.height * -0.5f);
		dust2.noGravity = true;
		dust2.fadeIn = 1.5f;
	}

	private void ForegroundEmber()
	{
		var startPos = new Vector2(Projectile.Center.X - Main.screenWidth / 2 + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y);

		if (!Collision.WetCollision(startPos, 2, 2))
			return;

		var p = new FireParticleScreen();
		p.Position = startPos;
		p.OriginalScreenPosition = Main.screenPosition;
		p.Velocity = new Vector2(0, Main.rand.NextFloat(.75f, 2));
		p.Rotation = Main.rand.NextFloat(MathHelper.PiOver4);
		p.Scale = Main.rand.NextFloat(0.25f, 0.35f);
		p.ParallaxStrength = (float)Math.Pow(p.Scale, 3);
		p.MaxTime = 360;

		ParticleHandler.SpawnParticle(p);
	}

	public override bool ShouldUpdatePosition() => false;
	public override bool PreDraw(ref Color lightColor) => false;
}