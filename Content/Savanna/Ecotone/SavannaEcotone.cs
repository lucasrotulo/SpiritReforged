using SpiritReforged.Common.TileCommon.CustomTree;
using SpiritReforged.Common.WorldGeneration;
using SpiritReforged.Common.WorldGeneration.Ecotones;
using SpiritReforged.Content.Savanna.Tiles;
using SpiritReforged.Content.Savanna.Tiles.AcaciaTree;
using SpiritReforged.Content.Savanna.Walls;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Content.Savanna.Ecotone;

internal class SavannaEcotone : EcotoneBase
{
	private static Rectangle SavannaArea;
	private static int Steps = 0;

	protected override void InternalLoad()
	{
		On_WorldGen.GrowPalmTree += PreventPalmTreeGrowth;
		On_WorldGen.PlaceSmallPile += PreventSmallPiles;
		On_WorldGen.PlaceTile += PreventLargePiles;
		On_WorldGen.PlacePot += ConvertPot;
	}

	private bool PreventSmallPiles(On_WorldGen.orig_PlaceSmallPile orig, int i, int j, int X, int Y, ushort type)
	{
		if (WorldGen.generatingWorld && type == TileID.SmallPiles && SavannaArea.Contains(new Point(i, j)))
			return false; //Skips orig

		return orig(i, j, X, Y, type);
	}

	private bool PreventLargePiles(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style)
	{
		if (WorldGen.generatingWorld && Type == TileID.LargePiles && SavannaArea.Contains(new Point(i, j)))
			return false; //Skips orig

		return orig(i, j, Type, mute, forced, plr, style);
	}

	private bool PreventPalmTreeGrowth(On_WorldGen.orig_GrowPalmTree orig, int i, int y)
	{
		if (WorldGen.generatingWorld && SavannaArea.Contains(new Point(i, y)))
			return false; //Skips orig

		return orig(i, y);
	}

	private bool ConvertPot(On_WorldGen.orig_PlacePot orig, int x, int y, ushort type, int style)
	{
		if (WorldGen.generatingWorld && SavannaArea.Contains(new Point(x, y)))
			style = WorldGen.genRand.Next(7, 10); //Jungle pots

		return orig(x, y, type, style);
	}

	public override void AddTasks(List<GenPass> tasks, List<EcotoneSurfaceMapping.EcotoneEntry> entries)
	{
		SavannaArea = Rectangle.Empty; //Initialize here in case the player decides to generate multiple worlds in one session
		WateringHoleGen.Area = Rectangle.Empty;

		int pyramidIndex = tasks.FindIndex(x => x.Name == "Pyramids");
		int grassIndex = tasks.FindIndex(x => x.Name == "Spreading Grass") + 2;
		int pilesIndex = tasks.FindIndex(x => x.Name == "Piles") + 3;

		if (pyramidIndex == -1 || grassIndex == -1 || pilesIndex == -1)
			return;

		tasks.Insert(pyramidIndex, new PassLegacy("Savanna", BaseGeneration(entries)));
		tasks.Insert(grassIndex, new PassLegacy("Populate Savanna", PopulateSavanna));
		tasks.Insert(pilesIndex, new PassLegacy("Grow Baobab", GrowBaobab));
	}

