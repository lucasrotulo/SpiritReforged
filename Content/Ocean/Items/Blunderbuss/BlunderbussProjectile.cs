using System.Linq;

namespace SpiritReforged.Content.Ocean.Items.Blunderbuss;

internal class BlunderbussProjectile : GlobalProjectile
{
	private static readonly Dictionary<Texture2D, Color> colorCache = [];

	public const int timeLeftMax = 25;
	public bool firedFromBlunderbuss;

	public override bool InstancePerEntity => true;
	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => lateInstantiation && !entity.arrow;

	public override bool PreDraw(Projectile projectile, ref Color lightColor)
	{
		const int trailLength = 5;

		if (!firedFromBlunderbuss)
			return true;

		Main.instance.LoadProjectile(873); //Ensure these textures are loaded before drawing
		Main.instance.LoadProjectile(686);

		var defaultTexture = TextureAssets.Projectile[projectile.type].Value;
		float time = MathHelper.Min((float)projectile.timeLeft / timeLeftMax, 1f);

		for (int i = 0; i < trailLength; i++)
		{
			var texture = TextureAssets.Projectile[873].Value;

			float lerp = 1f - i / (float)(trailLength - 1);
			var color = (Color.Lerp(GetBrightestColor(defaultTexture).MultiplyRGBA(Color.Black * .5f), GetBrightestColor(defaultTexture), lerp) with { A = 0 }) * lerp;
			var position = projectile.Center - Main.screenPosition - projectile.velocity * i * (1f - time);
			var scale = new Vector2(time, 1f) * projectile.scale;

			if (i == 0)
			{
				color = Color.White with { A = 0 };
				texture = TextureAssets.Projectile[686].Value;
				scale = new Vector2(MathHelper.Max(time, .25f), 1f) * projectile.scale * .45f;
			}

			Main.EntitySpriteDraw(texture, position, null, color, projectile.rotation, texture.Size() / 2, scale, SpriteEffects.None);
		}

		return false;
	}

	public override bool PreKill(Projectile projectile, int timeLeft)
	{
		if (!firedFromBlunderbuss)
			return true;

		return timeLeft > 0; //prevent on-kill effects (such as SoundID.Dig) when visibly faded out
	}

	private static Color GetBrightestColor(Texture2D texture)
	{
		if (colorCache.TryGetValue(texture, out Color value))
			return value;

		var data = new Color[texture.Width * texture.Height];
		texture.GetData(data);
		var brightest = data.OrderBy(x => x.ToVector3().Length()).FirstOrDefault();

		return (brightest == default) ? Color.Goldenrod : brightest;
	}
}
