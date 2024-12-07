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

namespace SpiritReforged.Content.Savanna;

internal class SavannaEcotone : EcotoneBase
{
	private static Rectangle SavannaArea = Rectangle.Empty;
	private static Rectangle WaterHoleArea = Rectangle.Empty;

	private static bool HasWaterHole => !WaterHoleArea.IsEmpty;
	private static bool HasSavanna => !SavannaArea.IsEmpty;

	private static int Steps = 0;

	protected override void InternalLoad()
	{
		On_WorldGen.GrowPalmTree += PreventPalmTreeGrowth;
		On_WorldGen.PlaceSmallPile += PreventSmallPiles;
		On_WorldGen.PlaceTile += PreventLargePiles;
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

	public override void AddTasks(List<GenPass> tasks, List<EcotoneSurfaceMapping.EcotoneEntry> entries)
	{
		int pyramidIndex = tasks.FindIndex(x => x.Name == "Pyramids");
		int grassIndex = tasks.FindIndex(x => x.Name == "Spreading Grass") + 2;
		int pilesIndex = tasks.FindIndex(x => x.Name == "Piles") + 3;

		if (pyramidIndex == -1 || grassIndex == -1 || pilesIndex == -1)
			return;

		tasks.Insert(pyramidIndex, new PassLegacy("Savanna", BaseGeneration(entries)));
		tasks.Insert(grassIndex, new PassLegacy("Populate Savanna", PopulateSavanna));
		tasks.Insert(pilesIndex, new PassLegacy("Grow Baobab", GrowBaobab));
	}

	private void PopulateSavanna(GenerationProgress progress, GameConfiguration configuration)
	{
		if (!HasSavanna)
			return;

		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.SavannaObjects");
		HashSet<int> treeSpacing = [];

		if (HasWaterHole)
			WateringHole(WaterHoleArea.X, WaterHoleArea.Y, true);

		GrowStones();

		if (WorldGen.genRand.NextBool(3))
			Campsite();

		for (int i = SavannaArea.Left; i < SavannaArea.Right; ++i)
		{
			for (int j = SavannaArea.Top - 2; j < SavannaArea.Bottom; ++j)
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

					const int minimumTreeSpace = 35;
					int treeDistance = Math.Abs(i - treeSpacing.OrderBy(x => Math.Abs(i - x)).FirstOrDefault());

					if (WorldGen.genRand.NextBool(20) && treeDistance > minimumTreeSpace) //Add trees
						if (CustomTree.GrowTree<AcaciaTree>(i, j - 1))
							treeSpacing.Add(i);

					GrowStuffOnGrass(i, j);
				}
			}
		}

