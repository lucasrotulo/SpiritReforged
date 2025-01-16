﻿namespace SpiritReforged.Common.Visuals.CustomText;

internal class WavyText : CustomText
{
	private int _strength, _length;

	public override string Key => "wavy";

	public override bool ParseParams(string parameters)
	{
		_strength = 12;
		_length = 200;

		if (parameters is null)
			return false;

		string[] values = parameters.Split(',');
		int strength = 0, length = 0;

		for (int i = 0; i < values.Length; i++)
		{
			if (!int.TryParse(values[i], out int type))
				return false;

			if (i == 0)
				strength = type;
			else if (i == 1)
				length = type;
			else
				break;
		}

		_strength = strength;
		_length = length;

		return true;
	}

	public override void Draw(Rectangle panel, string[] text, int numLines)
	{
		var color = Main.MouseTextColorReal;

		if (Main.SettingsEnabled_OpaqueBoxBehindTooltips)
		{
			color = Color.Lerp(color, Color.White, 1f);
			Utils.DrawInvBG(Main.spriteBatch, panel, new Color(23, 25, 81, 255) * 0.925f * 0.85f);
		}

		var textPosition = new Vector2(panel.X + 10, panel.Y + 5);

		var effect = AssetLoader.LoadedShaders["Ripple"];
		effect.Parameters["progress"].SetValue((float)(Main.timeForVisualEffects / 10f % MathHelper.TwoPi));
		effect.Parameters["strength"].SetValue(.001f * _strength);
		effect.Parameters["length"].SetValue(.001f * _length);

		effect.CurrentTechnique.Passes[0].Apply();

		for (int line = 0; line < numLines; line++)
			Utils.DrawBorderStringFourWay(Main.spriteBatch, FontAssets.MouseText.Value, text[line], textPosition.X, textPosition.Y + line * 30, color, Color.Black, Vector2.Zero);
	}
}
