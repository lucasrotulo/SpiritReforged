using SpiritReforged.Common.Visuals.Glowmasks;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Ocean.Items.JellyfishStaff;

[AutoloadGlowmask("255, 255, 255")]
public class JellyfishStaff : ModItem
{
	public override void SetDefaults()
	{
		Item.width = 34;
		Item.height = 34;
		Item.value = Item.sellPrice(0, 0, 25, 0);
		Item.rare = ItemRarityID.Blue;
		Item.mana = 10;
		Item.damage = 16;
		Item.knockBack = 2.5f;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = 30;
		Item.useAnimation = 30;
		Item.DamageType = DamageClass.Summon;
		Item.noMelee = true;
		Item.shoot = ModContent.ProjectileType<JellyfishMinion>();
		Item.UseSound = SoundID.Item44;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => position = Main.MouseWorld;

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.altFunctionUse != 2;

	public override void Update(ref float gravity, ref float maxFallSpeed) => Lighting.AddLight(Item.position, .224f, .133f, .255f);

	public override void AddRecipes()
	{
		Recipe recipe = CreateRecipe();
		recipe.AddIngredient(ItemID.Coral, 5);
		recipe.AddIngredient(ItemID.Glowstick, 5);
		recipe.AddTile(TileID.Anvils);
		recipe.Register();
	}
}