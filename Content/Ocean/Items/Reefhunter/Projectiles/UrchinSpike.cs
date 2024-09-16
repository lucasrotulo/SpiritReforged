﻿using SpiritReforged.Common.Easing;
using SpiritReforged.Common.PrimitiveRendering;
using SpiritReforged.Common.PrimitiveRendering.Trail_Components;
using SpiritReforged.Common.ProjectileCommon;

namespace SpiritReforged.Content.Ocean.Items.Reefhunter.Projectiles;

public class UrchinSpike : ModProjectile, ITrailProjectile
{
	private static Asset<Texture2D> GlowmaskTexture;
	public override void SetStaticDefaults()
	{
		// DisplayName.SetDefault("Urchin");
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		if (!Main.dedServ)
			GlowmaskTexture = ModContent.Request<Texture2D>(Texture + "Glow");
	}

	private bool hasTarget = false;
	private Vector2 relativePoint = Vector2.Zero;

	public override void SetDefaults()
	{
		Projectile.width = 6;
		Projectile.height = 6;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.friendly = true;
		Projectile.penetrate = 1;
		Projectile.aiStyle = 0;
		Projectile.extraUpdates = 3;
		Projectile.timeLeft = 60;
		Projectile.scale = Main.rand.NextFloat(0.7f, 1.1f);
	}

	public void DoTrailCreation(TrailManager tm) => tm.CreateTrail(Projectile, new LightColorTrail(new Color(87, 35, 88) * 0.2f, Color.Transparent), new RoundCap(), new DefaultTrailPosition(), 8 * Projectile.scale, 75);

	public override bool? CanDamage() => !hasTarget;
	public override bool? CanCutTiles() => !hasTarget;

	public override void AI()
	{
		Projectile.alpha = (255 - (int)(Projectile.timeLeft / 60f * 255));
		Projectile.scale = EaseFunction.EaseCircularOut.Ease(Projectile.Opacity);
		Projectile.velocity *= 0.96f;
		if (!hasTarget)
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

		else
		{
			NPC npc = Main.npc[(int)Projectile.ai[1]];
			Projectile.velocity *= 0.92f;
			relativePoint += Projectile.velocity;

			if (!npc.active)
				Projectile.Kill();
			else
				Projectile.Center = npc.Center + relativePoint;
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Projectile.ai[1] = target.whoAmI;
		Projectile.tileCollide = false;
		Projectile.netUpdate = true;
		Projectile.alpha = 0;
		Projectile.penetrate++;
		Projectile.velocity *= 0.8f;

		hasTarget = true;
		relativePoint = Projectile.Center - target.Center;

		if (!Main.dedServ)
			AssetLoader.VertexTrailManager.TryEndTrail(Projectile, 12);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Vector2 texSize = TextureAssets.Projectile[Projectile.type].Value.Size();
		Projectile.QuickDraw(origin: new Vector2(texSize.X / 2, 0));
		Projectile.QuickDrawTrail(baseOpacity: 0.25f, drawOrigin: new Vector2(texSize.X / 2, 0));

		return false;
	}
	public override void PostDraw(Color lightColor)
	{
		Texture2D tex = GlowmaskTexture.Value;

		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, UrchinBall.GlowColor(50) * Projectile.Opacity * Projectile.Opacity, Projectile.rotation, new Vector2(tex.Size().X / 2f, 0), Projectile.scale, SpriteEffects.None, 0f);
	}
}