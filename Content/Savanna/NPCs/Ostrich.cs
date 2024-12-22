using SpiritReforged.Common.Easing;
using SpiritReforged.Common.NPCCommon;
using SpiritReforged.Common.Particle;
using SpiritReforged.Common.TileCommon.TileSway;
using SpiritReforged.Content.Particles;
using SpiritReforged.Content.Vanilla.Items.Food;
using System.Linq;
using Terraria.Audio;
using Terraria.Utilities;

namespace SpiritReforged.Content.Savanna.NPCs;

[SpawnPack(2, 4)]
[AutoloadBanner]
public class Ostrich : ModNPC
{
	private static readonly int[] endFrames = [3, 7, 5, 8, 9, 6, 6];
	private const float runSpeed = 4f;
	private const int drownTimeMax = 60 * 5;
	private const int noCollideTimeMax = 10;

	private bool OnTransitionFrame => (int)NPC.frameCounter == endFrames[AIState] - 1;
	private bool Charging => Math.Abs(NPC.velocity.X) > runSpeed;
	private bool chargeSound = false;

	private float frameRate = .2f;
	private int drownTime;

	private enum State : byte
	{
		Stopped,
		Idle1,
		Idle2,
		Running,
		MunchStart,
		Munching,
		MunchEnd
	}

	public int AIState { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
	public ref float Counter => ref NPC.ai[1];
	public ref float NoCollideTime => ref NPC.localAI[0]; //Counts how long the NPC hasn't been grounded for

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
		SavannaGlobalNPC.savannaFaunaTypes.Add(Type);

		Main.npcFrameCount[Type] = 9; //Rows
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(40, 60);
		NPC.damage = 20;
		NPC.defense = 0;
		NPC.value = 45f;
		NPC.lifeMax = 62;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = .45f;
		NPC.direction = 1; //Don't start at 0
		AIType = -1;
	}

	public override void AI()
	{
		NPC.TargetClosest(false);
		var target = Main.player[NPC.target];

		switch (AIState)
		{
			case (int)State.Stopped:

				frameRate = .1f;

				if (ShouldRunAway(16 * 10))
					ChangeState(State.Running);
				else if (Main.rand.NextBool(50) && Main.netMode != NetmodeID.MultiplayerClient)
				{
					var action = new WeightedRandom<State>();
					action.Add(State.Running, 1.2f);

					if (!NPC.wet && Collision.SolidCollision(NPC.Bottom + new Vector2(50 * NPC.direction, 0), 4, 4)) //Is there solid collision at the head position?
						action.Add(State.MunchStart);

					action.Add(State.Idle1);
					action.Add(State.Idle2);

					var selected = (State)action;
					ChangeState(selected, selected is not State.Idle1 and not State.Idle2);
				}

				NPC.velocity.X = 0;

				break;

			case (int)State.Running:

				const float runSpeed = 4f;
				frameRate = MathHelper.Clamp(Math.Abs(NPC.velocity.X) / runSpeed, .5f, 1f) * .25f;

				TryJump();

				if (ShouldRunAway(16 * 28, 2.5f) && !Charging) //Prioritize running from the player
					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(NPC.Center.X - target.Center.X) * runSpeed, .1f);
				else if (Counter % 160 == 159 && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(2) || Counter > 500 || NPC.velocity == Vector2.Zero)
				{
					if (NPC.velocity.X == 0)
					{
						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							NPC.velocity.X = Main.rand.NextFloat(1.25f, 1.5f) * (Main.rand.NextBool() ? -1 : 1); //Wander
							NPC.netUpdate = true;
						}
					}
					else
						ChangeState(State.Stopped);
				}

