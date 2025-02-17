using SpiritReforged.Common.BuffCommon;
using SpiritReforged.Common.MathHelpers;
using SpiritReforged.Common.Misc;
using SpiritReforged.Common.ProjectileCommon;
using Terraria.Audio;

namespace SpiritReforged.Content.Jungle.Toucane;

[AutoloadMinionBuff]
public class ToucanMinion : BaseMinion
{
	private const int TargetMemoryMax = 20;

	private ref float AiState => ref Projectile.ai[0];
	private ref float AiTimer => ref Projectile.ai[1];

	private const float STATE_HOVERTORESTSPOT = 0;
	private const float STATE_RESTING = 1;
	private const float STATE_HOVERTOTARGET = 2;
	private const float STATE_FEATHERSHOOT = 3;
	private const float STATE_GLIDING = 4;

	private int _featherShotFrameTime;
	/// <summary> For how long this minion can select a target without considering collision. </summary>
	private int _targetMemory;
	/// <summary> The whoAmI of the last NPC target. Expires with <see cref="_targetMemory"/>. </summary>
	private int _lastTargetInMemory = -1;

	public ToucanMinion() : base(700, 1200, new Vector2(40, 40)) { }

	public override void AbstractSetStaticDefaults()
	{
		Main.projFrames[Type] = 8;
		ProjectileID.Sets.TrailCacheLength[Type] = 8;
		ProjectileID.Sets.TrailingMode[Type] = 2;
	}

	public override void AbstractSetDefaults() => Projectile.localNPCHitCooldown = 20;

	public override bool DoAutoFrameUpdate(ref int framespersecond, ref int startframe, ref int endframe)
	{
		if (AiState == STATE_GLIDING)
		{
			Projectile.frame = 4;
			return false;
		}

		if (AiState == STATE_RESTING)
		{
			Projectile.frame = 7;
			return false;
		}

		if (_featherShotFrameTime > 0)
		{
			Projectile.frame = 6;
			return false;
		}

		endframe = 4; //only animate through first 3 frames
		framespersecond = (int)MathHelper.Lerp(6, 14, Math.Min(Projectile.velocity.Length() / 6, 1));
		return true;
	}

	public override bool MinionContactDamage() => AiState == STATE_GLIDING;

	public override bool PreAI()
	{
		Projectile.rotation = Projectile.velocity.X * 0.05f;
		return true;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (AiState == STATE_GLIDING)
		{
			Projectile.Bounce(oldVelocity, .75f);

			//Only do collision effects if the change in velocity is significant enough
			float strength = oldVelocity.Length();
			if (strength > 2f && oldVelocity.Distance(Projectile.velocity) > strength / 4f)
			{
				SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
				Collision.HitTiles(Projectile.Center, Projectile.velocity, Projectile.width, Projectile.height);
			}
		}

		return false;
	}

	public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
	{
		fallThrough = AiState != STATE_RESTING;
		return true;
	}

