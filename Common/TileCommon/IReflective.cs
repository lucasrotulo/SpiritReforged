using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Effects;

namespace SpiritReforged.Common.TileCommon;

internal interface IReflective
{
	//public void DrawReflection(SpriteBatch spriteBatch, Texture2D reflection, Vector2 offset, Color color)
	//	=> spriteBatch.Draw(reflection, offset, null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0); //Draw the reflection target texture
}

internal class ReflectionGlobalTile : GlobalTile
{
	private static readonly HashSet<Point16> targetTileData = [];

	private int mirrorY;

	private static RenderTarget2D playerTarget;
	private static RenderTarget2D tileTarget;
	private static MethodInfo playerMethod;

	public override void Load()
	{
		Main.QueueMainThreadAction(() =>
		{
			var gd = Main.instance.GraphicsDevice;
			playerTarget = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			tileTarget = new RenderTarget2D(gd, gd.Viewport.Width, gd.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}); //Initialize our rendertargets

		playerMethod = typeof(Main).GetMethod("DrawPlayers_AfterProjectiles", BindingFlags.NonPublic | BindingFlags.Instance);

		On_TileDrawing.PreDrawTiles += ResetPoints;
		On_Main.CheckMonoliths += DrawIntoTargets;
		On_Main.DoDraw_Tiles_Solid += DrawReflection;
	}

	private void ResetPoints(On_TileDrawing.orig_PreDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets)
	{
		orig(self, solidLayer, forRenderTargets, intoRenderTargets);

		bool flag = intoRenderTargets || Lighting.UpdateEveryFrame;
		if (solidLayer && flag)
			targetTileData.Clear();
	}

	private void DrawIntoTargets(On_Main.orig_CheckMonoliths orig)
	{
		orig();

		if (Main.gameMenu || Main.dedServ)
			return;

		var gd = Main.graphics.GraphicsDevice;

		var storedZoom = Main.GameViewMatrix.Zoom;
		var storedSpriteEffects = Main.GameViewMatrix.Effects;
		var storedRasterizer = Main.Rasterizer;

		Main.GameViewMatrix.Zoom = Vector2.One;
		Main.GameViewMatrix.Effects = SpriteEffects.FlipVertically;
		Main.Rasterizer = RasterizerState.CullClockwise;

		RenderTiles();
		RenderPlayer();

		Main.GameViewMatrix.Effects = storedSpriteEffects;
		Main.Rasterizer = storedRasterizer;
		Main.GameViewMatrix.Zoom = storedZoom;

		void RenderPlayer()
		{
			gd.SetRenderTarget(playerTarget);

			Main.graphics.GraphicsDevice.Clear(Color.Transparent);
			playerMethod?.Invoke(Main.instance, null);

			Overlays.Scene.Draw(Main.spriteBatch, RenderLayers.Entities, true);

			gd.SetRenderTarget(null);
		}

		void RenderTiles()
		{
			gd.SetRenderTarget(tileTarget);
			gd.Clear(Color.Transparent);
			Main.spriteBatch.Begin();

			foreach (var pt in targetTileData)
			{
				int x = pt.X;
				int y = pt.Y;

				var t = Main.tile[x, y];

				var source = new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16);
				var position = new Vector2(x, y) * 16 - Main.screenPosition;

				Main.spriteBatch.Draw(TextureAssets.Tile[t.TileType].Value, position, source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
			}

			Main.spriteBatch.End();
			gd.SetRenderTarget(null);
		}
	}

	private void DrawReflection(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
	{
		orig(self);

		if (targetTileData.Count == 0)
			return;

		var player = Main.LocalPlayer;
		if (Collision.SolidCollision(player.position, player.width, player.height + 2))
			mirrorY = (int)player.Bottom.Y;

		const int falloff = 40;

		float dist = Math.Abs(mirrorY - Main.LocalPlayer.Bottom.Y);
		var color = Color.White * (1f - dist / falloff) * .4f;
		var offset = new Vector2(0, 45 + dist * 2);
		var s = AssetLoader.LoadedShaders["SimpleMultiply"];

		s.Parameters["cTexture"].SetValue(tileTarget);
		s.Parameters["offset"].SetValue(new Vector2(1f / tileTarget.Width, 1f / tileTarget.Height) * offset);

		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, s, Main.Transform);
		Main.spriteBatch.Draw(playerTarget, offset, null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0); //Draw the reflection target texture
		Main.spriteBatch.End();
	}

	public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		if (TileLoader.GetTile(type) is IReflective)
			targetTileData.Add(new Point16(i, j));
	}
}
