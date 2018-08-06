using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	// SPEECH BASED BLEND STATE SETUP:

	[System.Serializable]
	public struct FABlendSetupSpeech
	{
		#region Fields

		public FABlendCurve blendCurve;

		public FABlendState blendStateGroup0;
		public FABlendState blendStateGroup1;
		public FABlendState blendStateGroup2;
		public FABlendState blendStateGroup3;
		public FABlendState blendStateGroup4;
		public FABlendState blendStateGroup5;
		public FABlendState blendStateGroup6;
		public FABlendState blendStateGroup7;
		public FABlendState blendStateGroup8;
		public FABlendState blendStateGroup9;
		public FABlendState blendStateSilence;

		#endregion
		#region Properties

		public static FABlendSetupSpeech Default
		{
			get
			{
				FABlendSetupSpeech bs = new FABlendSetupSpeech();

				bs.blendCurve = FABlendCurve.Linear;

				bs.blendStateGroup0 = FABlendState.Default;
				bs.blendStateGroup1 = FABlendState.Default;
				bs.blendStateGroup2 = FABlendState.Default;
				bs.blendStateGroup3 = FABlendState.Default;
				bs.blendStateGroup4 = FABlendState.Default;
				bs.blendStateGroup5 = FABlendState.Default;
				bs.blendStateGroup6 = FABlendState.Default;
				bs.blendStateGroup7 = FABlendState.Default;
				bs.blendStateGroup8 = FABlendState.Default;
				bs.blendStateGroup9 = FABlendState.Default;
				bs.blendStateSilence = FABlendState.Default;

				return bs;
			}
		}

		#endregion
		#region Methods

		public FABlendState getBlendState(FABasePhonemes phoneme)
		{
			switch (phoneme)
			{
				case FABasePhonemes.Group0_AI:
					return blendStateGroup0;
				case FABasePhonemes.Group1_E:
					return blendStateGroup1;
				case FABasePhonemes.Group2_U:
					return blendStateGroup2;
				case FABasePhonemes.Group3_O:
					return blendStateGroup3;
				case FABasePhonemes.Group4_CDGK:
					return blendStateGroup4;
				case FABasePhonemes.Group5_FV:
					return blendStateGroup5;
				case FABasePhonemes.Group6_LTh:
					return blendStateGroup6;
				case FABasePhonemes.Group7_MBP:
					return blendStateGroup7;
				case FABasePhonemes.Group8_WQ:
					return blendStateGroup8;
				case FABasePhonemes.Group9_Rest:
					return blendStateSilence;
				default:
					break;
			}

			return blendStateSilence;
		}

		#endregion
	}
}

