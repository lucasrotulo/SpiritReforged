using SpiritReforged.Common.PlayerCommon;
using System.Linq;

namespace SpiritReforged.Content.Underground.ExplorerTreads;

internal class ExplorerTreadsPlayer : ModPlayer
{
	public override bool CanBeHitByProjectile(Projectile proj)
	{
		int[] trapTypes = [ProjectileID.PoisonDart, ProjectileID.PoisonDartTrap, ProjectileID.SporeTrap, ProjectileID.SporeTrap2, ProjectileID.SpearTrap, ProjectileID.GeyserTrap, ProjectileID.FlamethrowerTrap, ProjectileID.FlamesTrap, ProjectileID.SpikyBallTrap, ProjectileID.RollingCactus, ProjectileID.RollingCactusSpike, ProjectileID.Boulder];
		if (Player.HasAccessory<ExplorerTreads>() && trapTypes.Contains(proj.type))
		{
			if (ExplorerTreads.DoDodgeEffect(Player, Player.GetSource_OnHurt(proj)))
				return false;
		}

		return true;
	}

	public override bool FreeDodge(Player.HurtInfo info)
	{
		if (Player.HasAccessory<ExplorerTreads>() && info.DamageSource.SourceOtherIndex == 3) //Spikes
		{
			if (ExplorerTreads.DoDodgeEffect(Player, Player.GetSource_OnHurt(info.DamageSource)))
				return true;
		}

		return false;
	}
}