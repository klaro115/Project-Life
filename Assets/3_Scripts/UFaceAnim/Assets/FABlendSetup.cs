using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	// EMOTION BASED BLEND STATE SETUP:

	[System.Serializable]
	public struct FABlendSetupEmotion
	{
		#region Fields

		public FABlendCurve blendCurve;

		public FABlendState blendStateJoy;
		public FABlendState blendStateSadness;
		public FABlendState blendStateAnger;
		public FABlendState blendStateFear;
		public FABlendState blendStateTrust;
		public FABlendState blendStateDisgust;
		public FABlendState blendStateSurprise;
		public FABlendState blendStateAnticipation;
		public FABlendState blendStateNeutral;

		#endregion
		#region Properties

		public static FABlendSetupEmotion Default
		{
			get
			{
				FABlendSetupEmotion bs = new FABlendSetupEmotion();

				bs.blendCurve = FABlendCurve.Linear;

				bs.blendStateJoy = FABlendState.Default;
				bs.blendStateSadness = FABlendState.Default;
				bs.blendStateAnger = FABlendState.Default;
				bs.blendStateFear = FABlendState.Default;
				bs.blendStateTrust = FABlendState.Default;
				bs.blendStateDisgust = FABlendState.Default;
				bs.blendStateSurprise = FABlendState.Default;
				bs.blendStateAnticipation = FABlendState.Default;
				bs.blendStateNeutral = FABlendState.Default;

				return bs;
			}
		}

		#endregion
	}

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
	}
}

