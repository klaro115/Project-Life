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

		private FAEmotion currentEmotion = FAEmotion.Neutral;

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

		public void setEmotion(FAEmotion emotion)
		{
			currentEmotion = emotion;
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
				{
					float t = Mathf.Abs(k);
					float f = 3 * t * t - 2 * t * t * t;
					return f * Mathf.Sign(k);
				}
				case FABlendCurve.Square:
					return k * k * Mathf.Sign(k);
				default:
					// Default to linear behaviour:
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
			FABlendCurve blendCurve = preset.blendShapeSetup.blendCurve;

			Vector4 emotVec = currentEmotion.Vector;

			float kSad = getBlendFactor(-emotVec.x, blendCurve);
			float kJoy = getBlendFactor(emotVec.x, blendCurve);
			float kDis = getBlendFactor(-emotVec.y, blendCurve);
			float kTru = getBlendFactor(emotVec.y, blendCurve);
			float kFea = getBlendFactor(-emotVec.z, blendCurve);
			float kAng = getBlendFactor(emotVec.z, blendCurve);
			float kSur = getBlendFactor(-emotVec.w, blendCurve);
			float kAnt = getBlendFactor(emotVec.w, blendCurve);

			// Create 'currentState' by interpolating between preset states using the current emotion:
			FABlendState bsSad = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateSadness, kSad);
			FABlendState bsJoy = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateJoy, kJoy);
			FABlendState bsDis = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateDisgust, kDis);
			FABlendState bsTru = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateTrust, kTru);
			FABlendState bsFea = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateFear, kFea);
			FABlendState bsAng = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateAnger, kAng);
			FABlendState bsSur = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateSurprise, kSur);
			FABlendState bsAnt = FABlendState.lerp(ref preset.blendShapeSetup.blendStateNeutral, ref preset.blendShapeSetup.blendStateAnticipation, kAnt);

			// Blend individual opposing emotional 'dimensions':
			FABlendState bsJoySad = FABlendState.lerp(ref bsSad, ref bsJoy, 0.5f * emotVec.x + 0.5f);
			FABlendState bsDisTru = FABlendState.lerp(ref bsDis, ref bsTru, 0.5f * emotVec.y + 0.5f);
			FABlendState bsFeaAng = FABlendState.lerp(ref bsFea, ref bsAng, 0.5f * emotVec.z + 0.5f);
			FABlendState bsSurAnt = FABlendState.lerp(ref bsSur, ref bsAnt, 0.5f * emotVec.w + 0.5f);

			FABlendState bsTotal = bsJoySad + bsDisTru + bsFeaAng + bsSurAnt;

			return bsTotal;
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
