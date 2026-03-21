using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    [Header("One Shots")]
    public EventReference MirrorBreakEvent;
    public EventReference DebrisImpactEvent;

    [Header("Loops")]
    public EventReference DebrisLoopEvent;
    public EventReference PendulumDroneEvent;

    [Header("Parameter Names")]
    public string DebrisAmountParameter = "amount";
    public string PendulumAmountParameter = "amount";

    EventInstance debris_loop_instance;
    EventInstance pendulum_drone_instance;

    bool debris_loop_started = false;
    bool pendulum_loop_started = false;

    public void Initialize(SimulationManager sim)
    {
        Start_loops_if_needed();
    }

    void OnDestroy()
    {
        Stop_and_release(ref debris_loop_instance, ref debris_loop_started);
        Stop_and_release(ref pendulum_drone_instance, ref pendulum_loop_started);
    }

    void Start_loops_if_needed()
    {
        if (!debris_loop_started && !DebrisLoopEvent.IsNull)
        {
            debris_loop_instance = RuntimeManager.CreateInstance(DebrisLoopEvent);
            debris_loop_instance.start();
            debris_loop_started = true;
        }

        if (!pendulum_loop_started && !PendulumDroneEvent.IsNull)
        {
            pendulum_drone_instance = RuntimeManager.CreateInstance(PendulumDroneEvent);
            pendulum_drone_instance.start();
            pendulum_loop_started = true;
        }
    }

    void Stop_and_release(ref EventInstance instance, ref bool started_flag)
    {
        if (!started_flag)
            return;

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        started_flag = false;
    }

    public void PlayMirrorBreak(Vector3 world_position)
    {
        if (MirrorBreakEvent.IsNull)
            return;

        RuntimeManager.PlayOneShot(MirrorBreakEvent, world_position);
    }

    public void PlayDebrisImpact(Vector3 world_position)
    {
        if (DebrisImpactEvent.IsNull)
            return;

        RuntimeManager.PlayOneShot(DebrisImpactEvent, world_position);
    }

    [Header("Parameter Ranges")]
    public float DebrisAmountMin = 0f;
    public float DebrisAmountMax = 100f;
    public float PendulumAmountMin = -19.7f;
    public float PendulumAmountMax = 19.7f;

    public void SetDebrisAmountNormalized(float normalized_amount)
    {
        if (!debris_loop_started)
            return;

        float t = Mathf.Clamp01(normalized_amount);
        float mapped_amount = Mathf.Lerp(DebrisAmountMin, DebrisAmountMax, t);
        debris_loop_instance.setParameterByName(DebrisAmountParameter, mapped_amount);
    }

    public void SetDebrisAmount(float raw_amount)
    {
        if (!debris_loop_started)
            return;

        float clamped_amount = Mathf.Clamp(raw_amount, DebrisAmountMin, DebrisAmountMax);
        debris_loop_instance.setParameterByName(DebrisAmountParameter, clamped_amount);
    }

    public void SetPendulumDroneAmountRaw(float raw_amount)
    {
        if (!pendulum_loop_started)
            return;

        float clamped_amount = Mathf.Clamp(raw_amount, PendulumAmountMin, PendulumAmountMax);
        pendulum_drone_instance.setParameterByName(PendulumAmountParameter, clamped_amount);
    }
}