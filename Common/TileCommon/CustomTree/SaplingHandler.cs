using System.Linq;

namespace SpiritReforged.Common.TileCommon.CustomTree;

internal class SaplingHandler : ILoadable
{
	private static readonly Dictionary<int, int[]> saplingDefinitions = []; //Stores the sapling type and tile anchors, respectively

	/// <summary> Allows saplings without an associated ModTree to be grown using an acorn. </summary>
	internal static void RegisterSapling(int tileType) =>
		saplingDefinitions.Add(tileType, TileObjectData.GetTileData(tileType, 0).AnchorValidTiles);

	public void Load(Mod mod) => On_Player.FigureOutWhatToPlace += PlaceFromDefinition;

	private void PlaceFromDefinition(On_Player.orig_FigureOutWhatToPlace orig, Player self, Tile targetTile, Item sItem, out int tileToCreate, out int previewPlaceStyle, out bool? overrideCanPlace, out int? forcedRandom)
	{
		orig(self, targetTile, sItem, out tileToCreate, out previewPlaceStyle, out overrideCanPlace, out forcedRandom);

		if (sItem.type != ItemID.Acorn)
			return;

		var below = Main.tile[Player.tileTargetX, Player.tileTargetY + 1];
		if (WorldGen.SolidTile(below))
		{
			var matches = saplingDefinitions.Where(x => x.Value.Contains(below.TileType));

			if (matches.Any())
				tileToCreate = matches.First().Key;
		}
	}

	public void Unload() { }
}
