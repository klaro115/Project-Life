using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHairGen
{
	/// <summary>
	/// A singular hair strand object in the pre-mesh phase. Used during hair mesh creation.
	/// </summary>
	[System.Serializable]
	public struct HGStrand
	{
		#region Fields

		public float x;			// Angular coordinate along circumference of the skull. (0-1 maps to 0-360deg)
		public float y;			// Angular coordinate along the longitudinal lines of the skull. (0-1 maps to 0-180deg)
		public float length;	// Length of the entire hair strand. [m]
		public float width;		// Width/diameter of the entire hair strand's geometry. [m]
		public float segments;	// Number of subdivisions or quads the strand is to be made up of.
		public Vector3 forward;	// Hair growth direction, aka skull surface normal at the base of the strand.
		public Vector3 normal;	// Hair growth normal, aka local 'up' direction along which to align quads.

		public int vIndexStart;	// Index of the first vertex in the geometry buffer.
		public int vIndexCount;	// Number of vertices making up this hair strand's geometry.
		public int tIndexStart;	// Index of the first triangle index in the geometry buffer.
		public int tIndexCount; // Number of triangle indices making up this strand's geometry. (index count = 3 x triangle count)

		public HGNode[] nodes;  // Buffer for storing the positions of the individual edge loops where segments/divisions connect.
		public int nodeCount;	// The number of node entries actually used/generated within the nodes buffer. (node count = quad count + 1)

		#endregion
		#region Properties

		public static HGStrand Default
		{
			get
			{
				return new HGStrand()
				{
					x = 0,
					y = 0,
					length = 0.25f,
					width = 0.05f,
					segments = 1,
					forward = Vector3.up,
					normal = Vector3.right,

					vIndexStart = -1,
					vIndexCount = 0,
					tIndexStart = -1,
					tIndexCount = 0,

					nodes = null,
					nodeCount = 0,
				};
			}
		}

		#endregion
	}
}
