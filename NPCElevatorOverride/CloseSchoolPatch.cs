using HarmonyLib;
using MTM101BaldAPI.Registers;
using TheNPCElevator.NPCElevatorClasses;
using UnityEngine;

namespace TheNPCElevator.NPCElevatorOverride
{
	[HarmonyPatch(typeof(TimeOut), "Begin")]
	internal static class CloseSchoolPatch
	{
		[HarmonyPostfix]
		static void ClosingSchoolForNPCs(TimeOut __instance)
		{
			//Debug.Log("Closing school! So goes to the npcs!");
			var potentialElevators = Object.FindObjectsOfType<NPCElevator>();
			if (potentialElevators.Length == 0)
				return;

			foreach (var npc in __instance.ec.Npcs)
			{
				// Checks for Character enum to prevent npcs like Students from entering too
				if (npc.Navigator.enabled && npc.Character != Character.Null && (npc.GetMeta()?.tags.Contains("student") ?? false)) // If it is a student
				{
					npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_GoToNPCElevator(npc, potentialElevators[Random.Range(0, potentialElevators.Length)]));
				}
			}
		}
	}
}
