using System;
using UnityEngine;

namespace ProceduralAnim.Poses
{
	[System.Serializable]
	public struct PAHandPose
	{
		#region Fields

		[Range(-1.0f, 1.0f)]
		public float thumbRoll;
		[Range(-1.0f, 1.0f)]
		public float thumbSpread;
		[Range(-1.0f, 1.0f)]
		public float thumbOpenClose;

		[Range(-1.0f, 1.0f)]
		public float fingerSpread;
		[Range(0.0f, 1.0f)]
		public float indexOpenClose;
		[Range(0.0f, 1.0f)]
		public float middleOpenClose;
		[Range(0.0f, 1.0f)]
		public float ringOpenClose;
		[Range(0.0f, 1.0f)]
		public float pinkyOpenClose;

		#endregion
		#region Properties

		public static PAHandPose Default => new PAHandPose()
		{
			thumbRoll = 0,
			thumbSpread = 0,
			thumbOpenClose = 0,

			fingerSpread = 0,
			indexOpenClose = 0,
			middleOpenClose = 0,
			ringOpenClose = 0,
			pinkyOpenClose = 0,
		};

		#endregion
	}
}
