using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFaceAnim.Audio
{
	[System.Serializable]
	public struct FAPhonemeVector
	{
		#region Fields

		public float weightGroup0;
		public float weightGroup1;
		public float weightGroup2;
		public float weightGroup3;
		public float weightGroup4;
		public float weightGroup5;
		public float weightGroup6;
		public float weightGroup7;
		public float weightGroup8;
		public float weightGroup9;

		public float energy;

		#endregion
		#region Methods

		public void findMax(out FABasePhonemes phoneme, out float weight)
		{
			weight = -1.0f;
			phoneme = FABasePhonemes.None;
			if(weightGroup0 > weight)
			{
				weight = weightGroup0;
				phoneme = FABasePhonemes.Group0_AI;
			}
			if (weightGroup1 > weight)
			{
				weight = weightGroup1;
				phoneme = FABasePhonemes.Group1_E;
			}
			if (weightGroup2 > weight)
			{
				weight = weightGroup2;
				phoneme = FABasePhonemes.Group2_U;
			}
			if (weightGroup3 > weight)
			{
				weight = weightGroup3;
				phoneme = FABasePhonemes.Group3_O;
			}
			if (weightGroup4 > weight)
			{
				weight = weightGroup4;
				phoneme = FABasePhonemes.Group4_CDGK;
			}
			if (weightGroup5 > weight)
			{
				weight = weightGroup5;
				phoneme = FABasePhonemes.Group5_FV;
			}
			if (weightGroup6 > weight)
			{
				weight = weightGroup6;
				phoneme = FABasePhonemes.Group6_LTh;
			}
			if (weightGroup7 > weight)
			{
				weight = weightGroup7;
				phoneme = FABasePhonemes.Group7_MBP;
			}
			if (weightGroup8 > weight)
			{
				weight = weightGroup8;
				phoneme = FABasePhonemes.Group8_WQ;
			}
			if (weightGroup9 > weight)
			{
				weight = weightGroup9;
				phoneme = FABasePhonemes.Group9_Rest;
			}
		}

		#endregion
		#region Properties

		public static FAPhonemeVector Blank
		{
			get
			{
				FAPhonemeVector pv = new FAPhonemeVector();

				pv.weightGroup0 = 0;
				pv.weightGroup1 = 0;
				pv.weightGroup2 = 0;
				pv.weightGroup3 = 0;
				pv.weightGroup4 = 0;
				pv.weightGroup5 = 0;
				pv.weightGroup6 = 0;
				pv.weightGroup7 = 0;
				pv.weightGroup8 = 0;
				pv.weightGroup9 = 0;

				pv.energy = 0;

				return pv;
			}
		}

		#endregion
	}
}