	private static WorldGenLegacyMethod BaseGeneration(List<EcotoneSurfaceMapping.EcotoneEntry> entries) => (progress, _) =>
	{
		//Don't generate next to the ocean
		static bool NotOcean(EcotoneSurfaceMapping.EcotoneEntry e) => e.Start.X > GenVars.leftBeachEnd
			&& e.End.X > GenVars.leftBeachEnd && e.Start.X < GenVars.rightBeachStart && e.End.X < GenVars.rightBeachStart;

		IEnumerable<EcotoneSurfaceMapping.EcotoneEntry> validEntries
			= entries.Where(x => x.SurroundedBy("Desert", "Jungle") && Math.Abs(x.Start.Y - x.End.Y) < 120 && NotOcean(x));

		if (!validEntries.Any())
			return;

		var entry = validEntries.ElementAt(WorldGen.genRand.Next(validEntries.Count()));

		if (entry is null)
			return;

		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.SavannaTerrain");

		int startX = entry.Start.X - 0;
		int endX = entry.End.X + 0;
		short startY = EcotoneSurfaceMapping.TotalSurfaceY[(short)entry.Start.X];
		short endY = EcotoneSurfaceMapping.TotalSurfaceY[(short)entry.End.X];
		HashSet<int> validIds = [TileID.Dirt, TileID.Grass, TileID.ClayBlock, TileID.CrimsonGrass, TileID.CorruptGrass, TileID.Stone];

		var topBottomY = new Point(Math.Min(startY, endY), Math.Max(startY, endY));

		Steps = WorldGen.genRand.Next(2, 5);

		var sandNoise = new FastNoiseLite(WorldGen.genRand.Next());
		sandNoise.SetFrequency(0.04f);
		int xOffsetForFactor = -1;
		float curve = 0;

		for (int x = startX; x < endX; ++x)
		{
			float factor = GetBaseLerpFactorForX(startX, endX, xOffsetForFactor, x); //Step height

			int addY = (int)MathHelper.Lerp(startY, endY, curve);
			int y = addY - (int)(sandNoise.GetNoise(x, 600) * 2);
			int depth = WorldGen.genRand.Next(20);
			int minDepth = (int)Main.worldSurface - y;

			if (curve < factor) //easing (hills)
			{
				int fullHeight = (startY - endY) / Steps; //The average height of each step
				const float steepness = .05f;

				//Control hill shape using a lazy sine that remains similar between steps
				float amount = fullHeight == 0 ? 0 : (float)Math.Sin(1f + y % fullHeight / (float)fullHeight * 2) * steepness;
				curve += Math.Max(amount, .008f);
			}

			bool hitSolid = false;
			float taper = Math.Clamp((float)Math.Sin((float)(x - startX) / (endX - startX) * Math.PI) * 1.75f, 0, 1);
			int startHeight = Math.Min(HighestSurfacePoint(x) - y, 0);

			for (int i = startHeight; i < (30 + depth + minDepth) * taper; ++i)
			{
				int realY = y + i;
				var tile = Main.tile[x, realY];

				if (i >= 0)
				{
					if (i >= 15 && (tile.HasTile || tile.WallType != WallID.None))
						hitSolid = true; //Replace tiles a minimum of 15 times, and until a solid tile or wall is hit

					if (i >= 0 && !hitSolid)
						tile.HasTile = true;

					if (tile.HasTile && !validIds.Contains(tile.TileType) && !TileID.Sets.Ore[tile.TileType])
						continue;

					if (realY < topBottomY.X)
						topBottomY.X = realY;

					if (realY > topBottomY.Y)
						topBottomY.Y = realY;

					float noise = (sandNoise.GetNoise(x, 0) + 1) * 5 + 6;
					int type = i <= noise ? ModContent.TileType<SavannaDirt>() : GetSandType(x, realY);

					if (i > 90 + depth - noise)
						type = TileID.Sandstone;

					if (tile.TileType == TileID.Stone || TileID.Sets.Ore[tile.TileType])
						type = GetHardConversion(type, y);

					tile.TileType = (ushort)type;

					if (i > 1) //Convert walls
					{
						if (tile.TileType is TileID.Sand or TileID.HardenedSand)
							tile.WallType = WallID.HardenedSand;
						else if (tile.TileType == TileID.Sandstone)
							tile.WallType = WallID.Sandstone;

						if (tile.WallType is WallID.None or WallID.DirtUnsafe)
							tile.WallType = (ushort)ModContent.WallType<SavannaDirtWall>();
					}
					else
						tile.Clear(TileDataType.Wall); //Clear walls above the Savanna surface
				}
				else
					tile.Clear(TileDataType.All);
			}

			xOffsetForFactor += (int)Math.Round(Math.Max(sandNoise.GetNoise(x, 0), 0) * 5);
		}

		SavannaArea = new Rectangle(startX, topBottomY.X, endX - startX, topBottomY.Y - topBottomY.X);

		if (WorldGen.genRand.NextBool()) //Start watering hole gen
		{
			if (IterateGen(200, HoleGen, out int i, out int j, "Savanna Watering Hole"))
				WateringHoleGen.GenerateWateringHole(i, j);

			static bool HoleGen(int i, int j)
			{
				HashSet<int> soft = [TileID.Dirt, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.Sand, ModContent.TileType<SavannaDirt>(), ModContent.TileType<SavannaGrass>()];
				return soft.Contains(Main.tile[i, j].TileType);
			}
		}

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

		static ushort GetHardConversion(int tileType, int y)
		{
			if (y < 6 && TileID.Sets.Ore[tileType] || tileType == TileID.Stone)
				return (ushort)ModContent.TileType<SavannaDirt>(); //Convert ores and stone close to the surface

			return (ushort)(tileType == TileID.Stone ? TileID.ClayBlock : tileType);
		}

		static int HighestSurfacePoint(int x)
		{
			int y = (int)(Main.worldSurface * 0.35); //Sky height
			while (!Main.tile[x, y].HasTile && Main.tile[x, y].WallType == WallID.None && Main.tile[x, y].LiquidAmount == 0 || WorldMethods.CloudsBelow(x, y, out int addY))
				y++;

			return y;
		}
	};

