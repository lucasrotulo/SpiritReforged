using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace SpiritReforged.Content.Ocean.NPCs;

[AutoloadCritter]
public class TinyCrab : ModNPC
{
	public override void SetStaticDefaults() => Main.npcFrameCount[NPC.type] = 4;

	public override void SetDefaults()
	{
		NPC.dontCountMe = true;
		NPC.width = 18;
		NPC.height = 18;
		NPC.damage = 0;
		NPC.defense = 0;
		NPC.lifeMax = 5;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = .45f;
		NPC.aiStyle = 67;
		NPC.npcSlots = 0;
		NPC.alpha = 255;
		AIType = NPCID.Bunny;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Ocean");

	public override void AI() => NPC.alpha = Math.Max(NPC.alpha - 5, 0); //Fade in

	public override void FindFrame(int frameHeight)
	{
		NPC.frameCounter += 0.15f;
		NPC.frameCounter %= Main.npcFrameCount[NPC.type];
		int frame = (int)NPC.frameCounter;
		NPC.frame.Y = frame * frameHeight;
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
		{
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TinyCrabGore").Type, 1f);
			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TinyCrabGore").Type, Main.rand.NextFloat(.5f, .7f));
		}
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		var config = ModContent.GetInstance<Common.ConfigurationCommon.ReforgedServerConfig>();
		if (!config.VentCritters)
			return 0;

		return spawnInfo.Water && NPC.CountNPCS(Type) < 10 && Tiles.VentSystem.GetValidPoints(spawnInfo.Player).Count > 0 ? .25f : 0;
	}

	public override int SpawnNPC(int tileX, int tileY)
	{
		int index = NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), tileX, tileY, Type);
		var points = Tiles.VentSystem.GetValidPoints(Main.player[Main.npc[index].FindClosestPlayer()]);

		//Select a random vent position relative to the player
		Main.npc[index].position = points[Main.rand.Next(points.Count)].ToVector2() * 16;

		return index;
	}
}