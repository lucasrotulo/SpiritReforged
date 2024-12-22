using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace SpiritReforged.Common.WorldGeneration.Ecotones;

internal class EcotoneSurfaceMapping : ModSystem
{
	public class EcotoneEntry(Point start, EcotoneEdgeDefinition definition)
	{
		public Point Start = start;
		public Point End;
		public HashSet<Point> SurfacePoints = [];
		public EcotoneEdgeDefinition Definition = definition;
		public EcotoneEdgeDefinition Left;
		public EcotoneEdgeDefinition Right;

		public bool TileFits(int i, int j) => Definition.ValidIds.Contains(Main.tile[i, j].TileType);
		public bool SurroundedBy(string one, string two) => Left.Name == one && Right.Name == two || Left.Name == two && Right.Name == one;

		public override string ToString() => $"{Start} to {End}; of {Definition}:{SurfacePoints.Count}";
	}

	private List<EcotoneEntry> Entries = [];

	internal static readonly HashSet<Point> TotalSurfacePoints = [];
	internal static readonly Dictionary<short, short> TotalSurfaceY = [];

	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
	{
		int mapIndex = tasks.FindIndex(x => x.Name == "Corruption");

		if (mapIndex == -1)
			return;

		foreach (var ecotone in EcotoneBase.Ecotones)
			ecotone.AddTasks(tasks, Entries);

		tasks.Insert(mapIndex + 1, new PassLegacy("Map Ecotones", MapEcotones));

//#if DEBUG
//		tasks.Add(new PassLegacy("Ecotone Debug", (progress, config) =>
//		{
//			foreach (var item in Entries)
//			{
//				for (int x = item.Start.X; x < item.End.X; ++x)
//				{
//					for (int nY = 90; nY < 100; ++nY)
//						WorldGen.PlaceTile(x, nY, item.Definition.DisplayId, true, true);
//				}
//			}
//		}));
//#endif
	}

	private void MapEcotones(GenerationProgress progress, GameConfiguration configuration)
	{
		const int StartX = 250;

		progress.Message = "Mapping ecotones";

		Entries.Clear();
		TotalSurfacePoints.Clear();
		TotalSurfaceY.Clear();

		int transitionCount = 0;
		EcotoneEntry entry = null;

		for (int x = StartX; x < Main.maxTilesX - StartX; ++x)
		{
			int y = 80;

			while (!WorldGen.SolidOrSlopedTile(x, y) || WorldMethods.CloudsBelow(x, y, out int addY))
				y++;

			if (entry is null)
			{
				entry = new EcotoneEntry(new Point(StartX, y), EcotoneEdgeDefinitions.GetEcotone("Ocean"));
				entry.Left = EcotoneEdgeDefinitions.GetEcotone("Ocean");
			}

			if (!entry.TileFits(x, y))
				transitionCount++;

			if (transitionCount > 20 && EcotoneEdgeDefinitions.TryGetEcotoneByTile(Main.tile[x, y].TileType, out var def) && def.Name != entry.Definition.Name)
			{ 
				EcotoneEdgeDefinition old = entry.Definition;
				entry.End = new Point(x, y);
				entry.Right = def;
				Entries.Add(entry);

				if (x < 390 || x > Main.maxTilesX - 390)
					def = EcotoneEdgeDefinitions.GetEcotone("Ocean");
				
				entry = new EcotoneEntry(new Point(x, y), def);
				entry.Left = old;
				transitionCount = 0;
			}

			entry.SurfacePoints.Add(new Point(x, y));
			TotalSurfacePoints.Add(new Point(x, y));
			TotalSurfaceY.Add((short)x, (short)y);

			if (x == Main.maxTilesX - StartX - 1)
				entry.End = new Point(x, y);
		}

		entry.Right = EcotoneEdgeDefinitions.GetEcotone("Ocean");
		Entries.Add(entry);
		Entries = new(Entries.OrderBy(x => x.Start.X));
	}
}
