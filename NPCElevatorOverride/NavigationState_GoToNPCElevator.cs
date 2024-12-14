using TheNPCElevator.NPCElevatorClasses;

namespace TheNPCElevator.NPCElevatorOverride
{
	public class NavigationState_GoToNPCElevator(NPC npc, NPCElevator elevatorTarget) : NavigationState(npc, 999)
	{
		public override void Enter()
		{
			base.Enter();
			npc.Navigator.FindPath(elevatorTarget.TargettingSpot.FloorWorldPosition);
		}

		public override void DestinationEmpty()
		{
			if (npc.ec.CellFromPosition(npc.transform.position) != elevatorTarget.TargettingSpot)
				npc.Navigator.FindPath(elevatorTarget.TargettingSpot.FloorWorldPosition);
			else
				elevatorTarget.DespawnNPC(npc);
		}
	}
}
