using UnityEngine;
using System.Collections;

namespace UFaceAnim
{
	[System.Serializable]
	public struct FAEmotion
	{
		// NOTE: rather than define all dimensions ourselves, using a vector4 was the more convenient
		// solution, as it already has all the vector math, so we don't have to implement that again.
		#region Fields

		[SerializeField]
		private Vector4 vector;

		#endregion
		#region Properties

		public float ValueSadnessJoy
		{
			get { return vector.x; }
		}
		public float ValueDisgustTrust
		{
			get { return vector.y; }
		}
		public float ValueFearAnger
		{
			get { return vector.z; }
		}
		public float ValueSurpiseAnticipation
		{
			get { return vector.w; }
		}
		public float ValueNeutral
		{
			get { return 1.0f - vector.magnitude; }
		}

		/// <summary>
		/// The underlying vector structure of the emotion.
		/// </summary>
		/// <value>The vector.</value>
		public Vector4 Vector
		{
			get { return vector; }
			set { vector = value; clamp(); }
		}

		public static FAEmotion Neutral
		{
			get { return  new FAEmotion() { vector=Vector4.zero }; }
		}
		public static FAEmotion Joy
		{
			get { return  new FAEmotion() { vector=new Vector4(1,0,0,0) }; }
		}
		public static FAEmotion Sadness
		{
			get { return  new FAEmotion() { vector=new Vector4(-1,0,0,0) }; }
		}
		public static FAEmotion Trust
		{
			get { return  new FAEmotion() { vector=new Vector4(0,1,0,0) }; }
		}
		public static FAEmotion Disgust
		{
			get { return  new FAEmotion() { vector=new Vector4(0,-1,0,0) }; }
		}
		public static FAEmotion Anger
		{
			get { return  new FAEmotion() { vector=new Vector4(0,0,1,0) }; }
		}
		public static FAEmotion Fear
		{
			get { return  new FAEmotion() { vector=new Vector4(0,0,-1,0) }; }
		}
		public static FAEmotion Anticipation
		{
			get { return  new FAEmotion() { vector=new Vector4(0,0,0,1) }; }
		}
		public static FAEmotion Surprise
		{
			get { return  new FAEmotion() { vector=new Vector4(0,0,0,-1) }; }
		}

		#endregion
		#region Methods

		public void clamp()
		{
			if(vector.sqrMagnitude > 1)
				vector.Normalize();
		}
		public void normalize()
		{
			vector.Normalize();
		}
		public void scale(float k)
		{
			vector *= k;
		}
		public void scale(Vector4 v)
		{
			vector = Vector4.Scale(vector, v);
		}
		public void invert()
		{
			vector *= -1.0f;
		}
		public void randomize()
		{
			vector.x = Random.Range(-1,1);
			vector.y = Random.Range(-1,1);
			vector.z = Random.Range(-1,1);
			vector.w = Random.Range(-1,1);
		}
		public void round()
		{
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			vector.z = Mathf.Round(vector.z);
			vector.w = Mathf.Round(vector.w);
		}

		public static FAEmotion lerp(FAEmotion a, FAEmotion b, float k)
		{
			Vector4 v = Vector4.Lerp(a.vector, b.vector, k);
			return new FAEmotion() { vector=v };
		}

		#endregion
	}
}
