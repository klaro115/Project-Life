using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[AddComponentMenu("Scripts/UFaceAnim/Controller")]
	public class FAController : MonoBehaviour
	{
		#region Types

		public enum SpeechMode
		{
			Energy,
			Phonemes,
		}

		#endregion
		#region Fields

		private bool initialized = false;

		public SpeechMode speechMode = SpeechMode.Phonemes;
		public float phonemeBlendRate = 18.0f;
		public float phonemeEnergyRate = 36.0f;

		[SerializeField]
		private FAPresets preset = null;
		private FARenderer[] renderers = null;

		private FABasePhonemes targetPhoneme = FABasePhonemes.None;
		private float targetPhonemeEnergy = 0.0f;
		private float currentPhonemeEnergy = 0.0f;

		//private FAEmotion currentEmotion = FAEmotion.Neutral;

		private FABlendState currentBlendSpeech = FABlendState.Default;
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

			targetPhonemeEnergy = 0;
			currentPhonemeEnergy = 0;

			//...
		}

		public void setPhoneme(FABasePhonemes phoneme, float energy)
		{
			targetPhoneme = phoneme;
			targetPhonemeEnergy = energy;
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
		public void update(float deltaTime)
		{
			// Make sure the controller and all members are always initialized:
			if(!initialized)
			{
				initialize();
			}

			// Emotion based weighting:
			FABlendState newEmotState = updateBlendStatesEmotion();

			// Speech based weighting:
			FABlendState newPhonState = updateBlendStatesSpeech(deltaTime);

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

		private FABlendState updateBlendStatesEmotion()
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

			return FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref bsTotal, kTotal);
		}

		private FABlendState updateBlendStatesSpeech(float deltaTime)
		{
			FABlendCurve blendCurve = preset.blendShapesSpeech.blendCurve;

			currentPhonemeEnergy = Mathf.Lerp(currentPhonemeEnergy, targetPhonemeEnergy, phonemeEnergyRate * deltaTime);
			float energy = Mathf.Clamp01(currentPhonemeEnergy);
			float kEnergy = getBlendFactor(energy, blendCurve);

			FABlendState bsSpeech = FABlendState.Default;

			// Depending on settings, use the much cheaper energy based blend mode instead:
			if (speechMode == SpeechMode.Energy)
			{
				bsSpeech = preset.blendShapesSpeech.blendStateGroup0;
				//bsSpeech = FABlendState.lerp(ref preset.blendShapesSpeech.blendStateSilence, ref preset.blendShapesSpeech.blendStateGroup0, kEnergy);
				currentBlendSpeech = FABlendState.lerp(ref currentBlendSpeech, ref bsSpeech, phonemeBlendRate * deltaTime);
				return FABlendState.lerp(ref preset.blendShapesSpeech.blendStateSilence, ref currentBlendSpeech, kEnergy);
			}

			// Create blend state by interpolating towards a preset states using the current phoneme:
			switch (targetPhoneme)
			{
				case FABasePhonemes.Group0_AI:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup0;
					break;
				case FABasePhonemes.Group1_E:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup1;
					break;
				case FABasePhonemes.Group2_U:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup2;
					break;
				case FABasePhonemes.Group3_O:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup3;
					break;
				case FABasePhonemes.Group4_CDGK:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup4;
					break;
				case FABasePhonemes.Group5_FV:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup5;
					break;
				case FABasePhonemes.Group6_LTh:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup6;
					break;
				case FABasePhonemes.Group7_MBP:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup7;
					break;
				case FABasePhonemes.Group8_WQ:
					bsSpeech = preset.blendShapesSpeech.blendStateGroup8;
					break;
				case FABasePhonemes.Group9_Rest:
					bsSpeech = preset.blendShapesSpeech.blendStateSilence;
					break;
				default:
					break;
			}

			currentBlendSpeech = FABlendState.lerp(ref currentBlendSpeech, ref bsSpeech, phonemeBlendRate * deltaTime);
			return FABlendState.lerp(ref preset.blendShapesSpeech.blendStateSilence, ref currentBlendSpeech, kEnergy);
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
