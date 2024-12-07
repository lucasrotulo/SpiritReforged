using System.Linq;
using Terraria.Audio;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.ProjectileCommon;
using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Particles;
using SpiritReforged.Common.Easing;
using SpiritReforged.Common.BuffCommon;

namespace SpiritReforged.Content.Forest.FairyWhistle;

[AutoloadMinionBuff()]
public class FairyMinion : BaseMinion
{
	public FairyMinion() : base(400, 500, new Vector2(20, 20)) { }

	internal static Color PARTICLE_GREEN = new(124, 255, 47);

	public override void AbstractSetStaticDefaults()
	{
		// DisplayName.SetDefault("Fairy");
		Main.projFrames[Projectile.type] = 4;
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
	}

	public override void AbstractSetDefaults() => Projectile.alpha = 255;

	public override bool DoAutoFrameUpdate(ref int framespersecond, ref int startframe, ref int endframe)
	{
		framespersecond = (int)MathHelper.Lerp(10, 20, Math.Min(Projectile.velocity.Length() / 6, 1));
		return true;
	}

	public override bool MinionContactDamage() => false;

	public override bool PreAI()
	{
		Projectile.rotation = Projectile.velocity.X * 0.05f;
		Projectile.alpha = Math.Max(Projectile.alpha - 3, 0);

		foreach (Projectile p in Main.projectile.Where(x => x.active && x != null && x.type == Projectile.type && x.owner == Projectile.owner && x != Projectile))
			if (p.Hitbox.Intersects(Projectile.Hitbox))
				Projectile.velocity += Projectile.DirectionFrom(p.Center) / 10;

		return true;
	}

	private ref float AiTimer => ref Projectile.ai[0];

	public override void IdleMovement(Player player)
	{
		AiTimer = 0;
		if (Math.Abs(Projectile.velocity.X) > 1) //dont flip too fast
			Projectile.direction = Projectile.spriteDirection = Math.Sign(Projectile.velocity.X) > 0 ? -1 : 1;

		Vector2 desiredPosition = player.MountedCenter - new Vector2(0, 60 + (float)Math.Sin(Main.GameUpdateCount / 6f) * 6);
		Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Lerp(Projectile.Center, desiredPosition, 0.15f) - Projectile.Center, 0.1f);
		if (Projectile.Distance(desiredPosition) > 600)
			Projectile.Center = desiredPosition;
	}

	private const int SHOOTTIME = 50;

	public override void TargettingBehavior(Player player, NPC target)
	{
		Projectile.direction = Projectile.spriteDirection = Math.Sign(Projectile.DirectionTo(target.Center).X) > 0 ? -1 : 1;
		Vector2 desiredPosition = player.MountedCenter - new Vector2(0, 60 + (float)Math.Sin(Main.GameUpdateCount / 6f) * 6);
		desiredPosition += Projectile.DirectionTo(target.Center) * 30;
		Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Lerp(Projectile.Center, desiredPosition, 0.15f) - Projectile.Center, 0.1f);
		if (++AiTimer >= SHOOTTIME)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.DirectionTo(target.Center) * 3.5f, ModContent.ProjectileType<FairyProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
			Projectile.velocity -= Projectile.DirectionTo(target.Center) * 4;
			AiTimer = 0 + Main.rand.Next(-5, 6);
			Projectile.netUpdate = true;

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(SoundID.Item9 with { PitchVariance = 0.3f }, Projectile.Center);

				for (int i = 0; i < 10; i++)
					ParticleHandler.SpawnParticle(new GlowParticle(Projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 1f),
						PARTICLE_GREEN * 0.66f, Main.rand.NextFloat(0.35f, 0.5f), 35, 10, p => RandomCurveParticleMovement(p, 0.12f, 0.97f)));
			}
		}
	}

	public static void RandomCurveParticleMovement(Particle p, float maxRadians, float decelerationRate) => p.Velocity = p.Velocity.RotatedByRandom(maxRadians) * decelerationRate;

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D bloom = AssetLoader.LoadedTextures["Bloom"];
		float bloomOpacity = EaseFunction.EaseQuadOut.Ease(AiTimer / SHOOTTIME); //glow brighter when closer to shot time
		bloomOpacity = Math.Max(bloomOpacity, 0.2f);

		Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null, new Color(124, 255, 47, 0) * Projectile.Opacity * bloomOpacity, 0, bloom.Size() / 2, 0.18f, SpriteEffects.None, 0);

		var partialBright = Color.Lerp(Color.White, lightColor, 0.75f);
		Projectile.QuickDrawTrail(null, 0.2f * Projectile.Opacity, drawColor: partialBright);
		Projectile.QuickDraw(null, drawColor: partialBright);

		for (int i = 0; i < 6; i++)
		{
			Vector2 glowmaskRadialOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 6);
			glowmaskRadialOffset *= 2f;
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center + glowmaskRadialOffset - Main.screenPosition, Projectile.DrawFrame(),
				Color.White.Additive() * Projectile.Opacity * bloomOpacity * 0.1f, Projectile.rotation, Projectile.DrawFrame().Size() / 2, Projectile.scale, Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
		}

		return false;
	}
}