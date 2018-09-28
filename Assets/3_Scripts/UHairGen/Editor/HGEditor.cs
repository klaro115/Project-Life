using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UHairGen
{
	[System.Serializable]
	public class HGEditor : EditorWindow
	{
		#region Fields

		private bool rebuild = true;

		public HGHair hair = null;
		public HGHairBody body = null;

		private ReorderableList rolRegions = null;

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
			if (selectionGO != null && (body == null || selectionGO.transform.root != body.transform.root))
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

			EditorGUILayout.LabelField("Regions:", EditorStyles.boldLabel);
			drawRegionsList();
			drawRegionsPreview();

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
		#region Methods HairRegions

		private void drawRegionsList()
		{
			if (rolRegions == null)
			{
				rolRegions = new ReorderableList(hair.regions, typeof(HGRegion), true, true, true, true);
				rolRegions.onAddCallback = callbackRegionAdd;
				rolRegions.onRemoveCallback = callbackRegionRemove;
				rolRegions.drawElementCallback = callbackRegionElement;
				rolRegions.drawHeaderCallback = callbackRegionHeader;
				//rolRegions.elementHeight = 32;
			}

			rolRegions.DoLayoutList();
		}

		private void callbackRegionAdd(ReorderableList inList)
		{
			rebuild = true;
			HGRegion[] regionsOld = (HGRegion[])inList.list;
			HGRegion newRegion = new HGRegion(0, 0.2f, new HGRange(0.2f));
			if (regionsOld == null || regionsOld.Length == 0)
			{
				hair.regions = new HGRegion[1] { newRegion };
				inList.list = hair.regions;
				return;
			}
			HGRegion[] regionsNew = new HGRegion[regionsOld.Length + 1];
			for (int i = 0; i < regionsOld.Length; ++i)
				regionsNew[i] = regionsOld[i];
			newRegion.x = 1.0f;
			regionsNew[regionsOld.Length] = newRegion;
			hair.regions = regionsNew;
			inList.list = hair.regions;
		}
		private void callbackRegionRemove(ReorderableList inList)
		{
			rebuild = true;
			int index = inList.index;
			HGRegion[] regionsOld = (HGRegion[])inList.list;
			if (regionsOld == null || regionsOld.Length < 2)
			{
				hair.regions = null;
				inList.list = null;
				inList.index = -1;
				return;
			}
			HGRegion[] regionsNew = new HGRegion[regionsOld.Length - 1];
			for (int i = 0; i < regionsNew.Length; ++i)
				regionsNew[i] = regionsOld[i >= index ? i + 1 : i];
			hair.regions = regionsNew;
			inList.list = hair.regions;
		}
		private void callbackRegionElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (hair.regions == null || index < 0 || index >= hair.regions.Length) return;

			float h = rect.height;
			float x = rect.x;
			float y = rect.y;
			float w0 = Mathf.Floor(rect.width * 0.25f);
			float w1 = Mathf.Floor(rect.width * 0.5f);
			Rect r0 = new Rect(x, y, w0-1, h);
			Rect r1 = new Rect(x + w0 + 1, y, w0-1, h);
			Rect r2 = new Rect(x + w1 + 2, y, w1-2, h);

			hair.regions[index].x = EditorGUI.Slider(r0, hair.regions[index].x, 0.0f, 1.0f);
			hair.regions[index].y = EditorGUI.Slider(r1, hair.regions[index].y, 0.0f, 1.0f);
			EditorGUI.MinMaxSlider(r2, ref hair.regions[index].length.min, ref hair.regions[index].length.max, 0, 1);
		}
		private void callbackRegionHeader(Rect rect)
		{
			float w = rect.width - 13;
			float w0 = Mathf.Floor(w * 0.25f);
			float w1 = Mathf.Floor(w * 0.5f);
			EditorGUI.LabelField(new Rect(rect.x + 13, rect.y, w0, rect.height), "x");
			EditorGUI.LabelField(new Rect(rect.x + 13 + w0, rect.y, w0, rect.height), "y");
			EditorGUI.LabelField(new Rect(rect.x + 13 + w1, rect.y, w1, rect.height), "Length (min/max)");
		}

		private void drawRegionsPreview()
		{
			EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);

			const float r0 = 32.0f;
			float offsetX = 1;
			float offsetY = 102 + rolRegions.GetHeight();
			Vector3 c = new Vector3(128+offsetX, 128+offsetY, 0);

			EditorGUI.DrawRect(new Rect(offsetX, offsetY, 256, 256), Color.black);
			EditorGUI.DrawRect(new Rect(offsetX+1, offsetY+1, 254, 254), new Color(0.277f, 0.277f, 0.277f));

			Handles.color = Color.black;
			Handles.DrawWireDisc(c, Vector3.forward, r0);
			Handles.DrawLine(c - Vector3.up * r0, c + Vector3.up * r0);
			Handles.DrawLine(c - Vector3.right * r0, c + Vector3.right * r0);
			Handles.color = Color.red;
			Handles.DrawLine(c + Vector3.up * r0, c + Vector3.up * r0 * 2);

			if (hair.regions == null || hair.regions.Length == 0) return;

			for (int i = 0; i < hair.regions.Length; ++i)
			{
				HGRegion region = hair.regions[i];
				float l = r0 + region.length.Center * 10;
				float ang = region.x * Mathf.PI * 2.0f;
				float angY = region.y * Mathf.PI * 0.5f;
				float lY = Mathf.Sin(angY) * l;
				float posX = Mathf.Cos(ang) * lY - 2 + c.x;
				float posY = Mathf.Sin(ang) * lY - 2 + c.y;
				EditorGUI.DrawRect(new Rect(posX, posY, 4, 4), Color.yellow);
			}
			Handles.color = Color.yellow;
			for (int i = 0; i < 32; ++i)
			{

			}
		}

		#endregion
	}
}
