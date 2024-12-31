namespace SpiritReforged.Common.TileCommon.CheckItemUse;

internal interface ICheckItemUse
{
	/// <summary> Allows you to make things happen when a player targets this tile and uses an item, like the Staff of Regrowth growing grass on dirt. </summary>
	/// <param name="type"> The type of item used. </param>
	/// <param name="i"> The selected tile's X position. </param>
	/// <param name="j"> The selected tile's Y position. </param>
	/// <returns> Whether the item of 'type' did something when used. Return null for vanilla effects. </returns>
	public bool? CheckItemUse(int type, int i, int j);
}
