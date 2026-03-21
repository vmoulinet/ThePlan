using UnityEngine;
using TMPro;

public class TMPSubtleShake : MonoBehaviour
{
	public TMP_Text target;

	[Header("Shake")]
	public float amplitude = 0.00055f;
	public float vertical_ratio = 0.18f;
	public float frequency = 36f;
	public float noise_frequency = 52f;
	public float noise_amount = 0.06f;

	[Header("Drift")]
	public float drift_speed = 0.01f;

	Vector3 base_pos;
	float time_offset;
	RectTransform rect_transform;

	void Awake()
	{
		if (target == null)
			target = GetComponent<TMP_Text>();

		rect_transform = transform as RectTransform;

		if (rect_transform != null)
			base_pos = rect_transform.anchoredPosition3D;
		else
			base_pos = transform.localPosition;

		time_offset = Random.value * 100f;
	}

	void OnEnable()
	{
		if (rect_transform == null)
			rect_transform = transform as RectTransform;

		if (rect_transform != null)
			base_pos = rect_transform.anchoredPosition3D;
		else
			base_pos = transform.localPosition;
	}

	void LateUpdate()
	{
		float t = Time.unscaledTime + time_offset;

		float x = Mathf.Sin(t * frequency) * amplitude;
		float y = Mathf.Sin(t * (frequency * 0.83f) + 1.37f) * amplitude * vertical_ratio;

		float nx = (Mathf.PerlinNoise(t * noise_frequency, 0f) - 0.5f) * amplitude * noise_amount;
		float ny = (Mathf.PerlinNoise(0f, t * noise_frequency * 0.85f) - 0.5f) * amplitude * noise_amount * vertical_ratio;

		time_offset += Time.unscaledDeltaTime * drift_speed;

		Vector3 offset = new Vector3(x + nx, y + ny, 0f);

		if (rect_transform != null)
			rect_transform.anchoredPosition3D = base_pos + offset;
		else
			transform.localPosition = base_pos + offset;
	}

	public void Reset_position()
	{
		if (rect_transform != null)
			rect_transform.anchoredPosition3D = base_pos;
		else
			transform.localPosition = base_pos;
	}

}