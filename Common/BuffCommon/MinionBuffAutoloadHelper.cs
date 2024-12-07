using System.Linq;
using System.Reflection;
using Terraria.DataStructures;

namespace SpiritReforged.Common.BuffCommon;

[AttributeUsage(AttributeTargets.Class)]
public class AutoloadMinionBuff() : Attribute { }

public class AutoloadMinionPlayer : ModPlayer
{
	public IDictionary<int, bool> ActiveMinionDict = new Dictionary<int, bool> { }; //type of projectile, then whether or not that minion is active

	public override void Initialize()
	{
		ActiveMinionDict = new Dictionary<int, bool> { };
		foreach (KeyValuePair<int, int> kvp in AutoloadMinionDictionary.BuffDictionary)
			ActiveMinionDict.Add(kvp.Key, false);
	}

	public override void ResetEffects()
	{
		Dictionary<int, bool> dummy = new();
		foreach (KeyValuePair<int, bool> kvp in ActiveMinionDict)
			dummy.Add(kvp.Key, false);

		ActiveMinionDict = dummy;
	}

	public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (AutoloadMinionDictionary.BuffDictionary.TryGetValue(type, out int buffType))
			Player.AddBuff(buffType, 180);

		return base.Shoot(item, source, position, velocity, type, damage, knockback);
	}
}

public class AutoloadMinionGlobalProjectile : GlobalProjectile
{
	public override bool PreAI(Projectile projectile)
	{
		if (AutoloadMinionDictionary.BuffDictionary.ContainsKey(projectile.type))
		{
			Player player = Main.player[projectile.owner];
			AutoloadMinionPlayer modPlayer = player.GetModPlayer<AutoloadMinionPlayer>();
			if (player.dead || !player.active)
				modPlayer.ActiveMinionDict[projectile.type] = false;

			if (modPlayer.ActiveMinionDict[projectile.type])
				projectile.timeLeft = 2;
		}

		return true;
	}
}

public static class AutoloadMinionDictionary
{
	internal static IDictionary<int, int> BuffDictionary = new Dictionary<int, int> { }; //type of projectile, then corresponding type of buff

	public static void AddBuffs(Assembly code)
	{
		var autoloadminions = code.GetTypes().Where(x => x.IsSubclassOf(typeof(ModProjectile)) && Attribute.IsDefined(x, typeof(AutoloadMinionBuff))); //read the assembly to find classes that are mod projectiles, and have the autoload minion buff attribute
		foreach (Type MinionType in autoloadminions)
		{
			var attribute = (AutoloadMinionBuff)Attribute.GetCustomAttribute(MinionType, typeof(AutoloadMinionBuff));
			var mProjectile = (ModProjectile)Activator.CreateInstance(MinionType);

			SpiritReforgedMod.Instance.AddContent(new AutoloadedMinionBuff(SpiritReforgedMod.Instance.Find<ModProjectile>(MinionType.Name).Type, MinionType.Name + "_Buff", MinionType.FullName.Replace(".", "/") + "_Buff"));
			BuffDictionary.Add(SpiritReforgedMod.Instance.Find<ModProjectile>(MinionType.Name).Type, SpiritReforgedMod.Instance.Find<ModBuff>(MinionType.Name + "_Buff").Type);
		}
	}

	public static void Unload() => BuffDictionary.Clear();
}

public sealed class AutoloadedMinionBuff(int MinionType, string InternalName, string RealTexture) : ModBuff
{
	private readonly int MinionType = MinionType;
	private readonly string InternalName = InternalName;
	private readonly string RealTexture = RealTexture;

	public override string Texture => RealTexture;
	public override string Name => InternalName;

	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		AutoloadMinionPlayer modPlayer = player.GetModPlayer<AutoloadMinionPlayer>();
		if (player.ownedProjectileCounts[MinionType] > 0)
			modPlayer.ActiveMinionDict[MinionType] = true;

		if (!modPlayer.ActiveMinionDict[MinionType])
		{
			player.DelBuff(buffIndex);
			buffIndex--;
			return;
		}

		player.buffTime[buffIndex] = 180;
	}
}
