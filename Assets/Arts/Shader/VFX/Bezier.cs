using UnityEngine;
using UnityEngine.Events;

namespace DayDream
{
	public class Bezier : MonoBehaviour
	{
		[Tooltip("Bezier control points in world space. Supports 3 (quadratic) or 4 (cubic) points.")]
		public Vector3[] path = new Vector3[3];

		[Tooltip("If true, interpret path points as world positions; otherwise local.")]
		public bool isWorld = true;

		[Min(0.0001f)]
		public float duration = 1f;

		[Tooltip("Easing curve over normalized time.")]
		public AnimationCurve ease = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public UnityEvent onFinished = new UnityEvent();

		private float _startTime;
		private bool _isPlaying;

		public void SetOnFinished(UnityEvent evt)
		{
			onFinished = evt ?? new UnityEvent();
		}

		public void ResetToBeginning()
		{
			_isPlaying = false;
			_startTime = Time.time;
			if (path == null || path.Length == 0) return;
			Vector3 p = Evaluate(0f);
			if (isWorld) transform.position = p; else transform.localPosition = p;
		}

		public void PlayForward()
		{
			_startTime = Time.time;
			_isPlaying = true;
		}

		private void Update()
		{
			if (!_isPlaying) return;
			float t = Mathf.Clamp01((Time.time - _startTime) / Mathf.Max(0.0001f, duration));
			float u = ease != null ? Mathf.Clamp01(ease.Evaluate(t)) : t;
			Vector3 p = Evaluate(u);
			if (isWorld) transform.position = p; else transform.localPosition = p;
			if (t >= 1f)
			{
				_isPlaying = false;
				try { onFinished?.Invoke(); } catch { }
			}
		}

		private Vector3 Evaluate(float t)
		{
			if (path == null || path.Length < 2) return transform.position;
			if (path.Length == 3)
			{
				return EvaluateQuadratic(path[0], path[1], path[2], t);
			}
			if (path.Length >= 4)
			{
				return EvaluateCubic(path[0], path[1], path[2], path[3], t);
			}
			// Fallback: linear
			return Vector3.Lerp(path[0], path[path.Length - 1], t);
		}

		private static Vector3 EvaluateQuadratic(in Vector3 a, in Vector3 b, in Vector3 c, float t)
		{
			float u = 1f - t;
			return u * u * a + 2f * u * t * b + t * t * c;
		}

		private static Vector3 EvaluateCubic(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, float t)
		{
			float u = 1f - t;
			float uu = u * u;
			float tt = t * t;
			return (uu * u) * a + (3f * uu * t) * b + (3f * u * tt) * c + (tt * t) * d;
		}
	}
}