		const int shrubSpread = 12;
		const int rootSpread = 3;
		foreach (int t in treeSpacing)
		{
			for (int x = t - rootSpread; x < t + rootSpread; x++) //Place roots around trees
			{
				if (!WorldGen.genRand.NextBool(3))
					continue;

				int y = SavannaArea.Top;
				FindGround(x, ref y);

				if (WorldGen.genRand.NextBool(3))
					WorldGen.PlaceTile(x, y - 1, ModContent.TileType<SavannaShrubs>(), true, style: WorldGen.genRand.Next(11));

				int type = WorldGen.genRand.NextBool() ? ModContent.TileType<AcaciaRootsLarge>() : ModContent.TileType<AcaciaRootsSmall>();
				int styleRange = TileObjectData.GetTileData(type, 0).RandomStyleRange;

				WorldGen.PlaceTile(x, y - 1, type, true, style: WorldGen.genRand.Next(styleRange));
			}

			for (int x = t - shrubSpread; x < t + shrubSpread; x++) //Place shrubs around trees
			{
				int y = SavannaArea.Top;
				FindGround(x, ref y);

				if (WorldGen.genRand.NextBool(3))
					WorldGen.PlaceTile(x, y - 1, ModContent.TileType<SavannaShrubs>(), true, style: WorldGen.genRand.Next(11));
			}
		}
	}

	private static int FindGround(int i, ref int j)
	{
		while (WorldGen.SolidOrSlopedTile(i, j - 1))
			j--; //Up

		while (!WorldGen.SolidOrSlopedTile(i, j))
			j++; //Down

		return j;
	}

	private static void GrowStuffOnGrass(int i, int j)
	{
		if (WorldGen.genRand.NextBool(17)) //Elephant grass patch
			CreatePatch(WorldGen.genRand.Next(5, 11), 0, ModContent.TileType<ElephantGrass>(), ModContent.TileType<ElephantGrassShort>());

		if (WorldGen.genRand.NextBool(9)) //Foliage pouch
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
				FindGround(x, ref y);

				int type = types[WorldGen.genRand.Next(types.Length)];
				int styleRange = TileObjectData.GetTileData(type, 0).RandomStyleRange;

				WorldGen.PlaceTile(x, y - 1, type, true, style: WorldGen.genRand.Next(styleRange));
			}
		}
	}

	private void GrowBaobab(GenerationProgress progress, GameConfiguration configuration)
	{
		if (!HasSavanna)
			return;

		if (!HasWaterHole || WorldGen.genRand.NextBool(3) && SavannaArea.Width > 150)
		{
			progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.GreatBaobab");

			const int tries = 50;
			for (int a = 0; a < tries; a++)
			{
				int i = WorldGen.genRand.Next(SavannaArea.Left, SavannaArea.Right);
				int j = SavannaArea.Top;

				FindGround(i, ref j);
				if (Main.tile[i, j].TileType == ModContent.TileType<SavannaGrass>() && Main.tile[i, j - 1].LiquidAmount < 50)
				{
					BaobabGen.GenerateBaobab(i, j);
					return;
				}

				if (a == tries - 1)
					SpiritReforgedMod.Instance.Logger.Info("Generator exceeded maximum tries for structure: Great Baobab");
			}
		}
	}

	private static void GrowStones()
	{
		const int rockTries = 50;
		int rocksMax = Math.Min(SavannaArea.Width / 120, 4);
		int rocks = 0;

		for (int a = 0; a < rockTries; a++) //rocks
		{
			int i = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(SavannaArea.Left, SavannaArea.Left + 60) 
				: WorldGen.genRand.Next(SavannaArea.Right - 60, SavannaArea.Right);
			int j = SavannaArea.Top;

			FindGround(i, ref j);

			if (Main.tile[i, j].TileType == ModContent.TileType<SavannaDirt>())
			{
				CreateStone(i, j, WorldGen.genRand.Next(5, 10), WorldGen.genRand.Next(3, 6));
				if (++rocks > rocksMax)
					break;
			}
		}

		return;
		static void CreateStone(int i, int j, int width, int height)
		{
			HashSet<int> soft = [TileID.Dirt, TileID.CorruptGrass, TileID.CrimsonGrass, ModContent.TileType<SavannaDirt>()];
			i -= width / 2; //Automatically center

			for (int x = 0; x < width; x++)
			{
				FindGround(i + x, ref j);
				int _height = (int)(Math.Abs(Math.Sin(x / (float)width * Math.PI)) * height) + 1;

				for (int y = j; y < j + _height; y++)
				{
					var tile = Main.tile[i + x, y];
					if (soft.Contains(tile.TileType))
					{
						tile.HasTile = true;
						tile.TileType = TileID.Stone;
					}
				}
			}
		}
	}

	private static void Campsite()
	{
		const int tries = 200;
		for (int a = 0; a < tries; a++)
		{
			int i = WorldGen.genRand.Next(SavannaArea.Left, SavannaArea.Right);
			int j = SavannaArea.Top;

			FindGround(i, ref j);
			if (Main.tile[i, j].TileType == ModContent.TileType<SavannaDirt>())
			{
				const int halfCampfireDistance = 8;
				if (TileObject.CanPlace(i, j - 1, TileID.LargePiles2, 26, 0, out _, true)) //Can we place the tent here? If so, try placing the campfire nearby
				{
					int y = j;
					for (int x = i - halfCampfireDistance; x < i + halfCampfireDistance; x++)
					{
						FindGround(x, ref y);
						if (Math.Abs(x - i) > 2) //Don't overlap the tent position. This assumes tile widths are 3 each
						{
							int campfireType = ModContent.TileType<RoastCampfire>();

							WorldGen.PlaceTile(x, y - 1, campfireType, true); //Place the campfire, and if successful, place the tent in our predetermined location
							if (Main.tile[x, y - 1].TileType == campfireType)
							{
								WorldGen.PlaceTile(i, j - 1, TileID.LargePiles2, true, style: 26);
								return; //Success!
							}
						}
					}
				}
			}

			if (a == tries - 1)
				SpiritReforgedMod.Instance.Logger.Info("Generator exceeded maximum tries for structure: Savanna Campsite");
		}
	}

	private static void WateringHole(int i, int j, bool addWater = false)
	{
		if (addWater)
		{
			int waterY = j + 1;
			for (int y = j; y < j + WaterHoleArea.Height; y++)
			{
				if (!WorldGen.SolidOrSlopedTile(i - 1, y) || !WorldGen.SolidOrSlopedTile(i + WaterHoleArea.Width + 1, y))
					waterY++; //Determine water height based on adjacent solid tiles
			}

			for (int x = i; x < i + WaterHoleArea.Width; x++) //Water fill
			{
				int y = waterY;
				while (!WorldGen.SolidTile(x, y + 1))
				{
					var t = Framing.GetTileSafely(x, y);
					t.LiquidType = LiquidID.Water;
					t.LiquidAmount = (byte)((y == waterY) ? 50 : 255);

					y++; //Down
				}

				ClaySplotch(x, y + 1);
			}

			return;
		}

		int width = WorldGen.genRand.Next(20, 26);
		int depth = WorldGen.genRand.Next(20, 28);
		CreateHole(i, j, width, depth);

		for (int x = WaterHoleArea.Left - 1; x < WaterHoleArea.Right + 1; x++) //Clear walls
		{
			for (int y = WaterHoleArea.Top - 1; y < WaterHoleArea.Bottom + 1; y++)
			{
				var tile = Main.tile[x, y];
				tile.Clear(TileDataType.Wall);
			}
		}

		const int halfDistance = 35;
		for (int a = 0; a < 5; a++) //Generate surrounding sand patches
		{
			int x = i + WorldGen.genRand.Next(-halfDistance, halfDistance);
			FindGround(x, ref j);

			WorldGen.TileRunner(x, j, 10, 1, TileID.Sand);
		}

		for (int x = i - halfDistance; x < i + halfDistance; x++)
		{
			FindGround(x, ref j);

			var t = Main.tile[x, j];
			if (t.TileType == TileID.Sand)
				t.HasTile = false; //Cave in surface sand spots

			if (WorldGen.genRand.NextBool(3)) //Generate shrubs
				WorldGen.PlaceTile(x, j, ModContent.TileType<SavannaShrubs>(), true, style: WorldGen.genRand.NextFromList(0, 3, 4));
		}

		return;
		static int DigDown(int i, int j, int distance)
		{
			FindGround(i, ref j);
			for (int y = 0; y < distance; y++)
			{
				var t = Main.tile[i, j];
				t.ClearEverything();
				j++;
			}

			return j;
		}

		static void ClaySplotch(int i, int j)
		{
			i -= 1;
			HashSet<int> replaceTypes = [ModContent.TileType<SavannaDirt>(), ModContent.TileType<SavannaGrass>()];

			for (int x = i; x < i + 3; x++)
			{
				for (int y = j; y < j + 4; y++)
				{
					var t = Main.tile[x, y];
					if (t.HasTile && replaceTypes.Contains(t.TileType))
						t.TileType = TileID.ClayBlock;
				}
			}
		}

		static void CreateHole(int i, int j, int width, int depth)
		{
			i -= width / 2; //Automatically center
			WaterHoleArea = new Rectangle(i, j, width, depth);

			for (int x = 0; x < width; x++)
			{
				int minDepth = (int)(Math.Abs(Math.Sin(x / (float)width * Math.PI)) * depth);
				DigDown(i + x, j, minDepth);
			}
		}
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
		HashSet<int> noKillIds = [TileID.Cloud, TileID.RainCloud]; //Ignore these types when clearing tiles above the biome

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
				float amount = (fullHeight == 0) ? 0 : (float)Math.Sin(1f + y % fullHeight / (float)fullHeight * 2) * steepness;
				curve += Math.Max(amount, .008f);
			}

			bool hitSolid = false;
			float taper = Math.Clamp((float)Math.Sin((float)(x - startX) / (endX - startX) * Math.PI) * 1.75f, 0, 1);

			for (int i = GetConnectedY(x, y, -80, [.. noKillIds]); i < (30 + depth + minDepth) * taper; ++i)
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

					if (i > 1)
					{
						if (tile.WallType is WallID.None or WallID.DirtUnsafe)
							tile.WallType = (ushort)ModContent.WallType<SavannaDirtWall>();
					}
					else
						tile.Clear(TileDataType.Wall); //Clear walls above the Savanna surface
				}
				else if (!noKillIds.Contains(tile.TileType))
					tile.Clear(TileDataType.All);
			}

			xOffsetForFactor += (int)Math.Round(Math.Max(sandNoise.GetNoise(x, 0), 0) * 5);
		}

		SavannaArea = new Rectangle(startX, topBottomY.X, endX - startX, topBottomY.Y - topBottomY.X);

		if (WorldGen.genRand.NextBool()) //Start watering hole gen
		{
			HashSet<int> soft = [TileID.Dirt, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.Sand,
			ModContent.TileType<SavannaDirt>(), ModContent.TileType<SavannaGrass>()];

			const int tries = 200;
			for (int a = 0; a < tries; a++)
			{
				int i = WorldGen.genRand.Next(SavannaArea.Left, SavannaArea.Right);
				int j = SavannaArea.Top;

				FindGround(i, ref j);

				if (soft.Contains(Main.tile[i, j].TileType))
				{
					WateringHole(i, j);
					break;
				}

				if (a == tries - 1)
					SpiritReforgedMod.Instance.Logger.Info("Generator exceeded maximum tries for structure: Savanna Watering Hole");
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

			return (ushort)((tileType == TileID.Stone) ? TileID.ClayBlock : tileType);
		}

		static int GetConnectedY(int x, int y, int startYOffset, int[] ignoreTypes) //Scans up all connected walls or tiles
		{
			while (y > 0)
			{
				var tile = Main.tile[x, y + startYOffset];
				if (!tile.HasTile && tile.WallType == WallID.None && tile.LiquidAmount == 0 || ignoreTypes.Contains(tile.TileType))
					break;

				startYOffset--;
			}

			return startYOffset;
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
