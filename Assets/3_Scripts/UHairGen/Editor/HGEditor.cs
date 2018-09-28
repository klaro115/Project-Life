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
		[SerializeField]
		private bool showRegionPreview = true;

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
			}
			else if(hair.regions != null && rolRegions.list != hair.regions)
			{
				rolRegions.list = hair.regions;
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
				inList.index = 0;
				return;
			}
			HGRegion[] regionsNew = new HGRegion[regionsOld.Length + 1];
			for (int i = 0; i < regionsOld.Length; ++i)
				regionsNew[i] = regionsOld[i];
			newRegion.x = 1.0f;
			regionsNew[regionsOld.Length] = newRegion;
			hair.regions = regionsNew;
			inList.list = hair.regions;
			inList.index = regionsNew.Length - 1;
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
			float w0 = Mathf.Floor(rect.width * 0.2f);
			float w1 = Mathf.Floor(rect.width * 0.4f);
			Rect r0 = new Rect(x, y, w0-1, h);
			Rect r1 = new Rect(x + w0 + 1, y, w0-1, h);
			Rect r2 = new Rect(x + w1 + 2, y, rect.width-2*w0-2, h);

			hair.regions[index].x = EditorGUI.FloatField(r0, hair.regions[index].x);
			hair.regions[index].y = EditorGUI.FloatField(r1, hair.regions[index].y);
			EditorGUI.MinMaxSlider(r2, ref hair.regions[index].length.min, ref hair.regions[index].length.max, 0, 1);
		}
		private void callbackRegionHeader(Rect rect)
		{
			float w = rect.width - 13;
			float w0 = Mathf.Floor(w * 0.2f);
			float w1 = Mathf.Floor(w * 0.6f);
			EditorGUI.LabelField(new Rect(rect.x + 13, rect.y, w0, rect.height), "x");
			EditorGUI.LabelField(new Rect(rect.x + 13 + w0, rect.y, w0, rect.height), "y");
			EditorGUI.LabelField(new Rect(rect.x + 13 + 2 * w0, rect.y, w1, rect.height), "Length (min/max)");
		}

		private void drawRegionsPreview()
		{
			// Allow the preview section to be opened and closed via a foldout:
			showRegionPreview = EditorGUILayout.Foldout(showRegionPreview, "Preview", true);
			if (!showRegionPreview) return;

			// Draw the preview container and some reference objects:
			const float previewLengthScale = 64.0f;
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

			Vector3 cc = new Vector3(offsetX + 235, offsetY+235, 0);
			Handles.color = Color.blue;
			Handles.DrawLine(cc, cc + Vector3.right * 16);
			Handles.color = Color.red;
			Handles.DrawLine(cc, cc + Vector3.up * 16);
			EditorGUI.DrawRect(new Rect(cc.x - 2, cc.y - 2, 2, 2), Color.green);

			if (hair.regions == null || hair.regions.Length == 0) return;

			// Draw control points as designated by the entries in the hairstyle's regions array:
			float modY = 1.0f;
			for (int i = 0; i < hair.regions.Length; ++i)
			{
				HGRegion region = hair.regions[i];
				float l = r0 + region.length.max * previewLengthScale;
				float ang = region.x * Mathf.PI * 2.0f;
				float angY = region.y * Mathf.PI * 0.5f;
				modY = Mathf.Sin(angY);
				Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
				Vector2 pos = (Vector2)c + dir * modY * l;
				Vector2 pos0 = (Vector2)c + dir * modY * r0;
				EditorGUI.DrawRect(new Rect(pos0.x - 1.5f, pos0.y - 1.5f, 3, 3), new Color(1,1,0,0.35f));
				EditorGUI.DrawRect(new Rect(pos.x - 1.5f, pos.y - 1.5f, 3, 3), Color.yellow);
			}

			// Draw two closed lines/curves showing the min/max lengths of the hair strands radially across the hair body:
			int curIndex = 0;
			HGRegion curRegion = hair.regions[curIndex];

			Handles.color = Color.yellow;
			modY = Mathf.Sin(curRegion.y * Mathf.PI * 0.5f);
			Vector3 prevMin = c + Vector3.right * (r0 + curRegion.length.min * previewLengthScale) * modY;
			Vector3 prevMax = c + Vector3.right * (r0 + curRegion.length.max * previewLengthScale) * modY;
			for (int i = 0; i < 33; ++i)
			{
				float x = i * 0.03125f; // aka: x = i / 32;
				curRegion = hair.lerpRegions(x, ref curIndex);

				float angY = curRegion.y * Mathf.PI * 0.5f;
				float ang = x * Mathf.PI * 2;
				modY = Mathf.Sin(angY);
				Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0) * modY;
				Vector3 posMin = c + dir * (r0 + curRegion.length.min * previewLengthScale);
				Vector3 posMax = c + dir * (r0 + curRegion.length.max * previewLengthScale);
				Handles.DrawLine(prevMin, posMin);
				Handles.DrawLine(prevMax, posMax);

				prevMin = posMin;
				prevMax = posMax;
			}
		}

		#endregion
	}
}
