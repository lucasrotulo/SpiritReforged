﻿namespace SpiritReforged.Common.TileCommon.CustomTree;

internal static class TreeHelper
{
	/// <summary> Calculates the horizontal offset of a palm tree using the vanilla method. </summary>
	public static Vector2 GetPalmTreeOffset(int i, int j) => new(Framing.GetTileSafely(i, j).TileFrameY - 2, 0);

	public static short GetPalmOffset(int j, int variance, int height, ref short offset)
	{
		if (j != 0 && offset != variance)
		{
			double num5 = j / height;
			if (!(num5 < 0.25))
			{
				if ((!(num5 < 0.5) || !WorldGen.genRand.NextBool(13)) && (!(num5 < 0.7) || !WorldGen.genRand.NextBool(9)) && num5 < 0.95)
					WorldGen.genRand.Next(5);

				short num6 = (short)Math.Sign(variance);
				offset = (short)(offset + (short)(num6 * 2));
			}
		}

		return offset;
	}
}
