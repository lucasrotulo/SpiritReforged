﻿using SpiritReforged.Common.NPCCommon;
using SpiritReforged.Content.Savanna.Tiles;
using Terraria.GameContent.Personalities;

namespace SpiritReforged.Content.Savanna.Biome;

public class SavannaBiome : ModBiome
{
	private int GetMusic()
	{
		if (Main.LocalPlayer.townNPCs > 2f || Main.LocalPlayer.ZoneGraveyard || Main.bloodMoon)
			return -1;

		return Main.dayTime ? MusicLoader.GetMusicSlot(Mod, "Assets/Music/Savanna") : MusicLoader.GetMusicSlot(Mod, "Assets/Music/SavannaNight");
	}

	public override void SetStaticDefaults() => NPCHappinessHelper.SetAverage<SavannaBiome>(ModContent.GetInstance<JungleBiome>(), ModContent.GetInstance<DesertBiome>());

	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
	public override int Music => GetMusic();
	public override ModWaterStyle WaterStyle => ModContent.GetInstance<SavannaWaterStyle>();
	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<SavannaBGStyle>();
	public override string BackgroundPath => MapBackground;
	public override string MapBackground => "SpiritReforged/Assets/Textures/Backgrounds/SavannaMapBG";

	public override bool IsBiomeActive(Player player)
	{
		bool surface = player.ZoneSkyHeight || player.ZoneOverworldHeight;
		return SavannaTileCounts.InSavanna && surface;
	}
}

internal class SavannaTileCounts : ModSystem
{
	internal static int[] SavannaTypes;
	public int savannaCount;

	public static bool InSavanna => ModContent.GetInstance<SavannaTileCounts>().savannaCount >= 400;

	public override void SetStaticDefaults() => SavannaTypes = [ModContent.TileType<SavannaGrass>(), ModContent.TileType<SavannaGrassCorrupt>(), ModContent.TileType<SavannaGrassCrimson>(), ModContent.TileType<SavannaGrassHallow>(), ModContent.TileType<SavannaDirt>()];

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
	{
		savannaCount = 0;

		foreach (int type in SavannaTypes)
			savannaCount += tileCounts[type];
	}
}
