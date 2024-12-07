using SpiritReforged.Common.Visuals;

namespace SpiritReforged.Content.Ocean;

internal class DeepOceanScene : ModSceneEffect
{
	public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/DeepOcean");
	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
	public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => Mod.Find<ModSurfaceBackgroundStyle>("DeepOceanBackgroundStyle");

	public override bool IsSceneEffectActive(Player player) => player.ZoneBeach && player.GetModPlayer<OceanPlayer>().Submerged(30) && NotInDepths(player);
	private static bool NotInDepths(Player plr) => !ModLoader.TryGetMod("ThoriumMod", out Mod thor) || thor.Call("GetZoneAquaticDepths", plr) is bool depths && !depths;
}
