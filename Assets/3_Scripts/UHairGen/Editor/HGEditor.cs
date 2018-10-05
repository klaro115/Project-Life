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
		private ReorderableList rolAnchors = null;

		private HGEditorAnchorSpline splineEditor = null;

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

					if(splineEditor != null && splineEditor.Body != body)
					{
						splineEditor.setBody(body);
					}

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

			EditorGUILayout.LabelField("Anchors:", EditorStyles.boldLabel);
			drawAnchorsList();
			if (hair.anchors != null && rolAnchors.index >= 0 && rolAnchors.index < hair.anchors.Length)
			{
				drawAnchorEditor(ref hair.anchors[rolAnchors.index]);
			}

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

			if (hair.regions != null)
				rolRegions.DoLayoutList();
			else
				if (GUILayout.Button("Create region")) callbackRegionAdd(rolRegions);
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

			float offsetX = 1;
			float offsetY = 102 + rolRegions.GetHeight();

			HGEditorRegionPreview.drawPreview(hair, offsetX, offsetY);
		}

		#endregion
		#region Methods Anchors

		private void drawAnchorsList()
		{
			if (rolAnchors == null)
			{
				rolAnchors = new ReorderableList(hair.anchors, typeof(HGAnchor), true, true, true, true);
				rolAnchors.onAddCallback = callbackAnchorAdd;
				rolAnchors.onRemoveCallback = callbackAnchorRemove;
				rolAnchors.drawElementCallback = callbackAnchorElement;
				rolAnchors.drawHeaderCallback = callbackAnchorHeader;
			}
			else if (hair.anchors != null && rolAnchors.list != hair.anchors)
			{
				rolAnchors.list = hair.anchors;
			}

			if (hair.anchors != null)
				rolAnchors.DoLayoutList();
			else
				if (GUILayout.Button("Create anchor")) callbackAnchorAdd(rolAnchors);
		}

		private void callbackAnchorAdd(ReorderableList inList)
		{
			rebuild = true;
			if (splineEditor != null)
			{
				splineEditor.Close();
				splineEditor = null;
			}
			HGAnchor[] anchorsOld = (HGAnchor[])inList.list;
			HGAnchor newAnchor = new HGAnchor() { name="new Anchor", x=0, y=0, type=HGAnchorType.Point };
			if (anchorsOld == null || anchorsOld.Length == 0)
			{
				hair.anchors = new HGAnchor[1] { newAnchor };
				inList.list = hair.anchors;
				inList.index = 0;
				return;
			}
			HGAnchor[] anchorsNew = new HGAnchor[anchorsOld.Length + 1];
			for (int i = 0; i < anchorsOld.Length; ++i)
				anchorsNew[i] = anchorsOld[i];
			anchorsNew[anchorsOld.Length] = newAnchor;
			hair.anchors = anchorsNew;
			inList.list = hair.anchors;
			inList.index = anchorsNew.Length - 1;
		}
		private void callbackAnchorRemove(ReorderableList inList)
		{
			rebuild = true;
			if (splineEditor != null)
			{
				splineEditor.Close();
				splineEditor = null;
			}
			int index = inList.index;
			HGAnchor[] anchorsOld = (HGAnchor[])inList.list;
			if (anchorsOld == null || anchorsOld.Length < 2)
			{
				hair.anchors = null;
				inList.list = null;
				inList.index = -1;
				return;
			}
			HGAnchor[] anchorsNew = new HGAnchor[anchorsOld.Length - 1];
			for (int i = 0; i < anchorsNew.Length; ++i)
				anchorsNew[i] = anchorsOld[i >= index ? i + 1 : i];
			hair.anchors = anchorsNew;
			inList.list = hair.anchors;
		}
		private void callbackAnchorElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (hair.anchors == null || index < 0 || index >= hair.anchors.Length) return;

			float w = rect.width - 32;
			float h = rect.height;
			float x = rect.x;
			float y = rect.y;
			float w0 = Mathf.Floor(w * 0.6f);
			float w1 = Mathf.Floor(w * 0.2f);
			Rect r0 = new Rect(x, y, w0 - 2, h);
			Rect r1 = new Rect(x + w0 + 1, y, w1 - 1, h);
			Rect r2 = new Rect(x + w0 + w1 + 2, y, w1 - 1, h);
			Rect r3 = new Rect(x + rect.width - 32, y, 32, h);

			HGAnchor anchor = hair.anchors[index];
			EditorGUI.LabelField(r0, anchor.name);
			hair.anchors[index].x = EditorGUI.FloatField(r1, anchor.x);
			hair.anchors[index].y = EditorGUI.FloatField(r2, anchor.y);
			string typeTxt = null;
			switch (anchor.type)
			{
				case HGAnchorType.Directional:
					typeTxt = "Dir.";
					break;
				case HGAnchorType.Spline:
					typeTxt = "Spline";
					break;
				default:
					typeTxt = "Point";
					break;
			}
			EditorGUI.LabelField(r3, typeTxt);
		}
		private void callbackAnchorHeader(Rect rect)
		{
			float w = rect.width - 13 - 32;
			float w0 = Mathf.Floor(w * 0.6f);
			float w1 = Mathf.Floor(w * 0.2f);
			EditorGUI.LabelField(new Rect(rect.x + 13, rect.y, w0, rect.height), "Anchor Name");
			EditorGUI.LabelField(new Rect(rect.x + 13 + w0, rect.y, w1, rect.height), "x");
			EditorGUI.LabelField(new Rect(rect.x + 13 + w0 + w1, rect.y, w1, rect.height), "y");
			EditorGUI.LabelField(new Rect(rect.x + rect.width - 32, rect.y, 32, rect.height), "Type");
		}

		private void drawAnchorEditor(ref HGAnchor anchor)
		{
			const float labelWidth = 146;

			anchor.name = EditorGUILayout.TextField("Anchor name", anchor.name);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Position (x, y)", GUILayout.Width(labelWidth));
			anchor.x = EditorGUILayout.FloatField(anchor.x);
			anchor.y = EditorGUILayout.FloatField(anchor.y);
			EditorGUILayout.EndHorizontal();

			anchor.pullRadius = EditorGUILayout.FloatField("Pull radius", anchor.pullRadius);
			anchor.overwriteStiffness = EditorGUILayout.FloatField("Exit stiffness", anchor.overwriteStiffness);

			anchor.type = (HGAnchorType)EditorGUILayout.EnumPopup("Behaviour type", anchor.type);
			switch (anchor.type)
			{
				case HGAnchorType.Directional:
					{
						anchor.exitDirection = EditorGUILayout.Vector3Field("Exit direction", anchor.exitDirection);
					}
					break;
				case HGAnchorType.Spline:
					{
						anchor.exitDirection = EditorGUILayout.Vector3Field("Exit direction", anchor.exitDirection);
						if(GUILayout.Button("Edit Spline"))
						{
							if(splineEditor == null) splineEditor = HGEditorAnchorSpline.getWindow();
							if (splineEditor.Hair != hair) splineEditor.setContext(hair, body, rolAnchors.index);
						}
					}
					break;
				default:
					break;
			}
		}

		#endregion
	}
}
