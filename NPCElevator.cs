using UnityEngine;

namespace TheNPCElevator
{
	public class NPCElevator : EnvironmentObject
	{
		[SerializeField]
		internal Sprite[] sprElvs;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audClose, audOpen;

		[SerializeField]
		internal SpriteRenderer sprRenderer;
	}
}
