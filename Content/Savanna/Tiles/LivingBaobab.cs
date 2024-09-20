﻿namespace SpiritReforged.Content.Savanna.Tiles;

internal class LivingBaobab : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = true;
		Main.tileBlendAll[Type] = true;
		Main.tileMergeDirt[Type] = true;
		Main.tileBlockLight[Type] = true;

		AddMapEntry(new Color(142, 125, 106));
		HitSound = SoundID.Dig;
	}
}
