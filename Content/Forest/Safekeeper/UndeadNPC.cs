using SpiritReforged.Common.Particle;
using SpiritReforged.Common.PlayerCommon;
using Terraria.Audio;
using Terraria.GameContent.Drawing;

namespace SpiritReforged.Content.Forest.Safekeeper;

public class UndeadNPC : GlobalNPC
{
	private static readonly HashSet<int> undeadTypes = [NPCID.Zombie, NPCID.ZombieDoctor, NPCID.ZombieElf, NPCID.ZombieElfBeard, NPCID.ZombieElfGirl, NPCID.ZombieEskimo, 
		NPCID.ZombieMerman, NPCID.ZombieMushroom, NPCID.ZombieMushroomHat, NPCID.ZombiePixie, NPCID.ZombieRaincoat, NPCID.ZombieSuperman, NPCID.ZombieSweater, 
		NPCID.ZombieXmas, NPCID.ArmedTorchZombie, NPCID.ArmedZombie, NPCID.ArmedZombieCenx, NPCID.ArmedZombieEskimo, NPCID.ArmedZombiePincussion, NPCID.ArmedZombieSlimed, 
		NPCID.ArmedZombieSwamp, NPCID.ArmedZombieTwiggy, NPCID.BaldZombie, NPCID.BloodZombie, NPCID.FemaleZombie, NPCID.MaggotZombie, NPCID.PincushionZombie, 
		NPCID.TheGroom, NPCID.TheBride, NPCID.SlimedZombie, NPCID.SwampZombie, NPCID.TorchZombie, NPCID.TwiggyZombie, NPCID.Drippler, NPCID.Skeleton, NPCID.SkeletonAlien, 
		NPCID.SkeletonArcher, NPCID.SkeletonAstonaut, NPCID.SkeletonTopHat, NPCID.BoneThrowingSkeleton, NPCID.BoneThrowingSkeleton2, NPCID.BoneThrowingSkeleton3, 
		NPCID.BoneThrowingSkeleton4, NPCID.ArmoredSkeleton, NPCID.ArmoredViking, NPCID.BlueArmoredBones, NPCID.BlueArmoredBonesMace, NPCID.BlueArmoredBonesNoPants, 
		NPCID.BlueArmoredBonesSword, NPCID.HellArmoredBones, NPCID.HellArmoredBonesMace, NPCID.HellArmoredBonesSpikeShield, NPCID.HellArmoredBonesSword, 
		NPCID.RustyArmoredBonesAxe, NPCID.RustyArmoredBonesFlail, NPCID.RustyArmoredBonesSword, NPCID.RustyArmoredBonesSwordNoArmor, NPCID.Necromancer, 
		NPCID.NecromancerArmored, NPCID.SkeletonSniper, NPCID.SkeletonCommando, NPCID.RuneWizard, NPCID.Tim, NPCID.BoneLee, NPCID.AngryBones, NPCID.AngryBonesBig, 
		NPCID.AngryBonesBigHelmet, NPCID.AngryBonesBigMuscle, NPCID.UndeadMiner, NPCID.UndeadViking, NPCID.BoneSerpentBody, NPCID.BoneSerpentHead, NPCID.BoneSerpentTail, 
		NPCID.DemonEye, NPCID.DemonEyeOwl, NPCID.DemonEyeSpaceship, NPCID.ServantofCthulhu, NPCID.EyeofCthulhu, NPCID.SkeletronHand, NPCID.SkeletronHead];

	private static readonly HashSet<NPC> toDraw = [];
	private static bool trackingGore;

	private const float decayRate = .025f;
	private float decayTime = 1;

	private static readonly HashSet<int> customUndeadTypes = [];
	internal static bool AddCustomUndead(params object[] args)
	{
		if (args.Length == 2 && args[1] is int customType) //Context, undead type
			return customUndeadTypes.Add(customType);

		return false;
	}

	internal static bool IsUndeadType(int type) => undeadTypes.Contains(type) || customUndeadTypes.Contains(type) || NPCID.Sets.Zombies[type] || NPCID.Sets.Skeletons[type] || NPCID.Sets.DemonEyes[type];

