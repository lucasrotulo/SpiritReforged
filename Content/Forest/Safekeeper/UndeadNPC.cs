﻿using SpiritReforged.Common.Multiplayer;
using SpiritReforged.Common.Particle;
using SpiritReforged.Common.PlayerCommon;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader.IO;

namespace SpiritReforged.Content.Forest.Safekeeper;

public class UndeadNPC : GlobalNPC
{
	private const float DecayRate = .025f;

	private static readonly HashSet<int> UndeadTypes = [NPCID.Zombie, NPCID.ZombieDoctor, NPCID.ZombieElf, NPCID.ZombieElfBeard, NPCID.ZombieElfGirl, NPCID.ZombieEskimo, 
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
		NPCID.DemonEye, NPCID.DemonEyeOwl, NPCID.DemonEyeSpaceship, NPCID.ServantofCthulhu, NPCID.EyeofCthulhu, NPCID.SkeletronHand, NPCID.SkeletronHead, 
		NPCID.PossessedArmor, NPCID.Paladin, NPCID.DarkCaster, NPCID.RaggedCaster, NPCID.DiabolistRed, NPCID.DiabolistWhite, NPCID.Eyezor, NPCID.CursedSkull, 
		NPCID.GiantCursedSkull, NPCID.Frankenstein, NPCID.DD2SkeletonT1, NPCID.DD2SkeletonT3, NPCID.Poltergeist, NPCID.Wraith, NPCID.FloatyGross, NPCID.Mummy, 
		NPCID.BloodMummy, NPCID.DarkMummy, NPCID.LightMummy];

	private static readonly HashSet<NPC> ToDraw = [];
	private static readonly HashSet<int> CustomUndeadTypes = [];

	private static bool TrackingGore;
	/// <summary> Decreases by <see cref="DecayRate"/>. </summary>
	private float decayTime = 1;

	internal static bool AddCustomUndead(params object[] args)
	{
		if (args.Length == 2 && args[1] is int customType) //Context, undead type
			return CustomUndeadTypes.Add(customType);

		return false;
	}

	internal static bool IsUndeadType(int type) => UndeadTypes.Contains(type) || CustomUndeadTypes.Contains(type) || NPCID.Sets.Zombies[type] || NPCID.Sets.Skeletons[type] || NPCID.Sets.DemonEyes[type];

	private static bool ShouldTrackGore(NPC self) => self.TryGetGlobalNPC(out UndeadNPC _) && self.lastInteraction != 255 && Main.player[self.lastInteraction].HasAccessory<SafekeeperRing>();

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
		TrackingGore = ShouldTrackGore(self);

		orig(self, hit);

		TrackingGore = false;
	}

	private static int StopGore(On_Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig, Terraria.DataStructures.IEntitySource source, Vector2 Position, Vector2 Velocity, int Type, float Scale)
	{
		int result = orig(source, Position, Velocity, Type, Scale);

		if (TrackingGore)
			Main.gore[result].active = false; //Instantly deactivate the spawned gore

		return result;
	}

	private static void DrawNPCsInQueue(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
	{
		const float spotScale = .5f;

		orig(self, behindTiles);

		if (ToDraw.Count == 0)
			return; //Nothing to draw; don't restart the spritebatch

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

		var effect = AssetLoader.LoadedShaders["NoiseFade"];

		foreach (var npc in ToDraw) //Draw all shader-affected NPCs
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

		ToDraw.Clear();
	}

	public override bool CheckDead(NPC npc)
	{
		if (!Main.player[npc.lastInteraction].HasAccessory<SafekeeperRing>())
			return true;

		npc.NPCLoot();
		BurnAway(npc);

		npc.life = 1;
		npc.dontTakeDamage = true;
		npc.netUpdate = true;

		decayTime -= DecayRate;

		if (Main.netMode != NetmodeID.SinglePlayer)
			new BurnUndeadData((byte)npc.whoAmI).Send();

		return false;
	}

	/// <summary> Visual and sound effects for burning away. </summary>
	public static void BurnAway(NPC npc)
	{
		if (Main.dedServ)
			return;

		SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/NPCDeath/Fire_1") with { Pitch = .25f, PitchVariance = .2f }, npc.Center);
		SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Projectile/Explosion_Liquid") with { Pitch = .8f, PitchVariance = .2f }, npc.Center);

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

	public override void PostAI(NPC npc)
	{
		if (decayTime == 1)
			return;

		if ((decayTime -= DecayRate) <= 0)
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

		ToDraw.Add(npc);
		return false;
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		binaryWriter.Write(decayTime);
		binaryWriter.Write(npc.dontTakeDamage); //dontTakeDamage isn't normally synced, so sync it here
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		decayTime = binaryReader.ReadSingle();
		npc.dontTakeDamage = binaryReader.ReadBoolean();
	}
}

internal class BurnUndeadData : PacketData
{
	private readonly byte _npcIndex;

	public BurnUndeadData() { }
	public BurnUndeadData(byte npcIndex) => _npcIndex = npcIndex;

	public override void OnReceive(BinaryReader reader, int whoAmI)
	{
		byte index = reader.ReadByte();

		if (Main.netMode == NetmodeID.Server)
		{
			new BurnUndeadData(index).Send();
			return;
		}

		UndeadNPC.BurnAway(Main.npc[index]);
	}

	public override void OnSend(ModPacket modPacket) => modPacket.Write(_npcIndex);
}
