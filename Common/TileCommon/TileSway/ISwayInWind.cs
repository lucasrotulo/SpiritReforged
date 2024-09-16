﻿using Terraria.DataStructures;

namespace SpiritReforged.Common.TileCommon.TileSway;

/// <summary> Helps draw a tile affected by wind - rotates around tile origin. </summary>
public interface ISwayInWind
{
	/// <summary> Add natural wind cycle and grid wind push math here. Called once per multitile. </summary>
	/// <param name="topLeft"> The top left tile in the multitile. </param>
	public float SetWindSway(Point16 topLeft)
	{
		var data = TileObjectData.GetTileData(Framing.GetTileSafely(topLeft));
		float rotation = Main.instance.TilesRenderer.GetWindCycle(topLeft.X, topLeft.Y, TileSwaySystem.Instance.SunflowerWindCounter);

		if (!WorldGen.InAPlaceWithWind(topLeft.X, topLeft.Y, data.Width, data.Height))
			rotation = 0f;

		return rotation + TileSwayHelper.GetHighestWindGridPushComplex(topLeft.X, topLeft.Y, data.Width, data.Height, 30, 2f, 1, true);
	}

	/// <summary> Use this to modify rotation before offset is calculated. Called once per tile. </summary>
	public void ModifyRotation(int i, int j, ref float rotation) { }
	public void DrawInWind(int i, int j, SpriteBatch spriteBatch, Vector2 offset, float rotation, Vector2 origin);
}
