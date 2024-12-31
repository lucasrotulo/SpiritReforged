using System.Linq;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader.IO;

namespace SpiritReforged.Common.SimpleEntity;

public class SimpleEntitySystem : ModSystem
{
	internal static Dictionary<Type, int> types = [];
	internal static Asset<Texture2D>[] textures;

	/// <summary> Instances that were created on load. </summary>
	private static SimpleEntity[] templates;

	/// <summary> Entities that exist in the world. </summary>
	internal static SimpleEntity[] entities;

	internal const int maxEntities = 200;
	private static int nextIndex;

	public static SimpleEntity NewEntity(int type, Vector2 position)
	{
		var entity = templates[type].Clone();
		entity.active = true;
		entity.whoAmI = nextIndex;
		entity.Center = position;

		entities[nextIndex] = entity;

		for (int i = nextIndex; i < entities.Length; i++)
		{
			if (entities[i] is null)
			{
				nextIndex = i;
				break;
			}
		} //Move up the array to the next available slot

		return entity;
	}

	public static void RemoveEntity(int whoAmI)
	{
		entities[whoAmI] = null;
		nextIndex = whoAmI;
	}

	public override void Load()
	{
		templates = new SimpleEntity[maxEntities];
		entities = new SimpleEntity[maxEntities];
		textures = new Asset<Texture2D>[maxEntities];

		foreach (Type type in SpiritReforgedMod.Instance.Code.GetTypes())
			if (type.IsSubclassOf(typeof(SimpleEntity)) && !type.IsAbstract && type != typeof(SimpleEntity))
			{
				int myType = types.Count;
				types[type] = myType;

				var instance = (SimpleEntity)Activator.CreateInstance(type);
				textures[myType] = ModContent.Request<Texture2D>(instance.TexturePath);
				instance.Load();
				templates[myType] = instance;
			}

		Array.Resize(ref textures, types.Count);
		Array.Resize(ref templates, types.Count);

		On_TileDrawing.Update += UpdateEntities;
		On_Main.DoDraw_Tiles_NonSolid += DrawEntities;
	}

	private void UpdateEntities(On_TileDrawing.orig_Update orig, TileDrawing self)
	{
		orig(self);

		for (int i = 0; i < maxEntities; i++)
			entities[i]?.Update();
	}

	private void DrawEntities(On_Main.orig_DoDraw_Tiles_NonSolid orig, Main self)
	{
		orig(self);

		for (int i = 0; i < maxEntities; i++)
			entities[i]?.Draw(Main.spriteBatch);
	}

	public override void OnWorldUnload()
	{
		for (int i = 0; i < maxEntities; i++)
			entities[i] = null; //Unload all of our entities with the world

		nextIndex = 0;
	}

	private const string commonKey = "simpleEntities";
	public override void SaveWorldData(TagCompound tag)
	{
		static List<TagCompound> SaveData()
		{
			var list = new List<TagCompound>();

			for (int i = 0; i < maxEntities; i++)
			{
				var entity = entities[i];
				if (entity == null || !entity.saveMe)
					continue;

				TagCompound tag = [];

				tag["x"] = (int)entity.position.X;
				tag["y"] = (int)entity.position.Y;
				tag["name"] = entity.GetType().Name;

				list.Add(tag);
			}

			return list;
		}

		tag[commonKey] = SaveData();
	}

	public override void LoadWorldData(TagCompound tag)
	{
		var list = tag.GetList<TagCompound>(commonKey);
		if (list == default)
			return;

		for (int i = 0; i < list.Count; i++)
		{
			var tagInList = list[i];

			var position = new Vector2(tagInList.GetInt("x"), tagInList.GetInt("y"));
			string name = tagInList.GetString("name");
			NewEntity(types.FirstOrDefault(x => x.Key.Name == name).Value, position);
		}
	}
}