	public override void IdleMovement(Player player)
	{
		AiTimer++;
		_featherShotFrameTime = 0;
		_targetMemory = 0;
		_lastTargetInMemory = -1;

		if (AiState is not STATE_HOVERTORESTSPOT and not STATE_RESTING)
		{
			AiTimer = 0;
			AiState = STATE_HOVERTORESTSPOT;
		}

		Projectile.direction = Projectile.spriteDirection = Projectile.Center.X < player.MountedCenter.X ? -1 : 1;
		Vector2 targetCenter = player.MountedCenter - new Vector2(50 * (IndexOfType + 1) * player.direction, 0);
		//take the base desired homing position, and try to find the lowest solid tile for the minion to rest at from it
		Point tilepos = targetCenter.ToTileCoordinates();
		int tilesfrombase = 0;
		int maxtilesfrombase = 15;
		bool canstartrest = true; //dont start resting if rest position is too far from the base desired position

		int startX = tilepos.X + (Projectile.direction > 0 ? -1 : 0);
		while (CollisionCheckHelper.CheckSolidTilesAndPlatforms(new Rectangle(startX, tilepos.Y, 1, 1))) //move up until not inside a tile
		{
			tilepos.Y--;
			if (++tilesfrombase >= maxtilesfrombase)
			{
				canstartrest = false;
				break;
			}
		}

		while (!CollisionCheckHelper.CheckSolidTilesAndPlatforms(new Rectangle(startX, tilepos.Y + 1, 1, 1))) //move down until just above a tile
		{
			tilepos.Y++;
			if (++tilesfrombase >= maxtilesfrombase)
			{
				canstartrest = false;
				break;
			}
		}

		if (AiState == STATE_RESTING || AiState == STATE_HOVERTORESTSPOT && canstartrest) //if not too far, or too far but already resting, set the desired position to the lowest non solid tile
			targetCenter = tilepos.ToWorldCoordinates();

		switch (AiState)
		{
			case STATE_HOVERTORESTSPOT:
				Projectile.tileCollide = false;
				Projectile.AccelFlyingMovement(targetCenter, 0.15f, 0.1f, 15);
				//check if close enough to and above the rest spot, and not inside a tile, if so, start resting
				if (Projectile.Distance(targetCenter) < 10 && Projectile.Center.Y < targetCenter.Y && canstartrest && !CollisionCheckHelper.CheckSolidTilesAndPlatforms(new Rectangle(startX, tilepos.Y, 1, 1)) && AiTimer > 30)
				{
					AiTimer = 0;
					AiState = STATE_RESTING;
				}

				break;
			case STATE_RESTING:
				Projectile.tileCollide = true;
				Projectile.velocity.X *= 0.7f;
				Projectile.velocity.Y += 0.4f;
				//if too far from the new resting position, start flying to it
				if (Projectile.Distance(targetCenter) > 150)
				{
					AiState = STATE_HOVERTORESTSPOT;
					AiTimer = 0;
					Projectile.velocity.Y = -5;
				}

				break;
		}

		if (Projectile.Distance(targetCenter) > 1800)
		{
			Projectile.Center = targetCenter;
			Projectile.netUpdate = true;
		}
	}

	public override bool CanSelectTarget(NPC target)
	{
		var projRect = Projectile.getRect();
		var npcRect = target.getRect();
		bool inCollisionRange = Collision.CanHitLine(projRect.Top(), 0, 0, npcRect.TopLeft(), npcRect.Width, npcRect.Height);

		if (target.whoAmI == _lastTargetInMemory || inCollisionRange)
		{
			if (inCollisionRange)
			{
				_targetMemory = TargetMemoryMax;
				_lastTargetInMemory = target.whoAmI;
			}

			return true;
		}

		return false;
	}

	private bool CanLandProjectile(NPC target) => Collision.CanHitLine(Projectile.Center, 0, 0, target.Center, 0, 0);