				if (Math.Abs(NPC.velocity.X) < 1)
					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction, .03f);

				break;

			case (int)State.MunchStart:

				if (OnTransitionFrame)
					ChangeState(State.Munching);

				break;

			case (int)State.Munching:

				if (ShouldRunAway(16 * 8))
				{
					ChangeState(State.MunchEnd);
					frameRate = .25f; //Stop in a hurry
				}
				else if (OnTransitionFrame && Counter % 30 == 0 && Main.rand.NextBool(3))
				{
					if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(4) || Counter > 600)
						ChangeState(State.MunchEnd, true);
					else
						NPC.frameCounter = 0; //Randomly restart the animation
				}

				if (!Main.dedServ && !OnTransitionFrame && (int)NPC.frameCounter % (endFrames[AIState] / 2) == 0)
				{
					var tilePos = ((NPC.Center + new Vector2(NPC.width * NPC.direction, 8)) / 16).ToPoint();
					var direction = Vector2.UnitX * (Main.rand.NextBool() ? -1 : 1);

					TileSwayHelper.SetWindTime(tilePos.X, tilePos.Y, direction);
				} //Cause tiles to sway while munching

				break;
		}

		if (AIState is ((int)State.Idle1) or ((int)State.Idle2) or ((int)State.MunchEnd)) //Any idle animation
		{
			if (OnTransitionFrame)
			{
				ChangeState(State.Stopped);
				NPC.frameCounter = endFrames[AIState] - 1; //Skip the animation in this context
			}
		}

		if (NPC.wet && Collision.WetCollision(NPC.position, NPC.width, NPC.height / 3))
		{
			if (++drownTime > drownTimeMax)
			{
				NPC.velocity *= .99f;
				if (Main.rand.NextBool(8))
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.BreatheBubble);

				if (drownTime % 3 == 0 && --NPC.life <= 0)
				{
					SoundEngine.PlaySound(NPC.DeathSound, NPC.Center);
					HitEffect(new NPC.HitInfo());
				}
			}
			else
				NPC.velocity.Y = Math.Max(NPC.velocity.Y - .75f, -3f);
		}
		else if (!NPC.wet)
			drownTime = 0;

		if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height + 2))
			NoCollideTime++;
		else
			NoCollideTime = 0;

		if (Charging && !chargeSound && Counter != 0)
		{
			SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown with { Volume = .5f, PitchVariance = .5f, MaxInstances = 0 }, NPC.Center);
			chargeSound = true;
		}

		if (Charging && Counter % 15 == 0)
			ParticleHandler.SpawnParticle(new OstrichImpact(
				NPC,
				NPC.Center,
				Vector2.Zero,
				270,
				100f,
				(Math.Sign(NPC.velocity.X) == 1) ? MathHelper.Pi : 0,
				30,
				.6f));

		if (NPC.velocity.X < 0)
			NPC.direction = NPC.spriteDirection = -1;
		else if (NPC.velocity.X > 0)
			NPC.direction = NPC.spriteDirection = 1;

		Counter++;

		bool ShouldRunAway(int distance, float limit = 4f) => NPC.Distance(target.Center) < distance && target.velocity.Length() > limit;
		void TryJump(float height = 6.5f)
		{
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

			if (NPC.collideX && NPC.velocity == Vector2.Zero) //Jump
				NPC.velocity.Y = -height;
		}
	}

	private void ChangeState(State toState, bool sync = false)
	{
		if (AIState == (int)toState) //We switched to a new state
			return;

		NPC.frameCounter = 0;
		frameRate = .2f;
		Counter = 0;
		chargeSound = false;
		AIState = (int)toState;

		if (sync && Main.dedServ)
			NPC.netUpdate = true;
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot) => Charging;

	public override bool CanHitNPC(NPC target) => Charging;

	public override bool CanBeHitByNPC(NPC attacker) => SavannaGlobalNPC.savannaFaunaTypes.Contains(attacker.type) && attacker.type != Type;

	public override void HitEffect(NPC.HitInfo hit)
	{
		bool dead = NPC.life <= 0;

		SoundEngine.PlaySound(SoundID.NPCHit11 with { PitchVariance = .5f, MaxInstances = 0, Pitch = -.25f }, NPC.Center);
		SoundEngine.PlaySound(SoundID.Grass with { Volume = .45f, PitchVariance = .5f, MaxInstances = 0 }, NPC.Center);


		for (int i = 0; i < (dead ? 30 : 4); i++)
		{
			Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, Scale: Main.rand.NextFloat(.8f, 2f))
				.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f);
		}

		if (!Main.dedServ && dead)
		{
			for (int i = 1; i < 6; i++)
				Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.getRect()), NPC.velocity * Main.rand.NextFloat(), Mod.Find<ModGore>("Ostrich" + i).Type);

			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCDeath/Ostrich_Death") with { Volume = .75f, PitchVariance = .5f, Pitch = -.5f, MaxInstances = 0 }, NPC.Center);
		}

		const int scareDistance = 16 * 20;
		var pack = Main.npc.Where(x => x.active && x.type == Type 
			&& (x.whoAmI == NPC.whoAmI || x.Distance(NPC.Center) < scareDistance)); //All NPC instances of this type, including this one

		foreach (var npc in pack) //Scare nearby Ostriches
		{
			(npc.ModNPC as Ostrich).ChangeState(State.Running);
			npc.velocity.X = hit.HitDirection * (runSpeed + 1);
		}
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frame.Width = 96; //frameHeight = 90
		NPC.frame.X = NPC.frame.Width * AIState;

		NPC.frameCounter += frameRate;

		if (AIState is (int)State.Running) //Running is the only state that loops automatically
			NPC.frameCounter %= endFrames[AIState];
		else if (NPC.frameCounter >= endFrames[AIState])
			NPC.frameCounter--;

		if (!NPC.wet && NoCollideTime > noCollideTimeMax)
			(NPC.frame.X, NPC.frame.Y) = (3 * NPC.frame.Width, 3 * frameHeight); //Airborne frame
		else
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		var texture = TextureAssets.Npc[Type].Value;
		var source = NPC.frame with { Width = NPC.frame.Width - 2, Height = NPC.frame.Height - 2 }; //Remove padding
		var position = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY - (source.Height - NPC.height) / 2 + 4);
		var extraOffset = new Vector2(16 * NPC.spriteDirection, 0);

		var effects = (NPC.spriteDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		var color = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));

		Main.EntitySpriteDraw(texture, position + extraOffset, source, color, NPC.rotation, source.Size() / 2, NPC.scale, effects);

		return false;
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		if (spawnInfo.Player.InModBiome<Biome.SavannaBiome>() && !spawnInfo.Water)
			return .2f;

		return 0;
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.AddCommon<RawMeat>(3);
}

