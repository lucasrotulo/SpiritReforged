using SpiritReforged.Content.Ocean.Hydrothermal.Tiles;
using SpiritReforged.Content.Ocean.Tiles;
using System.Linq;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Common.WorldGeneration.Micropasses;

internal class GravelMicropass : Micropass
{
	public override string WorldGenName => "Gravel";

	public override int GetWorldGenIndexInsert(List<GenPass> passes, ref bool afterIndex)
	{
		afterIndex = false;
		return passes.FindIndex(genpass => genpass.Name.Equals("Cactus, Palm Trees, & Coral")); //Earlier passes try to generate dirt on top of gravel
	}

	public override void Run(GenerationProgress progress, GameConfiguration config)
	{
		const int maxFails = 300;

		progress.Message = Language.GetTextValue("Mods.SpiritReforged.Generation.Gravel");

		int count = 0;
		int maxCount = 8 + 2 * WorldGen.GetWorldSize(); //The maximum number of gravel patches which can generate in a world
		int distance = WorldGen.oceanDistance;

		int waterTopLeft = 0, waterTopRight = 0;
		GetWaterTop(ref waterTopLeft, ref waterTopRight);

		for (int f = 0; f < maxFails; f++)
		{
			int minY = waterTopLeft;

			int x = WorldGen.genRand.Next(40, distance);
			if (WorldGen.genRand.NextBool())
			{
				x = WorldGen.genRand.Next(Main.maxTilesX - distance, Main.maxTilesX - 40);
				minY = waterTopRight;
			}

			int y = WorldGen.genRand.Next(minY + 20, (int)WorldGen.oceanLevel);

			if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileID.Sand && !Main.tile[x, y - 1].HasTile 
				&& Main.tile[x, y - 1].LiquidType == LiquidID.Water && Main.tile[x, y - 1].LiquidAmount > 0)
			{
				if (!TileObject.CanPlace(x, y - 1, ModContent.TileType<HydrothermalVent>(), 0, 1, out _, true))
					continue;

				if (WorldGen.genRand.NextBool(5)) //Generate a platform
				{
					int dims = 2;
					for (int i = 0; i < dims * dims; i++)
					{
						var p = new Point(x - i % dims, y - i / dims);

						WorldGen.PlaceTile(p.X, p.Y, ModContent.TileType<Gravel>());
						if (Main.tile[p.X, p.Y].TileType == ModContent.TileType<Gravel>())
						{
							Framing.GetTileSafely(p).Slope = SlopeType.Solid;
							Framing.GetTileSafely(p).IsHalfBlock = false;
						}
					}

					y--;
				}

				WorldGen.PlaceObject(x, y - 1, ModContent.TileType<HydrothermalVent>(), true,
					Main.rand.Next(TileObjectData.GetTileData(ModContent.TileType<HydrothermalVent>(), 0).RandomStyleRange));

				MoveDown(x, ref y, ModContent.TileType<Gravel>()); //This is the lowest we move y directly
				WorldGen.OreRunner(x, y, WorldGen.genRand.Next(5, 10), 
					WorldGen.genRand.Next(10, 15), (ushort)ModContent.TileType<Gravel>()); //Initial gravel patch

				if (WorldGen.genRand.NextBool()) //Generate small protrusions (rocks)
				{
					int offX = x + WorldGen.genRand.Next(-4, 4);
					int offY = y;

					MoveDown(offX, ref offY);
					MoveUp(offX, ref offY); //Surface
					SmallRoundedRock(offX, ref offY, WorldGen.genRand.Next(2, 4));
				}

				if (WorldGen.genRand.NextBool(4)) //Extension gravel patch
				{
					int offX = x + WorldGen.genRand.Next(-3, 3);
					int offY = y;

					MoveDown(offX, ref offY, ModContent.TileType<Gravel>());
					WorldGen.OreRunner(offX, offY, WorldGen.genRand.Next(3, 8), 
						WorldGen.genRand.Next(6, 13), (ushort)ModContent.TileType<Gravel>());
				}

				int magmaCount = WorldGen.genRand.Next(4);
				for (int i = 0; i < magmaCount; i++) //Bottom magma patches
				{
					int offX = x + WorldGen.genRand.Next(-10, 10);
					int offY = y;

					MoveDown(offX, ref offY, ModContent.TileType<Gravel>());
					if (WorldGen.SolidTile(offX, offY - 1)) //Only move back up (to the gravel tile, presumably) if it's buried
						offY--;

					if (Main.tile[offX, offY].HasTile && Main.tile[offX, offY].TileType == ModContent.TileType<Gravel>())
						WorldGen.OreRunner(offX, offY, 4, 1, (ushort)ModContent.TileType<Magmastone>());
					else
						i--; //Try again
				}

				f--; //Decrease fails
				if (++count >= maxCount)
					break;
			}
		}

		static void MoveDown(int x, ref int y, int ignore = -1)
		{
			while (!Main.tile[x, y].HasTile || ignore != -1 && Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ignore)
				y++;
		}

		static void MoveUp(int x, ref int y)
		{
			while (WorldGen.SolidOrSlopedTile(Main.tile[x, y - 1]))
				y--;
		}
	}

	private static void GetWaterTop(ref int left, ref int right)
	{
		for (int x = 0; x < 2; x++)
		{
			int i = (x > 0) ? Main.maxTilesX - 40 : 40;
			int j = 0;

			while (j < (int)WorldGen.oceanLevel && Main.tile[i, j].LiquidType != LiquidID.Water || Main.tile[i, j].LiquidAmount == 0)
				j++;

			if (i != 0)
				right = j;
			else
				left = j;
		}
	}

	private static void SmallRoundedRock(int x, ref int y, int size)
	{
		//Tiles we're allowed to modify/destroy
		int[] canTouch = [TileID.Sand, TileID.HardenedSand, ModContent.TileType<Gravel>(), ModContent.TileType<OceanKelp>(), 
			ModContent.TileType<Kelp1x2>(), ModContent.TileType<Kelp2x2>(), ModContent.TileType<Kelp2x3>()];

		if (!GenVars.structures.CanPlace(new Rectangle(x, y, size, size)))
			return;

		for (int i = 0; i < size * size; i++)
		{
			var p = new Point(x - i % size, y - i / size);
			if (Main.tile[p.X, p.Y].HasTile && !canTouch.Contains(Main.tile[p.X, p.Y].TileType))
				continue;

			WorldGen.KillTile(p.X, p.Y);
			WorldGen.PlaceTile(p.X, p.Y, ModContent.TileType<Gravel>());

			if (i >= size * (size - 1)) //Top layer slope logic
			{
				if (size == 2)
					Framing.GetTileSafely(p).IsHalfBlock = true;
				else if (i % size == 0)
					Framing.GetTileSafely(p).Slope = SlopeType.SlopeDownLeft;
				else if (i % size == size - 1)
					Framing.GetTileSafely(p).Slope = SlopeType.SlopeDownRight;
			}
		}

		y -= size - 1;
	}
}
