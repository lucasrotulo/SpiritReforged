using SpiritReforged.Common.MathHelpers;
using SpiritReforged.Common.WorldGeneration;
using SpiritReforged.Content.Savanna.Tiles;
using SpiritReforged.Content.Savanna.Walls;
using System.Linq;

namespace SpiritReforged.Content.Savanna.Ecotone;

internal static class BaobabGen
{
	public static void GenerateBaobab(int x, int y)
	{
		int width = 10;
		int height = WorldGen.genRand.Next(25, 30);

		CreateBase(x, y, width, height);
		CreateRoots(x, y, width);
		CreateBranches(x, y - height + 6, width);
	}

	private static void CreateBase(int x, int y, int width, int height)
	{
		var openingSize = new Point(4, 8);
		var opening = new Rectangle(x - openingSize.X / 2, y - openingSize.Y - 1, openingSize.X, openingSize.Y);
		int curveHeight = width;

		x -= width / 2; //Center

		for (int offX = 0; offX < width; offX++)
		{
			int realHeight = height - curveHeight + (int)(Math.Sin((float)offX / (width - 1) * Math.PI) * curveHeight);
			for (int offY = 0; offY < realHeight; offY++)
			{
				var pos = new Point(x + offX, y - offY);

				if (opening.Contains(pos))
					WorldGen.PlaceLiquid(pos.X, pos.Y, (byte)LiquidID.Water, 190);
				else
				{
					WorldGen.KillTile(pos.X, pos.Y);
					WorldGen.PlaceTile(pos.X, pos.Y, ModContent.TileType<LivingBaobab>(), true);
				}
			}
		}

		for (int i = x; i < x + width; i++) //Fill walls
			for (int j = y; j > y - height; j--)
			{
				OpenFlags flags = OpenTools.GetOpenings(i, j, false, false);

				if ((flags == OpenFlags.None || opening.Contains(new Point(i, j))) && Main.tile[i, j].TileType == ModContent.TileType<LivingBaobab>())
					WorldGen.PlaceWall(i, j, ModContent.WallType<LivingBaobabWall>(), true);
			}

		WorldGen.PlaceTile(opening.Center.X - 1, opening.Bottom - 1, ModContent.TileType<BaobabPod>(), true);
	}

	private static void CreateRoots(int x, int y, int width)
	{
		int repeats = 5;

		for (int i = 0; i < repeats; ++i)
		{
			int startX = (int)MathHelper.Lerp(x - width / 2, x + width / 2, (float)i / (repeats - 1));
			var points = Spline.CreateSpline(GetBranchPositions(startX, y, startX > x ? 1 : -1, 3, true), 3);

			PointToPointRunner.SingleTile(new(points), (ref Vector2 position, ref Vector2 direction)
				=> CreateChunk((int)position.X, (int)position.Y, ModContent.TileType<LivingBaobab>(), 2), false);
		}
	}

	private static void CreateBranches(int x, int y, int width)
	{
		const float radians = 3.5f;
		const int repeats = 4;

		for (int i = 0; i < repeats; ++i)
		{
			var start = (new Vector2(x, y) - (Vector2.UnitY * (width / 2 - 1)).RotatedBy(radians / (repeats - 1) * i - radians / 2)).ToPoint();
			var points = GetBranchPositions(start.X, start.Y, start.X > x ? 1 : -1, WorldGen.genRand.Next(2, 4), false);

			PointToPointRunner.SingleTile(new(points), (ref Vector2 position, ref Vector2 direction)
				=> CreateChunk((int)position.X, (int)position.Y, ModContent.TileType<LivingBaobab>(), 2), false);

			var last = points.Last().ToPoint();
			WorldGen.TileRunner(last.X, last.Y, 15, 5, ModContent.TileType<LivingBaobabLeaf>(), true, overRide: false);
		}

		const int leafAreaSize = 100; //Approximate wall generation
		var leafArea = new Rectangle(x - leafAreaSize / 2, y - leafAreaSize / 2, leafAreaSize, leafAreaSize);
		for (int _x = leafArea.Left; _x < leafArea.Right; _x++)
			for (int _y = leafArea.Top; _y < leafArea.Bottom; _y++)
			{
				OpenFlags flags = OpenTools.GetOpenings(_x, _y, false, false);

				if (flags == OpenFlags.None && Main.tile[_x, _y].TileType == ModContent.TileType<LivingBaobabLeaf>())
					WorldGen.PlaceWall(_x, _y, ModContent.WallType<LivingBaobabLeafWall>());
			}
	}

	private static Vector2[] GetBranchPositions(int x, int y, int dir, int size, bool down)
	{
		var current = new Point(x, y);
		List<Vector2> points = [current.ToVector2()];
		bool hori = WorldGen.genRand.NextBool();
		int vDir = down ? 1 : -1;

		for (int i = 0; i < size; ++i)
		{
			if (hori)
				current.X += WorldGen.genRand.Next(3, 7) * dir;
			else
				current.Y += WorldGen.genRand.Next(2, 5) * vDir;

			hori = !hori;
			points.Add(current.ToVector2());
		}

		return [.. points];
	}

	private static void CreateChunk(int i, int j, int type, int size)
	{
		for (int x = i; x < i + size; x++)
			for (int y = j; y < j + size; y++)
			{
				WorldGen.KillTile(x, y);
				WorldGen.PlaceTile(x, y, type);
			}
	}
}
