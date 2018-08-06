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
		#region Methods

		public FABlendState getBlendState(FAEmotion emotion)
		{
			Vector4 emotVec = emotion.Vector;

			float kSad = FABlendStateTools.getBlendFactor(-emotVec.x, blendCurve);
			float kJoy = FABlendStateTools.getBlendFactor(emotVec.x, blendCurve);
			float kDis = FABlendStateTools.getBlendFactor(-emotVec.y, blendCurve);
			float kTru = FABlendStateTools.getBlendFactor(emotVec.y, blendCurve);
			float kFea = FABlendStateTools.getBlendFactor(-emotVec.z, blendCurve);
			float kAng = FABlendStateTools.getBlendFactor(emotVec.z, blendCurve);
			float kSur = FABlendStateTools.getBlendFactor(-emotVec.w, blendCurve);
			float kAnt = FABlendStateTools.getBlendFactor(emotVec.w, blendCurve);

			// Create 'currentState' by interpolating between preset states using the current emotion:
			FABlendState bsSad = FABlendState.lerp(ref blendStateNeutral, ref blendStateSadness, kSad);
			FABlendState bsJoy = FABlendState.lerp(ref blendStateNeutral, ref blendStateJoy, kJoy);
			FABlendState bsDis = FABlendState.lerp(ref blendStateNeutral, ref blendStateDisgust, kDis);
			FABlendState bsTru = FABlendState.lerp(ref blendStateNeutral, ref blendStateTrust, kTru);
			FABlendState bsFea = FABlendState.lerp(ref blendStateNeutral, ref blendStateFear, kFea);
			FABlendState bsAng = FABlendState.lerp(ref blendStateNeutral, ref blendStateAnger, kAng);
			FABlendState bsSur = FABlendState.lerp(ref blendStateNeutral, ref blendStateSurprise, kSur);
			FABlendState bsAnt = FABlendState.lerp(ref blendStateNeutral, ref blendStateAnticipation, kAnt);

			// Blend individual opposing emotional 'dimensions':
			FABlendState bsJoySad = FABlendState.lerp(ref bsSad, ref bsJoy, 0.5f * emotVec.x + 0.5f);
			FABlendState bsDisTru = FABlendState.lerp(ref bsDis, ref bsTru, 0.5f * emotVec.y + 0.5f);
			FABlendState bsFeaAng = FABlendState.lerp(ref bsFea, ref bsAng, 0.5f * emotVec.z + 0.5f);
			FABlendState bsSurAnt = FABlendState.lerp(ref bsSur, ref bsAnt, 0.5f * emotVec.w + 0.5f);

			// Return the sum of all weighted states:
			return bsJoySad + bsDisTru + bsFeaAng + bsSurAnt;
		}

		#endregion
	}
}

