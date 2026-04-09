using UnityEngine;

public class DebrisKillZone : MonoBehaviour
{
	[Header("Settings")]
	public bool DebugLog = false;

	void OnTriggerEnter(Collider other)
	{
		MirrorDebris debris = other.GetComponentInParent<MirrorDebris>();
		if (debris == null)
			return;

		if (DebugLog)
			Debug.Log("[debris_kill_zone] destroying " + debris.name);

		Destroy(debris.gameObject);
	}
}
