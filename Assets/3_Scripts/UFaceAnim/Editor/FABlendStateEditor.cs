using System.Collections;
using UnityEngine;
using UnityEditor;

namespace UFaceAnim.Editor
{
	[System.Serializable]
	public class FABlendStateEditor : EditorWindow
	{
		#region Types

		public enum Mode
		{
			Emotions,
			Phonemes,
			Library
		}

		#endregion
		#region Fields

		[SerializeField]
		private bool changed = false;
		private Mode mode = Mode.Emotions;

		public FAPresets asset = null;
		public FAController controller = null;

		[SerializeField]
		private FABlendState currentState = FABlendState.Default;
		private FABaseEmotions targetEmotion = FABaseEmotions.Neutral;
		private FABasePhonemes targetPhoneme = FABasePhonemes.None;
		private string targetLibState = "";

		private FAEmotion testEmotion = FAEmotion.Neutral;

		#endregion
		#region Methods

		[MenuItem("Window/UFaceAnim/Blend State Editor")]
		public static FABlendStateEditor showEditorWindow()
		{
			Rect rect = new Rect(100, 100, 300, 508);
			FABlendStateEditor bse = EditorWindow.GetWindowWithRect<FABlendStateEditor>(rect, true, "Blend State");
			return bse;
		}

		private void Update()
		{
			// Select controller straight from the user's live editor selection:
			GameObject selectionGO = Selection.activeGameObject;
			if (selectionGO != null && (controller == null || selectionGO != controller.gameObject))
			{
				controller = selectionGO.GetComponent<FAController>();
				if(asset == null && controller != null && controller.Preset != null)
				{
					asset = controller.Preset;
				}
				Repaint();
			}

			// Update the emotion at runtime while in play-mode:
			if(asset != null && controller != null && Application.isPlaying)
			{
				controller.update(Time.deltaTime);
			}
		}

