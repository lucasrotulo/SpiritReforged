using SpiritReforged.Common.Visuals.Glowmasks;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Underground.Railgun;

[AutoloadGlowmask("200,200,200")]
public class ZiplineGun : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 44;
		Item.height = 48;
		Item.useTime = 30;
		Item.useAnimation = 30;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.noMelee = true;
		Item.value = Item.sellPrice(0, 1, 0, 0);
		Item.rare = ItemRarityID.Green;
		Item.autoReuse = false;
		Item.shoot = ModContent.ProjectileType<Zipline>();
		Item.shootSpeed = 8f;
	}

	public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

	public override bool AltFunctionUse(Player player) => true;

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 40f;
		if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
			position += muzzleOffset;

		if (player.ownedProjectileCounts[type] > 0)
		{
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.ModProjectile is not Zipline zipline)
					continue;

				if (proj.owner == player.whoAmI && proj.active && proj.type == type && player.altFunctionUse == 2 == zipline.Right)
				{
					proj.Kill();

					if (zipline.isHovering)
						return false;
					break;
				}
			}
		}

		SoundEngine.PlaySound(SoundID.Item102, position);

		for (int i = 0; i < 8; i++)
			Dust.NewDustPerfect(position, player.altFunctionUse == 2 ? DustID.FireworkFountain_Yellow : DustID.Electric, (velocity * Main.rand.NextFloat()).RotatedByRandom(1f), 0, default, .5f).noGravity = true;

		Projectile.NewProjectile(source, position + velocity, velocity, type, 0, 0, player.whoAmI, player.altFunctionUse == 2 ? 1 : 0);

		return false;
	}
}