using SpiritReforged.Common.Particle;
using SpiritReforged.Content.Particles;

namespace SpiritReforged.Content.Ocean.Hydrothermal;

public class FireParticleScreen : ScreenParticle
{
	public override ParticleDrawType DrawType => ParticleDrawType.CustomBatchedAdditiveBlend;

	public override void UpdateOnScreen()
	{
		if (TimeActive >= MaxTime)
			Kill();

		Velocity.X += Main.windSpeedCurrent / 15;
		Velocity.X *= 0.99f;
		Velocity.Y *= 1.0035f;
		Velocity = Velocity.RotatedByRandom(0.1f);

		Color = Color.Lerp(new Color(246, 255, 0), new Color(232, 37, 2), 
			(float)TimeActive / MaxTime) * (float)Math.Sin(MathHelper.Pi * (TimeActive / (float)MaxTime)) * 0.85f * ActiveOpacity;
	}

	public override void CustomDraw(SpriteBatch spriteBatch)
	{
		Texture2D bloom = AssetLoader.LoadedTextures["Bloom"];

		spriteBatch.Draw(bloom, GetDrawPosition(), null, Color * 0.6f, 0, bloom.Size() / 2, Scale * Main.GameViewMatrix.Zoom / 5f, SpriteEffects.None, 0);
		spriteBatch.Draw(Texture, GetDrawPosition(), null, Color, Velocity.ToRotation(), Texture.Size() / 2, Scale * Main.GameViewMatrix.Zoom * .75f, SpriteEffects.None, 0);
	}
}