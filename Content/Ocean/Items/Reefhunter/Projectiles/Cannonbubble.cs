﻿using SpiritReforged.Common.Misc;
using SpiritReforged.Common.PrimitiveRendering;
using SpiritReforged.Common.PrimitiveRendering.PrimitiveShape;
using SpiritReforged.Common.ProjectileCommon;
using SpiritReforged.Content.Ocean.Items.Reefhunter.CascadeArmor;
using Terraria.Audio;

namespace SpiritReforged.Content.Ocean.Items.Reefhunter.Projectiles;

public class Cannonbubble : ModProjectile
{
	public const float MAX_SPEED = 15f; //Initial projectile velocity, used in item shootspeed

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
		ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
	}

	public override void SetDefaults()
	{
		Projectile.width = 24;
		Projectile.height = 24;
		Projectile.DamageType = DamageClass.Ranged;
		Projectile.friendly = true;
		Projectile.penetrate = 5;
		Projectile.timeLeft = 360;
		Projectile.aiStyle = 0;
		Projectile.scale = Main.rand.NextFloat(0.9f, 1.1f);
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 30;
	}

	private ref float JiggleStrength => ref Projectile.ai[0];
	private ref float JiggleTime => ref Projectile.ai[1];

	public override void AI()
	{
		Projectile.velocity *= 0.975f;
		Projectile.rotation = Projectile.velocity.ToRotation();

		const float JiggleDecay = 0.99f; //Exponentially slows down in speed
		JiggleStrength *= JiggleDecay;
		JiggleTime += JiggleStrength;

		const float heightDeviation = 0.25f;
		const float sinePeriod = 100; //1.66 seconds
		Projectile.position.Y += (float)Math.Sin(MathHelper.TwoPi * Projectile.timeLeft / sinePeriod) * heightDeviation;

		if (Projectile.wet)
			Projectile.velocity.Y -= 0.08f;

		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile p = Main.projectile[i];
			if (p.type == Projectile.type && p.active && p != null && p.whoAmI != Projectile.whoAmI && p.Hitbox.Intersects(Projectile.Hitbox))
				BubbleCollision(p);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		var outline = TextureAssets.Projectile[Projectile.type].Value;
		Vector2 squishScale = BubbleSquishScale(0.35f, 0.1f);
		List<SquarePrimitive> bubbleTrail = [];
		var primDimensions = new Vector2(Projectile.width * squishScale.X, -Projectile.height * squishScale.Y);

		for (int i = ProjectileID.Sets.TrailCacheLength[Projectile.type] - 1; i > 0; i--)
		{
			float progress = 1 - (i / (float)ProjectileID.Sets.TrailCacheLength[Projectile.type]);

			var square = new SquarePrimitive()
			{
				Color = lightColor * progress * 0.2f,
				Height = primDimensions.X * progress,
				Length = primDimensions.Y * progress,
				Position = Projectile.oldPos[i] + Projectile.Size/2 - Main.screenPosition,
				Rotation = MathHelper.TwoPi - MathHelper.PiOver2 + Projectile.oldRot[i]
			};
			bubbleTrail.Add(square);
		}

		bubbleTrail.Add(new SquarePrimitive()
		{
			Color = lightColor,
			Height = primDimensions.X,
			Length = primDimensions.Y,
			Position = Projectile.Center - Main.screenPosition,
			Rotation = MathHelper.TwoPi - MathHelper.PiOver2 + Projectile.rotation
		});

		Effect bubbleEffect = ModContent.Request<Effect>("SpiritReforged/Assets/Shaders/TextureMap", AssetRequestMode.ImmediateLoad).Value;
		bubbleEffect.Parameters["uTexture"].SetValue(outline);
		bubbleEffect.Parameters["rotation"].SetValue(MathHelper.TwoPi + Projectile.rotation);
		PrimitiveRenderer.DrawPrimitiveShapeBatched(bubbleTrail.ToArray(), bubbleEffect);

		return false;
	}

	//Find the scale vector by which to draw the bubble's outline and interior with
	private Vector2 BubbleSquishScale(float velDelta, float jiggleDelta)
	{
		float squishAmount = GetSpeedRatio() * velDelta; //velocity based
		const float sineSpeed = 5 * MathHelper.Pi;
		squishAmount += (float)Math.Sin(sineSpeed * JiggleTime / 60) * jiggleDelta * JiggleStrength; //jiggling based
		return new Vector2(1 + squishAmount, 1 - squishAmount) * Projectile.scale;
	}

	private float GetSpeedRatio(float exponent = 1) => (float)Math.Pow(MathHelper.Min(Projectile.velocity.Length() / MAX_SPEED, 1), exponent);

	public override void OnKill(int timeLeft)
	{
		int dustCount = Main.rand.Next(7, 12);
		SoundEngine.PlaySound(SoundID.Item54, Projectile.Center);

		for (int i = 0; i < dustCount; ++i)
		{
			Vector2 speed = Main.rand.NextVec2CircularEven(0.5f, 0.5f);
			Dust.NewDust(Projectile.Center, 0, 0, DustID.BubbleBurst_Blue, speed.X * .5f, speed.Y * .5f, 0, default, Main.rand.NextFloat(0.5f, 1f));

			if (Main.rand.NextBool(3))
			{
				int d = Dust.NewDust(Projectile.Center + new Vector2(Main.rand.Next(-20, 20), 0), 0, 0, ModContent.DustType<Dusts.BubbleDust>(), speed.X * .35f, Main.rand.NextFloat(-3f, -.5f), 0, default, Main.rand.NextFloat(0.75f, 1.5f));
				Main.dust[d].velocity = Main.rand.NextVec2CircularEven(2.5f, 2.5f);
			}
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		NPCCollision(target, 3, 0.5f, 2f);
		OnCollideExtra();
	}

	private void NPCCollision(Entity target, float stoppedSpeed, float lowAddedVelMod, float highAddedVelMod)
	{
		var lastTargetHitboxX = new Rectangle((int)(target.position.X - target.velocity.X), (int)target.position.Y, target.width, target.height);
		var lastTargetHitboxY = new Rectangle((int)target.position.X, (int)(target.position.Y - target.velocity.Y), target.width, target.height);
		var lastProjHitboxX = new Rectangle((int)(Projectile.position.X - Projectile.velocity.X), (int)Projectile.position.Y, Projectile.width, Projectile.height);
		var lastProjHitboxY = new Rectangle((int)Projectile.position.X, (int)(Projectile.position.Y - Projectile.velocity.Y), Projectile.width, Projectile.height);

		//Check for x collision by checking if the hitboxes intersect using the last tick's x positions, repeat for y collision using last tick's y positions
		//There's likely a more accurate method without detouring projectile npc collision? Not sure at the moment, working backwards from tile collision logic
		bool collideX = !lastTargetHitboxX.Intersects(lastProjHitboxX);
		bool collideY = !lastTargetHitboxY.Intersects(lastProjHitboxY);

		//Reverse velocity based on collision direction, 
		//add target velocity if it was in the opposite direction of projectile movement (ricocheting off of target), 
		//or if projectile velocity is slow enough (pushed by target)
		bool projStopped = Projectile.velocity.Length() < stoppedSpeed;
		if (collideX)
		{
			Projectile.velocity.X *= -1;

			if (projStopped)
				Projectile.velocity.X += target.velocity.X * highAddedVelMod;

			if (Math.Sign(target.velocity.X) == Math.Abs(Projectile.velocity.X))
				Projectile.velocity.X += target.velocity.X * lowAddedVelMod;
		}

		if (collideY)
		{
			Projectile.velocity.Y *= -1;

			if (projStopped)
				Projectile.velocity.Y += target.velocity.Y * highAddedVelMod;

			else if (Math.Sign(target.velocity.Y) == Math.Sign(Projectile.velocity.Y))
				Projectile.velocity.Y += target.velocity.Y * lowAddedVelMod;
		}

		Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(MathHelper.Pi / 8, MathHelper.Pi / 4) * (Main.rand.NextBool() ? -1 : 1));

		Projectile.netUpdate = true;
	}

	//Causes bubbles to ricochet off each other, and pushes them outside of each other if they overlap
	private void BubbleCollision(Projectile otherBubble)
	{
		var otherBubbleModProj = otherBubble.ModProjectile as Cannonbubble;
		while (otherBubble.Hitbox.Intersects(Projectile.Hitbox))
			Projectile.Center += Projectile.DirectionFrom(otherBubble.Center); //Push out if stuck inside

		//2 dimensional moving circle collision formula- via https://www.vobarian.com/collisions/
		//Simplified here: assuming all masses are equal, therefore normal scalar velocity post-collision is simply equal to the normal scalar velocity of the other vector pre-collision
		Vector2 normal = Projectile.DirectionFrom(otherBubble.Center);
		Vector2 tangent = normal.RotatedBy(MathHelper.PiOver2);
		float scalarTangentThis = Projectile.velocity.X * tangent.X + Projectile.velocity.Y * tangent.Y;
		float scalarTangentOther = otherBubble.velocity.X * tangent.X + otherBubble.velocity.Y * tangent.Y;
		float scalarNormalThis = Projectile.velocity.X * normal.X + Projectile.velocity.Y * normal.Y;
		float scalarNormalOther = otherBubble.velocity.X * normal.X + otherBubble.velocity.Y * normal.Y;

		Projectile.velocity = scalarTangentThis * tangent + scalarNormalOther * normal;
		otherBubble.velocity = scalarTangentOther * tangent + scalarNormalThis * normal;
		float speedMultiplier = Math.Max(GetSpeedRatio(2), otherBubbleModProj.GetSpeedRatio(2)); //Less dust at low collision speed
		if (speedMultiplier <= 0.1f)
			speedMultiplier = 0f;

		OnCollideExtra(0.5f * speedMultiplier, speedMultiplier);
		(otherBubble.ModProjectile as Cannonbubble).JiggleStrength = 1;
	}

	private void OnCollideExtra(float dustMult = 1f, float dustScaleMult = 1f)
	{
		JiggleStrength = 1;

		int dustCount = (int)(Main.rand.Next(6, 9) * dustMult);
		for (int i = 0; i < dustCount; i++)
		{
			int direction = Main.rand.NextBool() ? -1 : 1;
			Vector2 speed = Projectile.velocity.RotatedBy(MathHelper.PiOver2 * direction).RotatedByRandom(MathHelper.Pi / 15);
			int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.BubbleBurst_Blue, speed.X * .25f, speed.Y * .25f, 0, default, Main.rand.NextFloat(1f, 1.25f) * dustScaleMult);
			Main.dust[d].noGravity = true;
		}

		if (!Main.dedServ)
			SoundEngine.PlaySound(SoundID.Item54 with { PitchVariance = 0.3f, Volume = 0.5f * dustMult }, Projectile.Center);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Projectile.Bounce(oldVelocity);
		Projectile.penetrate--;
		OnCollideExtra();
		return false;
	}
}
