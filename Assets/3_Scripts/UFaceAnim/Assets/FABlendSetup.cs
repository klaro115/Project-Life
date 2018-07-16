using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[System.Serializable]
	public struct FABlendSetup
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

		public static FABlendSetup Default
		{
			get
			{
				FABlendSetup bs = new FABlendSetup();

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
}