	private static bool ShouldTrackGore(NPC self, int dmg = 0) => self.life - dmg <= 0 && self.TryGetGlobalNPC(out UndeadNPC _) && Main.player[self.lastInteraction].HasAccessory<SafekeeperRing>();

	public override bool InstancePerEntity => true;
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => IsUndeadType(entity.type);

	public override void Load()
	{
		On_NPC.HitEffect_HitInfo += TrackGore;
		On_Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += StopGore;
		On_Main.DrawNPCs += DrawNPCsInQueue;
	}

	private static void TrackGore(On_NPC.orig_HitEffect_HitInfo orig, NPC self, NPC.HitInfo hit)
	{
		trackingGore = ShouldTrackGore(self, hit.Damage);

		orig(self, hit);

		trackingGore = false;
	}

	private static int StopGore(On_Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig, Terraria.DataStructures.IEntitySource source, Vector2 Position, Vector2 Velocity, int Type, float Scale)
	{
		int result = orig(source, Position, Velocity, Type, Scale);

		if (trackingGore)
			Main.gore[result].active = false; //Instantly deactivate the spawned gore

		return result;
	}

	private static void DrawNPCsInQueue(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
	{
		const float spotScale = .5f;

		orig(self, behindTiles);

		if (toDraw.Count == 0)
			return; //Nothing to draw; don't restart the spritebatch

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		var effect = AssetLoader.LoadedShaders["NoiseFade"];

		foreach (var npc in toDraw) //Draw all shader-affected NPCs
		{
			var effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			var texture = TextureAssets.Npc[npc.type].Value;
			var frame = npc.frame with { Y = 0 };

			float decayTime = 0;
			if (npc.TryGetGlobalNPC(out UndeadNPC gNPC))
				decayTime = gNPC.decayTime;

			effect.Parameters["power"].SetValue(decayTime * 50f);
			effect.Parameters["size"].SetValue(new Vector2(1, Main.npcFrameCount[npc.type]) * spotScale);
			effect.Parameters["noiseTexture"].SetValue(AssetLoader.LoadedTextures["vnoise"]);
			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(texture, npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), frame, Color.Black, npc.rotation, frame.Size() / 2, npc.scale, effects, 0);
		}

		Main.spriteBatch.End();
		Main.spriteBatch.Begin();

		toDraw.Clear();
	}

	public override bool CheckDead(NPC npc)
	{
		if (!Main.player[npc.lastInteraction].HasAccessory<SafekeeperRing>())
			return true;

		decayTime -= decayRate;
		npc.life = 1;
		npc.dontTakeDamage = true;

		npc.NPCLoot();

		SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCDeath/Fire_1") with { Pitch = .25f, PitchVariance = .2f }, npc.Center);
		SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/Explosion_Liquid") with { Pitch = .8f, PitchVariance = .2f }, npc.Center);

		if (!Main.dedServ)
		{
			var pos = npc.Center;
			for (int i = 0; i < 3; i++)
				ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.AshTreeShake, new ParticleOrchestraSettings() with { PositionInWorld = pos });

			ParticleHandler.SpawnParticle(new Particles.LightBurst(npc.Center, 0, Color.Goldenrod with { A = 0 }, npc.scale * .8f, 10));

			for (int i = 0; i < 15; i++)
			{
				ParticleHandler.SpawnParticle(new Particles.GlowParticle(npc.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f),
					Color.White, Color.Lerp(Color.Goldenrod, Color.Orange, Main.rand.NextFloat()), 1, Main.rand.Next(10, 20), 8));
			}
		}

		return false;
	}

	public override void PostAI(NPC npc)
	{
		if (decayTime == 1)
			return;

		if ((decayTime -= decayRate) <= 0)
		{
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCDeath/Dust_1") with { Pitch = .75f, PitchVariance = .25f }, npc.Center);
			npc.active = false;
		}

		var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Asphalt, Scale: Main.rand.NextFloat(.5f, 1.5f));
		dust.velocity = -npc.velocity;
		dust.noGravity = true;
		dust.color = new Color(25, 20, 20);
	}

	public override bool CanHitNPC(NPC npc, NPC target) => decayTime == 1;
	public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => decayTime == 1;

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (decayTime == 1)
			return true;

		toDraw.Add(npc);
		return false;
	}
}
