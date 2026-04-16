using UnityEngine;

public class CurtainRipple : MonoBehaviour
{
    const int MAX_RIPPLES = 8;

    [Header("References")]
    public Renderer CurtainRenderer;

    [Header("Ripple")]
    public float RippleSpeed = 3f;
    public float RippleWavelength = 0.5f;
    public float RippleAmplitude = 0.08f;
    public float RippleLifetime = 2f;

    [Header("Detection")]
    public string DebrisTag = "";
    public LayerMask DebrisLayer = ~0;

    [Header("Debug")]
    public bool DebugLog = false;

    Vector4[] ripple_origins = new Vector4[MAX_RIPPLES];
    Vector4[] ripple_params = new Vector4[MAX_RIPPLES]; // x=birth_time, y=amplitude, z=lifetime, w=active
    int next_ripple_index = 0;
    MaterialPropertyBlock mpb;

    static readonly int id_ripple_origins = Shader.PropertyToID("_RippleOrigins");
    static readonly int id_ripple_params = Shader.PropertyToID("_RippleParams");
    static readonly int id_ripple_speed = Shader.PropertyToID("_RippleSpeed");
    static readonly int id_ripple_wavelength = Shader.PropertyToID("_RippleWavelength");
    static readonly int id_ripple_count = Shader.PropertyToID("_RippleCount");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        if (CurtainRenderer == null)
            CurtainRenderer = GetComponent<Renderer>();

        for (int i = 0; i < MAX_RIPPLES; i++)
        {
            ripple_origins[i] = Vector4.zero;
            ripple_params[i] = Vector4.zero;
        }
    }

    void Update()
    {
        PushToShader();
    }

    void PushToShader()
    {
        if (CurtainRenderer == null)
            return;

        CurtainRenderer.GetPropertyBlock(mpb);
        mpb.SetVectorArray(id_ripple_origins, ripple_origins);
        mpb.SetVectorArray(id_ripple_params, ripple_params);
        mpb.SetFloat(id_ripple_speed, RippleSpeed);
        mpb.SetFloat(id_ripple_wavelength, RippleWavelength);
        mpb.SetInt(id_ripple_count, MAX_RIPPLES);
        CurtainRenderer.SetPropertyBlock(mpb);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ShouldRipple(other))
            return;

        Vector3 hit_point = other.ClosestPoint(transform.position);
        AddRipple(hit_point);
    }

    bool ShouldRipple(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrEmpty(DebrisTag) && !other.CompareTag(DebrisTag))
        {
            // Fallback: check for MirrorDebris component
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

    public void AddRipple(Vector3 world_position)
    {
        int index = next_ripple_index;
        next_ripple_index = (next_ripple_index + 1) % MAX_RIPPLES;

        ripple_origins[index] = new Vector4(world_position.x, world_position.y, world_position.z, 1f);
        ripple_params[index] = new Vector4(Time.time, RippleAmplitude, RippleLifetime, 1f);

        if (DebugLog)
            Debug.Log("[curtain_ripple] new ripple at " + world_position.ToString("F2") + " | slot=" + index);
    }
}
