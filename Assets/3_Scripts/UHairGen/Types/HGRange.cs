using UnityEngine;

namespace UHairGen
{
	[System.Serializable]
	public struct HGRange
	{
		#region Constructors

		public HGRange(float value, float margin = 0.0f)
		{
			min = value - margin;
			max = value + margin;
		}

		#endregion
		#region Fields

		public float min;
		public float max;

		#endregion
		#region Properties

		public float Center
		{
			get { return 0.5f * (min + max); }
		}

		#endregion
		#region Methods

		public float getRandom()
		{
			return Random.Range(min, max);
		}

		#endregion
	}
}
