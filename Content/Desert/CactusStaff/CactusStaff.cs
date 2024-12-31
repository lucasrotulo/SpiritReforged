using System.Linq;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Desert.CactusStaff;

public class CactusStaff : ModItem
{
	public override void SetDefaults()
	{
		Item.damage = 6;
		Item.DamageType = DamageClass.Magic;
		Item.mana = 7;
		Item.width = 38;
		Item.height = 42;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.noMelee = true;
		Item.knockBack = 4f;
		Item.value = 200;
		Item.rare = ItemRarityID.Blue;
		Item.UseSound = SoundID.Item8;
		Item.shoot = ModContent.ProjectileType<CactusWallProj>();
		Item.shootSpeed = 8f;
		Item.autoReuse = false;
	}

	public override bool CanUseItem(Player player) => !Collision.SolidCollision(Main.MouseWorld, 16, 16);

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		const int maxDepth = 80;

		velocity = Vector2.Zero;
		position = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates(8, 4);

		for (int i = 0; i < maxDepth; i++)
		{
			position.Y += 8;

			if (WorldGen.SolidTile(position.ToTileCoordinates()) || Main.tileSolidTop[Framing.GetTileSafely(position.ToTileCoordinates()).TileType])
				break;

			if (i == maxDepth - 1)
				return false; //No solid ground was found within maxDepth
		}

		position.Y -= 32;
		Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, player.ownedProjectileCounts[Item.shoot]);

		if (player.ownedProjectileCounts[Item.shoot] >= 4) //4 Cactus walls max
		{
			var oldest = Main.projectile.Where(x => x.active && x.owner == player.whoAmI && x.type == Item.shoot).OrderBy(x => x.timeLeft).FirstOrDefault();

			if (oldest != default)
				oldest.Kill();
		}

		return false;
	}

	public override void AddRecipes() => CreateRecipe()
			.AddIngredient(ItemID.Cactus, 12)
			.AddIngredient(ItemID.FallenStar, 1)
			.AddTile(TileID.WorkBenches)
			.Register();
}
