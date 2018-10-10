using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UHairGen
{
	[System.Serializable]
	public class HGEditorAnchorSpline : EditorWindow
	{
		#region Fields

		private HGHair hair = null;
		private HGHairBody body = null;
		private int anchorIndex = 0;

		#endregion
		#region Properties

		public HGHair Hair
		{
			get { return hair; }
		}
		public HGHairBody Body
		{
			get { return body; }
		}
		//temp (the compiler won't shut up without this...)
		public int AnchorIndex { get { return anchorIndex; } }

		#endregion
		#region Methods

		public static HGEditorAnchorSpline getWindow()
		{
			return GetWindowWithRect<HGEditorAnchorSpline>(new Rect(100, 100, 250, 300), true, "Spline Editor", true);
		}

		public void setBody(HGHairBody inBody)
		{
			if (inBody != null) body = inBody;
			//...
		}

		public void setContext(HGHair inHair, HGHairBody inBody, int inAnchorIndex)
		{
			hair = inHair;
			anchorIndex = inAnchorIndex;

			setBody(inBody);
		}

		void OnGUI()
		{
			if(hair == null || body == null)
			{
				EditorGUILayout.LabelField("Please select a hair body.");
			}

			// TODO
		}

		#endregion
	}
}
