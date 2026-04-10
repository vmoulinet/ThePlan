using UnityEngine;

public class DebrisKillZone : MonoBehaviour
{
	[Header("Settings")]
	public bool DebugLog = false;

	void OnTriggerEnter(Collider other)
	{
		MirrorDebris debris = other.GetComponentInParent<MirrorDebris>();
		if (debris != null)
		{
			if (DebugLog)
				Debug.Log("[kill_zone] destroying debris " + debris.name);
			Destroy(debris.gameObject);
			return;
		}

		MirrorActor mirror = other.GetComponentInParent<MirrorActor>();
		if (mirror != null && !mirror.IsBroken)
		{
			if (DebugLog)
				Debug.Log("[kill_zone] force breaking mirror " + mirror.name);
			mirror.ForceBreak();
		}
	}
}
