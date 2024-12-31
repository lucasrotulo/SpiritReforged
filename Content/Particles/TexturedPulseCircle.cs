using SpiritReforged.Common.Easing;
using SpiritReforged.Common.Particle;

namespace SpiritReforged.Content.Particles;

public class TexturedPulseCircle : PulseCircle
{
	private readonly string _texture;
	private readonly float _textureExponent;
	private readonly Vector2 _textureStretch;
	public TexturedPulseCircle(Vector2 position, Color ringColor, Color bloomColor, float ringWidth, float maxRadius, int maxTime, string texture, Vector2 textureStretch, EaseFunction MovementStyle = null, bool inverted = false, float endRingWidth = 0, float textureExponent = 1) : base(position, ringColor, bloomColor, ringWidth, maxRadius, maxTime, MovementStyle, inverted, endRingWidth)
	{
		_texture = texture;
		_textureStretch = textureStretch;
		_textureExponent = textureExponent;
	}

	public TexturedPulseCircle(Entity attatchedEntity, Color ringColor, Color bloomColor, float ringWidth, float maxRadius, int maxTime, string texture, Vector2 textureStretch, EaseFunction MovementStyle = null, Vector2? startingPosition = null, bool inverted = false, float endRingWidth = 0, float textureExponent = 1) : base(attatchedEntity, ringColor, bloomColor, ringWidth, maxRadius, maxTime, MovementStyle, startingPosition, inverted, endRingWidth)
	{
		_texture = texture;
		_textureStretch = textureStretch;
		_textureExponent = textureExponent;
	}

	public TexturedPulseCircle(Vector2 position, Color color, float ringWidth, float maxRadius, int maxTime, string texture, Vector2 textureStretch, EaseFunction MovementStyle = null, bool inverted = false, float endRingWidth = 0, float textureExponent = 1) : this(position, color, color * 0.25f, ringWidth, maxRadius, maxTime, texture, textureStretch, MovementStyle, inverted, endRingWidth) { }

	public TexturedPulseCircle(Entity attatchedEntity, Color color, float ringWidth, float maxRadius, int maxTime, string texture, Vector2 textureStretch, EaseFunction MovementStyle = null, Vector2? startingPosition = null, bool inverted = false, float endRingWidth = 0, float textureExponent = 1) : this(attatchedEntity, color, color * 0.25f, ringWidth, maxRadius, maxTime, texture, textureStretch, MovementStyle, startingPosition, inverted, endRingWidth) { }

	public override ParticleLayer DrawLayer => ParticleLayer.AbovePlayer;

	internal override string EffectPassName => "TexturedStyle";

	internal override void EffectExtras(ref Effect curEffect)
	{
		if(!AssetLoader.LoadedTextures.TryGetValue(_texture, out Texture2D value))
		{
			throw new ArgumentNullException(_texture, "Given input does not correspond to a loaded asset.");
		}

		else
		{
			curEffect.Parameters["uTexture"].SetValue(value);
			curEffect.Parameters["textureStretch"].SetValue(new Vector2(_textureStretch.X, _textureStretch.Y));
			curEffect.Parameters["texExponent"].SetValue(_textureExponent);
			curEffect.Parameters["scroll"].SetValue(Progress / 3);
		}
	}
}