public class OstrichImpact(Entity entity, Vector2 basePosition, Vector2 velocity, float width, float length, float rotation, int maxTime, float taperExponent, int detatchTime = -1) : MotionNoiseCone(entity, basePosition, velocity, width, length, rotation, maxTime, detatchTime)
{
	internal override bool UseLightColor => true;
	private readonly float _taperExponent = taperExponent;
	internal override float GetScroll() => -1.5f * (EaseFunction.EaseCircularOut.Ease(Progress) + TimeActive / 60f);

	internal override Color BrightColor => Color.Transparent;
	internal override Color DarkColor => Color.LightGray with { A = 220 };

	internal override void DissipationStyle(ref float dissipationProgress, ref float finalExponent, ref float xCoordExponent)
	{
		dissipationProgress = EaseFunction.EaseQuadIn.Ease(Progress);
		finalExponent = 3f;
		xCoordExponent = 1.2f;
	}

	internal override float ColorLerpExponent => 1.5f;

	internal override int NumColors => 4;

	internal override float FinalIntensity => 1.2f;

	internal override void TaperStyle(ref float totalTapering, ref float taperExponent)
	{
		totalTapering = 1;
		taperExponent = _taperExponent;
	}

	internal override void TextureExponent(ref float minExponent, ref float maxExponent, ref float lerpExponent)
	{
		minExponent = 0.8f;
		maxExponent = 40f;
		lerpExponent = 2.25f;
	}

	internal override void XDistanceFade(ref float centeredPosition, ref float exponent)
	{
		float easedProgress = EaseFunction.EaseQuadIn.Ease(Progress);
		centeredPosition = MathHelper.Lerp(0.15f, 0.5f, easedProgress);
		exponent = MathHelper.Lerp(2.5f, 4f, easedProgress);
	}
}
