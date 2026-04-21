using UnityEngine;

public class CurtainRipple : MonoBehaviour
{
    [Header("Shockwave Prefab")]
    public GameObject ShockwavePrefab;

    [Header("Spawn")]
    public Transform SpawnPoint;
    public bool SpawnAtImpactPoint = false;
    public float SpawnRotationOffsetX = -45f;
    public float SpawnCooldown = 0.15f;
    public float AutoDestroyAfter = 4f;

    [Header("Detection")]
    public string DebrisTag = "";
    public LayerMask DebrisLayer = ~0;

    [Header("Debug")]
    public bool DebugLog = false;

    float last_spawn_time = -999f;

    void Reset()
    {
        SpawnPoint = transform;
    }

    void Start()
    {
        if (!DebugLog)
            return;

        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("[curtain_ripple] start | collider=" + (col != null ? col.GetType().Name : "NONE") +
                  " | is_trigger=" + (col != null && col.isTrigger) +
                  " | rigidbody=" + (rb != null) +
                  " | layer=" + LayerMask.LayerToName(gameObject.layer));
    }

    void OnTriggerEnter(Collider other)
    {
        if (DebugLog)
        {
            MirrorDebris mdb = other != null ? other.GetComponentInParent<MirrorDebris>() : null;
            Debug.Log("[curtain_ripple] trigger enter | other=" + (other != null ? other.name : "null") +
                      " | layer=" + (other != null ? LayerMask.LayerToName(other.gameObject.layer) : "n/a") +
                      " | has_mirror_debris=" + (mdb != null));
        }

        if (!ShouldRipple(other))
            return;

        Vector3 spawn_pos;
        Transform anchor = SpawnPoint != null ? SpawnPoint : transform;

        if (SpawnAtImpactPoint)
            spawn_pos = other.ClosestPoint(anchor.position);
        else
            spawn_pos = anchor.position;

        Quaternion spawn_rot = anchor.rotation * Quaternion.Euler(SpawnRotationOffsetX, 0f, 0f);

        SpawnShockwave(spawn_pos, spawn_rot);
    }

    bool ShouldRipple(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrEmpty(DebrisTag) && !other.CompareTag(DebrisTag))
        {
            if (other.GetComponentInParent<MirrorDebris>() == null)
                return false;
        }
        else if (string.IsNullOrEmpty(DebrisTag))
        {
            if (other.GetComponentInParent<MirrorDebris>() == null)
                return false;
        }

        return true;
    }

    void SpawnShockwave(Vector3 position, Quaternion rotation)
    {
        if (ShockwavePrefab == null)
            return;

        if (Time.time - last_spawn_time < SpawnCooldown)
            return;

        GameObject instance = Instantiate(ShockwavePrefab, position, rotation);

        if (AutoDestroyAfter > 0f)
            Destroy(instance, AutoDestroyAfter);

        last_spawn_time = Time.time;

        if (DebugLog)
            Debug.Log("[curtain_ripple] spawned shockwave at " + position.ToString("F2"));
    }
}
