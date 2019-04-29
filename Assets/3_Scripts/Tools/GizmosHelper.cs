using System;
using UnityEngine;

namespace Tools
{
	public static class GizmosHelper
	{
		public static void DrawOrientation(Vector3 position, Quaternion rotation, float axisLength = 0.1f)
		{
			Vector3 x = rotation * Vector3.right * axisLength;
			Vector3 y = rotation * Vector3.up * axisLength;
			Vector3 z = rotation * Vector3.forward * axisLength;

			Gizmos.color = Color.red;
			Gizmos.DrawRay(position, x);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(position, y);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(position, z);
		}
		public static void DrawOrientation(Transform transform)
		{
			if (transform == null) return;

			Vector3 position = transform.position;

			Gizmos.color = Color.red;
			Gizmos.DrawRay(position, transform.right);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(position, transform.up);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(position, transform.forward);
		}
	}
}
