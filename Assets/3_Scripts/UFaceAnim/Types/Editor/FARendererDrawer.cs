using System.Collections;
using UnityEngine;
using UnityEditor;

namespace UFaceAnim.Inspector
{
	[CustomPropertyDrawer(typeof(FARenderer.Target))]
	public class FARendererDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			//return base.GetPropertyHeight(property, label);
			return 16;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//base.OnGUI(position, property, label);
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			int prevIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			float w = position.width;
			float w0 = w * 0.7f;
			float w1 = w - w0;
			float h = position.height;

			Rect targetRect = new Rect(position.x, position.y, w0, h);
			Rect negatiRect = new Rect(position.x + w0, position.y, 20, h);
			Rect negatiPostRect = new Rect(position.x + w0 + 16, position.y, w1-16, h);

			GUIContent negatiCont = new GUIContent("Neg", "Wether to remap this target's weights to a negative value range, ex.: [0,-1] -> [0,1]");

			EditorGUI.PropertyField(targetRect, property.FindPropertyRelative("target"), GUIContent.none);
			EditorGUI.PropertyField(negatiRect, property.FindPropertyRelative("negativeRange"), GUIContent.none);
			EditorGUI.LabelField(negatiPostRect, negatiCont);

			//...

			EditorGUI.indentLevel = prevIndent;

			EditorGUI.EndProperty();
		}
	}
}
