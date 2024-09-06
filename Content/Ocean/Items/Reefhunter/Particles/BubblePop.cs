using SpiritReforged.Common.Easing;
using SpiritReforged.Common.Particle;

namespace SpiritReforged.Content.Ocean.Items.Reefhunter.Particles;

public class BubblePop : Particle
{
	private const int NUMFRAMES = 8;
	private int _maxTime;
	private float _opacity;

	public BubblePop(Vector2 position, float scale, float opacity, int animationTime)
	{
		Position = position;
		Scale = scale;
		_opacity = opacity;
		_maxTime = animationTime;
	}

	public override ParticleDrawType DrawType => ParticleDrawType.CustomNonPremultiplied;

	public override void Update()
	{
		if (TimeActive > _maxTime)
			Kill();
	}

	public override void CustomDraw(SpriteBatch spriteBatch)
	{
		Texture2D drawTex = ParticleHandler.GetTexture(Type);
		float frameProgress = (float)TimeActive / _maxTime;
		int frameNumber = (int)Math.Floor((double)(frameProgress * NUMFRAMES));
		int frameHeight = drawTex.Height / NUMFRAMES;
		var drawFrame = new Rectangle(0, frameNumber * frameHeight, drawTex.Width, frameHeight);

		Color lightColor = Lighting.GetColor(Position.ToTileCoordinates().X, Position.ToTileCoordinates().Y);

		spriteBatch.Draw(drawTex, Position - Main.screenPosition, drawFrame, lightColor * _opacity, 0, drawFrame.Size() / 2, Scale, SpriteEffects.None, 0);
	}
}
