using Terraria.DataStructures;

namespace SpiritReforged.Content.Jungle.Toucane;

public class ToucaneItem : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 46;
		Item.height = 54;
		Item.damage = 9;
		Item.value = Item.sellPrice(0, 2, 0, 0);
		Item.rare = ItemRarityID.Green;
		Item.mana = 12;
		Item.knockBack = 3f;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 30;
		Item.useAnimation = 30;
		Item.DamageType = DamageClass.Summon;
		Item.noMelee = true;
		Item.shoot = ModContent.ProjectileType<ToucanMinion>();
		Item.UseSound = SoundID.Item44;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		position = Main.MouseWorld;
		Projectile.NewProjectile(source, position, Main.rand.NextVector2Circular(3, 3), type, damage, knockback, player.whoAmI);
		return false;
	}
}