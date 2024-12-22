using SpiritReforged.Common.Easing;
using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Ocean.Items.Reefhunter.Particles;
using SpiritReforged.Content.Ocean.Items.Reefhunter.Projectiles;
using SpiritReforged.Content.Particles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace SpiritReforged.Content.Ocean.Items.Reefhunter;

public class ClawCannon : ModItem
{
	public override void SetDefaults()
	{
		Item.damage = 18;
		Item.width = 38;
		Item.height = 26;
		Item.useTime = Item.useAnimation = 30;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.knockBack = 4;
		Item.value = Item.sellPrice(0, 0, 5, 0);
		Item.rare = ItemRarityID.Blue;
		Item.crit = 6;
		Item.autoReuse = true;
		Item.noMelee = true;
		Item.DamageType = DamageClass.Ranged;
		Item.shootSpeed = 15f;
		Item.UseSound = SoundID.Item85;
		Item.shoot = ModContent.ProjectileType<Cannonbubble>();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (!Main.dedServ)
		{
			SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Item/Woosh_1") with { PitchVariance = 0.4f, Pitch = -1.5f, Volume = 1.2f, MaxInstances = 3 }, player.Center);

			PulseCircle[] pulseCircles =
			[
				new TexturedPulseCircle(position + velocity, Cannonbubble.RINGCOLOR * 2, Cannonbubble.RINGCOLOR * 2, 1, 40, 25, "LiquidTrail", new Vector2(1, 0.5f), EaseFunction.EaseCircularOut, false, 0.6f),
                new TexturedPulseCircle(position + velocity * 1.5f, Cannonbubble.RINGCOLOR * 2, Cannonbubble.RINGCOLOR * 2, 1, 60, 30, "LiquidTrail", new Vector2(1, 0.5f), EaseFunction.EaseCircularOut, false, 0.6f),
			];

			for(int i = 0; i < pulseCircles.Length; i++)
			{
				pulseCircles[i].Velocity = 0.75f * Vector2.Normalize(velocity) / (1 + 3*i);
				ParticleHandler.SpawnParticle(pulseCircles[i].WithSkew(0.7f, velocity.ToRotation()).UsesLightColor());
			}

			for (int i = 0; i < 4; ++i)
				ParticleHandler.SpawnParticle(new BubbleParticle(position + velocity + player.velocity / 2, Vector2.Normalize(velocity).RotatedByRandom(MathHelper.Pi / 6) * Main.rand.NextFloat(2f, 4), Main.rand.NextFloat(0.4f, 0.7f), 40));
		}

		return true;
	}

	public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

	public override void AddRecipes()
	{
		var recipe = CreateRecipe();
		recipe.AddIngredient(ModContent.ItemType<IridescentScale>(), 6);
		recipe.AddIngredient(ModContent.ItemType<MineralSlag>(), 14);
		recipe.AddTile(TileID.Anvils);
		recipe.Register();
	}
}
