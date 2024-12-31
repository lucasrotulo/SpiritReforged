using Terraria.DataStructures;

namespace SpiritReforged.Common.TileCommon.CheckItemUse;

internal class CheckItem : GlobalItem
{
	public override bool? UseItem(Item item, Player player)
	{
		var target = new Point(Player.tileTargetX, Player.tileTargetY);
		var tile = Main.tile[target.X, target.Y];

		if (tile.HasTile && player.InInteractionRange(target.X, target.Y, TileReachCheckSettings.Simple) && TileLoader.GetTile(tile.TileType) is ICheckItemUse check)
			return check.CheckItemUse(item.type, target.X, target.Y);

		return null;
	}
}
