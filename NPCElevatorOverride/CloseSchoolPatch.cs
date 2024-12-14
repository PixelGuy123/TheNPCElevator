using HarmonyLib;
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
				if (npc.Navigator.enabled && !CannotLeaveSchool(npc))
				{
					npc.behaviorStateMachine.ChangeNavigationState(new NavigationState_GoToNPCElevator(npc, potentialElevators[Random.Range(0, potentialElevators.Length)]));
				}
			}
        }

		static bool CannotLeaveSchool(NPC npc) =>
			npc.Character == Character.Principal || npc.Character == Character.Baldi; // Mods can patch this bool to tell whether their custom npcs can go or not (for example, BB Times has replacement characters for Principal)
    }
}
