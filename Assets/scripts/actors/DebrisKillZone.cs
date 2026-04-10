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
				Debug.Log("[kill_zone] returning debris to pool " + debris.name);
			debris.ReturnToPool();
			return;
		}

		MirrorActor mirror = other.GetComponentInParent<MirrorActor>();
		if (mirror != null && !mirror.IsBroken)
		{
			if (DebugLog)
				Debug.Log("[kill_zone] respawning mirror " + mirror.name);
			if (mirror.CurrentSpawnPoint != null)
				mirror.ResetToSpawn(mirror.CurrentSpawnPoint);
		}
	}
}
