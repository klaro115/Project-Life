using UnityEngine;
using UnityEditor;

namespace MathExt
{
	[CustomPropertyDrawer(typeof(Complex))]
	public class ComplexDrawer : PropertyDrawer
	{
		private const float w0 = 12;
		private const float height = 16;
		private const float wHFactor = 0.375f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			float wTotal = position.width;
			float wH0 = wTotal * wHFactor;
			float wH1 = wTotal * (1.0f - wHFactor);
			float w1 = wH1 / 2 - w0;

			float p0 = position.x + wH0;
			float p1 = p0 + w0;
			float p2 = p1 + w1;
			float p3 = p2 + w0;
			float pY = position.y;

			SerializedProperty a = property.FindPropertyRelative("a");
			SerializedProperty b = property.FindPropertyRelative("b");

			EditorGUI.LabelField(new Rect(position.x, pY, wH0, height), label);
			EditorGUI.LabelField(new Rect(p0, pY, w0, height), "R");
			a.floatValue = EditorGUI.FloatField(new Rect(p1, pY, w1, height), a.floatValue);
			EditorGUI.LabelField(new Rect(p2, pY, w0, height), "I");
			b.floatValue = EditorGUI.FloatField(new Rect(p3, pY, w1, height), b.floatValue);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return height;
		}
	}
}
