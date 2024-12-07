using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;

namespace SpiritReforged.Common.Visuals;

// Todo: Needs better water slopes?

/// <summary>
/// Handles transparency edits for <see cref="LiquidRenderer.DrawNormalLiquids(SpriteBatch, Vector2, int, float, bool)"/>,
/// <see cref="Main.DrawBlack(bool)"/> (my beloathed), <see cref="TileDrawing"/>.DrawPartialLiquid, and 
/// <see cref="Lighting.GetCornerColors(int, int, out VertexColors, float)"/>.<br/>
/// 
/// Allows transparency functionality everywhere.
/// </summary>
internal class WaterEdits : ModSystem
{
	public bool DrawingLiquid = false;

	public override void Load()
	{
		On_LiquidRenderer.DrawNormalLiquids += CheckLiquid;
		On_Lighting.GetCornerColors += HijackLiquidSlopeColoring;
		On_TileDrawing.DrawPartialLiquid += FixSlopes;

		IL_Main.DrawBlack += HijackDrawBlack;
	}

	private void HijackDrawBlack(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchLdsfld<Main>(nameof(Main.spriteBatch))))
			return;

		if (!c.TryGotoPrev(x => x.MatchLdloc(out int _)))
			return;

		if (!c.TryGotoPrev(x => x.MatchLdloc(out int _)))
			return;

		c.Index++;

		c.Emit(OpCodes.Pop); // Remove old local (13) value off stack, modify them in ModifyEdges, then push 13 on the stack again
		c.Emit(OpCodes.Ldloca_S, (byte)13);
		c.Emit(OpCodes.Ldloc_S, (byte)11);
		c.Emit(OpCodes.Ldloca_S, (byte)14);
		c.EmitDelegate(ModifyEdges);
		c.Emit(OpCodes.Ldloc_S, (byte)13);
	}

	public static void ModifyEdges(ref int left, int y, ref int drawPosX)
	{
		if (Lighting.Brightness(left, y) < 0.1f)
			return;

		if (left > drawPosX)
			left--;

		if (left - drawPosX > 0)
			drawPosX++;
	}

	private void FixSlopes(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behind, Tile tileCache, ref Vector2 pos,
		ref Rectangle size, int liquidType, ref VertexColors colors)
	{
		DrawingLiquid = true;

		ModifyVertexColors(ref colors, 0.8f);

		orig(self, behind, tileCache, ref pos, ref size, liquidType, ref colors);
		DrawingLiquid = false;
	}

	private void HijackLiquidSlopeColoring(On_Lighting.orig_GetCornerColors orig, int centerX, int centerY, out VertexColors vertices, float scale)
	{
		orig(centerX, centerY, out vertices, scale);

		if (DrawingLiquid)
			ModifyVertexColors(ref vertices);
	}

	private static void ModifyVertexColors(ref VertexColors vertices, float opacity = 1f)
	{
		ModifyColor(ref vertices.TopLeftColor, opacity);
		ModifyColor(ref vertices.TopRightColor, opacity);
		ModifyColor(ref vertices.BottomLeftColor, opacity);
		ModifyColor(ref vertices.BottomRightColor, opacity);
	}

	private static void ModifyColor(ref Color color, float opacity = 1f)
	{
		float luminance = Luminance(color);
		color = Color.Lerp(color, new Color(8, 8, 80) * 0.6f * opacity, 1 - luminance);

		if (luminance < 0.2f)
			color = Color.Lerp(color, Color.Black, 1 - luminance / 0.2f);
	}

	private static float Luminance(Color color) => 0.299f * color.R / 255f + 0.587f * color.G / 255f + 0.114f * color.B / 255f;

	private void CheckLiquid(On_LiquidRenderer.orig_DrawNormalLiquids orig, LiquidRenderer self, SpriteBatch batch, Vector2 off, int style, float alpha, bool bg)
	{
		DrawingLiquid = true;
		orig(self, batch, off, style, alpha, bg);
		DrawingLiquid = false;
	}
}
