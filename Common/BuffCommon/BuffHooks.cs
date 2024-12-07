namespace SpiritReforged.Common.BuffCommon;

public class BuffHooks : ILoadable
{
	public delegate void ModifyBuffTimeDelegate(int buffType, ref int buffTime, Player player, bool quickBuff);
	public static event ModifyBuffTimeDelegate ModifyBuffTime;

	private static bool UsedQuickBuff;

	public void Load(Mod mod)
	{
		On_Player.AddBuff += BuffTime;
		On_Player.QuickBuff += TrackQuickBuff;
	}

	private void TrackQuickBuff(On_Player.orig_QuickBuff orig, Player self)
	{
		UsedQuickBuff = true;
		orig(self);
		UsedQuickBuff = false;
	}

	private void BuffTime(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
	{
		ModifyBuffTime.Invoke(type, ref timeToAdd, self, UsedQuickBuff);
		orig(self, type, timeToAdd, quiet, foodHack);
	}

	public void Unload() => ModifyBuffTime = null;
}
