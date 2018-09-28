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

		public float x;
		public float y;
		public float length;
		public float segments;
		Vector3 forward;
		Vector3 normal;

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
					segments = 1,
					forward = Vector3.up,
					normal = Vector3.right,
				};
			}
		}

		#endregion
	}
}