	private void PopulateSavanna(GenerationProgress progress, GameConfiguration configuration)
	{
		if (SavannaArea.IsEmpty)
			return;

		const int chanceMax = 90; //Maximum odds to generate a tree
		const int minimumTreeSpace = 7;

		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.SavannaObjects");
		HashSet<int> treeSpacing = [];
		HashSet<Point16> grassTop = [];

		WateringHoleGen.AddWaterAndClay();
		GrowStones();

		for (int i = SavannaArea.Left; i < SavannaArea.Right; ++i)
			for (int j = SavannaArea.Top - 1; j < SavannaArea.Bottom; ++j)
			{
				OpenFlags flags = OpenTools.GetOpenings(i, j);
				var tile = Main.tile[i, j];

				if (flags == OpenFlags.None)
					continue;

				if (tile.TileType == TileID.Stone && WorldGen.genRand.NextBool()) //Place stone piles on stone
				{
					if (WorldGen.genRand.NextBool(3))
						WorldGen.PlaceTile(i, j - 1, ModContent.TileType<SavannaRockLarge>(), true, style: WorldGen.genRand.Next(3));
					else
						WorldGen.PlaceTile(i, j - 1, ModContent.TileType<SavannaRockSmall>(), true, style: WorldGen.genRand.Next(3));
				}

				if (tile.TileType == ModContent.TileType<SavannaDirt>())
				{
					tile.TileType = (ushort)ModContent.TileType<SavannaGrass>(); //Grow grass on dirt

					if (flags.HasFlag(OpenFlags.Above))
						grassTop.Add(new Point16(i, j));
				}

				if (WorldGen.genRand.NextBool(120)) //Rare bones
					WorldGen.PlaceTile(i - 1, j, TileID.LargePiles2, true, style: WorldGen.genRand.Next(52, 55));
			}

		if (WorldGen.genRand.NextBool(3))
			Campsite();

		int treeOdds = chanceMax;
		foreach (var p in grassTop)
		{
			(int i, int j) = (p.X, p.Y - 1);

			GrowStuffOnGrass(i, j);

			int treeDistance = Math.Abs(i - treeSpacing.OrderBy(x => Math.Abs(i - x)).FirstOrDefault());
			if (treeDistance > minimumTreeSpace)
			{
				if (WorldGen.genRand.NextBool(treeOdds) && GrowTree(i, j))
				{
					treeSpacing.Add(i);
					treeOdds = chanceMax;
				}
				else
				{
					treeOdds = Math.Max(treeOdds - 1, 2); //Decrease the odds every time a tree fails to generate
				}
			}
		}
	}

	private void GrowBaobab(GenerationProgress progress, GameConfiguration configuration)
	{
		if (SavannaArea.IsEmpty)
			return;

		if (WateringHoleGen.Area.IsEmpty || WorldGen.genRand.NextBool(3) && SavannaArea.Width > 150)
		{
			if (IterateGen(50, BaobabTreeGen, out int i, out int j, "Great Baobab"))
				BaobabGen.GenerateBaobab(i, j);

			static bool BaobabTreeGen(int i, int j)
				=> Main.tile[i, j].TileType == ModContent.TileType<SavannaGrass>() && Main.tile[i, j - 1].LiquidAmount < 50;
		}
	}

	private static void GrowStuffOnGrass(int i, int j)
	{
		if (WorldGen.genRand.NextBool(13)) //Elephant grass patch
			CreatePatch(WorldGen.genRand.Next(5, 11), 0, ModContent.TileType<ElephantGrass>(), ModContent.TileType<ElephantGrassShort>());

		if (WorldGen.genRand.NextBool(9)) //Foliage patch
			CreatePatch(WorldGen.genRand.Next(6, 13), 2, ModContent.TileType<SavannaFoliage>());

		if (WorldGen.genRand.NextBool(50)) //Termite mound
		{
			int type = WorldGen.genRand.NextFromList(ModContent.TileType<TermiteMoundSmall>(),
				ModContent.TileType<TermiteMoundMedium>(), ModContent.TileType<TermiteMoundLarge>());
			int style = WorldGen.genRand.Next(TileObjectData.GetTileData(type, 0).RandomStyleRange);

			WorldGen.PlaceTile(i, j - 1, type, true, style: style);
		}

		void CreatePatch(int size, int chance, params int[] types)
		{
			for (int x = i - size / 2; x < i + size / 2; x++)
			{
				if (chance > 1 && !WorldGen.genRand.NextBool(chance))
					continue;

				int y = j;
				WorldMethods.FindGround(x, ref y);

				int type = types[WorldGen.genRand.Next(types.Length)];
				int styleRange = TileObjectData.GetTileData(type, 0).RandomStyleRange;

				WorldGen.PlaceTile(x, y - 1, type, true, style: WorldGen.genRand.Next(styleRange));
			}
		}
	}

