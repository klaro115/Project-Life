using UnityEngine;

namespace UHairGen
{
	public enum HGAnchorType
	{
		Point,			// Hair strands leaving the anchor point may follow whichever direction they may desire.
		Directional,	// Hair strands' flow is immediately redirected towards a fixed direction; their stiffness may be overriden.
		Spline,
	}

	[System.Serializable]
	public struct HGAnchor
	{
		#region Fields

		public string name;					// Name designation for identifying the anchor in editor.
		public float x;						// Radial position along horizontal plane. [0-1 maps to 0-360deg]
		public float y;						// Radial position along longitudinal lines. [0-1 maps to 0-180deg]
		public float pullRadius;            // Maximum distance up to which strands may be pulled into and bound by this anchor. [m]
		public float centerRadius;			// Minimum distance before a strand is considered to have reached the anchor. [m]
		public float overwriteStiffness;	// New stiffness value attributed to hair strands pulled in by this anchor. (negative means no overwriting)
		public HGAnchorType type;           // The type of anchor behaviour: Point, Directional, or Spline.
		public Vector3 exitDirection;       // Exit vector, aka the direction in which the hair strands are spit out. (Directional & Spline type only)
		public HGNode[] splineNodes;        // Spline nodes describing a curve for the anchored hair strands to follow.

		#endregion
		#region Methods

		public Vector3 getPosition(float bodyRadius)
		{
			return HGMath.getRadialDirection(x, y) * bodyRadius;
		}

		public bool checkExitPoint(HGNode n0, HGNode n1, float bodyRadius = 1.0f)
		{
			// First, see if the following node is going to cross the anchor's radius anyways:
			float centerRadiusSq = centerRadius * centerRadius;
			Vector3 anchorPos = getPosition(bodyRadius);
			if (Vector3.SqrMagnitude(anchorPos - n1.position) < centerRadiusSq) return true;

			// Alternately, check wether the line formed by the nodes n0 and n1 passed within the center radius of the anchor:
			Vector3 anchorProj = HGMath.findClosestPointOnLine(n0.position, n1.position, anchorPos);
			Vector3 projLine = anchorPos - anchorProj;
			return Vector3.SqrMagnitude(projLine) < centerRadiusSq;
		}

		public bool checkExitSpline(HGNode n0, HGNode n1)
		{
			// TODO
			return false;
		}

		#endregion
	}
}
