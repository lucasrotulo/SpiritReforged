namespace SpiritReforged.Content.Snow.Frostbite;

public class FrostbiteNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public float slowDegree;
	private float slowAmt;
	public bool beingSlowed => slowDegree > 0;

	public override void ResetEffects(NPC npc) => slowDegree = 0;

	public override bool PreAI(NPC npc)
	{
		if (beingSlowed)
		{
			if ((slowAmt += slowDegree) >= 1)
			{
				slowAmt--;
				return true;
			}

			return false;
		}

		return true;
	}

	public override void PostAI(NPC npc)
	{
		if (beingSlowed)
			npc.position -= npc.velocity * (float)(1f - slowAmt) * (npc.boss ? 0.25f : 1f);
	}
}