using SpiritReforged.Common.NPCCommon;
using SpiritReforged.Content.Vanilla.Items.Food;
using System.Linq;
using Terraria.Audio;

namespace SpiritReforged.Content.Savanna.NPCs;

[SpawnPack(2, 3)]
[AutoloadBanner]
public class Hyena : ModNPC
{
	private static readonly int[] endFrames = [4, 2, 5, 5, 5, 13, 7];
	private const int drownTimeMax = 60 * 10;
	private const int noCollideTimeMax = 10;

	private bool OnTransitionFrame => (int)NPC.frameCounter == endFrames[AIState] - 1;
	private bool NearlyDead => NPC.life < NPC.lifeMax / 4;

	private int drownTime;
	private bool angryBark = false;
	private enum State : byte
	{
		TrotEnd,
		TrotStart,
		Trotting,
		TrottingAngry,
		BarkingAngry,
		Laugh,
		Walking
	}

	public int AIState { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
	public ref float Counter => ref NPC.ai[1];
	public ref float NoCollideTime => ref NPC.localAI[0]; //Counts how long the NPC hasn't been grounded for

	private bool IsAngry => AIState is ((int)State.TrottingAngry) or ((int)State.BarkingAngry);

	private Entity SelectTarget()
	{
		NPC.TargetClosest(false);
		var player = Main.player[NPC.target];

		if (NearlyDead)
			return player; //Don't target NPCs when health is low

		var nearby = Main.npc.Where(x => x.active && x.type != Type && SavannaGlobalNPC.savannaFaunaTypes.Contains(x.type)).OrderBy(x => x.Distance(NPC.Center)).FirstOrDefault();
		if (nearby is null)
			return player;

		return (NPC.Distance(player.Center) < NPC.Distance(nearby.Center)) ? player : nearby;
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
		SavannaGlobalNPC.savannaFaunaTypes.Add(Type);

		Main.npcFrameCount[Type] = 13; //Rows
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(40, 40);
		NPC.damage = 10;
		NPC.defense = 0;
		NPC.lifeMax = 40;
		NPC.value = 38f;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = .45f;
		NPC.direction = 1; //Don't start at 0
		AIType = -1;
	}

	public override void AI()
	{
		var target = SelectTarget();
		int alertDistance = (target is Player) ? 16 * 12 : 16 * 8;

		switch (AIState)
		{
			case (int)State.TrotEnd: //Doubles as our idle state
				if (Counter == 100 && Main.rand.NextBool())
				{
					SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Ambient/Hyena_Laugh") with { Volume = 1.25f, PitchVariance = 0.4f, MaxInstances = 3 }, NPC.Center);
					ChangeState(State.Laugh, false);
				}
				else if (OnTransitionFrame)
				{
					if (Counter >= 30)
						NPC.direction = NPC.spriteDirection = (target.Center.X < NPC.Center.X) ? -1 : 1;

					if (NPC.Distance(target.Center) > alertDistance + 24)
					{
						if (Counter % 150 == 149 && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(3))
							ChangeState(State.Walking, sync: true);
					}
					else if (NPC.Distance(target.Center) < alertDistance)
					{
						if (target is Player)
							ChangeState(State.TrotStart);
						else if (Counter % 500 == 499 && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(2))
							ChangeState(State.TrottingAngry);
					}
				}

				NPC.velocity.X = 0;
				break;

			case (int)State.TrotStart:
				if (OnTransitionFrame)
					ChangeState(State.Trotting);

				break;

			case (int)State.Trotting:
				const float runSpeed = 4.8f;

				TryJump();

				if (NPC.wet)
				{
					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * runSpeed, .05f); //Always swim ahead
				}
				else
				{
					if (NPC.Distance(target.Center) > alertDistance + 96 && Counter % 100 == 0)
						ChangeState(State.TrotEnd);
					else
						NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(NPC.Center.X - target.Center.X) * runSpeed, .08f);
				}

				Separate();
				break;

			case (int)State.Laugh:
				if (OnTransitionFrame)
				{
					ChangeState(State.TrotEnd, false);
					NPC.frameCounter = endFrames[AIState];
				}

				break;

			case (int)State.Walking:
				const float walkSpeed = 1.5f;

				TryJump();

				if (NearlyDead)
				{
					if (Main.rand.NextBool(8))
						Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);

					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(NPC.Center.X - target.Center.X) * walkSpeed, .08f);
				}
				else
				{
					if (NPC.Distance(target.Center) > alertDistance + 24 && !(Counter > 60 && Math.Abs(NPC.velocity.X) < .3f))
						NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(target.Center.X - NPC.Center.X) * walkSpeed, .05f);
					else
						ChangeState(State.TrotEnd);
				}

				Separate();
				break;
		}

		if (IsAngry) //Accounts for states TrottingAngry and BarkingAngry
		{
			if (!angryBark)
			{
				SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Ambient/Hyena_Bark") with { Volume = .18f, PitchVariance = .4f, MaxInstances = 0 }, NPC.Center);
				angryBark = true;
			}

			TryJump();

			const float runSpeed = 5.5f;
			float distance = MathHelper.Clamp(NPC.Distance(target.Center) / (16 * 5), 0, 1);
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(target.Center.X - NPC.Center.X) * runSpeed, .002f + distance * .08f);

			if (AIState == (int)State.BarkingAngry)
			{
				if (OnTransitionFrame)
					ChangeState(State.TrottingAngry, false);
			}
			else if (Main.rand.NextBool(100))
				ChangeState(State.BarkingAngry, false);

			if (NPC.Distance(target.Center) > 16 * 30) //Deaggro when far enough away
				ChangeState(State.Trotting);

			Separate();
		}
		else if (target is Player p && p.statLife < p.statLifeMax2 * .25f || target is NPC n && n.life < n.lifeMax * .25f)
			ChangeState(State.TrottingAngry); //Begin to chase the target if they are low on health

		if (NPC.wet && Collision.WetCollision(NPC.position, NPC.width, NPC.height / 2))
		{
			if (++drownTime > drownTimeMax) //Drown behaviour
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
				NPC.velocity.Y = Math.Max(NPC.velocity.Y - .75f, -1.5f);

			if (AIState is (int)State.TrotStart or (int)State.TrotEnd or (int)State.Walking && !NearlyDead)
				ChangeState(State.Trotting, false);
		}
		else if (!NPC.wet)
			drownTime = 0;

		if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height + 2))
			NoCollideTime++;
		else
			NoCollideTime = 0;

		if (NPC.velocity.X < 0)
			NPC.direction = NPC.spriteDirection = -1;
		else if (NPC.velocity.X > 0)
			NPC.direction = NPC.spriteDirection = 1;

		Counter++;

		void Separate(int distance = 32)
		{
			var nearest = Main.npc.OrderBy(x => x.Distance(NPC.Center)).Where(x => x.active 
				&& x.whoAmI != NPC.whoAmI && x.type == Type && x.Distance(NPC.Center) < distance).FirstOrDefault();

			if (nearest != default)
			{
				float update = Math.Sign(NPC.Center.X - nearest.Center.X) * .1f;

				if (Math.Sign(NPC.velocity.X) == Math.Sign(NPC.velocity.X + update)) //Does this require a change in direction?
					NPC.velocity.X += update;
			}
		}

		void TryJump(float height = 6.5f)
		{
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

			if (NPC.collideX && NPC.velocity == Vector2.Zero) //Jump
				NPC.velocity.Y = -height;
		}
	}

	private void ChangeState(State toState, bool resetCounter = true, bool sync = false)
	{
		if (AIState == (int)toState) //We switched to a new state
			return;

		NPC.frameCounter = 0;
		AIState = (int)toState;

		if (resetCounter)
			Counter = 0;
		if (sync && Main.dedServ)
			NPC.netUpdate = true;
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot) => IsAngry;

	public override bool CanHitNPC(NPC target) => IsAngry;

	public override void OnHitNPC(NPC target, NPC.HitInfo hit)
	{
		if (target.life <= 0)
			ChangeState(State.Trotting, sync: true); //Deaggro if I killed my target
	}

	public override bool CanBeHitByNPC(NPC attacker) => SavannaGlobalNPC.savannaFaunaTypes.Contains(attacker.type) && attacker.type != Type;

	public override void HitEffect(NPC.HitInfo hit)
	{
		bool dead = NPC.life <= 0;

		for (int i = 0; i < (dead ? 20 : 3); i++)
		{
			Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, Scale: Main.rand.NextFloat(.8f, 2f))
				.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f);
		}

		if (!Main.dedServ && dead)
			for (int i = 1; i < 4; i++)
				Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.getRect()), NPC.velocity * Main.rand.NextFloat(.3f), Mod.Find<ModGore>("Hyena" + i).Type);

		SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCHit/Hyena_Hit") with { Volume = .75f, Pitch = -.05f, PitchVariance = .4f, MaxInstances = 0 }, NPC.Center);

		if (dead)
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCDeath/Hyena_Death") with { Volume = .75f, Pitch = .2f, MaxInstances = 0 }, NPC.Center);

		TryAggro();
		const int detectDistance = 16 * 25;
		var pack = Main.npc.Where(x => x.active && x.type == Type 
			&& (x.whoAmI == NPC.whoAmI || x.Distance(NPC.Center) < detectDistance)); //All NPC instances of this type, including this one

		foreach (var npc in pack)
			(npc.ModNPC as Hyena).TryAggro();
	}

	private void TryAggro()
	{
		if (NearlyDead)
			ChangeState(State.Walking);
		else
			ChangeState(State.TrottingAngry); //Anger nearby Hyena
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frame.Width = 72; //frameHeight = 48
		NPC.frame.X = NPC.frame.Width * AIState;

		NPC.frameCounter += .2f;

		if (AIState is (int)State.Trotting or (int)State.TrottingAngry or (int)State.Walking) //Movement states loop automatically
			NPC.frameCounter %= endFrames[AIState];
		else if (NPC.frameCounter >= endFrames[AIState])
			NPC.frameCounter--;

		if (!NPC.wet && NoCollideTime > noCollideTimeMax)
			(NPC.frame.X, NPC.frame.Y) = (1, 0); //Airborne frame
		else
			NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		var texture = TextureAssets.Npc[Type].Value;
		var source = NPC.frame with { Width = NPC.frame.Width - 2, Height = NPC.frame.Height - 2 }; //Remove padding
		var position = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY - (source.Height - NPC.height) / 2 + 4);
		
		var effects = (NPC.spriteDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		var color = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));

		Main.EntitySpriteDraw(texture, position, source, color, NPC.rotation, source.Size() / 2, NPC.scale, effects);

		return false;
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		if (spawnInfo.Player.InModBiome<Biome.SavannaBiome>() && !spawnInfo.Water)
			return .2f;

		return 0;
	}
	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.AddCommon<RawMeat>(3);
		npcLoot.AddCommon(ItemID.Leather, 2, 1, 2);
	}
}
