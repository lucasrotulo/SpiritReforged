using SpiritReforged.Common.WorldGeneration;
using SpiritReforged.Content.Savanna.Tiles;
using SpiritReforged.Content.Savanna.Walls;
using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;

namespace SpiritReforged.Content.Savanna.Ecotone;

internal static class BaobabGen
{
	public static void GenerateBaobab(int x, int y)
	{
		const int width = 16;
		int height = WorldGen.genRand.Next(27, 31);

		CreateBase(x, y, width, height);
		CreateRoots(x, y, width);
		CreateBranches(x, y - height + 4);
	}

	private static void CreateBase(int x, int y, int width, int height)
	{
		const float curveMargin = .4f;

		int curveHeight = (int)(height * curveMargin);
		int preCurveHeight = (int)(height * (1f - curveMargin));

		var openingSize = new Point(4, 8);
		var opening = new Rectangle(x - openingSize.X / 2, y - openingSize.Y - 1, openingSize.X, openingSize.Y);

		WorldUtils.Gen(new Point(x - width / 2, y - preCurveHeight), new Shapes.Rectangle(width, preCurveHeight), 
			Actions.Chain(new Actions.SetTile((ushort)ModContent.TileType<LivingBaobab>())));

		WorldUtils.Gen(new Point(x - openingSize.X / 2, y - openingSize.Y - 1), new Shapes.Rectangle(openingSize.X, openingSize.Y), 
			Actions.Chain(new Actions.ClearTile(), new Actions.PlaceWall((ushort)ModContent.WallType<LivingBaobabWall>())));

		for (int i = 0; i < 2; i++) //Curved top
			WorldUtils.Gen(new Point(x - 1 + i, y - preCurveHeight - 1), new Shapes.Mound(width / 2, curveHeight), Actions.Chain(new Actions.SetTile((ushort)ModContent.TileType<LivingBaobab>())));

		WorldGen.PlaceTile(opening.Center.X - 1, opening.Bottom - 1, ModContent.TileType<BaobabPod>(), true);
	}

	private static void CreateRoots(int x, int y, int width)
	{
		const int roots = 5;
		float half = width / 2;

		for (int i = 0; i < roots; ++i)
		{
			int startX = (int)MathHelper.Lerp(x - half, x + half - 1, i / (roots - 1f));
			WorldUtils.Gen(new Point(startX, y - 1), new ShapeRoot(MathHelper.PiOver2, 15, 3), Actions.Chain(new Actions.SetTile((ushort)ModContent.TileType<LivingBaobab>())));
		}
	}

	private static void CreateBranches(int x, int y)
	{
		const int branches = 4;
		const float radians = 3.5f;

		for (int i = 0; i < branches; ++i)
		{
			var start = MoveOut(i / ((float)branches - 1) * -radians, new Vector2(x, y)) - new Point(0, 1);
			var points = GetBranchPositions(start.X, start.Y, start.X < x, true, WorldGen.genRand.Next(2, 4));

			PointToPointRunner.SingleTile(new(points), (ref Vector2 position, ref Vector2 direction)
				=> CreateChunk((int)position.X, (int)position.Y, ModContent.TileType<LivingBaobab>(), 2), false);

			ShapeData data = new();
			int halfWidth = WorldGen.genRand.Next(7, 13);
			var last = points.Last().ToPoint();

			WorldUtils.Gen(last, new Shapes.Mound(halfWidth, WorldGen.genRand.Next(4, 6)),
				Actions.Chain(new Modifiers.SkipTiles((ushort)ModContent.TileType<LivingBaobab>()), new Modifiers.Blotches(2, 0.1), 
				new Actions.SetTile((ushort)ModContent.TileType<LivingBaobabLeaf>()).Output(data), new Actions.PlaceWall((ushort)ModContent.WallType<LivingBaobabLeafWall>()))); //Add a canopy

			WorldUtils.Gen(points.Last().ToPoint(), new ModShapes.InnerOutline(data, false), Actions.Chain(new Actions.ClearWall()));

			for (int b = last.X - halfWidth; b < last.X + halfWidth; b++) //Randomly generate baobab fruit below canopies
			{
				if (WorldGen.genRand.NextBool(4))
					WorldGen.PlaceObject(b, last.Y + 1, ModContent.TileType<Items.BaobabFruit.BaobabFruitTile>(), true, WorldGen.genRand.Next(2));
			}
		}

		static Point MoveOut(float angle, Vector2 origin)
		{
			var dir = Vector2.UnitX.RotatedBy(angle);

			while (Framing.GetTileSafely((origin + dir).ToPoint()).TileType == ModContent.TileType<LivingBaobab>())
				origin += dir;

			return origin.ToPoint();
		}
	}

	private static Vector2[] GetBranchPositions(int x, int y, bool left, bool up, int size)
	{
		bool hori = true;
		var current = new Vector2(x, y);
		List<Vector2> positions = [current];

		for (int i = 0; i < size; ++i)
		{
			float falloff = 1f - (float)i / size;

			if (hori)
				current.X += (int)Math.Max(WorldGen.genRand.Next(3, 8) * falloff, 1) * (left ? -1 : 1);
			else
				current.Y += (int)Math.Max(WorldGen.genRand.Next(3, 6) * falloff, 1) * (up ? -1 : 1);

			hori = !hori;
			positions.Add(current);
		}

		return [.. positions];
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
