using System.Linq;
using static SpiritReforged.Common.Misc.ChestPoolUtils;

namespace SpiritReforged.Common.Misc;

public static class ChestPoolUtils
{
	/// <summary>
	/// Struct containing information related to chest item pools. <br />
	/// Inputting an int[] for Items results in one of the items from that array being picked for that slot randomly, while inputting an int or short directly adds the corresponding item.
	/// </summary>
	public struct ChestInfo
	{
		public object Items;
		public int Stack;
		public float Chance;

		public ChestInfo(object Items, int Stack = 1, float Chance = 1)
		{
			this.Items = Items;
			this.Stack = Stack;
			this.Chance = Chance;
		}

		public List<ChestInfo> ToList() => new List<ChestInfo> { this };
	}

	/// <summary>
	/// Method to greatly reduce the amount of effort needed to make a chest pool. <br />
	/// Input the chest's pool as a list of structs representing the item pool for each slot, stack for that pool, and chance to be added.
	/// </summary>

	//Vanilla Chest IDs for TileID.Containers
	public const int woodChests = 0;
	public const int goldChests = 1;
	public const int lockedgoldChests = 2;
	public const int ivyChests = 10;
	public const int snowChests = 11;
	public const int skyChests = 13;
	public const int livingWoodChests = 12;
	public const int spiderChests = 15;
	public const int waterChests = 17;
	public const int dynastyChests = 28;

	/// <summary>
	/// Vanilla Chest IDs for TileID.Containers2
	/// To use these, override the default argument for tileType with TileID.Containers2
	/// </summary>
	public const int sandstoneChests = 11;

	// Helper method for adding items to chests
	private static void AddItemsToChest(IEnumerable<ChestInfo> list, Chest chest, int itemIndex)
	{
		foreach (ChestInfo chestInfo in list)
		{ 
			switch (chestInfo.Items)
			{
				case int[] itemPool:
					chest.item[itemIndex].SetDefaults(itemPool[Main.rand.Next(itemPool.Length)]);
					chest.item[itemIndex].stack = chestInfo.Stack;
					break;
				case int intItem:
					chest.item[itemIndex].SetDefaults((int)chestInfo.Items);
					chest.item[itemIndex].stack = chestInfo.Stack;
					break;
				case short shortItem:
					chest.item[itemIndex].SetDefaults((short)chestInfo.Items);
					chest.item[itemIndex].stack = chestInfo.Stack;
					break;
			}

			itemIndex++;
		}
	}
	public static void PlaceChestItems(List<ChestInfo> list, Chest chest, int startIndex = 0)
	{
		int itemIndex = startIndex;

		var newList = new List<ChestInfo>();

		foreach (ChestInfo c in list)
		{ //prune the list based on the chances of items being added and stacks
			if (Main.rand.NextFloat() >= c.Chance || c.Stack == 0)
				continue; //skip

			newList.Add(c);
		}

		if (chest.item[itemIndex].active && newList.Count > 0)
		{ //check if the spot items are being added to is active
			int itemsToMove = itemIndex + chest.item.Skip(itemIndex).Where(x => x.active).Count();
			for (int i = itemsToMove; i >= itemIndex; i--)
				chest.item[i + newList.Count] = chest.item[i].Clone();
		}

		AddItemsToChest(newList, chest, itemIndex);
	}

	public static void PlaceModChestItemsWCheck(List<ChestInfo> list, Chest chest, ref bool[] placedItems)
	{
		int itemIndex = 0;

		int[] importantItemPool = (int[])list.ElementAt(0).Items;
		int itemToPlace = 0;
		bool canPlace = false;
		while (!canPlace)
		{ //check if the chosen item has been placed before, and if all items havent already been placed
			itemToPlace = WorldGen.genRand.Next(importantItemPool.Length);
			if (!placedItems[itemToPlace] || placedItems.All(x => x == true))
			{
				placedItems[itemToPlace] = true;
				canPlace = true;
				break;
			}
		}

		if (canPlace)
		{
			placedItems[itemToPlace] = true;
			chest.item[itemIndex].SetDefaults(importantItemPool[itemToPlace]);
			itemIndex++;
		}

		AddItemsToChest(list.Skip(1), chest, itemIndex);

	}

	public static void AddToModdedChest(List<ChestInfo> list, int chestType)
	{
		for (int chestIndex = 0; chestIndex < Main.chest.Length; chestIndex++)
		{
			Chest chest = Main.chest[chestIndex];
			if (chest != null && Main.tile[chest.x, chest.y].TileType == chestType)
				PlaceChestItems(list, chest);
		}
	}

	public static void AddToModdedChestWithOverlapCheck(List<ChestInfo> list, int chestType)
	{
		int[] items = (int[])list.ElementAt(0).Items;
		bool[] placedItems = new bool[items.Length];
		for (int chestIndex = 0; chestIndex < Main.chest.Length; chestIndex++)
		{
			Chest chest = Main.chest[chestIndex];
			if (chest != null && Main.tile[chest.x, chest.y].TileType == chestType)
				PlaceModChestItemsWCheck(list, chest, ref placedItems);
		}
	}

	public static void AddToVanillaChest(List<ChestInfo> list, int chestFrame, int index = 0, ushort tileType = TileID.Containers)
	{
		chestFrame *= 36;
		for (int chestIndex = 0; chestIndex < Main.chest.Length; chestIndex++)
		{
			Chest chest = Main.chest[chestIndex];
			if (chest != null && Main.tile[chest.x, chest.y].TileType == tileType && Main.tile[chest.x, chest.y].TileFrameX == chestFrame)
				PlaceChestItems(list, chest, index);
		}
	}

	public static void AddToVanillaChest(ChestInfo item, int chestFrame, int index, ushort tileType = TileID.Containers)
	{
		chestFrame *= 36;
		for (int chestIndex = 0; chestIndex < Main.chest.Length; chestIndex++)
		{
			Chest chest = Main.chest[chestIndex];
			if (chest != null && Main.tile[chest.x, chest.y].TileType == tileType && Main.tile[chest.x, chest.y].TileFrameX == chestFrame)
				PlaceChestItems(item.ToList(), chest, index);

		}
	}
}
