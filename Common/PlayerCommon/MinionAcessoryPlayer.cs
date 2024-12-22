using SpiritReforged.Common.ItemCommon;
using Terraria;
using Terraria.ID;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpiritReforged.Common.PlayerCommon;

/// <summary> 
/// Uses the dictionary MinionDataByItemID to spawn all equipped minion accessories onto the player in one place.
/// </summary>
public class MinionAccessoryPlayer : ModPlayer
{
	public static Dictionary<int, MinionAccessoryData> MinionDataByItemId = new();

	// spawn all projectile types in the dictionary with their respective damages
	public override void PostUpdateEquips()
	{
		foreach(var projData in MinionDataByItemId)
		{
			int itemType = projData.Key;
			int projType = projData.Value.ProjType;
			int projDamage = projData.Value.Damage;

			if (Player.HasAccessory(itemType) && Player.ownedProjectileCounts[projType] < 1)
				Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), Player.Center, Vector2.Zero, projType, (int)Player.GetDamage(DamageClass.Summon).ApplyTo(projDamage), 0f, Player.whoAmI);
		}
	}
}
