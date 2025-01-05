﻿using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Map;
using Terraria.UI;

namespace SpiritReforged.Common.ItemCommon.Pins;

internal class PinMapLayer : ModMapLayer
{
	public static Dictionary<string, Asset<Texture2D>> Textures = null;

	private string _heldPinName;
	private float _heldOffset;

	/// <summary> Holds a pin of the given name on the cursor. </summary>
	/// <returns> Whether a pin of the given name exists. </returns>
	public static bool HoldPin(string name)
	{
		var pins = ModContent.GetInstance<PinSystem>().pins;

		if (pins.ContainsKey(name))
		{
			ModContent.GetInstance<PinMapLayer>()._heldPinName = name;
			return true;
		}
		else
			return false;
	}

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		var pins = ModContent.GetInstance<PinSystem>().pins;

		foreach (var pair in pins)
		{
			string name = pair.Key;
			var position = pins.Get<Vector2>(name);

			DrawPin(ref context, ref text, name, position);
		}
	}

	private void DrawPin(ref MapOverlayDrawContext context, ref string text, string name, Vector2 position)
	{
		float scale = 1;
		UpdatePin(name, ref position, ref scale, ref text); //Adjusts position and scale of held pins

		if (context.Draw(Textures[name].Value, position, Color.White, new SpriteFrame(1, 1, 0, 0), scale, scale, Alignment.Center).IsMouseOver)
		{
			if (!Main.mapFullscreen)
				return;

			UpdatePin(name, ref position, ref scale, ref text, true);
		}
	}

	private void UpdatePin(string name, ref Vector2 position, ref float scale, ref string text, bool hovering = false)
	{
		bool holdingThisPin = _heldPinName == name;

		if (holdingThisPin)
		{
			position = GetCursor();
			_heldOffset = MathHelper.Min(_heldOffset + .1f, 1);
			scale = 1 + _heldOffset * .15f;
		}

		if (hovering)
		{
			if (!holdingThisPin)
				text = Language.GetTextValue("Mods.SpiritReforged.Misc.Pins.Move");

			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				if (holdingThisPin)
				{
					_heldPinName = string.Empty;
					_heldOffset = 0;
					PinSystem.Place(name, GetCursor());

					SoundEngine.PlaySound(new SoundStyle("SpiritReforged/Assets/SFX/Item/MapPin") with { PitchVariance = 0.3f });
				}
				else
					_heldPinName = name;
			}

			if (Main.mouseRight)
				PinSystem.Remove(name);
		}

		Vector2 GetCursor()
		{
			var cursorPos = Main.MouseScreen - Main.ScreenSize.ToVector2() / 2;
			cursorPos = (cursorPos - new Vector2(0, _heldOffset * 5)) * (1 / Main.mapFullscreenScale) + Main.mapFullscreenPos;

			return cursorPos;
		}
	}
}
