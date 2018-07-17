using System.Collections;
using UnityEngine;
using UnityEditor;

namespace UFaceAnim.Editor
{
	[System.Serializable]
	public class FABlendStateEditor : EditorWindow
	{
		#region Fields

		[SerializeField]
		private bool changed = false;
		private bool phonemeMode = false;

		public FAPresets asset = null;
		public FAController controller = null;

		[SerializeField]
		private FABlendState currentState = FABlendState.Default;
		private FABaseEmotions targetEmotion = FABaseEmotions.Neutral;
		private FABasePhonemes targetPhoneme = FABasePhonemes.None;

		#endregion
		#region Methods

		[MenuItem("Window/UFaceAnim/Blend State Editor")]
		public static FABlendStateEditor showEditorWindow()
		{
			Rect rect = new Rect(100, 100, 300, 386);
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
				Repaint();
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
			EditorGUI.BeginDisabledGroup(!phonemeMode);
			if (GUILayout.Button("Emotions", EditorStyles.toolbarButton)) phonemeMode = false;
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(phonemeMode);
			if (GUILayout.Button("Phonemes", EditorStyles.toolbarButton)) phonemeMode = true;
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
			currentState.eyesWander = EditorGUILayout.Slider("Wander", currentState.eyesWander, -1, 1);

			// If a UI control was manipulated:
			if(GUI.changed)
			{
				changed = true;

				updateController();
			}
			GUI.changed = GUI.changed || prevChanged;

			EditorGUILayout.Separator();

			if(!phonemeMode)
			{
				GUIContent targetEmotCont = new GUIContent("Target emotion", "Which emotion to save the currently displayed blend state as.");
				targetEmotion = (FABaseEmotions)EditorGUILayout.EnumPopup(targetEmotCont, targetEmotion);
			}
			else
			{
				GUIContent targetPhonCont = new GUIContent("Target phoneme", "Which 'phoneme' to save the currently displayed blend state as.");
				targetPhoneme = (FABasePhonemes)EditorGUILayout.EnumPopup(targetPhonCont, targetPhoneme);
			}

			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Load target state"))
			{
				loadBlendState();
			}

			EditorGUI.BeginDisabledGroup(!phonemeMode && targetEmotion == FABaseEmotions.None);
			if (GUILayout.Button("Apply blend states"))
			{
				saveBlendStates();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
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

			if(!phonemeMode)
			{
				Debug.Log("TEST: Loading state: " + targetEmotion.ToString());
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
			else
			{
				Debug.Log("TEST: Loading state: " + targetPhoneme.ToString());
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

			updateController();
			changed = false;
		}

		private void saveBlendStates()
		{
			if (asset == null) return;

			if(!phonemeMode)
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
			else
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

			// Save asset:
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();
		}

		#endregion
	}
}
