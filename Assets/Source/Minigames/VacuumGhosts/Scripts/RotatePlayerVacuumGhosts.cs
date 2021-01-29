using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlayerVacuumGhosts : MonoBehaviour
{       /// <summary>The current angle in degrees.</summary>
	public float Angle;

	/// <summary>Should the <b>Angle</b> value be clamped?</summary>
	public bool Clamp;

	/// <summary>The minimum <b>Angle</b> value.</summary>
	public float ClampMin;

	/// <summary>The maximum <b>Angle</b> value.</summary>
	public float ClampMax;

	/// <summary>If you want this component to change smoothly over time, then this allows you to control how quick the changes reach their target value.
	/// -1 = Instantly change.
	/// 1 = Slowly change.
	/// 10 = Quickly change.</summary>
    public float Damping = -1.0f;

	[SerializeField]
	private float currentAngle;

	/// <summary>The <b>Angle</b> value will be incremented by the specified angle in degrees.</summary>
	public void IncrementAngle(float delta)
	{
		Angle += delta;
	}

	/// <summary>The <b>Angle</b> value will be decremented by the specified angle in degrees.</summary>
	public void DecrementAngle(float delta)
	{
		Angle -= delta;
	}

	/// <summary>This method will update the Angle value based on the specified vector.</summary>
	public void RotateToDelta(Vector2 delta)
	{
		if (delta.sqrMagnitude > 0.0f)
		{
			Angle = Mathf.Atan2(delta.x, delta.y) * Mathf.Rad2Deg;
		}
	}

	/// <summary>This method will immediately snap the current angle to its target value.</summary>
	[ContextMenu("Snap To Target")]
	public void SnapToTarget()
	{
		currentAngle = Angle;
	}

	protected virtual void Start()
	{
		currentAngle = Angle;
	}

	protected virtual void Update()
	{
		// Get t value
		var factor = Lean.Common.LeanHelper.GetDampenFactor(Damping, Time.deltaTime);

		if (Clamp == true)
		{
			Angle = Mathf.Clamp(Angle, ClampMin, ClampMax);
		}

		// Lerp angle
		currentAngle = Mathf.LerpAngle(currentAngle, Angle, factor);

		// Update rotation
		transform.rotation = Quaternion.Euler(0.0f, currentAngle, 0.0f);
	}
}

