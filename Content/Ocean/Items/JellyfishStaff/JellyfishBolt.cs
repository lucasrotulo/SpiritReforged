using SpiritReforged.Common.Easing;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Particles;
using System.IO;
using Terraria.Audio;

namespace SpiritReforged.Content.Ocean.Items.JellyfishStaff;

public class JellyfishBolt : ModProjectile
{
	public bool IsPink
	{
		get => (int)Projectile.ai[0] != 0;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	public bool SetSpawnPos
	{
		get => (int)Projectile.ai[1] != 0;
		set => Projectile.ai[1] = value ? 1 : 0;
	}

	public static int MAX_CHAIN_DISTANCE => (int)(JellyfishMinion.SHOOT_RANGE * 0.66f);
	public static int HITSCAN_STEP { get; set; } = 5;

	public Vector2 BoltStartPos = new(0, 0);

	public override string Texture => "Terraria/Images/Projectile_1"; //Use a basic texture because this projectile is hidden

	public override void SetStaticDefaults() => ProjectileID.Sets.MinionShot[Type] = true;

	public override void SetDefaults()
	{
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = 3; //Number of chains between enemies
		Projectile.timeLeft = JellyfishMinion.SHOOT_RANGE / HITSCAN_STEP;
		Projectile.height = 4;
		Projectile.width = 4;
		Projectile.hide = true;
		Projectile.ignoreWater = true;
		Projectile.extraUpdates = JellyfishMinion.SHOOT_RANGE / HITSCAN_STEP;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = Projectile.timeLeft * Projectile.penetrate;
		Projectile.ignoreWater = true;
	}

	public override void AI()
	{
		if (!SetSpawnPos)
		{
			BoltStartPos = Projectile.Center;
			SetSpawnPos = true;
			Projectile.netUpdate = true;
		}
	}

	public override void OnKill(int timeLeft)
	{
		//If the projectile times out and doesn't hit something
		if (timeLeft == 0 && Projectile.penetrate > 0 && !Main.dedServ)
			ParticleHandler.SpawnParticle(new LightningParticle(BoltStartPos, Projectile.Center, ParticleColor, 30, 30f));
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!Main.dedServ)
		{
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/ElectricSting") with { PitchVariance = 0.5f, Pitch = .65f, Volume = 0.8f, MaxInstances = 3 }, Projectile.Center);
			ParticleHandler.SpawnParticle(new LightningParticle(BoltStartPos, target.Center, ParticleColor, 30, 30f));

			HitEffects(target.Center);
		}

		if (Projectile.penetrate > 0)
		{
			NPC newTarget = Projectile.FindTargetWithinRange(MAX_CHAIN_DISTANCE, true);
			if (newTarget != null)
			{
				Projectile.timeLeft = MAX_CHAIN_DISTANCE;
				Projectile.velocity = Projectile.DirectionTo(newTarget.Center) * HITSCAN_STEP;
				BoltStartPos = target.Center;
				Projectile.netUpdate = true;
				Projectile.damage = (int)(Projectile.damage * 0.8f);
			}
			else
				Projectile.penetrate = 0;
		}
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (!Main.dedServ)
		{
			ParticleHandler.SpawnParticle(new LightningParticle(BoltStartPos, Projectile.Center, ParticleColor, 30, 30f));
			HitEffects(Projectile.Center);
		}

		return base.OnTileCollide(oldVelocity);
	}

	private void HitEffects(Vector2 center)
	{
		for (int i = 0; i < 8; i++)
			ParticleHandler.SpawnParticle(new GlowParticle(center, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1f, 3f), ParticleColor, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(30, 50), 5, delegate(Particle p) { p.Velocity *= 0.92f; }));

		for (int i = 0; i < 2; i++)
			ParticleHandler.SpawnParticle(new TexturedPulseCircle(
				center,
				Color.White.Additive(),
				ParticleColor.Additive() * 0.75f,
				0.7f,
				80 + Main.rand.NextFloat(20),
				25 + Main.rand.Next(10),
				"EnergyTrail",
				new Vector2(1f, 0.14f),
				EaseFunction.EaseCircularOut,
				false,
				0.5f).WithSkew(Main.rand.NextFloat(0.2f, 0.7f), Main.rand.NextFloat(MathHelper.PiOver2) + i * MathHelper.PiOver2));

		ParticleHandler.SpawnParticle(new DissipatingImage(center, Color.White.Additive(), Main.rand.NextFloat(MathHelper.TwoPi), 0.075f, Main.rand.NextFloat(-0.5f, 0.5f), "ElectricScorch", new(0.4f, 0.4f), new(3, 1.5f), 25));
	}

	private Color ParticleColor => IsPink ? new Color(255, 161, 225) : new Color(156, 255, 245);

	public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(BoltStartPos);

	public override void ReceiveExtraAI(BinaryReader reader) => BoltStartPos = reader.ReadVector2();
}
