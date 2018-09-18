using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UHairGen
{
	[System.Serializable]
	public class HGEditor : EditorWindow
	{
		#region Fields

		private bool rebuild = true;

		public HGHair hair = null;
		public HGHairBody body = null;
		//...

		#endregion
		#region Methods

		[MenuItem("Window/Hair/Style Editor")]
		public static HGEditor getWindow()
		{
			return GetWindow<HGEditor>("Hair Style Editor", true);
		}

		private void Update()
		{
			// Select hair body straight from the user's live editor selection:
			GameObject selectionGO = Selection.activeGameObject;
			HGHairBody prevBody = body;
			if (selectionGO != null && (body == null || selectionGO != body.gameObject))
			{
				body = selectionGO.GetComponentInChildren<HGHairBody>();
				if(prevBody != body && body != null)
				{
					if(body.hair != null)
						hair = body.hair;
					else if(hair != null)
						body.hair = hair;

					Repaint();
					rebuild = true;
				}
			}

			if (hair != null && body != null && Application.isEditor)
			{
				if(rebuild)
				{
					rebuild = false;
					body.destroy();
					body.create();
				}
			}
		}

		void OnGUI()
		{
			body = (HGHairBody)EditorGUILayout.ObjectField("Hair Body", body, typeof(HGHairBody), true);
			hair = (HGHair)EditorGUILayout.ObjectField("Hair asset", hair, typeof(HGHair), false);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("New asset"))
			{
				createNewHairAsset();
			}
			EditorGUI.BeginDisabledGroup(hair == null);
			if (GUILayout.Button("Reload asset"))
			{
				loadHairAsset();
			}
			if (GUILayout.Button("Save changes"))
			{
				saveHairAsset();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			if (body == null || hair == null) return;
			EditorGUILayout.Separator();

			for (int i = 0; i < hair.regions.Length; ++i)
			{
				EditorGUILayout.BeginHorizontal();
				hair.regions[i].x = EditorGUILayout.Slider("Pos X", hair.regions[i].x, 0.0f, 1.0f);
				hair.regions[i].y = EditorGUILayout.Slider("Pos Y", hair.regions[i].y, 0.0f, 1.0f);
				EditorGUILayout.EndHorizontal();

				drawRangeLayout("Length", ref hair.regions[i].length);
			}

			EditorGUILayout.Separator();


			//...
		}

		private void drawRange(Rect rect, string label, ref HGRange range)
		{
			EditorGUI.MinMaxSlider(rect, label, ref range.min, ref range.max, 0, 1);
		}
		private void drawRangeLayout(string label, ref HGRange range)
		{
			EditorGUILayout.MinMaxSlider(label, ref range.min, ref range.max, 0, 1);
		}

		#endregion
		#region Methods Assets

		private void createNewHairAsset()
		{
			hair = ScriptableObject.CreateInstance<HGHair>();
			if(hair.regions == null || hair.regions.Length == 0)
			{
				hair.regions = new HGRegion[1] { new HGRegion(0, 0.23f, new HGRange(0.1f)) };
			}
			AssetDatabase.CreateAsset(hair, "Assets/new Hairstyle.asset");
			rebuild = true;
		}
		private void loadHairAsset()
		{
			if (hair == null)
			{
				Debug.LogError("[HGEditor] Error! Unable to (re)load null hair asset. Aborting.");
				return;
			}
			string path = AssetDatabase.GetAssetPath(hair);
			if(string.IsNullOrEmpty(path)) return;
			hair = AssetDatabase.LoadAssetAtPath<HGHair>(path);
			rebuild = true;
		}
		private void saveHairAsset()
		{
			if (hair == null)
			{
				Debug.LogError("[HGEditor] Error! Unable to save null hair asset to file. Aborting.");
				return;
			}
			EditorUtility.SetDirty(hair);
			AssetDatabase.SaveAssets();
			rebuild = true;
		}

		#endregion
	}
}
