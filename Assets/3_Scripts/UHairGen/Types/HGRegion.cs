using System;

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
	}
}
