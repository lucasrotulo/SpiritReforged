﻿using Terraria.GameContent.ItemDropRules;
using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Common.ModCompat;
using System.Linq;

namespace SpiritReforged.Content.Savanna.Items.Fishing;

public class SavannaCrateHardmode : ModItem
{
	public override void SetStaticDefaults() => ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SavannaCrate>();
	
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<SavannaCrateHardmodeTile>());
		Item.rare = ItemRarityID.Green;
	}

	public override bool CanRightClick() => true;

	public override void ModifyItemLoot(ItemLoot itemLoot)
	{
		int[] dropOptions = [ModContent.ItemType<HuntingRifle.HuntingRifle>(),
			ItemID.SandstorminaBottle,
			ItemID.AnkletoftheWind,
			ItemID.MysticCoilSnake,
			ItemID.FeralClaws];
		if (FablesCompat.Enabled && FablesCompat.Instance.TryFind("CalamityFables/ToxicBlowpipe", out ModItem toxicBLowpipe))
			dropOptions = dropOptions.Append(toxicBLowpipe.Type).ToArray();

		var main = ItemDropRule.OneFromOptions(1, dropOptions);

		CrateHelper.HardmodeBiomeCrate(itemLoot, main, ItemDropRule.NotScalingWithLuck(ItemID.BambooBlock, 3, 20, 50), 
			ItemDropRule.NotScalingWithLuck(ItemID.DesertFossil, 3, 20, 50), ItemDropRule.NotScalingWithLuck(ItemID.Leather, 3, 5, 10));
	}
}

public class SavannaCrateHardmodeTile : SavannaCrateTile { }