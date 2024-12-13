using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using System.Collections;
using UnityEngine;

namespace TheNPCElevator
{
	[BepInPlugin(guid, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	[BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
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

			var nullRoom = Resources.FindObjectsOfTypeAll<EnvironmentController>()[0].nullRoom; // Placeholder
			npcElevatorPrefab.wallTex = nullRoom.wallTex;
			npcElevatorPrefab.floorTex = nullRoom.florTex;
			npcElevatorPrefab.ceilingTex = nullRoom.ceilTex;

			elObj.ConvertToPrefab(true);
		}

		void AddBuilderToLevelObjects(string lvlName, int lvlNum, SceneObject sceneObject)
		{
			var ld = sceneObject.levelObject;
			if (!ld) return;

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
