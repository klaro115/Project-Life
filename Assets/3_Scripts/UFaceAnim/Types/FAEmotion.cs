using UnityEngine;
using System.Collections;

namespace UFaceAnim
{
	[System.Serializable]
	public struct FAEmotion
	{
		// NOTE: rather than define all dimensions ourselves, using a vector4 was the more convenient
		// solution, as it already has all the vector math, so we don't have to implement that again.
		#region Constructors

		public FAEmotion(float sadnessJoy, float disgustTrust, float fearAnger, float surpriseAntic)
		{
			vector = new Vector4(sadnessJoy, disgustTrust, fearAnger, surpriseAntic);
		}
		public FAEmotion(Vector4 inVector)
		{
			vector = inVector;
		}

		#endregion
		#region Fields

		[SerializeField]
		private Vector4 vector;

		private static readonly char[] stringSplitChars = new char[1] { ',' };

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
			vector.x = Random.Range(-1.0f,1.0f);
			vector.y = Random.Range(-1.0f,1.0f);
			vector.z = Random.Range(-1.0f,1.0f);
			vector.w = Random.Range(-1.0f,1.0f);
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
		public static FAEmotion moveToward(FAEmotion a, FAEmotion b, float maxDelta)
		{
			Vector4 v = Vector4.MoveTowards(a.vector, b.vector, maxDelta);
			return new FAEmotion() { vector=v };
		}

		public static FAEmotion parseString(string emotionString)
		{
			if(string.IsNullOrEmpty(emotionString) || emotionString.Length < 4) return FAEmotion.Neutral;
			string txt = emotionString;

			// Emotion string encoding may start with the prefix '#E', skip that:
			if(txt[0] == '#')
			{
				txt = txt.Substring(2);
			}
			// The coding part should be enclosed in paranteses, get rid of those now:
			if(txt[0] == '(')
			{
				txt = txt.Substring(1, txt.Length - 2);
			}

			// Split the actual coding string into coding parts/blocks:
			string[] parts = txt.Split(stringSplitChars, stringSplitChars.Length);

			Vector4 vector = Vector4.zero;
			try
			{
				// Parse all coding parts one by one:
				for(int i = 0; i < parts.Length; ++i)
				{
					// NOTE: The individual coding parts come in the format 'ab=X', with 'ab' being the
					// emotion pair prefix (ex.: Sadness/Joy) and 'X' a percentage ranging from 0-100.
					// All characters must be lower-case, with no extra spaces!

					string part = parts[i];
					if(parts.Length < 4) continue;

					char prefix = part[0];
					int valueInt = System.Convert.ToInt32(part.Substring(3));
					float value = valueInt * 0.01f;

					switch (prefix)
					{
					case 's':	// 's' can be prefix 'sj' or 'sa'.
						{
							char prefix2 = part[1];
							if(prefix2 == 'j')		// 'sj' -> sadness/joy
								vector.x = value;
							else if(prefix2 == 'a')	// 'sa' -> surprise/anticipation
								vector.w = value;
						}
						break;
					case 'd':	// 'dt' -> disgust/trust
						vector.y = value;
						break;
					case 'f':	// 'fa' -> fear/anger
						vector.z = value;
						break;
					default:
						break;
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError("[FAEmotion] ERROR! An exception was caught trying to parse the " +
					"emotion string '" + emotionString + "'!\nMessage: " + ex.Message);
			}

			return new FAEmotion(vector);
		}
		public string encodeString(FAEmotion emotion)
		{
			Vector4 v = emotion.vector * 100;
			return string.Format("#E(sj={0},dt={0},fa={0},sa={0})", (int)v.x, (int)v.y, (int)v.z, (int)v.w);
		}

		#endregion
	}
}
