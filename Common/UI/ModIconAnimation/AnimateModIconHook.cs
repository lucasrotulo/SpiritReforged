using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace SpiritReforged.Common.UI.ModIconAnimation;

internal class AnimateModIconHook : ILoadable
{
	private static Hook ModUIInitHook = null;
	private static FieldInfo IconInfo;
	private static PropertyInfo ModNameInfo;

	public void Load(Mod mod)
	{
		var type = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIModItem");
		MethodInfo info = type.GetMethod("OnInitialize");
		ModUIInitHook = new Hook(info, HookModIcon, true);

		IconInfo = type.GetField("_modIcon", BindingFlags.NonPublic | BindingFlags.Instance);
		ModNameInfo = type.GetProperty("ModName", BindingFlags.Public | BindingFlags.Instance);
	}

	public static void HookModIcon(Action<object> orig, object self)
	{
		orig(self);

		string name = ModNameInfo.GetValue(self) as string;

		if (name == "SpiritReforged")
		{
			var icon = IconInfo.GetValue(self) as UIImage;

			var element = self as UIElement;
			element.RemoveChild(icon);
			var tex = SpiritReforgedMod.Instance.Assets.Request<Texture2D>("icon_animated", AssetRequestMode.ImmediateLoad);
			element.Append(new UIAnimatedImage(tex, 80, 80, 0, 0, 1, 4, 0)
			{
				FrameCount = 4,
				TicksPerFrame = 8,
			});
		}
	}

	public void Unload()
	{
		ModUIInitHook.Undo();
		ModUIInitHook = null;
		IconInfo = null;
		ModNameInfo = null;
	}
}