		public void OnGUI()
		{
			// Draw some fields for asset selection:
			asset = (FAPresets)EditorGUILayout.ObjectField("Preset Asset" + (changed ? "*" : ""), asset, typeof(FAPresets), false);
			controller = (FAController)EditorGUILayout.ObjectField("Controller", controller, typeof(FAController), true);

			if (asset == null) return;
			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Mode:", EditorStyles.toolbarButton);
			EditorGUI.BeginDisabledGroup(mode == Mode.Emotions);
			if (GUILayout.Button("Emotions", EditorStyles.toolbarButton)) mode = Mode.Emotions;
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(mode == Mode.Phonemes);
			if (GUILayout.Button("Phonemes", EditorStyles.toolbarButton)) mode = Mode.Phonemes;
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(mode == Mode.Library);
			if (GUILayout.Button("Library", EditorStyles.toolbarButton)) mode = Mode.Library;
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			bool prevChanged = GUI.changed;
			GUI.changed = false;

			EditorGUILayout.LabelField("Mouth:", EditorStyles.boldLabel);

			currentState.mouthOpenClose = EditorGUILayout.Slider("Close-Open", currentState.mouthOpenClose, 0, 1);
			currentState.mouthShowTeeth = EditorGUILayout.Slider("Show teeth", currentState.mouthShowTeeth, 0, 1);
			currentState.mouthInOut = EditorGUILayout.Slider("Out-In", currentState.mouthInOut, 0, 1);
			currentState.mouthCornerL = EditorGUILayout.Slider("Corner L", currentState.mouthCornerL, -1, 1);
			currentState.mouthCornerR = EditorGUILayout.Slider("Corner R", currentState.mouthCornerR, -1, 1);

			EditorGUILayout.LabelField("Brows:", EditorStyles.boldLabel);

			currentState.browsL = EditorGUILayout.Slider("Down-Up L", currentState.browsL, -1, 1);
			currentState.browsR = EditorGUILayout.Slider("Down-Up R", currentState.browsR, -1, 1);
			currentState.browsInOut = EditorGUILayout.Slider("Out-In", currentState.browsInOut, 0, 1);
			currentState.browsSharpFlat = EditorGUILayout.Slider("Flat-Sharp", currentState.browsSharpFlat, -1, 1);

			EditorGUILayout.LabelField("Eyes:", EditorStyles.boldLabel);

			currentState.eyesCloseL = EditorGUILayout.Slider("Close L", currentState.eyesCloseL, 0, 1);
			currentState.eyesCloseR = EditorGUILayout.Slider("Close R", currentState.eyesCloseR, 0, 1);
			currentState.eyesWander = EditorGUILayout.Slider("Wander", currentState.eyesWander, 0, 1);
			currentState.eyesDir.x = EditorGUILayout.Slider("Look L-R", currentState.eyesDir.x, -1, 1);
			currentState.eyesDir.y = EditorGUILayout.Slider("Look D-U", currentState.eyesDir.y, -1, 1);

			// If a UI control was manipulated:
			if(GUI.changed)
			{
				changed = true;

				updateController();
			}
			GUI.changed = GUI.changed || prevChanged;

			EditorGUILayout.Separator();

			switch (mode)
			{
			case Mode.Emotions:
				GUIContent targetEmotCont = new GUIContent("Target emotion", "Which emotion to save the currently displayed blend state as.");
				targetEmotion = (FABaseEmotions)EditorGUILayout.EnumPopup(targetEmotCont, targetEmotion);
				break;
			case Mode.Phonemes:
				GUIContent targetPhonCont = new GUIContent("Target phoneme", "Which 'phoneme' to save the currently displayed blend state as.");
				targetPhoneme = (FABasePhonemes)EditorGUILayout.EnumPopup(targetPhonCont, targetPhoneme);
				break;
			case Mode.Library:
				GUIContent targetLibCont = new GUIContent("Target entry", "String key under which to save the currently displayed blend state.");
				targetLibState = EditorGUILayout.DelayedTextField(targetLibCont, targetLibState);
				break;
			default:
				break;
			}

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Load target state"))
			{
				loadBlendState();
			}

			EditorGUI.BeginDisabledGroup(mode == Mode.Emotions && targetEmotion == FABaseEmotions.None);
			if (GUILayout.Button("Apply blend states"))
			{
				saveBlendStates();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
	
			if(mode == Mode.Emotions && controller != null)
			{
				EditorGUILayout.Separator();

				bool prevEmotChanged = GUI.changed;
				GUI.changed = false;

				Vector4 testEmotVector = testEmotion.Vector;
				testEmotVector.x = EditorGUILayout.Slider("Sadness/Joy", testEmotVector.x, -1, 1);
				testEmotVector.y = EditorGUILayout.Slider("Disgust/Trust", testEmotVector.y, -1, 1);
				testEmotVector.z = EditorGUILayout.Slider("Fear/Anger", testEmotVector.z, -1, 1);
				testEmotVector.w = EditorGUILayout.Slider("Surprise/Anticipation", testEmotVector.w, -1, 1);

				if(GUI.changed)
				{
					testEmotion.Vector = testEmotVector;
					controller.setEmotion(testEmotion);
					controller.update(Application.isPlaying ? Time.deltaTime : 999);
				}
				GUI.changed = GUI.changed || prevEmotChanged;
			}
		}

		private void updateController()
		{
			// If a controller is assigned/active, use it for a live preview:
			if (controller != null)
			{
				controller.update(currentState);
			}
		}

		private void loadBlendState()
		{
			if (asset == null) return;

			if(mode == Mode.Emotions)
			{
				// Load target state blend weights from asset:
				switch (targetEmotion)
				{
					case FABaseEmotions.Joy:
						currentState = asset.blendShapeSetup.blendStateJoy;
						break;
					case FABaseEmotions.Sadness:
						currentState = asset.blendShapeSetup.blendStateSadness;
						break;
					case FABaseEmotions.Disgust:
						currentState = asset.blendShapeSetup.blendStateDisgust;
						break;
					case FABaseEmotions.Trust:
						currentState = asset.blendShapeSetup.blendStateTrust;
						break;
					case FABaseEmotions.Fear:
						currentState = asset.blendShapeSetup.blendStateFear;
						break;
					case FABaseEmotions.Anger:
						currentState = asset.blendShapeSetup.blendStateAnger;
						break;
					case FABaseEmotions.Surprise:
						currentState = asset.blendShapeSetup.blendStateSurprise;
						break;
					case FABaseEmotions.Anticipation:
						currentState = asset.blendShapeSetup.blendStateAnticipation;
						break;
					default:
						currentState = asset.blendShapeSetup.blendStateNeutral;
						break;
				}
			}
			else if(mode == Mode.Phonemes)
			{
				// Load target state blend weights from asset:
				switch (targetPhoneme)
				{
					case FABasePhonemes.Group0_AI:
						currentState = asset.blendShapesSpeech.blendStateGroup0;
						break;
					case FABasePhonemes.Group1_E:
						currentState = asset.blendShapesSpeech.blendStateGroup1;
						break;
					case FABasePhonemes.Group2_U:
						currentState = asset.blendShapesSpeech.blendStateGroup2;
						break;
					case FABasePhonemes.Group3_O:
						currentState = asset.blendShapesSpeech.blendStateGroup3;
						break;
					case FABasePhonemes.Group4_CDGK:
						currentState = asset.blendShapesSpeech.blendStateGroup4;
						break;
					case FABasePhonemes.Group5_FV:
						currentState = asset.blendShapesSpeech.blendStateGroup5;
						break;
					case FABasePhonemes.Group6_LTh:
						currentState = asset.blendShapesSpeech.blendStateGroup6;
						break;
					case FABasePhonemes.Group7_MBP:
						currentState = asset.blendShapesSpeech.blendStateGroup7;
						break;
					case FABasePhonemes.Group8_WQ:
						currentState = asset.blendShapesSpeech.blendStateGroup8;
						break;
					case FABasePhonemes.Group9_Rest:
						currentState = asset.blendShapesSpeech.blendStateGroup9;
						break;
					default:
						currentState = asset.blendShapesSpeech.blendStateSilence;
						break;
				}
			}
			else if(mode == Mode.Library)
			{
				FABlendStateHandle outHandle = FABlendStateHandle.Default;
				if(asset.blendStateLibrary.getState(targetLibState, ref outHandle))
				{
					currentState = outHandle.state;
				}
				else
				{
					Debug.LogError("[FABlendStateEditor] Error: Library blend state '" + targetLibState +
						"' does not exist in preset asset '" + asset.name + "'.");
				}
			}

			updateController();
			changed = false;
		}

		private void saveBlendStates()
		{
			if (asset == null) return;

			if(mode == Mode.Emotions)
			{
				// Write the current state to the selected target state in the asset:
				switch (targetEmotion)
				{
					case FABaseEmotions.Joy:
						asset.blendShapeSetup.blendStateJoy = currentState;
						break;
					case FABaseEmotions.Sadness:
						asset.blendShapeSetup.blendStateSadness = currentState;
						break;
					case FABaseEmotions.Disgust:
						asset.blendShapeSetup.blendStateDisgust = currentState;
						break;
					case FABaseEmotions.Trust:
						asset.blendShapeSetup.blendStateTrust = currentState;
						break;
					case FABaseEmotions.Fear:
						asset.blendShapeSetup.blendStateFear = currentState;
						break;
					case FABaseEmotions.Anger:
						asset.blendShapeSetup.blendStateAnger = currentState;
						break;
					case FABaseEmotions.Surprise:
						asset.blendShapeSetup.blendStateSurprise = currentState;
						break;
					case FABaseEmotions.Anticipation:
						asset.blendShapeSetup.blendStateAnticipation = currentState;
						break;
					case FABaseEmotions.Neutral:
						asset.blendShapeSetup.blendStateNeutral = currentState;
						break;
					default:
						break;
				}
			}
			else if(mode == Mode.Phonemes)
			{
				// Write the current state to the selected target state in the asset:
				switch (targetPhoneme)
				{
					case FABasePhonemes.Group0_AI:
						asset.blendShapesSpeech.blendStateGroup0 = currentState;
						break;
					case FABasePhonemes.Group1_E:
						asset.blendShapesSpeech.blendStateGroup1 = currentState;
						break;
					case FABasePhonemes.Group2_U:
						asset.blendShapesSpeech.blendStateGroup2 = currentState;
						break;
					case FABasePhonemes.Group3_O:
						asset.blendShapesSpeech.blendStateGroup3 = currentState;
						break;
					case FABasePhonemes.Group4_CDGK:
						asset.blendShapesSpeech.blendStateGroup4 = currentState;
						break;
					case FABasePhonemes.Group5_FV:
						asset.blendShapesSpeech.blendStateGroup5 = currentState;
						break;
					case FABasePhonemes.Group6_LTh:
						asset.blendShapesSpeech.blendStateGroup6 = currentState;
						break;
					case FABasePhonemes.Group7_MBP:
						asset.blendShapesSpeech.blendStateGroup7 = currentState;
						break;
					case FABasePhonemes.Group8_WQ:
						asset.blendShapesSpeech.blendStateGroup8 = currentState;
						break;
					case FABasePhonemes.Group9_Rest:
						asset.blendShapesSpeech.blendStateGroup9 = currentState;
						break;
					case FABasePhonemes.None:
						asset.blendShapesSpeech.blendStateSilence = currentState;
						break;
					default:
						break;
				}
			}
			else if(mode == Mode.Library)
			{
				asset.blendStateLibrary.setState(targetLibState, currentState);
			}

			// Save asset:
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();
		}

		#endregion
	}
}
