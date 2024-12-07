using Terraria.Graphics.Capture;

namespace SpiritReforged.Content.Savanna.Biome;
public class SavannaBiome : ModBiome
{
	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
	public override int Music => (Main.LocalPlayer.townNPCs > 2f) ? -1 : MusicLoader.GetMusicSlot(Mod, "Assets/Music/Savanna");
	public override ModWaterStyle WaterStyle => ModContent.GetInstance<SavannaWaterStyle>();
	public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<SavannaBGStyle>();
	public override string BestiaryIcon => base.BestiaryIcon;
	public override string BackgroundPath => MapBackground;
	public override Color? BackgroundColor => base.BackgroundColor;
	public override string MapBackground => "SpiritReforged/Assets/Textures/Backgrounds/SavannaMapBG";

	public override bool IsBiomeActive(Player player)
	{
		bool surface = player.ZoneSkyHeight || player.ZoneOverworldHeight;
		return SavannaTileCounts.InSavanna && surface;
	}
}

internal class SavannaTileCounts : ModSystem
{
	public int savannaCount;

	public static bool InSavanna => ModContent.GetInstance<SavannaTileCounts>().savannaCount >= 400;

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) => savannaCount = tileCounts[ModContent.TileType<Tiles.SavannaGrass>()] + tileCounts[ModContent.TileType<Tiles.SavannaDirt>()];
}
