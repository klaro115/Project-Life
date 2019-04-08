using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim.Poses
{
	[System.Serializable]
	public class PAPose : ScriptableObject
	{
		#region Types

		[System.Serializable]
		public struct Element
		{
			public Element(string _name)
			{
				name = _name;
				localPosition = Vector3.zero;
				localRotation = Quaternion.identity;
			}
			public Element(string _name, Vector3 _localPosition, Quaternion _localRotation)
			{
				name = _name;
				localPosition = _localPosition;
				localRotation = _localRotation;
			}

			public string name;
			public Vector3 localPosition;
			public Quaternion localRotation;
		}

		#endregion
		#region Fields

		public Element[] members = null;

		#endregion
		#region Properties

		public static PAPose HandDefault => new PAPose()
		{
			members = new Element[]
			{
				new Element("Thumb0"),
				new Element("Thumb1"),
				new Element("Thumb2Tip"),

				new Element("Index0"),
				new Element("Index1"),
				new Element("Index2"),
				new Element("Index3"),

				new Element("Middle0"),
				new Element("Middle1"),
				new Element("Middle2"),
				new Element("Middle3"),

				new Element("Ring0"),
				new Element("Ring1"),
				new Element("Ring2"),
				new Element("Ring3"),

				new Element("Pinky0"),
				new Element("Pinky1"),
				new Element("Pinky2"),
				new Element("Pinky3"),
			}
		};

		#endregion
	}
}
