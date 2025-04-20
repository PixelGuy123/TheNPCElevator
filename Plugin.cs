using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using System.Collections;
using UnityEngine;
using PixelInternalAPI.Extensions;
using TheNPCElevator.NPCElevatorClasses;
using System.IO;
using System.Collections.Generic;

namespace TheNPCElevator
{
    [BepInPlugin(guid, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
	public class Plugin : BaseUnityPlugin
	{
		internal const string guid = "pixelguy.pixelmodding.baldiplus.thenpcelevator";
		internal static ManualLogSource logger;
		internal static string ModPath;
		private void Awake()
		{
			ModPath = AssetLoader.GetModPath(this);
			Harmony h = new(guid);
			h.PatchAll();
			logger = Logger;

			LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), false);
			//LoadingEvents.RegisterOnAssetsLoaded(Info, () =>
			//{
			//	var gen = Instantiate(Resources.FindObjectsOfTypeAll<TextTextureGenerator>()[0]);
			//	HashSet<PosterObject> seenPosters = [];
			//	foreach (var npc in NPCMetaStorage.Instance.All())
			//	{

			//		foreach (var npcVal in npc.prefabs)
			//		{
			//			if (npcVal.Value && npcVal.Value.Poster && seenPosters.Add(npcVal.Value.Poster))
			//			{
			//				try
			//				{
			//					SaveTextureAsPNG(gen.GenerateTextTexture(npcVal.Value.Poster), npcVal.Key + ".png");
			//				}
			//				catch (System.Exception e)
			//				{
			//					Debug.LogWarning("======== Failed to load texture from NPC: " + npcVal.Key + " ===============");
			//					Debug.LogException(e);
			//				}
			//			}
			//		}
			//	}
			//	Destroy(gen.gameObject);

			//	static void SaveTextureAsPNG(Texture2D texture, string fileName)
			//	{
			//		if (texture == null)
			//		{
			//			Debug.LogError("Cannot save null texture");
			//			return;
			//		}

			//		// Ensure the texture is readable
			//		if (!texture.isReadable)
			//		{
			//			Debug.LogError("Texture is not readable. Enable Read/Write in import settings.");
			//			return;
			//		}

			//		try
			//		{
			//			// Convert texture to PNG bytes
			//			byte[] pngData = texture.EncodeToPNG();
			//			if (pngData == null || pngData.Length == 0)
			//			{
			//				Debug.LogError("Failed to convert texture to PNG");
			//				return;
			//			}

			//			// Construct full save path
			//			string savePath = Path.Combine(Application.streamingAssetsPath, "exportedPNGs", fileName);

			//			string directory = Path.GetDirectoryName(savePath);
			//			if (!Directory.Exists(directory))
			//			{
			//				Directory.CreateDirectory(directory);
			//			}

			//			// Save file
			//			File.WriteAllBytes(savePath, pngData);
			//			Debug.Log($"Saved PNG to: {savePath}");
			//		}
			//		catch (System.Exception e)
			//		{
			//			Debug.LogError($"Failed to save PNG: {e.Message}");
			//		}
			//	}

			//}, true);

			GeneratorManagement.Register(this, GenerationModType.Addend, AddBuilderToLevelObjects);

		}

		IEnumerator PreLoad()
		{
			yield return 1;

			yield return "Creating NPC Elevator Builder Prefab";

			var elObj = new GameObject("Structure_NPCElevator");
			npcElevatorPrefab = elObj.gameObject.AddComponent<Structure_NPCElevator>();

			npcElevatorPrefab.wallTex = AssemblyExtensions.LoadTextureFromResources("wall.png");
			npcElevatorPrefab.floorTex = AssemblyExtensions.LoadTextureFromResources("floor.png");
			npcElevatorPrefab.ceilingTex = AssemblyExtensions.LoadTextureFromResources("ceil.png");

			elObj.ConvertToPrefab(true);

			var npcElObj = ObjectCreationExtensions.CreateSpriteBillboard(null, false).AddSpriteHolder(out var elRenderer, new Vector3(0f, 5f, 5f), 0);
			npcElObj.name = "NPCElevator";
			elRenderer.name = "NpcElevatorRenderer";
			elRenderer.transform.localScale = new(0.976f, 1f, 1f);
			npcElObj.gameObject.ConvertToPrefab(true);

			var collider = npcElObj.gameObject.AddComponent<BoxCollider>();
			collider.size = Vector3.one * 10f; // Nobody can go in, literally

			var elevatorRef = GenericExtensions.FindResourceObject<ElevatorDoor>();

			var npcEl = npcElObj.gameObject.AddComponent<NPCElevator>();
			npcEl.audMan = elRenderer.gameObject.CreatePropagatedAudioManager(45f, 75f);
			npcEl.audClose = elevatorRef.audDoorShut;
			npcEl.audOpen = elevatorRef.audDoorOpen;

			npcEl.sprRenderer = elRenderer;

			npcEl.sprElvs = AssetLoader.SpritesFromSpritesheet(5, 1, 25f, Vector2.one * 0.5f, AssemblyExtensions.LoadTextureFromResources("elevator.png"));

			npcElevatorPrefab.npcElevatorPre = npcEl;
		}

		void AddBuilderToLevelObjects(string lvlName, int lvlNum, SceneObject sceneObject)
		{
			var ld = sceneObject.levelObject;
			if (!ld) return;

			ld.MarkAsNeverUnload();
			//ld.timeLimit = 5;

			switch (lvlName)
			{
				case "F1":
					ld.forcedStructures = ld.forcedStructures.AddToArray(new()
					{
						prefab = npcElevatorPrefab,
						parameters = new()
						{
							minMax = [new(1, 1)]
						}
					});
					return;

				case "F2":
					ld.forcedStructures = ld.forcedStructures.AddToArray(new()
					{
						prefab = npcElevatorPrefab,
						parameters = new()
						{
							minMax = [new(1, 3)]
						}
					});
					return;

				case "F3":
					ld.forcedStructures = ld.forcedStructures.AddToArray(new()
					{
						prefab = npcElevatorPrefab,
						parameters = new()
						{
							minMax = [new(2, 5)]
						}
					});
					return;
			}
		}

		Structure_NPCElevator npcElevatorPrefab;
    }

	//[HarmonyPatch]
	//internal class Finalizers
	//{
	//	[HarmonyPatch(typeof(CullingManager), "CalculateOcclusionCullingForChunk", [typeof(int)])]
	//	[HarmonyFinalizer]
	//	static System.Exception AvoidThis(System.Exception __exception, int chunkId)
	//	{
	//		if (__exception != null)
	//		{
	//			Debug.LogWarning("At chunk: " + chunkId);
	//			Debug.LogException(__exception);
	//		}
	//		return null;
	//	}
	//}
}
