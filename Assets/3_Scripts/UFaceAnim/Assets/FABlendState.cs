using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[System.Serializable]
	public struct FABlendState
	{
		#region Fields

		public float mouthOpenClose;
		public float mouthShowTeeth;
		public float mouthInOut;
		public float mouthCornerL;
		public float mouthCornerR;

		public float browsInOut;
		public float browsL;
		public float browsR;
		public float browsSharpFlat;

		public float eyesCloseL;
		public float eyesCloseR;
		public float eyesWander;
		public Vector2 eyesDir;

		#endregion
		#region Properties

		public static FABlendState Default
		{
			get
			{
				FABlendState bs = new FABlendState();

				bs.mouthOpenClose = 0.0f;
				bs.mouthShowTeeth = 0.0f;
				bs.mouthInOut = 0.0f;
				bs.mouthCornerL = 0.0f;
				bs.mouthCornerR = 0.0f;

				bs.browsInOut = 0.0f;
				bs.browsL = 0.0f;
				bs.browsR = 0.0f;
				bs.browsSharpFlat = 0.0f;

				bs.eyesCloseL = 0.0f;
				bs.eyesCloseR = 0.0f;
				bs.eyesWander = 0.02f;
				bs.eyesDir = Vector2.zero;

				return bs;
			}
		}

		#endregion
		#region Methods

		public static FABlendState lerp(ref FABlendState bs0, ref FABlendState bs1, float k)
		{
			FABlendState res = Default;

			res.mouthOpenClose = Mathf.Lerp(bs0.mouthOpenClose, bs1.mouthOpenClose, k);
			res.mouthShowTeeth = Mathf.Lerp(bs0.mouthShowTeeth, bs1.mouthShowTeeth, k);
			res.mouthInOut = Mathf.Lerp(bs0.mouthInOut, bs1.mouthInOut, k);
			res.mouthCornerL = Mathf.Lerp(bs0.mouthCornerL, bs1.mouthCornerL, k);
			res.mouthCornerR = Mathf.Lerp(bs0.mouthCornerR, bs1.mouthCornerR, k);

			res.browsInOut = Mathf.Lerp(bs0.browsInOut, bs1.browsInOut, k);
			res.browsL = Mathf.Lerp(bs0.browsL, bs1.browsL, k);
			res.browsR = Mathf.Lerp(bs0.browsR, bs1.browsR, k);
			res.browsSharpFlat = Mathf.Lerp(bs0.browsSharpFlat, bs1.browsSharpFlat, k);

			res.eyesCloseL = Mathf.Lerp(bs0.eyesCloseL, bs1.eyesCloseL, k);
			res.eyesCloseR = Mathf.Lerp(bs0.eyesCloseR, bs1.eyesCloseR, k);
			res.eyesWander = Mathf.Lerp(bs0.eyesWander, bs1.eyesWander, k);
			res.eyesDir = Vector2.Lerp(bs0.eyesDir, bs1.eyesDir, k);

			return res;
		}

		#endregion
	}
}