	public override void TargettingBehavior(Player player, NPC target)
	{
		const int FeatherMinRange = 200;
		const int FeatherMaxRange = 600;
		const int FeatherShootTime = 30;
		const int FeatherShots = 3;
		const float GlideStartVelocity = 8;
		const float GlideMaxVelocity = 12;
		const int GlideTime = 45;

		Projectile.tileCollide = AiState is STATE_GLIDING or STATE_FEATHERSHOOT;

		switch (AiState)
		{
			case STATE_HOVERTORESTSPOT: //start flying to the target if in an idle ai state
			case STATE_RESTING:
				AiState = STATE_HOVERTOTARGET;
				AiTimer = 0;
				break;

			case STATE_HOVERTOTARGET:
				Projectile.direction = Projectile.spriteDirection = Projectile.Center.X < target.Center.X ? -1 : 1;
				if (Projectile.Distance(target.Center) <= FeatherMinRange && CanLandProjectile(target)) //switch to shooting feathers if close enough to a target
				{
					AiState = STATE_FEATHERSHOOT;
					AiTimer = 0;
					Projectile.netUpdate = true;
				}

				Projectile.AccelFlyingMovement(target.Center, 0.25f, 0.1f, 12);
				break;

			case STATE_FEATHERSHOOT:
				Projectile.direction = Projectile.spriteDirection = Projectile.Center.X < target.Center.X ? -1 : 1;
				if ((Projectile.Distance(target.Center) >= FeatherMaxRange || !CanLandProjectile(target)) && AiTimer <= FeatherShootTime * (FeatherShots + 0.5f)) //fly back to target if too far, and if before the anticipation before the glide
				{
					AiState = STATE_HOVERTOTARGET;
					AiTimer = 0;
					Projectile.netUpdate = true;
					break;
				}

				if (AiTimer >= FeatherShootTime * (FeatherShots + 1)) //start gliding after enough shots
				{
					AiState = STATE_GLIDING;
					if (Main.netMode != NetmodeID.Server)
					{
						SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown with { PitchVariance = 0.3f, Volume = 0.5f }, Projectile.Center);
						SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/BirdCry_1") with { PitchVariance = 0.3f, Volume = 0.5f }, Projectile.Center);
					}

					Projectile.velocity = Projectile.DirectionTo(target.Center).RotatedByRandom(MathHelper.PiOver4) * GlideStartVelocity;
					AiTimer = 0;
					Projectile.netUpdate = true;
					break;
				}

				if (AiTimer % FeatherShootTime == 0) //shoot feather after given amount of time, with some recoil on the minion
				{
					if (Main.netMode != NetmodeID.Server)
						SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/SmallProjectileWoosh_1") with { PitchVariance = 0.3f, Volume = 1.25f }, Projectile.Center);

					Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.DirectionTo(target.Center) * 8, ModContent.ProjectileType<ToucanFeather>(), (int)(Projectile.damage * 0.8), Projectile.knockBack, Projectile.owner);
					for (int j = 0; j < 6; j++)
					{
						var dust = Dust.NewDustPerfect(Projectile.Center, 90, Projectile.DirectionTo(target.Center).RotatedByRandom(MathHelper.Pi / 3) * Main.rand.NextFloat(1f, 2f), 100, default, Main.rand.NextFloat(0.15f, 0.3f));
						dust.fadeIn = 0.75f;
						dust.noGravity = true;
					}

					Projectile.velocity = -Projectile.DirectionTo(target.Center).RotatedByRandom(MathHelper.PiOver4) * 6;
					_featherShotFrameTime = FeatherShootTime / 2;
					Projectile.netUpdate = true;
				}

				if (AiTimer > FeatherShootTime * (FeatherShots + 0.5f)) //move backwards before the glide in anticipation
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionFrom(target.Center) * 6, 0.2f);
				else //otherwise slowly move towards the target to make up for the recoil
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center), 0.1f);

				break;

			case STATE_GLIDING:
				if (AiTimer >= GlideTime) //switch state after enough time has passed
				{
					AiState = Projectile.Distance(target.Center) < FeatherMinRange ? STATE_FEATHERSHOOT : STATE_HOVERTOTARGET; //fly to target if too far, otherwise continue firing feathers
					AiTimer = 0;
					Projectile.netUpdate = true;
					break;
				}

				if (Projectile.velocity.Length() < GlideMaxVelocity) //accelerate until reaching capped velocity
					Projectile.velocity *= 1.033f;

				//loop back around after passing the target
				Projectile.velocity = Projectile.velocity.Length() * Vector2.Normalize(Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * Projectile.velocity.Length(), 0.08f));

				//adjust direction and rotation to look like its flying in the direction it's moving in
				Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
				Projectile.rotation = Projectile.velocity.ToRotation() - (Projectile.direction > 0 ? MathHelper.Pi : 0);
				CanRetarget = false; //retargetting to closest npc not wanted here

				break;
		}

		_featherShotFrameTime = Math.Max(_featherShotFrameTime - 1, 0);
		_targetMemory = Math.Max(_targetMemory - 1, 0);

		if (_targetMemory == 0)
			_lastTargetInMemory = -1;

		AiTimer++;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (AiState == STATE_GLIDING)
			Projectile.QuickDrawTrail();

		Projectile.QuickDraw();
		return false;
	}
}