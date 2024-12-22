using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Particles;
using Terraria.Audio;
using SpiritReforged.Common.Easing;

namespace SpiritReforged.Content.Snow.Frostbite;

public class FrostbiteProj : ModProjectile
{
	private bool Released 
	{ 
		get => Projectile.ai[0] == 1; 
		set => Projectile.ai[0] = value ? 1 : 0; 
	}

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(120);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.penetrate = -1;
		Projectile.alpha = 255;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 300;

		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 12;
	}

	public override void AI()
	{
		Player owner = Main.player[Projectile.owner];
		int fadeInTime = 60;
		int fadeOutTime = 60;

		if (Projectile.timeLeft > fadeOutTime) //One-time on spawn effects
		{
			Projectile.scale = 0;
			SoundEngine.PlaySound(SoundID.NPCHit5 with { Volume = .5f, Pitch = -.15f }, Projectile.Center);

			for (int i = 0; i < 25; i++)
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Unit() * 25f, DustID.ApprenticeStorm, (Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 3f).RotatedBy(Main.rand.NextFloat(1.0f, 2.0f)), 180, Color.White with { A = 0 }, 2.5f).noGravity = true;
		}

		if (owner.channel && !Released)
		{
			owner.itemTime = owner.itemAnimation = 2;
			owner.reuseDelay = owner.HeldItem.useTime;

			if (owner.whoAmI == Main.myPlayer)
			{
				if (Main.MouseWorld != Projectile.Center)
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Main.MouseWorld) * Math.Min(Projectile.Distance(Main.MouseWorld) / 100, 5), 0.15f);

				Projectile.netUpdate = true;
			}

			Projectile.scale = Math.Min(Projectile.scale + 1f / fadeInTime, 1);
			Projectile.alpha = Math.Max(Projectile.alpha - 255 / fadeInTime, 0);
			Projectile.timeLeft = fadeOutTime;
		}
		else
		{
			Projectile.velocity *= .95f;

			Projectile.alpha = Math.Min(Projectile.alpha + 255 / fadeOutTime, 255);
			Projectile.scale = Math.Max(Projectile.scale - .01f, 0);
		}

		Projectile.rotation += (1.5f - Projectile.scale) * .1f;

		if (!owner.channel || ++Projectile.localAI[0] % 10 == 0 && !owner.CheckMana(owner.HeldItem.mana / 6, true))
			Released = true;

		Vector2 dirUnit = Main.rand.NextVector2Unit() * 100 * Projectile.scale;
		float randomMagnitude = Main.rand.NextFloat() * .3f;
		Vector2 dustVel = (Vector2.Normalize(dirUnit) * randomMagnitude).RotatedBy(Main.rand.NextFloat(1.0f, 2.0f));

		Dust.NewDustPerfect(Projectile.Center + dirUnit, Main.rand.NextBool(2) ? DustID.ApprenticeStorm : DustID.Snow, dustVel, 180, Color.White with { A = 0 }, randomMagnitude * 8.5f).noGravity = true;
		if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
		{
			Vector2 spawnPos = Projectile.Center + dirUnit / 2;
			Color color = (Color.Lerp(Color.White, Color.CornflowerBlue, Main.rand.NextFloat()) with { A = 0 }) * Main.rand.NextFloat(.10f, .28f) * Projectile.Opacity * Lighting.Brightness((int)(spawnPos.X / 16), (int)(spawnPos.Y / 16));

			ParticleHandler.SpawnParticle(new SmokeCloud(spawnPos, dustVel, color, randomMagnitude * .7f, EaseFunction.EaseQuadOut, Main.rand.Next(25, 50)));
		}

		owner.ChangeDir(owner.DirectionTo(Projectile.Center).X < 0 ? -1 : 1);
		owner.itemRotation = MathHelper.WrapAngle(owner.AngleTo(Projectile.Center) - owner.fullRotation - (owner.direction < 0 ? MathHelper.Pi : 0));
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		int damageRange = (int)(Projectile.Size.Length() * .15f);
		float distance = Projectile.Distance(target.Center);

		if (distance <= damageRange)
		{
			float damageMult = 2f - (distance / damageRange) * 1f;
			modifiers.FinalDamage *= damageMult;
		}

		target.AddBuff(ModContent.BuffType<Frozen>(), 300);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		Texture2D cursor = TextureAssets.Projectile[917].Value;

		static float LerpFunc(float frequency) => (float)Math.Sin(Main.timeForVisualEffects / frequency);

		float scale;
		Color color;
		for (int i = 0; i < 2; i++)
		{
			scale = Projectile.scale + Math.Max(1f, i * 1.1f) + LerpFunc(50f) / 5;
			color = (Projectile.GetAlpha(Color.White) with { A = 0 }) * (.08f + LerpFunc(i == 0 ? 25f : 30f) / 30f);
			float rotation = Projectile.rotation * (i == 0 ? 1 : -1);

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, color, rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);

			Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, Projectile.Center - Main.screenPosition, null, color * .3f, rotation, texture.Size() / 2, scale, SpriteEffects.None, 0);
		}

		scale = 1f + Projectile.scale / 3;

		color = Color.White with { A = 0 } * ((float)(.3f - Projectile.Opacity) * (Released ? 0 : 1));
		Main.EntitySpriteDraw(cursor, Projectile.Center + Projectile.velocity - Main.screenPosition, null, color, 0, cursor.Size() / 2, scale, SpriteEffects.None, 0);

		return false;
	}
}