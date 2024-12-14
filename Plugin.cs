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

			var npcElObj = ObjectCreationExtensions.CreateSpriteBillboard(null, false).AddSpriteHolder(out var elRenderer, new Vector3(5f, 5f, 0f), 0);
			npcElObj.name = "NPCElevator";
			elRenderer.name = "NpcElevatorRenderer";
			npcElObj.gameObject.ConvertToPrefab(true);

			var collider = npcElObj.gameObject.AddComponent<BoxCollider>();
			collider.size = Vector3.one * 10f; // Nobody can go in, literally

			var elevatorRef = GenericExtensions.FindResourceObject<ElevatorDoor>();

			var npcEl = npcElObj.gameObject.AddComponent<NPCElevator>();
			npcEl.audMan = npcEl.gameObject.CreatePropagatedAudioManager(45f, 75f);
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
			ld.timeLimit = 5;

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
