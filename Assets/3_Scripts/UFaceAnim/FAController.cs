using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[AddComponentMenu("Scripts/UFaceAnim/Controller")]
	public class FAController : MonoBehaviour
	{
		#region Fields

		private bool initialized = false;

		[SerializeField]
		private FAPresets preset = null;
		private FARenderer[] renderers = null;

		private FABasePhonemes targetPhoneme = FABasePhonemes.None;
		//private FAEmotion currentEmotion = FAEmotion.Neutral;

		private FABlendState currentBlendState = FABlendState.Default;

		#endregion
		#region Methods

		[ContextMenu("Initialize")]
		public void initialize()
		{
			initialized = true;

			renderers = GetComponentsInChildren<FARenderer>(true);
			for(int i = 0; i < renderers.Length; ++i)
			{
				renderers[i].initialize();
			}

			//...
		}

		private float getBlendFactor(float k, FABlendCurve blendCurve)
		{
			switch (blendCurve)
			{
				case FABlendCurve.SmoothStep:
					return 3 * k * k - 2 * k * k * k;
				case FABlendCurve.Square:
					return k * k;
					// Default to linear behaviour:
				default:
					break;
			}
			return k;
		}

		[ContextMenu("Update")]
		public void update()
		{
			// Make sure the controller and all members are always initialized:
			if(!initialized)
			{
				initialize();
			}

			// Emotion based weighting:
			FABlendState newEmotState;
			{
				// TODO: Implement emotion structure and read clamped/normalized values from there.

				//...

				FABlendCurve blendCurve = preset.blendShapeSetup.blendCurve;

				float kJoySad = getBlendFactor(0.0f, blendCurve);
				float kDisTru = getBlendFactor(0.0f, blendCurve);
				float kFeaAng = getBlendFactor(0.0f, blendCurve);
				float kSurAnt = getBlendFactor(0.0f, blendCurve);

				float kTotal = getBlendFactor(0.0f, blendCurve);

				// Create 'currentState' by interpolating between preset states using the current emotion:
				FABlendState bsJoySad = FABlendState.lerp(ref preset.blendShapeSetup.blendStateSadness, ref preset.blendShapeSetup.blendStateJoy, kJoySad);
				FABlendState bsDisTru = FABlendState.lerp(ref preset.blendShapeSetup.blendStateDisgust, ref preset.blendShapeSetup.blendStateTrust, kDisTru);
				FABlendState bsFeaAng = FABlendState.lerp(ref preset.blendShapeSetup.blendStateFear, ref preset.blendShapeSetup.blendStateAnger, kFeaAng);
				FABlendState bsSurAnt = FABlendState.lerp(ref preset.blendShapeSetup.blendStateSurprise, ref preset.blendShapeSetup.blendStateAnticipation, kSurAnt);

				FABlendState bsTotal = bsJoySad + bsDisTru + bsFeaAng + bsSurAnt;
				bsTotal.scale(0.25f);

				newEmotState = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref bsTotal, kTotal);
			}

			// Speech based weighting:
			FABlendState newPhonState;
			{
				// TODO: Implement interpolation between the current and target phoneme states. [later]

				//...

				FABlendCurve blendCurve = preset.blendShapesSpeech.blendCurve;

				float kEnergy = getBlendFactor(1.0f, blendCurve);

				// Create blend state by interpolating towards a preset states using the current phoneme:
				FABlendState bsSpeech = FABlendState.Default;
				switch (targetPhoneme)
				{
					case FABasePhonemes.Group0_AI:
						newPhonState = preset.blendShapesSpeech.blendStateGroup0;
						break;
					case FABasePhonemes.Group1_E:
						newPhonState = preset.blendShapesSpeech.blendStateGroup1;
						break;
					case FABasePhonemes.Group2_U:
						newPhonState = preset.blendShapesSpeech.blendStateGroup2;
						break;
					case FABasePhonemes.Group3_O:
						newPhonState = preset.blendShapesSpeech.blendStateGroup3;
						break;
					case FABasePhonemes.Group4_CDGK:
						newPhonState = preset.blendShapesSpeech.blendStateGroup4;
						break;
					case FABasePhonemes.Group5_FV:
						newPhonState = preset.blendShapesSpeech.blendStateGroup5;
						break;
					case FABasePhonemes.Group6_LTh:
						newPhonState = preset.blendShapesSpeech.blendStateGroup6;
						break;
					case FABasePhonemes.Group7_MBP:
						newPhonState = preset.blendShapesSpeech.blendStateGroup7;
						break;
					case FABasePhonemes.Group8_WQ:
						newPhonState = preset.blendShapesSpeech.blendStateGroup8;
						break;
					case FABasePhonemes.Group9_Rest:
						newPhonState = preset.blendShapesSpeech.blendStateSilence;
						break;
					default:
						break;
				}

				newPhonState = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref bsSpeech, kEnergy);
			}

			// Set the new state and update renderers:
			FABlendState newBlendState = newEmotState + newPhonState;
			update(newBlendState);
		}

		public void update(FABlendState state)
		{
			// Make sure the controller and all members are always initialized:
			if (!initialized)
			{
				initialize();
			}

			// Assign the new state:
			currentBlendState = state;

			// Send resulting blend states to renderers:
			updateRenderers();
		}

		private void updateRenderers()
		{
			if (renderers == null) return;

			for (int i = 0; i < renderers.Length; ++i)
			{
				FARenderer rend = renderers[i];
				if(rend != null)
				{
					rend.update(ref currentBlendState);
				}
			}
		}

		#endregion
	}
}
