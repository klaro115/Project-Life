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

			None
		}
		[System.Serializable]
		public struct SpeechSettings
		{
			public SpeechMode mode;
			public float phonemeBlendRate;
			public float phonemeEnergyRate;
		}
		[System.Serializable]
		public struct EmotionSettings
		{
			public float blendRate;
		}
		[System.Serializable]
		public struct ViewSettings
		{
			public float dirBlendRate;
			public float dirModifier;
			public Vector2 dirLimits;
		}

		#endregion
		#region Fields

		private bool initialized = false;
		private FABlendOverwrite overwriteMode = FABlendOverwrite.None;

		[SerializeField]
		private FAPresets preset = null;

		public SpeechSettings speech = new SpeechSettings()
		{
			mode = SpeechMode.Phonemes, phonemeBlendRate = 24.0f, phonemeEnergyRate = 36.0f
		};
		public EmotionSettings emotion = new EmotionSettings()
		{
			blendRate = 12.0f
		};
		public ViewSettings view = new ViewSettings()
		{
			dirBlendRate = 15.0f, dirModifier = 1.0f, dirLimits = new Vector2(1.0f, 0.7f)
		};

		private FARenderer[] renderers = null;

		private FABasePhonemes targetPhoneme = FABasePhonemes.None;
		private float targetPhonemeEnergy = 0.0f;
		private float currentPhonemeEnergy = 0.0f;

		private FAEmotion currentEmotion = FAEmotion.Neutral;

		private Vector2 currentViewDir = Vector2.zero;
		private Vector2 targetViewDir = Vector2.zero;

		private FABlendState targetBlendSpeech = FABlendState.Default;
		private FABlendState targetBlendEmotion = FABlendState.Default;
		private FABlendState currentBlendSpeech = FABlendState.Default;
		private FABlendState currentBlendState = FABlendState.Default;

		#endregion
		#region Properties

		public FAPresets Preset
		{
			get { return preset; }
			set { if(value != null) preset = value; }
		}

		#endregion
		#region Methods

		[ContextMenu("Initialize")]
		public void initialize()
		{
			// Raise default flags:
			initialized = true;
			overwriteMode = FABlendOverwrite.None;

			// Fetch and initialize all blend shape target renderers in children:
			renderers = GetComponentsInChildren<FARenderer>(true);
			for(int i = 0; i < renderers.Length; ++i)
			{
				renderers[i].initialize();
			}

			// Reset emotions and phonemes:
			currentEmotion = FAEmotion.Neutral;
			targetPhonemeEnergy = 0;
			currentPhonemeEnergy = 0;

			// Set blank blend states as placeholder/default weights:
			targetBlendEmotion = FABlendState.Default;
			targetBlendSpeech = FABlendState.Default;

			// Make sure there is always a preset assigned from the start:
			if(preset == null)
			{
				preset = ScriptableObject.CreateInstance<FAPresets>();
				preset.name = "none";
			}
		}

		public void setEmotion(string emotionString)
		{
			FAEmotion emotion = FAEmotion.parseString(emotionString);
			setEmotion(emotion);
		}
		public void setEmotion(FAEmotion emotion)
		{
			currentEmotion = emotion;
			updateTargetBlendEmotion();
		}
		public void setPhoneme(FABasePhonemes phoneme, float energy)
		{
			// NOTE: This must be called in very regular intervals (i.e. every frame) whenever
			// the character is speaking. Otherwise the mouth will not move at the appropriate
			// pace, making facial animation look stiff and not very credible. The emotional
			// state of a character on the other hand need only be updated via 'setEmotion()'
			// whenever the emotion changes noticably.
			// In short: speech => millisecond intervals, emotion => seconds to minute intervals.

			targetPhoneme = phoneme;
			targetPhonemeEnergy = energy;
			updateTargetBlendSpeech();
		}

		public void setOverwriteState(FABlendState state, FABlendOverwrite inOverwriteMode)
		{
			// NOTE: You can set the 'None' state here, but consider using 'unsetOverwriteState()' instead.
			// PURPOSE: Overwriting allows you to temporarily disable all emotion and/or speech based blending.
			// This can be useful whenever you want to drive facial animations via different means, such as
			// during pre-built cutscenes, or to show a painful expression if the character receives damage.

			overwriteMode = inOverwriteMode;

			switch (overwriteMode)
			{
			case FABlendOverwrite.All:
				targetBlendEmotion = state;
				targetBlendSpeech = FABlendState.Default;
				break;
			case FABlendOverwrite.Emotion:
				targetBlendEmotion = state;
				break;
			case FABlendOverwrite.Speech:
				targetBlendSpeech = state;
				break;
			default:
				break;
			}
		}
		public void unsetOverwriteState()
		{
			// Reset overwrite mode:
			FABlendOverwrite prevOverwrite = overwriteMode;
			overwriteMode = FABlendOverwrite.None;

			// Make sure to reset blend target states according to emotions and speech:
			if(prevOverwrite != overwriteMode)
			{
				updateTargetBlendEmotion();
				updateTargetBlendSpeech();
			}
		}

		public void setViewDirection(Vector3 direction)
		{
			float x = Mathf.Clamp(Vector3.Dot(direction, transform.right), -view.dirLimits.x, view.dirLimits.x);
			float y = Mathf.Clamp(Vector3.Dot(direction, transform.up), -view.dirLimits.y, view.dirLimits.y);
			targetViewDir = new Vector2(x, y) * view.dirModifier;
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
			FABlendState newBlendState = FABlendState.lerp(ref currentBlendState, ref targetBlendEmotion,
				emotion.blendRate * deltaTime);

			// Speech based weighting:
			if (speech.mode != SpeechMode.None)
			{
				FABlendState newPhonState = updateBlendStatesSpeech(deltaTime);

				newBlendState += newPhonState;
			}

			// View direction:
			currentViewDir = Vector2.Lerp(currentViewDir, targetViewDir, view.dirBlendRate * deltaTime);
			newBlendState.eyesDir += currentViewDir;

			// Set the new state and update renderers:
			currentBlendState = newBlendState;

			// Send resulting blend states to renderers:
			updateRenderers();
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

		private void updateTargetBlendEmotion()
		{
			if(overwriteMode == FABlendOverwrite.All || overwriteMode == FABlendOverwrite.Emotion)
				return;

			// Interpolate blend state between emotion blends based on current emotion state:
			targetBlendEmotion = preset.blendShapeSetup.getBlendState(currentEmotion);
		}
		private void updateTargetBlendSpeech()
		{
			if(overwriteMode == FABlendOverwrite.All || overwriteMode == FABlendOverwrite.Speech)
				return;

			// Depending on settings, use the simpler energy based blend mode:
			if (speech.mode == SpeechMode.Energy)
			{
				targetBlendSpeech = preset.blendShapesSpeech.blendStateGroup0;
			}
			// Or use the slightly more performance and blendshape-heavy phoneme method instead:
			else
			{
				targetBlendSpeech = preset.blendShapesSpeech.getBlendState(targetPhoneme);
			}
		}

		private FABlendState updateBlendStatesSpeech(float deltaTime)
		{
			FABlendCurve blendCurve = preset.blendShapesSpeech.blendCurve;

			currentPhonemeEnergy = Mathf.Lerp(currentPhonemeEnergy, targetPhonemeEnergy, speech.phonemeEnergyRate * deltaTime);
			float energy = Mathf.Clamp01(currentPhonemeEnergy);
			float kEnergy = FABlendStateTools.getBlendFactor(energy, blendCurve);

			// Create blend state by interpolating towards a preset states from the current phoneme:
			currentBlendSpeech = FABlendState.lerp(ref currentBlendSpeech, ref targetBlendSpeech, speech.phonemeBlendRate * deltaTime);
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
