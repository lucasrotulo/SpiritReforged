namespace SpiritReforged.Common.TileCommon.FurnitureTiles;

public abstract class FurnitureTile : ModTile, IAutoloadTileItem
{
	public ModItem ModItem => Mod.Find<ModItem>(Name + "Item");

	/// <summary> The defining material in most furniture recipes. </summary>
	public virtual int CoreMaterial => ItemID.None;

	public virtual void SetItemDefaults(ModItem item) { }
	public virtual void AddItemRecipes(ModItem item) { }

	public sealed override void SetStaticDefaults()
	{
		if (ModItem.Type > 0)
			RegisterItemDrop(ModItem.Type);

		StaticDefaults();
	}

	/// <summary> Functions like <see cref="SetStaticDefaults"/>. </summary>
	public virtual void StaticDefaults() { }
}
