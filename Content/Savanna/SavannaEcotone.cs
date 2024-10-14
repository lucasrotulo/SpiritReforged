﻿using SpiritReforged.Common.WorldGeneration;
using SpiritReforged.Common.WorldGeneration.Ecotones;
using SpiritReforged.Content.Savanna.Tiles;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Content.Savanna;

internal class SavannaEcotone : EcotoneBase
{
	private static Rectangle SavannaArea = Rectangle.Empty;

	private static int Steps = 0;
	private static bool HasSavanna = false;
	private EcotoneSurfaceMapping.EcotoneEntry Entry = null;
	private HashSet<int> CliffFaces = [];

	protected override void InternalLoad() => On_WorldGen.SpreadGrass += HijackSpreadGrass;

	private void HijackSpreadGrass(On_WorldGen.orig_SpreadGrass orig, int i, int j, int dirt, int grass, bool repeat, TileColorCache color)
	{
		ushort tileType = Main.tile[i, j].TileType;

		if (tileType == ModContent.TileType<SavannaGrass>() || tileType == ModContent.TileType<SavannaDirt>())
			return;

		orig(i, j, dirt, grass, repeat, color);
	}

	public override void AddTasks(List<GenPass> tasks, List<EcotoneSurfaceMapping.EcotoneEntry> entries)
	{
		int index = tasks.FindIndex(x => x.Name == "Pyramids");
		int secondIndex = tasks.FindIndex(x => x.Name == "Spreading Grass");

		if (index == -1 || secondIndex == -1)
			return;

		tasks.Insert(index, new PassLegacy("Savanna Base", BaseGeneration(entries)));
		tasks.Insert(secondIndex + 2, new PassLegacy("Grow Savanna", PopulateSavanna));
	}

	private void PopulateSavanna(GenerationProgress progress, GameConfiguration configuration)
	{
		if (!HasSavanna)
			return;

		Dictionary<Point16, OpenFlags> tiles = [];
		Dictionary<Point16, int> grassLocations = [];
		HashSet<int> types = [TileID.Dirt, TileID.Grass];

		for (int i = SavannaArea.Left; i < SavannaArea.Right; ++i)
		{
			for (int j = SavannaArea.Top; j < SavannaArea.Bottom; ++j)
			{
				OpenFlags flags = OpenTools.GetOpenings(i, j);
				Tile tile = Main.tile[i, j];

				if (types.Contains(tile.TileType))
					tile.TileType = (ushort)ModContent.TileType<SavannaDirt>();

				if (flags != OpenFlags.None)
				{
					tiles.Add(new Point16(i, j), flags);

					if (tile.TileType == ModContent.TileType<SavannaDirt>() && WorldGen.genRand.NextBool(120))
						grassLocations.Add(new Point16(i, j), WorldGen.genRand.Next(5, 9));
				}
			}
		}

		foreach ((Point16 position, OpenFlags flags) in tiles)
		{
			Tile tile = Main.tile[position];

			if (tile.TileType == TileID.Dirt || tile.TileType == TileID.Grass)
				tile.TileType = (ushort)ModContent.TileType<SavannaDirt>();

			if (tile.TileType == ModContent.TileType<SavannaDirt>())
			{
				tile.TileType = (ushort)ModContent.TileType<SavannaGrass>();

				var nearestGrassLocation = grassLocations.MinBy(x => x.Key.ToVector2().DistanceSQ(position.ToVector2()));
				GrowStuffOnGrass(position, (nearestGrassLocation.Key, nearestGrassLocation.Value));
				tile.WallType = WallID.None;
			}
		}
	}

	private static void GrowStuffOnGrass(Point16 position, (Point16 location, int size) nearestGrass)
	{
		float grassLoc = position.ToVector2().DistanceSQ(nearestGrass.location.ToVector2());

		if (grassLoc < nearestGrass.size * nearestGrass.size)
		{
			int type = MathF.Sqrt(grassLoc) >= nearestGrass.size - 1 ? ModContent.TileType<ElephantGrassShort>() : ModContent.TileType<ElephantGrass>();
			WorldGen.PlaceTile(position.X, position.Y - 1, type, true);
		}

		if (WorldGen.genRand.NextBool(12))
		{
			WorldGen.PlaceTile(position.X, position.Y - 1, ModContent.TileType<SavannaShrubs>(), style: WorldGen.genRand.Next(11));
		}
	}

