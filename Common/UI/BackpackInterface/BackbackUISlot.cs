using SpiritReforged.Common.ItemCommon.Backpacks;
using System.Linq;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.UI;

namespace SpiritReforged.Common.UI.BackpackInterface;

public class BackpackUISlot : UIElement
{
	public const int Context = ItemSlot.Context.ChestItem;
	public const float Scale = .85f;

	private static Asset<Texture2D> icon;

	private readonly bool _isVanity;

	public BackpackUISlot(bool isVanity)
	{
		_isVanity = isVanity;
		Width = Height = new StyleDimension(52 * Scale, 0f);
	}

	public override void OnInitialize() => icon = ModContent.Request<Texture2D>("SpiritReforged/Common/UI/BackpackInterface/BackpackIcon");

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		var mPlayer = Main.LocalPlayer.GetModPlayer<BackpackPlayer>();
		var item = _isVanity ? mPlayer.vanityBackpack : mPlayer.backpack; //Bind the player's backpack item

		if (Main.EquipPage != 2 || item is null)
			return;

		base.DrawSelf(spriteBatch);

		float oldScale = Main.inventoryScale;
		Main.inventoryScale = Scale;

		ItemSlot.Draw(spriteBatch, ref item, Context, GetDimensions().ToRectangle().TopLeft());

		if (item.IsAir) //Draw slot icons when empty
		{
			Texture2D texture;
			Rectangle source;

			if (_isVanity)
			{
				texture = TextureAssets.Extra[54].Value;
				source = texture.Frame(3, 6, 2, 0, -2, -2);
			}
			else
			{
				texture = icon.Value;
				source = texture.Frame();
			}

			spriteBatch.Draw(texture, GetDimensions().Center(), source, Color.White * .35f, 0, source.Size() / 2, Main.inventoryScale, SpriteEffects.None, 0);
		}

		if (!DrawVisibility(spriteBatch))
			HandleItemSlotLogic(ref item);

		Main.inventoryScale = oldScale;

		if (_isVanity) //Release the results
			mPlayer.vanityBackpack = item;
		else
			mPlayer.backpack = item;
	}

	private void HandleItemSlotLogic(ref Item item)
	{
		if (!IsMouseHovering)
			return;

		Main.LocalPlayer.mouseInterface = true;
		ItemSlot.OverrideHover(ref item, Context);
		ItemSlot.MouseHover(ref item, Context);

		if (Main.mouseLeft && Main.mouseLeftRelease && CanClickItem(item))
		{
			ItemSlot.LeftClick(ref item, Context);
			ItemSlot.RightClick(ref item, Context);
		}

		if (item.IsAir)
		{
			Main.mouseText = true;
			Main.hoverItemName = Language.GetTextValue("Mods.SpiritReforged.SlotContexts.Backpack");
		}
	}

	internal static bool CanClickItem(Item currentItem)
	{
		if (!currentItem.IsAir && currentItem.ModItem is BackpackItem backpack && backpack.items.Any(x => !x.IsAir))
		{
			if (currentItem.TryGetGlobalItem(out BackpackAnimation anim))
				anim.StartAnimation();

			return false;
		}

		var plr = Main.LocalPlayer;
		return plr.HeldItem.ModItem is BackpackItem || plr.HeldItem.IsAir || Main.mouseItem.IsAir;
	}

	/// <returns> Whether an interaction occured. </returns>
	private bool DrawVisibility(SpriteBatch spriteBatch)
	{
		if (_isVanity)
			return false;

		var mPlayer = Main.LocalPlayer.GetModPlayer<BackpackPlayer>();
		var visTexture = (mPlayer.packVisible ? TextureAssets.InventoryTickOn : TextureAssets.InventoryTickOff).Value;

		var point = new Point((int)(GetDimensions().X + 34), (int)GetDimensions().Y - 2);
		var area = new Rectangle(point.X, point.Y, visTexture.Width, visTexture.Height);
		
		spriteBatch.Draw(visTexture, area.Center(), null, Color.White * .8f, 0, visTexture.Size() / 2, 1, SpriteEffects.None, 0);

		if (area.Contains(Main.MouseScreen.ToPoint()) && !PlayerInput.IgnoreMouseInterface)
		{
			Main.HoverItem = new Item();
			Main.hoverItemName = Lang.inter[59].Value;

			Main.LocalPlayer.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				mPlayer.packVisible = !mPlayer.packVisible;
				SoundEngine.PlaySound(SoundID.MenuTick);

				if (Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, Main.myPlayer);
			}

			Main.HoverItem = new Item();
			Main.hoverItemName = Lang.inter[mPlayer.packVisible ? 59 : 60].Value;

			return true;
		}

		return false;
	}
}
