using Microsoft.Xna.Framework.Graphics;
using SpiritReforged.Common.Easing;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.Particle;
using SpiritReforged.Common.PrimitiveRendering;
using SpiritReforged.Common.PrimitiveRendering.PrimitiveShape;
using SpiritReforged.Common.PrimitiveRendering.Trail_Components;
using SpiritReforged.Common.ProjectileCommon;
using SpiritReforged.Common.Visuals.Glowmasks;
using SpiritReforged.Content.Particles;

namespace SpiritReforged.Content.Desert.Scarabeus.Items.Projectiles;

[AutoloadGlowmask("255,255,255", false)]
public class AdornedArrow : ModProjectile
{
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
		Projectile.aiStyle = -1;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 120;
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi / 2;
		Lighting.AddLight(Projectile.position, Color.LightGoldenrodYellow.ToVector3() / 2);
		Projectile.velocity *= 0.97f;
		Projectile.alpha += 255 / Projectile.timeLeft;

		if (Main.rand.NextBool() && Main.rand.NextFloat() < Projectile.Opacity)
		{
			Vector2 particleCenter = Projectile.Center;
			Vector2 particleVel = Vector2.Normalize(Projectile.velocity) * Main.rand.NextFloat(2);
			Color lightColor = Color.LightGoldenrodYellow.Additive();
			Color darkColor = Color.Lerp(Color.LightGoldenrodYellow, Color.OrangeRed, 0.5f).Additive() * 0.5f;
			float scale = Main.rand.NextFloat(0.3f, 0.4f);
			int lifeTime = Main.rand.Next(20, 40);
			static void delegateAction(Particle p)
			{
				p.Velocity = p.Velocity.RotatedByRandom(0.1f);
				p.Velocity *= 0.97f;
			}

			ParticleHandler.SpawnParticle(new GlowParticle(particleCenter, particleVel, lightColor, darkColor, scale, lifeTime, 2, delegateAction));
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Projectile.damage = (int)(Projectile.damage * 0.9f); 
		ParticleHandler.SpawnParticle(new LightBurst(target.Center, Main.rand.NextFloatDirection(), Color.LightGoldenrodYellow.Additive(), 0.6f, 30));
	}

	public override void OnKill(int timeLeft)
	{
		//for (int i = 0; i < 10; i++)
			//Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldCoin);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Color color = Color.LightGoldenrodYellow.Additive();

		ConeNoise(-3, 1.5f, 2);

		Projectile.QuickDrawTrail(baseOpacity: 1, drawColor: Color.LightGoldenrodYellow.Additive());
		Projectile.QuickDraw();

		GlowmaskProjectile.ProjIdToGlowmask.TryGetValue(Type, out GlowmaskInfo glowmaskInfo);
		Texture2D glowmaskTex = glowmaskInfo.Glowmask.Value;

		for (int i = 0; i < 6; i++)
		{
			Vector2 offset = Vector2.UnitX.RotatedBy((MathHelper.TwoPi * i / 6) + Projectile.rotation);

			Main.spriteBatch.Draw(glowmaskTex, Projectile.Center + offset - Main.screenPosition, null, color * Projectile.Opacity * 0.33f * EaseFunction.EaseCircularOut.Ease(1 - Projectile.Opacity), Projectile.rotation, glowmaskTex.Size()/2, Projectile.scale, SpriteEffects.None, 0);
		}

		Main.spriteBatch.RestartToDefault();

		ConeNoise(3, 1.5f, 0);

		return false;
	}

	private void ConeNoise(float spiral, float opacity, int timeOffset)
	{
		//ugghhhhhhhh this doesnt look good kms
		Effect effect = AssetLoader.LoadedShaders["SpiralNoiseCone"];
		Texture2D texture = AssetLoader.LoadedTextures["noise"];
		effect.Parameters["uTexture"].SetValue(texture);
		float easedProgress = EaseFunction.EaseCircularIn.Ease(Projectile.timeLeft / 120f);
		effect.Parameters["scroll"].SetValue(new Vector2(spiral, -easedProgress - timeOffset + Main.GlobalTimeWrappedHourly/5));

		effect.Parameters["uColor"].SetValue(Color.LightGoldenrodYellow.Additive().ToVector4());
		effect.Parameters["uColor2"].SetValue(Color.LightGoldenrodYellow.Additive().ToVector4());

		effect.Parameters["textureStretch"].SetValue(new Vector2(1, 1) * 0.4f);
		effect.Parameters["texExponentRange"].SetValue(new Vector2(5, 0.1f));

		effect.Parameters["finalIntensityMod"].SetValue(opacity);
		effect.Parameters["textureStrength"].SetValue(3);
		effect.Parameters["finalExponent"].SetValue(2f);
		effect.Parameters["flipCoords"].SetValue(true);

		var square = new SquarePrimitive
		{
			Color = Color.LightGoldenrodYellow.Additive() * EaseFunction.EaseQuadOut.Ease(Projectile.Opacity) * EaseFunction.EaseCircularOut.Ease(1 - Projectile.Opacity),
			Height = 70 * EaseFunction.EaseCircularOut.Ease(Projectile.Opacity),
			Length = 120,
			Position = Projectile.Center - Main.screenPosition - Vector2.Normalize(Projectile.velocity) * 10 * EaseFunction.EaseCircularOut.Ease(Projectile.Opacity),
			Rotation = Projectile.rotation - MathHelper.Pi,
		};

		PrimitiveRenderer.DrawPrimitiveShape(square, effect);
	}
}
