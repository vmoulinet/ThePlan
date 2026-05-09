using UnityEngine;

public class MirrorDebris : MonoBehaviour
{
	[Header("Broken Rig")]
	public Transform BrokenMirrorPivotX;

	[Header("Debug")]
	public bool DebugDebris = false;
	public float PendulumIgnoreDuration = 0.2f;
	public float InheritedVelocityMultiplier = 0.65f;
	public float DirectionalImpulseMultiplier = 0.45f;
	public float MaxDirectionalImpulse = 8f;

	[Header("Sink")]
	public float SinkForceLight = 1f;
	public float SinkForceFast = 20f;
	public float SinkFastBelowY = -1f;
	public float SinkDestroyBelowY = -10f;

	SoundManager sound_manager;
	MirrorActor source_actor;
	Vector3 source_inherited_velocity = Vector3.zero;
	Vector3 source_impact_direction = Vector3.zero;
	float source_impact_speed = 0f;

	Rigidbody[] cached_bodies;
	Vector3[] initial_local_positions;
	Quaternion[] initial_local_rotations;
	bool is_sinking = false;
	bool snapshot_taken = false;
	float activate_time = 0f;

	public bool IsSinking => is_sinking;
	public float ActivateTime => activate_time;

	void Awake()
	{
		CacheAndSnapshot();
	}

	void CacheAndSnapshot()
	{
		if (snapshot_taken)
			return;

		cached_bodies = GetComponentsInChildren<Rigidbody>(true);
		initial_local_positions = new Vector3[cached_bodies.Length];
		initial_local_rotations = new Quaternion[cached_bodies.Length];

		for (int i = 0; i < cached_bodies.Length; i++)
		{
			if (cached_bodies[i] != null)
			{
				initial_local_positions[i] = cached_bodies[i].transform.localPosition;
				initial_local_rotations[i] = cached_bodies[i].transform.localRotation;
			}
		}

		snapshot_taken = true;
	}

	public void ResetForReuse()
	{
		CacheAndSnapshot();

		is_sinking = false;
		activate_time = Time.time;
		source_actor = null;
		source_inherited_velocity = Vector3.zero;
		source_impact_direction = Vector3.zero;
		source_impact_speed = 0f;

		for (int i = 0; i < cached_bodies.Length; i++)
		{
			if (cached_bodies[i] == null)
				continue;

			cached_bodies[i].transform.localPosition = initial_local_positions[i];
			cached_bodies[i].transform.localRotation = initial_local_rotations[i];
			cached_bodies[i].linearVelocity = Vector3.zero;
			cached_bodies[i].angularVelocity = Vector3.zero;
			cached_bodies[i].isKinematic = false;
			cached_bodies[i].useGravity = true;
			cached_bodies[i].detectCollisions = true;
		}

		gameObject.SetActive(true);
	}

	public void ReturnToPool()
	{
		gameObject.SetActive(false);
	}

	float sink_y_offset = 0f;

	public void StartSinking()
	{
		is_sinking = true;
		sink_y_offset = 0f;

		if (cached_bodies == null || cached_bodies.Length == 0)
			cached_bodies = GetComponentsInChildren<Rigidbody>(true);

		for (int i = 0; i < cached_bodies.Length; i++)
		{
			if (cached_bodies[i] != null)
			{
				cached_bodies[i].isKinematic = true;
				cached_bodies[i].detectCollisions = false;
			}
		}
	}

	void Update()
	{
		if (!is_sinking)
			return;

		float speed = sink_y_offset < SinkFastBelowY ? SinkForceFast : SinkForceLight;
		sink_y_offset -= speed * Time.deltaTime;

		if (sink_y_offset < SinkDestroyBelowY)
		{
			ReturnToPool();
			return;
		}

		transform.position += Vector3.down * speed * Time.deltaTime;
	}

	public void InitializeFromMirror(MirrorActor actor)
	{
		if (actor == null)
			return;

		activate_time = Time.time;
		source_actor = actor;
		source_inherited_velocity = actor.Velocity * InheritedVelocityMultiplier;
		source_impact_direction = actor.LastBreakImpactDirection;
		source_impact_speed = actor.LastBreakImpactSpeed;

		if (actor.MirrorManager != null)
			sound_manager = actor.MirrorManager.SoundManager;

		if (sound_manager != null)
			sound_manager.PlayMirrorBreak(actor.transform.position);

		cached_bodies = GetComponentsInChildren<Rigidbody>(true);
		transform.position = actor.transform.position;
		transform.rotation = actor.transform.rotation;

		if (BrokenMirrorPivotX != null)
		{
			float wrapped_panel_x = Mathf.DeltaAngle(0f, actor.CurrentPanelXAngle);
			BrokenMirrorPivotX.localRotation = Quaternion.AngleAxis(wrapped_panel_x, Vector3.right);
		}
	}

	public void ApplyImpact(Vector3 impactPoint, float force, float radius, float upwardModifier)
	{
		if (cached_bodies == null || cached_bodies.Length == 0)
			cached_bodies = GetComponentsInChildren<Rigidbody>(true);

		Rigidbody[] bodies = cached_bodies;
		Vector3 inherited_velocity = source_inherited_velocity;
		Vector3 directional_impulse = Vector3.zero;

		if (source_impact_direction.sqrMagnitude > 0.0001f && source_impact_speed > 0f)
		{
			float impulse_strength = Mathf.Min(source_impact_speed * DirectionalImpulseMultiplier, MaxDirectionalImpulse);
			directional_impulse = source_impact_direction.normalized * impulse_strength;
		}

		for (int i = 0; i < bodies.Length; i++)
		{
			Rigidbody body = bodies[i];
			if (body == null)
				continue;

			body.linearVelocity = inherited_velocity;
			body.AddExplosionForce(force, impactPoint, radius, upwardModifier, ForceMode.Impulse);

			if (directional_impulse.sqrMagnitude > 0.0001f)
				body.AddForce(directional_impulse, ForceMode.Impulse);
		}

		if (DebugDebris)
		{
			Debug.Log(
				name +
				" | debris impact | source_actor=" + (source_actor != null ? source_actor.name : "null") +
				" | force=" + force.ToString("F2") +
				" | radius=" + radius.ToString("F2") +
				" | source_direction=" + source_impact_direction.ToString("F2") +
				" | source_speed=" + source_impact_speed.ToString("F2") +
				" | inherited_velocity=" + inherited_velocity.ToString("F2") +
				" | directional_impulse=" + directional_impulse.ToString("F2") +
				" | bodies=" + bodies.Length
			);
		}
	}
}
