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
	public struct HGAnchorNode
	{
		#region Constructors

		public HGAnchorNode(Vector3 pos, Vector3 rot, float tan = 0.1f)
		{
			position = pos;
			rotation = rot;
			tangent = tan;
		}

		#endregion
		#region Fields

		public Vector3 position;
		public Vector3 rotation;
		public float tangent;

		#endregion
	}

	[System.Serializable]
	public struct HGAnchor
	{
		#region Fields

		public string name;					// Name designation for identifying the anchor in editor.
		public float x;						// Radial position along horizontal plane. [0-1 maps to 0-360deg]
		public float y;						// Radial position along longitudinal lines. [0-1 maps to 0-180deg]
		public float pullRadius;			// Maximum distance up to which strands may be pulled into and bound by this anchor. [m]
		public float overwriteStiffness;	// New stiffness value attributed to hair strands pulled in by this anchor. (negative means no overwriting)
		public HGAnchorType type;           // The type of anchor behaviour: Point, Directional, or Spline.
		public Vector3 exitDirection;       // Exit vector, aka the direction in which the hair strands are spit out. (Directional & Spline type only)
		public HGAnchorNode[] splineNodes;  // Spline nodes describing a curve for the anchored hair strands to follow.

		#endregion
	}
}
