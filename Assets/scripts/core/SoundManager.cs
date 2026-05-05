using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[Header("One Shots")]
	public AudioClip[] MirrorBreakClips;
	public AudioClip[] DebrisImpactClips;
	public AudioSource OneShotSource;
	public float MirrorBreakVolume = 1f;
	public float DebrisImpactVolume = 1f;
	public float OneShotPitchRandom = 0.06f;

	[Header("Loops")]
	public AudioSource DebrisLoopSource;
	public AudioSource PendulumDroneSource;
	public AudioClip DebrisLoopClip;
	public AudioClip PendulumDroneClip;

	[Header("Debris Loop")]
	public float DebrisAmountMin = 0f;
	public float DebrisAmountMax = 100f;
	public float DebrisLoopMaxVolume = 0.8f;
	public float DebrisLoopMinPitch = 0.85f;
	public float DebrisLoopMaxPitch = 1.25f;
	public float DebrisLoopFadeSpeed = 8f;

	[Header("Pendulum Drone")]
	public float PendulumAmountMin = -19.7f;
	public float PendulumAmountMax = 19.7f;
	public float PendulumDroneMaxVolume = 0.75f;
	public float PendulumDroneFadeSpeed = 6f;

	[Header("Debug")]
	public bool DebugSound = false;

	float target_debris_amount = 0f;
	float current_debris_amount = 0f;
	float target_pendulum_amount = 0f;
	float current_pendulum_amount = 0f;

	public void Initialize(SimulationManager sim)
	{
		Ensure_audio_sources();
		Start_loops_if_needed();

		if (DebugSound)
		{
			Debug.Log(
				"[sound_manager] initialize | mirror_break_clips=" + (MirrorBreakClips != null ? MirrorBreakClips.Length : 0) +
				" | debris_impact_clips=" + (DebrisImpactClips != null ? DebrisImpactClips.Length : 0) +
				" | debris_loop=" + (DebrisLoopSource != null ? DebrisLoopSource.name : "null") +
				" | pendulum_drone=" + (PendulumDroneSource != null ? PendulumDroneSource.name : "null")
			);
		}
	}

	void Awake()
	{
		Ensure_audio_sources();
	}

	void Update()
	{
		Update_debris_loop();
		Update_pendulum_drone();
	}

	void Ensure_audio_sources()
	{
		if (OneShotSource == null)
		{
			GameObject one_shot_object = new GameObject("one_shot_audio_source");
			one_shot_object.transform.SetParent(transform, false);
			OneShotSource = one_shot_object.AddComponent<AudioSource>();
			OneShotSource.playOnAwake = false;
			OneShotSource.spatialBlend = 0f;
		}

		if (DebrisLoopSource == null)
			DebrisLoopSource = Create_loop_source("debris_loop_audio_source");

		if (PendulumDroneSource == null)
			PendulumDroneSource = Create_loop_source("pendulum_drone_audio_source");

		if (DebrisLoopSource != null && DebrisLoopClip != null)
			DebrisLoopSource.clip = DebrisLoopClip;

		if (PendulumDroneSource != null && PendulumDroneClip != null)
			PendulumDroneSource.clip = PendulumDroneClip;
	}

	AudioSource Create_loop_source(string source_name)
	{
		GameObject source_object = new GameObject(source_name);
		source_object.transform.SetParent(transform, false);

		AudioSource source = source_object.AddComponent<AudioSource>();
		source.playOnAwake = false;
		source.loop = true;
		source.volume = 0f;
		source.spatialBlend = 0f;
		return source;
	}

	void Start_loops_if_needed()
	{
		Start_loop_if_needed(DebrisLoopSource, DebrisLoopClip);
		Start_loop_if_needed(PendulumDroneSource, PendulumDroneClip);
	}

	void Start_loop_if_needed(AudioSource source, AudioClip clip)
	{
		if (source == null || clip == null)
			return;

		if (source.clip != clip)
			source.clip = clip;

		source.loop = true;
		source.pitch = 1f;

		if (!source.isPlaying)
			source.Play();
	}

	public void PlayMirrorBreak(Vector3 world_position)
	{
		Play_one_shot(MirrorBreakClips, world_position, MirrorBreakVolume, "mirror_break");
	}

	public void PlayDebrisImpact(Vector3 world_position)
	{
		Play_one_shot(DebrisImpactClips, world_position, DebrisImpactVolume, "debris_impact");
	}

	void Play_one_shot(AudioClip[] clips, Vector3 world_position, float volume, string label)
	{
		if (clips == null || clips.Length == 0)
			return;

		AudioClip clip = clips[Random.Range(0, clips.Length)];
		if (clip == null)
			return;

		float pitch = 1f + Random.Range(-OneShotPitchRandom, OneShotPitchRandom);

		GameObject one_shot_object = new GameObject("one_shot_" + label);
		one_shot_object.transform.position = world_position;

		AudioSource source = one_shot_object.AddComponent<AudioSource>();
		source.clip = clip;
		source.volume = Mathf.Clamp01(volume);
		source.pitch = pitch;
		source.spatialBlend = 1f;
		source.playOnAwake = false;
		source.Play();

		Destroy(one_shot_object, clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)) + 0.1f);

		if (DebugSound)
		{
			Debug.Log(
				"[sound_manager] one_shot | label=" + label +
				" | clip=" + clip.name +
				" | position=" + world_position.ToString("F2") +
				" | volume=" + volume.ToString("F2") +
				" | pitch=" + pitch.ToString("F2")
			);
		}
	}

	public void SetDebrisAmountNormalized(float normalized_amount)
	{
		float t = Mathf.Clamp01(normalized_amount);
		target_debris_amount = Mathf.Lerp(DebrisAmountMin, DebrisAmountMax, t);
	}

	public void SetDebrisAmount(float raw_amount)
	{
		target_debris_amount = Mathf.Clamp(raw_amount, DebrisAmountMin, DebrisAmountMax);
	}

	public void SetPendulumDroneAmountRaw(float raw_amount)
	{
		target_pendulum_amount = Mathf.Clamp(raw_amount, PendulumAmountMin, PendulumAmountMax);
	}

	void Update_debris_loop()
	{
		if (DebrisLoopSource == null)
			return;

		current_debris_amount = Mathf.MoveTowards(current_debris_amount, target_debris_amount, DebrisLoopFadeSpeed * Time.unscaledDeltaTime * Mathf.Max(1f, DebrisAmountMax));

		float normalized_amount = Mathf.InverseLerp(DebrisAmountMin, DebrisAmountMax, current_debris_amount);
		DebrisLoopSource.volume = Mathf.Clamp01(normalized_amount) * DebrisLoopMaxVolume;
		DebrisLoopSource.pitch = Mathf.Lerp(DebrisLoopMinPitch, DebrisLoopMaxPitch, Mathf.Clamp01(normalized_amount));

		if (DebrisLoopSource.clip != null && !DebrisLoopSource.isPlaying)
			DebrisLoopSource.Play();
	}

	void Update_pendulum_drone()
	{
		if (PendulumDroneSource == null)
			return;

		current_pendulum_amount = Mathf.MoveTowards(current_pendulum_amount, target_pendulum_amount, PendulumDroneFadeSpeed * Time.unscaledDeltaTime * Mathf.Max(1f, PendulumAmountMax));

		float normalized_side = Mathf.InverseLerp(PendulumAmountMin, PendulumAmountMax, current_pendulum_amount);
		float center_amount = 1f - Mathf.Abs((normalized_side * 2f) - 1f);

		PendulumDroneSource.volume = Mathf.Clamp01(center_amount) * PendulumDroneMaxVolume;

		if (PendulumDroneSource.clip != null && !PendulumDroneSource.isPlaying)
			PendulumDroneSource.Play();
	}
}