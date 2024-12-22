namespace SpiritReforged.Content.Ocean.Items.MessageBottle;

public class MessageBottleMount : ModMount
{
	int wetCounter;
	public override void SetStaticDefaults()
	{
		MountData.buff = ModContent.BuffType<BottleMountBuff>();
		MountData.spawnDust = 7;
		MountData.spawnDustNoGravity = true;
		MountData.heightBoost = 16;
		MountData.fallDamage = 0f;
		MountData.runSpeed = 0;
		MountData.flightTimeMax = 0;
		MountData.fatigueMax = 0;
		MountData.jumpHeight = 0;
		MountData.acceleration = 0f;
		MountData.swimSpeed = 3;
		MountData.jumpSpeed = 0;
		MountData.blockExtraJumps = true;
		MountData.totalFrames = 4;
		MountData.constantJump = true;
		MountData.playerYOffsets = [12, 12, 12, 12];
		MountData.yOffset = 5;
		MountData.xOffset = -15;
		MountData.playerHeadOffset = 22;
		MountData.standingFrameCount = 1;
		MountData.standingFrameDelay = 12;
		MountData.standingFrameStart = 0;
		MountData.inAirFrameCount = 1;
		MountData.inAirFrameDelay = 12;
		MountData.inAirFrameStart = 0;
		MountData.idleFrameCount = 1;
		MountData.idleFrameDelay = 12;
		MountData.idleFrameStart = 0;
		MountData.idleFrameLoop = true;

		if (Main.netMode != NetmodeID.Server)
		{
			MountData.textureWidth = MountData.frontTexture.Width();
			MountData.textureHeight = MountData.frontTexture.Height();
		}
	}
	public override void UpdateEffects(Player player)
	{
		const float MaxSpeed = 12f;

		if (Collision.WetCollision(player.position, player.width, player.height + 16) && !Collision.LavaCollision(player.position, player.width, player.height + 16))
		{
			MountData.runSpeed = 7;
			MountData.acceleration = 0.075f;

			if (Collision.WetCollision(player.position, player.width, player.height + 6))
			{
				player.velocity.Y -= player.gravity * 2.5f;
				if (player.velocity.Y <= -MaxSpeed)
					player.velocity.Y = -MaxSpeed;

				if (!Collision.WetCollision(player.position, player.width, (int)(player.height * 0.6f)))
					player.velocity.Y *= 0.92f;
			}
			else
			{
				if (player.velocity.Y is > (-MaxSpeed * 0.5f) and < 0.5f)
					player.velocity.Y = -player.gravity;
			}

			player.fishingSkill += 10;
			wetCounter = 15;

			#region visuals
			float sin = (float)Math.Sin(Main.timeForVisualEffects / 12f) * MathHelper.Min(Math.Abs(player.velocity.X) / 2, 1);

			if (Main.rand.NextBool(2) && sin > 0)
			{
				var dust = Dust.NewDustDirect(player.Bottom - new Vector2((player.direction == 1) ? 40 : 20, 0), 58, 0, DustID.BreatheBubble, Alpha: 150, Scale: Main.rand.NextFloat(1f, 2f));
				dust.noGravity = true;
			}

			if (Math.Abs(player.velocity.X) > 2)
			{
				var dust = Dust.NewDustDirect(player.Bottom - new Vector2((player.direction == 1) ? 40 : 20, 0), 58, 0, DustID.BubbleBurst_Blue, Scale: Main.rand.NextFloat());
				dust.velocity = new Vector2(-.1f * player.velocity.X, -2).RotatedByRandom(1);
			}
			else if (Main.rand.NextBool(8))
			{
				var dust = Dust.NewDustDirect(player.Bottom - new Vector2((player.direction == 1) ? 40 : 20, 0), 58, 0, DustID.BreatheBubble, Alpha: 150, Scale: Main.rand.NextFloat(.5f, 1.5f));
				dust.noGravity = true;
			}

			player.fullRotation = player.velocity.X * 0.005f * sin;
			#endregion
		}
		else 
		{
			if (Collision.LavaCollision(player.position, player.width, player.height + 16))
				player.QuickMount();

			wetCounter--;
			MountData.acceleration = 0.05f;

			player.gravity *= 1.5f;
			if (wetCounter < 0)
			{
				if (player.velocity.Y == 0)
				{
					player.velocity.X *= 0.92f;
					MountData.runSpeed = 0.05f;
				}
				else
					MountData.runSpeed = 7;
			}
		}
	}

	public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
	{
		float mult = MathHelper.Clamp(Math.Abs(mountedPlayer.velocity.X / 3f), 0, 1);

		if (mult < .1f)
			mountedPlayer.mount._frameCounter = 0;

		mountedPlayer.mount._frameCounter += 0.2f * mult;
		mountedPlayer.mount._frame = (int)(mountedPlayer.mount._frameCounter %= 4);

		return false;
	}
}