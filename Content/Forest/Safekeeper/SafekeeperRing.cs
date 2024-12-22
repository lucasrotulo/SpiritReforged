using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Common.Particle;
using SpiritReforged.Common.PlayerCommon;
using SpiritReforged.Content.Particles;
using Terraria.Audio;

namespace SpiritReforged.Content.Forest.Safekeeper;

//[AutoloadEquip(EquipType.HandsOn)]
public class SafekeeperRing : AccessoryItem
{
	public override void SetDefaults()
	{
		Item.width = Item.height = 12;
		Item.value = Item.buyPrice(gold: 2, silver: 50);
		Item.rare = ItemRarityID.Green;
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		base.UpdateAccessory(player, hideVisual);

		float mult = 0;
		foreach (var npc in Main.ActiveNPCs)
		{
			if (UndeadNPC.IsUndeadType(npc.type))
				mult = MathHelper.Max(mult, 1f - npc.Distance(player.Center) / 1200f);
		}

		if (mult > 0)
			EmitLight(player, mult, hideVisual);
	}

	private static void EmitLight(Player player, float strength, bool hideVisual)
	{
		var color = Color.LightGoldenrodYellow.ToVector3() * .34f * strength;
		Lighting.AddLight(player.Center, color.X, color.Y, color.Z);

		if (hideVisual || strength <= .8f)
			return;

		if (Main.rand.NextBool(30))
		{
			var position = Main.rand.NextVector2FromRectangle(player.getRect());
			var newCol = Color.Lerp(Color.Gold, Color.Orange, Main.rand.NextFloat());

			ParticleHandler.SpawnParticle(new GlowParticle(position, Vector2.UnitY * -Main.rand.NextFloat(.5f, 1f), newCol, Main.rand.NextFloat(.2f, .4f), 60, 20));
		}

		if (Main.rand.NextBool(12))
		{
			var rect = player.getRect();
			rect.Inflate(50, 50);

			var position = Main.rand.NextVector2FromRectangle(rect);
			var newCol = Color.Lerp(Color.Gold, Color.Orange, Main.rand.NextFloat());

			ParticleHandler.SpawnParticle(new GlowParticle(position, Vector2.UnitY * -Main.rand.NextFloat(.5f), newCol, Main.rand.NextFloat(.1f, .2f), 80, 5));
		}
	}
}

public class UndeadModPlayer : ModPlayer
{
	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		if (Player.HasAccessory<SafekeeperRing>() && UndeadNPC.IsUndeadType(target.type))
			modifiers.FinalDamage *= 1.25f;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		const int particleTime = 24;

		if (!Main.dedServ && Player.HasAccessory<SafekeeperRing>() && UndeadNPC.IsUndeadType(target.type) && target.life < MathHelper.Min(100, target.lifeMax * .25f))
		{
			var points = new Vector2[Main.rand.Next(2, 4)];

			for (int i = 0; i < points.Length; i++)
				points[i] = Main.rand.NextVector2FromRectangle(target.getRect());

			for (int i = 0; i < points.Length; i++)
			{
				ParticleHandler.SpawnParticle(new GlowParticle(points[i], Vector2.Zero, Color.White, Color.DarkGoldenrod, .5f, particleTime));

				if (i < points.Length - 1)
					ParticleHandler.SpawnParticle(new LightningParticle(points[i], points[i + 1], Color.Yellow, particleTime, 10f));
			}

			SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Pitch = .5f }, target.Center);
		}
	}
}
