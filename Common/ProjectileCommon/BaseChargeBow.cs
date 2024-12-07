using SpiritReforged.Common.Easing;
using SpiritReforged.Common.MathHelpers;
using System.IO;
using static Microsoft.Xna.Framework.MathHelper;
using static Terraria.Player;

namespace SpiritReforged.Common.ProjectileCommon;

public abstract class BaseChargeBow(float maxChargePower = 2f, float perfectShotPower = 1.5f, float perfectShotTime = 30) : ModProjectile
{
	protected float Charge { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
	protected float ChargeTime => Projectile.ai[1];  //Set by the item that spawns this projectile, using that item's usetime
	protected float SelectedAmmo => Projectile.ai[2];

	protected readonly float _chargePowerMax = maxChargePower; //The modifier to damage and shot speed when fully charged
	protected readonly float _perfectShotPower = perfectShotPower; //Additional modifier during a perfect shot

	protected float _perfectShotTime = perfectShotTime; //Amount of frames after fully charging that a perfect shot can be performed
	protected bool firing = false;
	protected Vector2 direction = Vector2.Zero;

	public sealed override void SetDefaults()
	{
		Projectile.hostile = false;
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Ranged;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		SafeSetDefaults();
	}

	protected virtual void SafeSetDefaults() { }
	protected virtual void SafeAI() { }

	public sealed override void AI()
	{
		if (!Projectile.TryGetOwner(out Player player))
		{
			Projectile.Kill();
			return;
		}

		SafeAI();
		AdjustDirection();

		player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
		player.heldProj = Projectile.whoAmI;
		player.itemTime = 2;
		player.itemAnimation = 2;
		Projectile.timeLeft = 2;
		Projectile.Center = player.MountedCenter + direction * 10;
		Projectile.velocity = Vector2.Zero;
		Projectile.rotation = direction.ToRotation();

		CompositeArmStretchAmount frontStretch = EaseFunction.EaseCircularOut.Ease(Charge) switch
		{
			< 0.25f => CompositeArmStretchAmount.Full,
			< 0.5f => CompositeArmStretchAmount.ThreeQuarters,
			< 0.75f => CompositeArmStretchAmount.Quarter,
			_ => CompositeArmStretchAmount.None
		};

		player.SetCompositeArmFront(true, frontStretch, player.itemRotation);
		player.SetCompositeArmBack(true, CompositeArmStretchAmount.Full, player.itemRotation);

		if (!firing)
		{
			if (player.channel)
			{
				if (Charge < 1)
					Charge = Min(Charge + 1 / ChargeTime, 1);
				else
					_perfectShotTime = Max(--_perfectShotTime, 0);

				Charging();
			}
			else
			{
				bool perfectShot = Charge == 1 && _perfectShotTime > 0;
				Shoot(perfectShot);
				firing = true;
			}
		}
		else
		{
			//Todo: make the bow projectile last a bit so the string can bounce back after the arrow fires
			Projectile.Kill();
		}
	}

	protected virtual void Shoot(bool perfectShot)
	{
		Projectile.TryGetOwner(out Player player);
		Item playerWeapon = player.HeldItem;
		float chargeMod = Lerp(1, _chargePowerMax, Charge);
		if (perfectShot)
			chargeMod *= _perfectShotPower;

		if (player.PickAmmo(playerWeapon, out int ammoID, out float speed, out int damage, out float knockBack, out int dummy))
		{
			speed *= chargeMod;
			damage = (int)(damage * chargeMod);
			knockBack *= chargeMod;

			Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, direction * speed, ammoID, damage, knockBack, Projectile.owner);
		}
		else
			Projectile.Kill();
	}

	protected virtual void Charging() => AdjustDirection();

	public override bool? CanDamage() => false;

	public override void SendExtraAI(BinaryWriter writer) => writer.WriteVector2(direction);

	public override void ReceiveExtraAI(BinaryReader reader) => direction = reader.ReadVector2();

	protected void AdjustDirection(float deviation = 0f)
	{
		Player player = Main.player[Projectile.owner];
		if (Main.myPlayer == player.whoAmI)
		{
			direction = Main.MouseWorld - (player.Center - new Vector2(4, 4));
			direction.Normalize();
			direction = direction.RotatedBy(deviation);
			Projectile.netUpdate = true;
		}

		player.itemRotation = direction.ToRotation();
		if (player.direction != 1)
			player.itemRotation -= 3.14f;

		player.itemRotation = WrapAngle(player.itemRotation) - player.direction * PiOver2;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D projTex = TextureAssets.Projectile[Type].Value;

		//Draw string
		int stringHalfLength = 15;
		int maxDrawback = 10;
		var stringOrigin = new Vector2(5, 25);
		float stringScale = 2;
		var stringColor = new Color(255, 234, 93).MultiplyRGB(lightColor);
		int splineIterations = 20;

		var pointTop = new Vector2(stringOrigin.X, stringOrigin.Y - stringHalfLength);
		var pointMiddle = new Vector2(stringOrigin.X - (maxDrawback * EaseFunction.EaseCircularOut.Ease(Charge)), stringOrigin.Y);
		var pointBottom = new Vector2(stringOrigin.X, stringOrigin.Y + stringHalfLength);
		Vector2[] spline = Spline.CreateSpline([pointTop, pointMiddle, pointBottom], splineIterations);
		for (int i = 0; i < splineIterations; i++)
		{
			var pixelPos = spline[i];

			pixelPos = pixelPos.RotatedBy(Projectile.rotation);
			pixelPos -= (projTex.Size() / 2).RotatedBy(Projectile.rotation);
			pixelPos += Projectile.Center - Main.screenPosition;

			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, pixelPos, new Rectangle(0, 0, 1, 1), stringColor, Projectile.rotation, Vector2.Zero, stringScale, SpriteEffects.None, 0);
		}

		//Draw arrow
		Texture2D arrowTex = TextureAssets.Projectile[(int)SelectedAmmo].Value;
		Vector2 arrowPos = pointMiddle.RotatedBy(Projectile.rotation);
		arrowPos -= (projTex.Size() / 2).RotatedBy(Projectile.rotation);
		arrowPos += Projectile.Center - Main.screenPosition;
		var arrowOrigin = new Vector2(arrowTex.Width / 2, arrowTex.Height);

		Main.spriteBatch.Draw(arrowTex, arrowPos, null, lightColor, Projectile.rotation + PiOver2, arrowOrigin, Projectile.scale, SpriteEffects.None, 0);

		//Draw proj
		Projectile.QuickDraw();

		return false;
	}
}
