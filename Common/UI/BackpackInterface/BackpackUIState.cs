using SpiritReforged.Common.ItemCommon.Backpacks;
using SpiritReforged.Common.UI.System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SpiritReforged.Common.UI.BackpackInterface;

internal class BackpackUIState : AutoUIState
{
	private BackpackUISlot functionalSlot;
	private BackpackUISlot vanitySlot;

	private bool _hadBackpack;
	private int _lastMapStyle;

	public override void OnInitialize()
	{
		Width = Height = StyleDimension.Fill;

		functionalSlot = new BackpackUISlot(false);
		Append(functionalSlot);

		vanitySlot = new BackpackUISlot(true);
		Append(vanitySlot);

		SetPositions();

		On_Main.DrawInventory += TryOpenUI;
	}

	private void TryOpenUI(On_Main.orig_DrawInventory orig, Main self)
	{
		orig(self);

		if (Main.playerInventory && UISystem.GetState<BackpackUIState>().UserInterface.CurrentState is null)
			UISystem.SetActive<BackpackUIState>();
	}

	public override void Update(GameTime gameTime)
	{
		if (!Main.playerInventory)
		{
			_hadBackpack = false; //Force the storage list to reload when the UI closes
			SetStorageSlots(true);

			UISystem.SetInactive<BackpackUIState>(); //Close the UI
			return;
		}

		bool hasBackpack = BackpackPlayer.TryGetBackpack(Main.LocalPlayer, out var _);

		if (hasBackpack && !_hadBackpack)
			SetStorageSlots(false);
		else if (!hasBackpack && _hadBackpack)
			SetStorageSlots(true);

		_hadBackpack = hasBackpack;

		if (Main.mapStyle != _lastMapStyle)
			SetPositions();

		_lastMapStyle = Main.mapStyle;

		base.Update(gameTime);
	}

	private void SetPositions()
	{
		if (Main.mapStyle == 1) //Standard minimap display
		{
			functionalSlot.Left = new StyleDimension(-186, 1);
			functionalSlot.Top = new StyleDimension(430, 0);

			vanitySlot.Left = new StyleDimension(functionalSlot.Left.Pixels - 48, 1);
			vanitySlot.Top = functionalSlot.Top;
		}
		else
		{
			functionalSlot.Left = new StyleDimension(-186, 1);
			functionalSlot.Top = new StyleDimension(174, 0);

			vanitySlot.Left = new StyleDimension(functionalSlot.Left.Pixels - 48, 1);
			vanitySlot.Top = functionalSlot.Top;
		}
	}

	private void SetStorageSlots(bool clear)
	{
		if (clear)
		{
			List<UIElement> removals = [];

			foreach (var item in Children)
				if (item is UIItemSlot or UIText)
					removals.Add(item);

			foreach (var item in removals)
				RemoveChild(item);
		}
		else
		{
			const int BaseX = 570;
			int xOff = 0, yOff = 0;

			Append(new UIText(Language.GetTextValue("Mods.SpiritReforged.SlotContexts.Backpack"), 0.725f, false)
			{
				Left = new StyleDimension(BaseX, 0),
				Top = new StyleDimension(86, 0),
				Width = StyleDimension.FromPixels(32),
				Height = StyleDimension.FromPixels(32),
				TextColor = Color.White * 0.95f,
				ShadowColor = Color.Transparent
			});

			var mPlayer = Main.LocalPlayer.GetModPlayer<BackpackPlayer>();
			var items = (mPlayer.backpack.ModItem as BackpackItem).items;

			for (int i = 0; i < items.Length; ++i) //Add backpack storage slots
			{
				var newSlot = new UIItemSlot(items, i, ItemSlot.Context.ChestItem)
				{
					Left = new StyleDimension(BaseX + xOff * 32, 0),
					Top = new StyleDimension(105 + yOff * 33, 0),
					Width = StyleDimension.FromPixels(32),
					Height = StyleDimension.FromPixels(32)
				};

				yOff++;

				if (yOff >= 4)
				{
					xOff++;
					yOff = 0;
				}

				Append(newSlot);
			}
		}
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		float lastScale = Main.inventoryScale;
		Main.inventoryScale = 0.6f; //Scale down storage slots

		base.DrawChildren(spriteBatch);

		Main.inventoryScale = lastScale;
	}
}
