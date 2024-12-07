namespace SpiritReforged.Common.PlayerCommon;

internal class FountainPlayer : ModPlayer
{
	private int fountainSlot = -1;

	private void TileReset() => fountainSlot = -1;

	public override void PreUpdate()
	{
		if (fountainSlot != -1)
			Main.SceneMetrics.ActiveFountainColor = fountainSlot;
	}

	/// <summary>
	/// Sets the player's current fountain to the given water style.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal void SetFountain<T>() where T : ModWaterStyle => fountainSlot = ModContent.GetInstance<T>().Slot;

	public class FountainResetSystem : ModSystem
	{
		public override void ResetNearbyTileEffects() => Main.LocalPlayer.GetModPlayer<FountainPlayer>().TileReset();
	}
}
