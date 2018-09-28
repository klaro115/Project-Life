using System;
using UnityEngine;

namespace UHairGen
{
	/// <summary>
	/// Place on the scalp where hair originates from.
	/// </summary>
	[System.Serializable]
	public struct HGRegion
	{
		#region Constructors

		public HGRegion(float _x, float _y, HGRange _length)
		{
			x = _x;
			y = _y;
			length = _length;
		}

		#endregion
		#region Fields

		public float x;
		public float y;
		public HGRange length;

		#endregion
		#region Methods

		public static HGRegion lerp(HGRegion a, HGRegion b, float k)
		{
			float x = Mathf.Lerp(a.x, b.x, k);
			float y = Mathf.Lerp(a.y, b.y, k);
			HGRange length = HGRange.lerp(a.length, b.length, k);

			return new HGRegion(x, y, length);
		}

		#endregion
	}
}
