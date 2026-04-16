using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SkyController : MonoBehaviour
{
    [Header("Volume")]
    public Volume GlobalVolume;

    [Header("HDRI")]
    public Cubemap[] HdriVariants;
    public int CurrentVariantIndex = 0;

    [Header("Sky Parameters")]
    [Range(0f, 360f)]
    public float Rotation = 0f;
    [Range(-10f, 10f)]
    public float Exposure = 1.5f;

    [Header("Color")]
    [ColorUsage(false, true)]
    public Color ColorTint = Color.white;
    [Range(-100f, 100f)]
    public float Temperature = 0f;
    [Range(-100f, 100f)]
    public float Tint = 0f;
    [Range(-100f, 100f)]
    public float Saturation = 0f;

    HDRISky hdri_sky;
    ColorAdjustments color_adjustments;
    WhiteBalance white_balance;

    float last_rotation;
    float last_exposure;
    int last_variant_index;
    Color last_color_tint;
    float last_temperature;
    float last_tint;
    float last_saturation;

    void Start()
    {
        FetchOverrides();
        ApplyAll();
        CacheState();
    }

    void Update()
    {
        if (!HasChanged())
            return;

        ApplyAll();
        CacheState();
    }

    void FetchOverrides()
    {
        if (GlobalVolume == null)
            return;

        VolumeProfile profile = GlobalVolume.profile;

        profile.TryGet(out hdri_sky);
        if (hdri_sky == null)
            Debug.LogWarning("SkyController | No HDRISky override found. Add one via Add Override > Sky > HDRI Sky.");

        if (!profile.TryGet(out color_adjustments))
        {
            color_adjustments = profile.Add<ColorAdjustments>();
        }

        if (!profile.TryGet(out white_balance))
        {
            white_balance = profile.Add<WhiteBalance>();
        }
    }

    void ApplyAll()
    {
        ApplySky();
        ApplyColor();
    }

    void ApplySky()
    {
        if (hdri_sky == null)
            return;

        if (HdriVariants != null && HdriVariants.Length > 0)
        {
            int index = Mathf.Clamp(CurrentVariantIndex, 0, HdriVariants.Length - 1);
            if (HdriVariants[index] != null)
            {
                hdri_sky.hdriSky.value = HdriVariants[index];
                hdri_sky.hdriSky.overrideState = true;
            }
        }

        hdri_sky.rotation.value = Rotation;
        hdri_sky.rotation.overrideState = true;

        hdri_sky.exposure.value = Exposure;
        hdri_sky.exposure.overrideState = true;
    }

    void ApplyColor()
    {
        if (color_adjustments != null)
        {
            color_adjustments.colorFilter.value = ColorTint;
            color_adjustments.colorFilter.overrideState = true;

            color_adjustments.saturation.value = Saturation;
            color_adjustments.saturation.overrideState = true;
        }

        if (white_balance != null)
        {
            white_balance.temperature.value = Temperature;
            white_balance.temperature.overrideState = true;

            white_balance.tint.value = Tint;
            white_balance.tint.overrideState = true;
        }
    }

    bool HasChanged()
    {
        return !Mathf.Approximately(Rotation, last_rotation)
            || !Mathf.Approximately(Exposure, last_exposure)
            || CurrentVariantIndex != last_variant_index
            || ColorTint != last_color_tint
            || !Mathf.Approximately(Temperature, last_temperature)
            || !Mathf.Approximately(Tint, last_tint)
            || !Mathf.Approximately(Saturation, last_saturation);
    }

    void CacheState()
    {
        last_rotation = Rotation;
        last_exposure = Exposure;
        last_variant_index = CurrentVariantIndex;
        last_color_tint = ColorTint;
        last_temperature = Temperature;
        last_tint = Tint;
        last_saturation = Saturation;
    }

    public void SetVariant(int index)
    {
        CurrentVariantIndex = index;
    }

    public void SetRotation(float degrees)
    {
        Rotation = degrees;
    }

    public void SetExposure(float value)
    {
        Exposure = value;
    }

    public void SetColorTint(Color color)
    {
        ColorTint = color;
    }

    public void SetTemperature(float value)
    {
        Temperature = Mathf.Clamp(value, -100f, 100f);
    }

    public void SetTint(float value)
    {
        Tint = Mathf.Clamp(value, -100f, 100f);
    }

    public void SetSaturation(float value)
    {
        Saturation = Mathf.Clamp(value, -100f, 100f);
    }
}
