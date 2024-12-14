using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNPCElevator.NPCElevatorClasses
{
    public class NPCElevator : EnvironmentObject
    {
        public void Initialize(Direction spawnDir, IntVector2 position)
        {
            dir = spawnDir;
            this.position = position;
            initialized = true;
        }

        public void DespawnNPC(NPC npc)
        {
            npc.enabled = false;
			npc.Navigator.enabled = false;
			npc.Navigator.Entity.SetFrozen(true);
            npc.Navigator.Entity.Teleport(ec.CellFromPosition(position).FloorWorldPosition + new Vector3(Random.Range(-offsetForTile.x, offsetForTile.x), 0, Random.Range(-offsetForTile.x, offsetForTile.x)));
            ec.Npcs.Remove(npc);
            npcsToDestroy.Add(npc);

            if (collectNpcCorou != null)
                StopCoroutine(collectNpcCorou);
            collectNpcCorou = StartCoroutine(CollectNPC());
        }
        public void Close(bool close)
        {
            if (close)
            {
                openState = false;
                //if (IsFullyOpened)
                //	audMan.PlaySingle(audClose);
                return;
            }
            openState = true;
            //if (IsFullyClosed)
            //	audMan.PlaySingle(audOpen);
        }

        void TryDespawnNPC(NPC npc)
        {
            string name = npc.name;
            try
            {
                npc.Despawn(); // In case modded npcs do something wrong, it's already in a try-catch block
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"----- WARNING: the NPC ${name} failed to despawn! Here\'s the exception it has thrown!");

                if (npc)
                    Destroy(npc.gameObject); // Tries to forcefully kill the npc, and re-throw the exception

                Debug.LogException(e);
            }
        }

        void Update() // frame++ is opening, frame-- is closing
        {
            if (!initialized)
                return;

            //frame += (openState ? 1f : -1f) * ec.EnvironmentTimeScale * Time.deltaTime * 3.5f;
            //frame = Mathf.Clamp(frame, 0, sprElvs.Length - 1);

            //sprRenderer.sprite = sprElvs[Mathf.FloorToInt(frame)];
        }

        IEnumerator CollectNPC()
        {
            Close(false);
            float t = delayToClose;
            while (t > 0f)
            {
                t -= ec.EnvironmentTimeScale * Time.deltaTime;
                yield return null;
            }

            Close(true);

            while (!IsFullyClosed)
                yield return null;

            while (npcsToDestroy.Count != 0)
            {
                if (npcsToDestroy[0])
                    TryDespawnNPC(npcsToDestroy[0]);
                npcsToDestroy.RemoveAt(0);
            }
        }

        [SerializeField]
        internal Sprite[] sprElvs = [];

        [SerializeField]
        internal AudioManager audMan;

        [SerializeField]
        internal SoundObject audClose, audOpen;

        [SerializeField]
        internal SpriteRenderer sprRenderer;

        [SerializeField]
        internal Vector2 offsetForTile = Vector2.one * 2.5f;

        [SerializeField]
        internal float delayToClose = 5f;

        readonly List<NPC> npcsToDestroy = [];
        Coroutine collectNpcCorou;

        Direction dir;
        IntVector2 position;
        float frame = 0f;
        bool openState = false, initialized = false;

        public bool IsFullyClosed => frame == 0f;
        public bool IsFullyOpened => frame == sprElvs.Length - 1;
        public Cell TargettingSpot => ec.CellFromPosition(position + dir.ToIntVector2());
    }
}
