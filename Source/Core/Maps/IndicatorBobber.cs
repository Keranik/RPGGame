using UnityEngine;

namespace RPGGame.Core.Maps;

/// <summary>
/// Simple bobbing animation for floating indicators above map tiles.
/// </summary>
public class IndicatorBobber : MonoBehaviour {
	private float k_baseY;
	private float k_amplitude;
	private float k_speed;
	private float k_offset;

	public void Initialize(float baseY, float amplitude = 0.15f, float speed = 2f) {
		k_baseY = baseY;
		k_amplitude = amplitude;
		k_speed = speed;
		k_offset = UnityEngine.Random.Range(0f, Mathf.PI * 2f); // Random start phase
	}

	private void Update() {
		var pos = transform.position;
		pos.y = k_baseY + Mathf.Sin(Time.time * k_speed + k_offset) * k_amplitude;
		transform.position = pos;
	}
}