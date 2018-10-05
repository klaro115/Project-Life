using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UHairGen
{
	public static class HGEditorRegionPreview
	{
		#region Types

		public enum Perspective
		{
			Topdown,
			Left,
			Right,
			Front,
			Back
		}

		#endregion
		#region Fields

		private static Perspective perspective = Perspective.Topdown;

		private static Vector3 c = new Vector3(128, 128, 0);
		private static Vector3 cc = new Vector3(235, 235, 0);

		private const float previewLengthScale = 64.0f;
		private const float r0 = 32.0f;

		#endregion
		#region Methods

		public static void drawPreview(HGHair hair, float offsetX, float offsetY)
		{
			if (hair == null) return;

			c = new Vector3(128 + offsetX, 128 + offsetY, 0);
			cc = new Vector3(offsetX + 235, offsetY + 235, 0);
			Vector2 cp = new Vector2(offsetX + 5, offsetY + 232);

			// Draw the preview container and some reference objects:
			EditorGUI.DrawRect(new Rect(offsetX, offsetY, 256, 256), Color.black);
			EditorGUI.DrawRect(new Rect(offsetX + 1, offsetY + 1, 254, 254), new Color(0.277f, 0.277f, 0.277f));

			Handles.color = Color.black;
			Handles.DrawWireDisc(c, Vector3.forward, r0);
			Handles.DrawLine(c - Vector3.up * r0, c + Vector3.up * r0);
			Handles.DrawLine(c - Vector3.right * r0, c + Vector3.right * r0);

			// Draw content differently, based on projection/perspective:
			switch (perspective)
			{
				case Perspective.Topdown:
					drawTopdown(hair);
					break;
				case Perspective.Left:
					drawRadial(hair, 0.75f);
					break;
				case Perspective.Right:
					drawRadial(hair, 0.25f);
					break;
				case Perspective.Front:
					drawRadial(hair, 0.0f);
					break;
				case Perspective.Back:
					drawRadial(hair, 0.5f);
					break;
				default:
					break;
			}

			// Draw buttons for selecting preview perspective:
			if (GUI.Button(new Rect(cp.x, cp.y, 18, 18), "F")) perspective = Perspective.Front;
			if (GUI.Button(new Rect(cp.x+19, cp.y, 18, 18), "B")) perspective = Perspective.Back;
			if (GUI.Button(new Rect(cp.x+38, cp.y, 18, 18), "L")) perspective = Perspective.Left;
			if (GUI.Button(new Rect(cp.x+57, cp.y, 18, 18), "R")) perspective = Perspective.Right;
			if (GUI.Button(new Rect(cp.x, cp.y-19, 18, 18), "T")) perspective = Perspective.Topdown;
		}

		private static void drawTopdown(HGHair hair)
		{
			// Draw axis handles to clarify current viewing perspective:
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
				EditorGUI.DrawRect(new Rect(pos0.x - 1.5f, pos0.y - 1.5f, 3, 3), new Color(1, 1, 0, 0.35f));
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

		private static void drawRadial(HGHair hair, float projectionX)
		{
			float xLow = projectionX - 0.25f;
			float xHigh = projectionX + 0.25f;

			if (hair.regions == null || hair.regions.Length == 0) return;

			// Draw control points as designated by the entries in the hairstyle's regions array:
			float modY = 1.0f;
			HGRegion region = new HGRegion();
			for (int i = 0; i < hair.regions.Length; ++i)
			{
				region = hair.regions[i];
				if (!HGMath.containsRadial(region.x, xLow, xHigh)) continue;

				float perspX = (region.x - xLow) * 2.0f;
				float cosX = Mathf.Cos(perspX * Mathf.PI);
				float posX = cosX * r0;

				float angY = (1 - region.y) * Mathf.PI * 0.5f;
				modY = Mathf.Sin(angY);

				Vector2 pos = new Vector2(c.x + posX, c.y - modY * r0);
				EditorGUI.DrawRect(new Rect(pos.x - 1.5f, pos.y - 1.5f, 3, 3), Color.yellow);
			}

			int refIndex = 0;
			region = hair.lerpRegions(0, ref refIndex);
			float prevPosX = Mathf.Cos(-xLow * 2 * Mathf.PI) * r0;
			Vector3 prevPosMin = c + new Vector3(prevPosX, region.length.min * previewLengthScale);
			Vector3 prevPosMax = c + new Vector3(prevPosX, region.length.max * previewLengthScale);
			for (int i = 0; i < 33; ++i)
			{
				float x = i * 0.03125f;
				region = hair.lerpRegions(x, ref refIndex);

				// Use a more transparent color to indicate a line is on the diametrically opposed side of the hair body:
				Handles.color = HGMath.containsRadial(x, xLow, xHigh) ? Color.yellow : new Color(1, 1, 0, 0.35f);

				float perspX = (x - xLow) * 2.0f;
				float cosX = Mathf.Cos(perspX * Mathf.PI);
				float posX = cosX * r0;

				float baseLength = region.y * r0;
				float posYMin = baseLength + region.length.min * previewLengthScale;
				float posYMax = baseLength + region.length.max * previewLengthScale;

				Vector3 curPosMin = c + new Vector3(posX, posYMin);
				Vector3 curPosMax = c + new Vector3(posX, posYMax);

				Handles.DrawLine(curPosMin, prevPosMin);
				Handles.DrawLine(curPosMax, prevPosMax);

				if (cosX < -0.99f || cosX > 0.99f)
				{
					Handles.DrawLine(c + Vector3.right * posX, curPosMax);
				}

				prevPosMin = curPosMin;
				prevPosMax = curPosMax;
			}
		}

		#endregion
	}
}
