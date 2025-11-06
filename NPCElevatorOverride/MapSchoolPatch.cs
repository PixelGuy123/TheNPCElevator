using HarmonyLib;
using TheNPCElevator.NPCElevatorClasses;

namespace TheNPCElevator.NPCElevatorOverride;

[HarmonyPatch(typeof(Map), nameof(Map.Find))]
static class AutoRevealNPCElevatorEntrance
{
    static void Prefix(Map __instance, int posX, int posZ)
    {
        for (int i = 0; i < NPCElevator.elevatorPositions.Count; i++)
        {
            var kvp = NPCElevator.elevatorPositions[i];
            if (kvp.Key.x == posX && kvp.Key.z == posZ) // If position corresponds to a NPC Elevator entrance
            {                                           // Then *Find* the elevator's cell
                var cell = __instance.Ec.CellFromPosition(kvp.Value);
                __instance.Find(kvp.Value.x, kvp.Value.z, cell.ConstBin, cell.room);
            }
        }
    }

}