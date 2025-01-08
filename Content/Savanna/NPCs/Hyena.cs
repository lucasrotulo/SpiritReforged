using SpiritReforged.Common.NPCCommon;
using SpiritReforged.Content.Vanilla.Items.Food;
using System.Linq;
using Terraria.Audio;
using static Terraria.Utilities.NPCUtils;

namespace SpiritReforged.Content.Savanna.NPCs;

[SpawnPack(2, 3)]
[AutoloadBanner]
public class Hyena : ModNPC
{
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

	private static readonly int[] endFrames = [4, 2, 5, 5, 5, 13, 7];
	private const int drownTimeMax = 300;

	private bool OnTransitionFrame => (int)NPC.frameCounter >= endFrames[AnimationState];
	public int AnimationState { get => (int)NPC.ai[0]; set => NPC.ai[0] = value; }
	public ref float Counter => ref NPC.ai[1];
	public ref float TargetSpeed => ref NPC.ai[2];

	private bool dealDamage;
	private int drownTime;
	private TargetSearchFlag whitelist = TargetSearchFlag.All;

	public override void SetStaticDefaults()
	{
		NPC.SetNPCTargets(ModContent.NPCType<Ostrich>());
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
		dealDamage = false;
		int searchDist = (whitelist is TargetSearchFlag.All) ? 350 : 450;
		var search = NPC.FindTarget(whitelist, SearchFilters.OnlyPlayersInCertainDistance(NPC.Center, searchDist), AdvancedTargetingHelper.NPCsByDistanceAndType(NPC, searchDist));
		bool wounded = NPC.life < NPC.lifeMax * .25f;

		TrySwim();
		TryJump();
		Separate();

		if (!search.FoundTarget || wounded) //Idle
		{
			whitelist = TargetSearchFlag.All;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, TargetSpeed, .025f);

			if (Counter % 250 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				TargetSpeed = Main.rand.NextFromList(-1, 0, 1) * Main.rand.NextFloat(.8f, 1.5f) * (wounded ? .5f : 1f);
				NPC.netUpdate = true;
			}

			if (Math.Abs(NPC.velocity.X) < .1f)
				ChangeAnimationState(State.TrotEnd);
			else
				ChangeAnimationState(State.Walking, true);

			if (wounded && Main.rand.NextBool(8))
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
		}
		else //Targeting
		{
			const int spotDistance = 16 * 12;

			TargetSpeed = 0;
			var target = NPC.GetTargetData();

			if (TryChaseTarget(search))
			{
				if (AnimationState == (int)State.BarkingAngry)
				{
					if (OnTransitionFrame)
						ChangeAnimationState(State.TrottingAngry);
				}
				else
				{
					ChangeAnimationState(State.TrottingAngry, true);

					if (Main.rand.NextBool(120)) //Randomly bark; not synced
					{
						ChangeAnimationState(State.BarkingAngry);
						SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Ambient/Hyena_Bark") with { Volume = .18f, PitchVariance = .4f }, NPC.Center);
					}
				}

				dealDamage = true;
			}
			else if (AnimationState == (int)State.Trotting)
			{
				NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(NPC.Center.X - target.Center.X) * 3f, .05f); //Run from the target
				ChangeAnimationState(State.Trotting, true);
			}
			else if (NPC.Distance(target.Center) > spotDistance)
			{
				NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(target.Center.X - NPC.Center.X) * 1.5f, .05f); //Move toward the target
				ChangeAnimationState(State.Walking, true);
			}
			else
			{
				NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0, .1f);
				
				if (AnimationState == (int)State.Laugh)
				{
					if (OnTransitionFrame)
					{
						ChangeAnimationState(State.TrotEnd);
						NPC.frameCounter = endFrames[AnimationState];
					}
				}
				else
				{
					ChangeAnimationState(State.TrotEnd);

					if (Main.rand.NextBool(150)) //Randomly laugh; not synced
					{
						ChangeAnimationState(State.Laugh);
						SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Ambient/Hyena_Laugh") with { Volume = 1.25f, PitchVariance = 0.4f, MaxInstances = 3 }, NPC.Center);
					}
				}

				if (target.Velocity.Length() > 3f && NPC.Distance(target.Center) < spotDistance - 16)
					ChangeAnimationState(State.Trotting); //Begin to run from the target
			}
		}

		if (NPC.velocity.X < 0) //Set direction
			NPC.direction = NPC.spriteDirection = -1;
		else if (NPC.velocity.X > 0)
			NPC.direction = NPC.spriteDirection = 1;

		Counter++;

		void TryJump(float height = 6.5f)
		{
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

			if (NPC.collideX && NPC.velocity == Vector2.Zero)
				NPC.velocity.Y = -height;
		}

		void Separate(int distance = 32)
		{
			var nearest = Main.npc.OrderBy(x => x.Distance(NPC.Center)).Where(x => x.active && x.whoAmI != NPC.whoAmI && x.type == Type && x.Distance(NPC.Center) < distance).FirstOrDefault();

			if (nearest != default)
			{
				float update = Math.Sign(NPC.Center.X - nearest.Center.X) * .1f;

				if (Math.Sign(NPC.velocity.X) == Math.Sign(NPC.velocity.X + update)) //Does this require a change in direction?
					NPC.velocity.X += update;
			}
		}

		void TrySwim()
		{
			if (NPC.wet && Collision.WetCollision(NPC.position, NPC.width, NPC.height / 2))
			{
				if (++drownTime > drownTimeMax) //Drown
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
			}
			else if (!NPC.wet)
				drownTime = 0;
		}
	}

	private bool TryChaseTarget(TargetSearchResults search)
	{
		if (NPC.HasPlayerTarget && (Main.player[NPC.target].statLife < Main.player[NPC.target].statLifeMax2 * .25f || whitelist is TargetSearchFlag.Players))
		{
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(Main.player[NPC.target].Center.X - NPC.Center.X) * 4.8f, .1f); //Chase the player target
			return true;
		}
		else if (search.FoundNPC && (search.NearestNPC.life < search.NearestNPC.lifeMax * .25f || whitelist is TargetSearchFlag.NPCs))
		{
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(search.NearestNPC.Center.X - NPC.Center.X) * 4.8f, .1f); //Chase the npc target
			return true;
		}

		return false;
	}

	private void ChangeAnimationState(State toState, bool loop = false)
	{
		if (OnTransitionFrame && loop)
			NPC.frameCounter = 0;

		if (AnimationState != (int)toState)
		{
			AnimationState = (int)toState;
			NPC.frameCounter = 0;
			Counter = 0;
		}
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot) => dealDamage;
	public override bool CanHitNPC(NPC target) => dealDamage;

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (!Main.dedServ)
		{
			bool dead = NPC.life <= 0;
			for (int i = 0; i < (dead ? 20 : 3); i++)
			{
				Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, Scale: Main.rand.NextFloat(.8f, 2f))
					.velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f);
			}

			if (dead)
			{
				for (int i = 1; i < 4; i++)
					Gore.NewGore(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.getRect()), NPC.velocity * Main.rand.NextFloat(.3f), Mod.Find<ModGore>("Hyena" + i).Type);

				SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCDeath/Hyena_Death") with { Volume = .75f, Pitch = .2f, MaxInstances = 0 }, NPC.Center);
			}

			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCHit/Hyena_Hit") with { Volume = .75f, Pitch = -.05f, PitchVariance = .4f, MaxInstances = 0 }, NPC.Center);
		}

		AggroNearby();
	}

	private void AggroNearby()
	{
		const int aggroRange = 16 * 25;
		const int searchDist = 400;

		var search = NPC.FindTarget(playerFilter: SearchFilters.OnlyPlayersInCertainDistance(NPC.Center, searchDist), npcFilter: AdvancedTargetingHelper.NPCsByDistanceAndType(NPC, searchDist));
		TargetSearchFlag flag;

		if (search.NearestTargetType == TargetType.NPC)
			flag = TargetSearchFlag.NPCs;
		else if (NPC.HasPlayerTarget)
			flag = TargetSearchFlag.Players;
		else
			return;

		foreach (var other in Main.ActiveNPCs)
		{
			if (other.type == Type && other.Distance(NPC.Center) <= aggroRange && other.ModNPC is Hyena hyena)
				hyena.whitelist = flag;
		}
	}

	public override void FindFrame(int frameHeight)
	{
		NPC.frame.Width = 72; //frameHeight = 48
		NPC.frame.X = NPC.frame.Width * AnimationState;

		if (AnimationState == (int)State.Walking)
			NPC.frameCounter += Math.Min(Math.Abs(NPC.velocity.X) / 5f, .2f);
		else
			NPC.frameCounter += .2f;

		NPC.frame.Y = (int)Math.Min(endFrames[AnimationState] - 1, NPC.frameCounter) * frameHeight;
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
