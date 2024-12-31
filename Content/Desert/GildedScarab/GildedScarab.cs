using SpiritReforged.Common.ItemCommon;
using SpiritReforged.Common.PlayerCommon;
using Terraria.Audio;

namespace SpiritReforged.Content.Desert.GildedScarab;

internal class GildedScarab : AccessoryItem
{
	public override void SetDefaults()
	{
		Item.width = 34;
		Item.height = 32;
		Item.rare = ItemRarityID.Blue;
		Item.accessory = true;
		Item.value = Item.sellPrice(gold: 1);
	}
}

internal class GildedScarabPlayer : ModPlayer
{
	public int ScarabDefense { get; private set; }

	public override void OnHurt(Player.HurtInfo info)
	{
		if (Player.HasAccessory<GildedScarab>() && !IsHazard())
		{
			if (Player.HasBuff(ModContent.BuffType<GildedScarabBuff>()))
			{
				Player.ClearBuff(ModContent.BuffType<GildedScarabBuff>());
				ScarabDefense = ScarabDefense + (int)(info.Damage / 8f) >= 50 || ScarabDefense >= 50 ? 50 : ScarabDefense + (int)(info.Damage / 8f);
			}
			else
			{
				SoundEngine.PlaySound(SoundID.Item76);
				ScarabDefense = info.Damage >= 400 ? 50 : 5 + (int)(info.Damage / 8f);
			}

			Player.AddBuff(ModContent.BuffType<GildedScarabBuff>(), 300);
		}

		bool IsHazard() => info.DamageSource.SourceOtherIndex is 3 or 2; //Lava or spike damage
	}
}