	private static bool GrowTree(int i, int j)
	{
		const int shrubSpread = 16;
		const int rootSpread = 3;

		bool success = CustomTree.GrowTree<AcaciaTree>(i, j);
		if (success) //Place shrubs and roots around trees
		{
			for (int x = i - rootSpread; x < i + rootSpread; x++)
			{
				if (!WorldGen.genRand.NextBool(3))
					continue;

				int y = SavannaArea.Top;
				WorldMethods.FindGround(x, ref y);

				if (WorldGen.genRand.NextBool(3))
					WorldGen.PlaceTile(x, y - 1, ModContent.TileType<SavannaShrubs>(), true, style: WorldGen.genRand.Next(11));

				int type = WorldGen.genRand.NextBool() ? ModContent.TileType<AcaciaRootsLarge>() : ModContent.TileType<AcaciaRootsSmall>();
				int styleRange = TileObjectData.GetTileData(type, 0).RandomStyleRange;

				WorldGen.PlaceTile(x, y - 1, type, true, style: WorldGen.genRand.Next(styleRange));
			}

			for (int x = i - shrubSpread; x < i + shrubSpread; x++)
			{
				int y = SavannaArea.Top;
				WorldMethods.FindGround(x, ref y);

				if (WorldGen.genRand.NextBool(4))
					WorldGen.PlaceTile(x, y - 1, ModContent.TileType<SavannaShrubs>(), true, style: WorldGen.genRand.Next(11));
			}
		}

		return success;
	}

	private static void GrowStones()
	{
		const int spawnRegion = 80;

		int rocksMax = Math.Min(SavannaArea.Width / 120, 4);
		int rocks = 0;

		IterateGen(80, RockGen, out int i, out int j);

		bool RockGen(int i, int j)
		{
			if (i > SavannaArea.Left + spawnRegion && i < SavannaArea.Right - spawnRegion)
				return false;

			(int width, int height) = (WorldGen.genRand.Next(3, 10), WorldGen.genRand.Next(3, 6));
			bool hasMoss = WorldGen.genRand.NextBool(3);
			var t = Main.tile[i, j];

			i -= width / 2; //Automatically center

			for (int x = 0; x < width; x++) //Generate the rock
			{
				WorldMethods.FindGround(i + x, ref j);
				int _height = (int)(Math.Abs(Math.Sin(x / (float)width * Math.PI)) * height) + 1;

				for (int y = j; y < j + _height; y++)
				{
					var tile = Main.tile[i + x, y];
					if (!tile.HasTile || tile.TileType == ModContent.TileType<SavannaDirt>())
					{
						tile.HasTile = true;
						tile.TileType = (OpenTools.GetOpenings(i + x, y) == OpenFlags.None || !hasMoss) ? TileID.Stone : TileID.BrownMoss;
					}
				}
			}

			return ++rocks > rocksMax;
		}
	}

	private static void Campsite()
	{
		IterateGen(200, CampsiteGen, out int i, out int j, "Savanna Campsite");

		static bool CampsiteGen(int i, int j)
		{
			const int halfCampfireDistance = 8;

			if (Main.tile[i, j].TileType != ModContent.TileType<SavannaGrass>() || !TileObject.CanPlace(i, j - 1, TileID.LargePiles2, 26, 0, out _, true))
				return false;
			//Can we place the tent here? If so, try placing the campfire nearby

			int y = j;
			for (int x = i - halfCampfireDistance; x < i + halfCampfireDistance; x++)
			{
				WorldMethods.FindGround(x, ref y);
				if (Math.Abs(x - i) > 2) //Don't overlap the tent position. This assumes tile widths are 3 each
				{
					int campfireType = ModContent.TileType<RoastCampfire>();

					WorldGen.PlaceTile(x, y - 1, campfireType, true); //Place the campfire, and if successful, place the tent in our predetermined location
					if (Main.tile[x, y - 1].TileType == campfireType)
					{
						WorldGen.PlaceTile(i, j - 1, TileID.LargePiles2, true, style: 26);
						return true; //Success!
					}
				}
			}

			return false;
		}
	}

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

	private delegate bool OnAttempt(int i, int j);
	private static bool IterateGen(int tries, OnAttempt isValid, out int x, out int y, string structureName = default)
	{
		for (int t = 0; t < tries; t++)
		{
			int i = WorldGen.genRand.Next(SavannaArea.Left, SavannaArea.Right);
			int j = SavannaArea.Top;

			WorldMethods.FindGround(i, ref j);

			if (isValid.Invoke(i, j))
			{
				(x, y) = (i, j);
				return true;
			}

			if (t == tries - 1 && structureName != default)
				SpiritReforgedMod.Instance.Logger.Info("Generator exceeded maximum tries for structure: " + structureName);
		}

		(x, y) = (0, 0);
		return false;
	}
}
