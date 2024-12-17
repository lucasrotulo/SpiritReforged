using SpiritReforged.Common.PrimitiveRendering.PrimitiveShape;
using SpiritReforged.Common.PrimitiveRendering;
using SpiritReforged.Common.ProjectileCommon;
using System.IO;
using static Terraria.Player;
using static SpiritReforged.Common.Easing.EaseFunction;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria.Audio;
using SpiritReforged.Common.Visuals.Glowmasks;
namespace SpiritReforged.Content.Desert.Scarabeus.Items.Projectiles;

[AutoloadGlowmask("255,255,255", false)]
public class AdornedBowHeld() : BaseChargeBow(2, 1.5f, 30)
{
	public override void SetStringDrawParams(out float stringLength, out float maxDrawback, out Vector2 stringOrigin, out Color stringColor)
	{
		stringLength = 30;
		maxDrawback = 10;
		stringOrigin = new Vector2(5, 25);
		stringColor = new Color(255, 234, 93);
	}

	protected override int? OverrideProjectile(bool fullCharge, bool perfectShot)
	{
		if (fullCharge)
			return ModContent.ProjectileType<AdornedArrow>();

		return null;
	}

	public override void PostDraw(Color lightColor) => base.PostDraw(lightColor);
}