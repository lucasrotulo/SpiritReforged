using SpiritReforged.Common.Particle;

namespace SpiritReforged.Content.Ocean.Items.Reefhunter.Particles;

public class BubblePop : Particle
{
	private const int NUMFRAMES = 8;
	private readonly float _opacity;

	public BubblePop(Vector2 position, float scale, float opacity, int animationTime, float rotation = 0f)
	{
		Position = position;
		Scale = scale;
		_opacity = opacity;
		MaxTime = animationTime;
		Rotation = rotation;
	}

	public override ParticleDrawType DrawType => ParticleDrawType.CustomBatchedAdditiveBlend;

	public override void CustomDraw(SpriteBatch spriteBatch)
	{
		Texture2D drawTex = ParticleHandler.GetTexture(Type);
		int frameNumber = (int)Math.Floor((double)(Progress * NUMFRAMES));
		int frameHeight = drawTex.Height / NUMFRAMES;
		var drawFrame = new Rectangle(0, frameNumber * frameHeight, drawTex.Width, frameHeight);

		Color lightColor = Lighting.GetColor(Position.ToTileCoordinates().X, Position.ToTileCoordinates().Y);

		spriteBatch.Draw(drawTex, Position - Main.screenPosition, drawFrame, lightColor * _opacity, Rotation, drawFrame.Size() / 2, Scale, SpriteEffects.None, 0);
	}
}
