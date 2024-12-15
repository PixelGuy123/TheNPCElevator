using System.Collections.Generic;
using UnityEngine;


namespace TheNPCElevator.NPCElevatorClasses
{
    public class Structure_NPCElevator : StructureBuilder
    {
        public override void Generate(LevelGenerator lg, System.Random rng)
        {
            base.Generate(lg, rng);
            List<Cell> potentialSpots = ec.mainHall.GetNewTileList();

            List<IntVector2> finalSpots = [];
            List<Direction> finalDirections = [];

            for (int i = 0; i < potentialSpots.Count; i++)
            {
                if (potentialSpots[i].HasAnyAllCoverage)
                {
                    var dir = potentialSpots[i].RandomUnoccupiedDirection(potentialSpots[i].HardWallAvailabilityBin, rng);
                    var nextCell = ec.CellFromPosition(potentialSpots[i].position + dir.ToIntVector2());
                    if (nextCell.Null)
                    {
                        finalSpots.Add(nextCell.position);
                        finalDirections.Add(dir.GetOpposite());
                    }
                }
            }


            if (finalSpots.Count == 0)
            {
                Debug.LogWarning("Structure_NPCElevator failed to find any potential spots for the elevators.");
				Finished();
                return;
            }

            int amount = rng.Next(parameters.minMax[0].x, parameters.minMax[0].z + 1);

            for (int i = 0; i < amount; i++)
            {
                if (finalSpots.Count == 0)
                    break;

                int idx = rng.Next(finalSpots.Count);

                var elevatorRoom = CreateElevatorRoom(lg, finalSpots[idx], finalDirections[idx]);
                elevatorRoom.ConnectRooms(ec.mainHall);

                ec.CreateCell(15, elevatorRoom.transform, finalSpots[idx], elevatorRoom);
                ec.ConnectCells(finalSpots[idx], finalDirections[idx]);
                var outCell = ec.CellFromPosition(finalSpots[idx] + finalDirections[idx].ToIntVector2());
                outCell.Block(finalDirections[idx].GetOpposite(), true);
                outCell.HardCoverWall(finalDirections[idx].GetOpposite(), true);

                var cell = ec.CellFromPosition(finalSpots[idx]);
                cell.HardCoverEntirely();
                cell.offLimits = true;

                var npcEl = Instantiate(npcElevatorPre, cell.ObjectBase);
                npcEl.Ec = ec;
                npcEl.transform.position = cell.FloorWorldPosition;
                npcEl.transform.rotation = finalDirections[idx].ToRotation();
				npcEl.Initialize(finalDirections[idx], finalSpots[idx]);


                finalSpots.RemoveAt(idx);
                finalDirections.RemoveAt(idx);
            }

            Finished();
        }

        public override void Load(List<StructureData> data)
        {
            base.Load(data);
            // Not really much, since you cannot create cells (or room controllers) from here

            Finished();
        }

        RoomController CreateElevatorRoom(LevelGenerator lg, IntVector2 position, Direction dir)
        {
            var elevatorRoom = Instantiate(lg.roomControllerPre, ec.transform);
            elevatorRoom.name = "NPCElevator_RoomController_" + (++roomId);
            elevatorRoom.ec = ec;
            elevatorRoom.color = new(0.75f, 0.75f, 0.75f);
            elevatorRoom.transform.localPosition = Vector3.zero;
            elevatorRoom.type = RoomType.Room;
            elevatorRoom.category = RoomCategory.Null;
            elevatorRoom.offLimits = true;
            elevatorRoom.acceptsExits = false;
            elevatorRoom.acceptsPosters = false;
            elevatorRoom.hasActivity = false;

            elevatorRoom.wallTex = wallTex;
            elevatorRoom.ceilTex = ceilingTex;
            elevatorRoom.florTex = floorTex;
            elevatorRoom.GenerateTextureAtlas();

            elevatorRoom.position = position;
            elevatorRoom.size = new(1, 1);
            elevatorRoom.maxSize = new(1, 1);
            elevatorRoom.dir = dir;

            ec.rooms.Add(elevatorRoom); // Without this, culling manager crashes because it'll never assign a chunk to the cells of this room
            return elevatorRoom;
        }

        int roomId = 0;

        [SerializeField]
        internal Texture2D wallTex, floorTex, ceilingTex;

        [SerializeField]
        internal NPCElevator npcElevatorPre;
    }
}
