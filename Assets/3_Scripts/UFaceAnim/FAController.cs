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

		#endregion
		#region Fields

		private bool initialized = false;

		public SpeechMode speechMode = SpeechMode.Phonemes;
		public float phonemeBlendRate = 18.0f;
		public float phonemeEnergyRate = 36.0f;
		public float emotionBlendRate = 12.0f;
		public float viewDirBlendRate = 15.0f;
		public float viewDirModifier = 1.0f;
		public Vector2 viewDirLimits = new Vector2(1, 0.7f);

		[SerializeField]
		private FAPresets preset = null;
		private FARenderer[] renderers = null;

		private FABasePhonemes targetPhoneme = FABasePhonemes.None;
		private float targetPhonemeEnergy = 0.0f;
		private float currentPhonemeEnergy = 0.0f;

		private FAEmotion currentEmotion = FAEmotion.Neutral;
		private FAEmotion targetEmotion = FAEmotion.Neutral;

		private Vector2 currentViewDir = Vector2.zero;
		private Vector2 targetViewDir = Vector2.zero;

		private FABlendState currentBlendSpeech = FABlendState.Default;
		private FABlendState currentBlendState = FABlendState.Default;

		#endregion
		#region Properties

		public FAPresets Preset
		{
			get { return preset; }
		}

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
			targetEmotion = emotion;
		}
		public void setPhoneme(FABasePhonemes phoneme, float energy)
		{
			targetPhoneme = phoneme;
			targetPhonemeEnergy = energy;
		}
		public void setViewDirection(Vector3 direction)
		{
			float x = Mathf.Clamp(Vector3.Dot(direction, transform.right), -viewDirLimits.x, viewDirLimits.x);
			float y = Mathf.Clamp(Vector3.Dot(direction, transform.up), -viewDirLimits.y, viewDirLimits.y);
			targetViewDir = new Vector2(x, y) * viewDirModifier;
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
			currentEmotion = FAEmotion.lerp(currentEmotion, targetEmotion, emotionBlendRate * deltaTime);
			FABlendState newBlendState = preset.blendShapeSetup.getBlendState(currentEmotion);

			// Speech based weighting:
			if (speechMode != SpeechMode.None)
			{
				FABlendState newPhonState = updateBlendStatesSpeech(deltaTime);

				newBlendState += newPhonState;
			}

			// View direction:
			currentViewDir = Vector2.Lerp(currentViewDir, targetViewDir, viewDirBlendRate * deltaTime);
			newBlendState.eyesDir += currentViewDir;

			// Set the new state and update renderers:
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

		private FABlendState updateBlendStatesSpeech(float deltaTime)
		{
			FABlendCurve blendCurve = preset.blendShapesSpeech.blendCurve;

			currentPhonemeEnergy = Mathf.Lerp(currentPhonemeEnergy, targetPhonemeEnergy, phonemeEnergyRate * deltaTime);
			float energy = Mathf.Clamp01(currentPhonemeEnergy);
			float kEnergy = FABlendStateTools.getBlendFactor(energy, blendCurve);

			FABlendState bsSpeech = FABlendState.Default;

			// Depending on settings, use the cheaper energy based blend mode instead:
			if (speechMode == SpeechMode.Energy)
			{
				bsSpeech = preset.blendShapesSpeech.blendStateGroup0;

				currentBlendSpeech = FABlendState.lerp(ref currentBlendSpeech, ref bsSpeech, phonemeBlendRate * deltaTime);
				return FABlendState.lerp(ref preset.blendShapesSpeech.blendStateSilence, ref currentBlendSpeech, kEnergy);
			}
			// Or use the slightly more performance and blendshape-heavy phoneme method:
			else
			{
				// Create blend state by interpolating towards a preset states using the current phoneme:
				bsSpeech = preset.blendShapesSpeech.getBlendState(targetPhoneme);
	
				currentBlendSpeech = FABlendState.lerp(ref currentBlendSpeech, ref bsSpeech, phonemeBlendRate * deltaTime);
				return FABlendState.lerp(ref preset.blendShapesSpeech.blendStateSilence, ref currentBlendSpeech, kEnergy);
			}
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
