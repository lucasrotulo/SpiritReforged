using Terraria.Audio;

namespace SpiritReforged.Content.Forest.ButterflyStaff;

public class ButterflyCritter : ModNPC
{
	public ref float SettleCounter => ref NPC.ai[3];

	private float deathCounter = 1f;
	private int settleCounter;
	private bool settled;

	private float CountDeath()
	{
		NPC.dontTakeDamage = true;
		return deathCounter -= 1f / 20;
	}

	public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 3; //Per column

	public override void SetDefaults()
	{
		NPC.Size = new(10);
		NPC.damage = 0;
		NPC.defense = 0;
		NPC.lifeMax = 1;
		NPC.dontCountMe = true;
		NPC.DeathSound = SoundID.NPCDeath6 with { Pitch = .2f };
		NPC.knockBackResist = .45f;
		NPC.aiStyle = 64;
		NPC.npcSlots = 0;
		NPC.noGravity = true;
		NPC.catchItem = ItemID.SilverCoin; //The player should never be able to catch this critter

		AIType = NPCID.Firefly;
	}

	public override bool PreAI()
	{
		if (settled)
		{
			NPC.TargetClosest(false);

			if (NPC.HasValidTarget)
			{
				var target = Main.player[NPC.target];

				if (target.Distance(NPC.Center) < 90)
				{
					settleCounter = 60 * 5;
					settled = false;
				}
			}

			if (settleCounter == 0)
			{
				settleCounter = 60 * 5;
				settled = false;
			}
		}
		else
		{
			NPC.rotation = 0;
		}

		if (settleCounter == 0)
		{
			var tile = Framing.GetTileSafely(NPC.Center);
			if (tile.WallType != WallID.None || tile.HasTile && Main.tileAxe[tile.TileType]) //Just settled
			{
				settled = true;
				settleCounter = 60 * 30;

				NPC.rotation = Main.rand.NextFloat(MathHelper.Pi);
				NPC.velocity = Vector2.Zero;
				NPC.netUpdate = true;
			}
		}

		if (deathCounter < 1) //Expire
		{
			if (CountDeath() <= 0)
				NPC.active = false;
		}

		settleCounter = Math.Max(settleCounter - 1, 0);
		return !settled;
	}

	public override void AI()
	{
		NPC.spriteDirection = NPC.direction;

		if (NPC.velocity != Vector2.Zero && Main.rand.NextBool(10))
		{
			var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkTorch, Scale: 2f);
			dust.noGravity = true;
			dust.velocity = Vector2.Zero;
			dust.noLightEmittence = true;
		}
	}

	public override bool? CanBeCaughtBy(Item item, Player player)
	{
		CountDeath();
		SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);

		return false;
	}

	public override bool CheckDead()
	{
		CountDeath();
		SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);
		NPC.life = 1; //Don't die
		NPC.velocity = Vector2.Zero;

		return false;
	}

	public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) => modifiers.HideCombatText();

	public override void FindFrame(int frameHeight)
	{
		NPC.frame.Width = 22;
		NPC.frame.X = NPC.frame.Width * (settled ? 1 : 0);

		NPC.frameCounter += .15f;
		NPC.frameCounter %= Main.npcFrameCount[Type];

		NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		const float blurStrength = 2f;

		var texture = TextureAssets.Npc[Type].Value;
		var frame = NPC.frame with { Width = NPC.frame.Width - 2, Height = NPC.frame.Height - 2 };
		var effects = (NPC.spriteDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

		const int images = 4;
		for (int i = 0; i < images; i++)
		{
			float radians = MathHelper.TwoPi / images * Lerp(50f) * i;
			var color = Color.Lerp(new Color(90, 70, 255, 50), Color.HotPink with { A = 50 }, Lerp(25f)) * deathCounter;
			var position = NPC.Center - screenPos + new Vector2(0f, blurStrength * (8f - deathCounter * 7f)).RotatedBy(radians) * Lerp(60f);

			spriteBatch.Draw(texture, position, frame, color, NPC.rotation, frame.Size() / 2, NPC.scale, effects, 0);
		}

		float Lerp(float rate) => (float)Math.Sin((Main.timeForVisualEffects + NPC.whoAmI * 3) / rate);

		return false;
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		var coord = new Point(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
		foreach (var zone in ModContent.GetInstance<ButterflySystem>().butterflyZones)
		{
			if (zone.Contains(coord))
				return .5f; //Commonly spawn butterflies in the zone

			if (coord.Y < zone.Y && Math.Abs(coord.X - zone.Center.X) < zone.Width)
				return Main.dayTime ? .047f : .08f; //Rarely spawn butterflies anywhere above the zone
		}

		return 0;
	}
}