	private static WorldGenLegacyMethod BaseGeneration(List<EcotoneSurfaceMapping.EcotoneEntry> entries) => (progress, _) =>
	{
		IEnumerable<EcotoneSurfaceMapping.EcotoneEntry> validEntries = entries.Where(x => x.SurroundedBy("Desert", "Jungle") && Math.Abs(x.Start.Y - x.End.Y) < 120);

		if (!validEntries.Any())
			return;

		var entry = validEntries.ElementAt(WorldGen.genRand.Next(validEntries.Count()));

		if (entry is null)
			return;

		HasSavanna = true;

		int startX = entry.Start.X - 0;
		int endX = entry.End.X + 0;
		short startY = EcotoneSurfaceMapping.TotalSurfaceY[(short)entry.Start.X];
		short endY = EcotoneSurfaceMapping.TotalSurfaceY[(short)entry.End.X];
		int[] validIds = [TileID.Dirt, TileID.Grass, TileID.ClayBlock, TileID.CrimsonGrass, TileID.CorruptGrass, TileID.Stone];

		var topBottomY = new Point(Math.Min(startY, endY), Math.Max(startY, endY));

		Steps = 3;// WorldGen.genRand.Next(14, 18);

		Dictionary<int, int> stepOffset = [];
		int offset = 0;

		for (int i = 0; i < Steps; ++i)
		{
			offset = WorldGen.genRand.Next(3, 10);

			if (i == Steps - 1)
				offset = 0;

			stepOffset.Add(i, offset);
		}

		var sandNoise = new FastNoiseLite(WorldGen.genRand.Next());
		sandNoise.SetFrequency(0.06f);
		int xOffsetForFactor = 0;

		for (int x = startX; x < endX; ++x)
		{
			float factor = GetBaseLerpFactorForX(startX, endX, xOffsetForFactor, x);
			int addY = (int)MathHelper.Lerp(startY, endY, factor);
			int ySlant = 0;// (int)MathHelper.Lerp(0, endY - startY, (x - startX) / (float)(endX - startX));
			int y = addY + stepOffset[(int)(factor * (Steps - 1))] - (int)(sandNoise.GetNoise(x, 600) * 2) + ySlant;
			int depth = WorldGen.genRand.Next(20);
			int minDepth = (int)Main.worldSurface - y;

			for (int i = -80; i < 30 + depth + minDepth; ++i)
			{
				int realY = y + i;
				Tile tile = Main.tile[x, realY];

				if (i >= 0)
				{
					if (i >= 0 && i < 15)
						tile.HasTile = true;

					if (tile.HasTile && !validIds.Contains(tile.TileType))
						continue;

					if (realY < topBottomY.X)
						topBottomY.X = realY;

					if (realY > topBottomY.Y)
						topBottomY.Y = realY;

					float noise = (sandNoise.GetNoise(x, 0) + 1) * 5 + 6;
					int type = i <= noise ? ModContent.TileType<SavannaDirt>() : GetSandType(x, realY);

					if (i > 90 + depth - noise)
						type = TileID.Sandstone;

					if (tile.TileType == TileID.Stone)
						type = TileID.ClayBlock;

					tile.TileType = (ushort)type;
					tile.WallType = WallID.DirtUnsafe;
				}
				else
					tile.Clear(TileDataType.All);
			}

			int oldOffset = xOffsetForFactor;
			xOffsetForFactor += (int)Math.Round(sandNoise.GetNoise(x, 0) * 2);
		}

		SavannaArea = new Rectangle(startX, topBottomY.X, endX - startX, topBottomY.Y - topBottomY.X);
		return;

		static ushort GetSandType(int x, int y)
		{
			int off = 0;

			while (WorldGen.SolidOrSlopedTile(x, y))
			{
				y++;
				off++;
			}

			return off < 3 ? TileID.Sandstone : TileID.Sand;
		}
	};

	private static float GetBaseLerpFactorForX(int startX, int endX, int xOffsetForFactor, int x)
	{
		float factor = (MathF.Min(x + xOffsetForFactor, endX) - startX) / (endX - startX);
		factor = ModifyLerpFactor(factor);
		return factor;
	}

	private static float ModifyLerpFactor(float factor)
	{
		float adj = Steps;
		factor = (int)((factor + 0.1f) * adj) / adj;
		return factor;
	}
}
