using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UHairGen
{
	public static class HGMath
	{
		#region Methods

		public static bool containsRadial(float x, float min, float max)
		{
			// Make sure all values are within a 0-1 range:
			if (x < 0) x += 1;
			else if (x >= 1) x -= 1;
			if (min < 0) min += 1;
			if (max >= 1) max -= 1;

			// Min and max are located within the same circle arc and in ascending order:
			if(max >= min)
			{
				return x >= min && x <= max;
			}
			// Min and max are not located on one same circle arc:
			else
			{
				// X lies within the min value and the radial clamping value (1.0):
				if (x >= min && x <= 1) return true;
				// X lies within the circle's lower clamping value (0.0) and the max value:
				else if (x >= 0 && x <= max) return true;
				else return false;
			}
		}

		public static Vector3 getRadialDirection(float x, float y)
		{
			Quaternion rotation = Quaternion.Euler(0, x * 360.0f, 0) * Quaternion.Euler(y * 180.0f, 0, 0);
			return rotation * Vector3.up;
		}

		/// <summary>
		/// Finds the closest point to a target along a line defined by two points.
		/// </summary>
		/// <param name="lineStart">The first (starting) point of the line segment.</param>
		/// <param name="lineEnd">The second (ending) point of the line segment.</param>
		/// <param name="target">The target position we're searching the closest point to on the line.</param>
		/// <returns></returns>
		public static Vector3 findClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 target)
		{
			Vector3 line = lineEnd - lineStart;
			Vector3 lineDir = line.normalized;
			Vector3 anchorLine = target - lineStart;
			return lineStart + Vector3.Project(anchorLine, lineDir);
		}

		#endregion
	}
}
