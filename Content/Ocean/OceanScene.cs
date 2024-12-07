using SpiritReforged.Common.Visuals;

namespace SpiritReforged.Content.Ocean;

internal class OceanScene : ModSceneEffect
{
	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

	public override bool IsSceneEffectActive(Player player) => player.ZoneBeach && (!player.GetModPlayer<OceanPlayer>().Submerged(35) || NotInDepths(player));
	private static bool NotInDepths(Player plr) => !ModLoader.TryGetMod("ThoriumMod", out Mod thor) || thor.Call("GetZoneAquaticDepths", plr) is bool depths && !depths;
}
