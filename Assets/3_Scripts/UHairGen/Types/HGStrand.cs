using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHairGen
{
	/// <summary>
	/// A singular hair strand object in the pre-mesh phase. Used during hair mesh creation.
	/// </summary>
	[System.Serializable]
	public class HGStrand
	{
		#region Fields

		public float x = 0;
		public float y = 0;
		public float length = 0.25f;
		public float segments = 1;

		#endregion
	}
}
