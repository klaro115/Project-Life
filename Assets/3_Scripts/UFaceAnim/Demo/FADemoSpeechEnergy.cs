using UnityEngine;

using UFaceAnim.Audio;

namespace UFaceAnim.Test
{
	[AddComponentMenu("Scripts/UFaceAnim/Test/Speech Energy")]
	public class FADemoSpeechEnergy : MonoBehaviour
	{
		#region Fields

		public FAAudioReader reader = null;
		public FAController controller = null;

		[Header("Test phonemes:")]
		public bool writePhonemes = false;
		public string phonemeOutput = null;
		public float minPhonemeDuration = 0.03f;

		private FABasePhonemes prevPhoneme = FABasePhonemes.None;
		private FABasePhonemes curPhoneme = FABasePhonemes.None;
		private float phonemeStartTime = 0.0f;
		private bool phonemePosted = false;

		[Header("Test view direction:")]
		public bool testViewDir = false;
		private Vector3 viewEyePos = Vector3.zero;
		private Vector3 viewTargetPos = Vector3.zero;
		private Vector3 viewDirection = Vector3.forward;

		#endregion
		#region Methods

		void Start()
		{
			controller.initialize();

			phonemeStartTime = 0.0f;
			if (writePhonemes) phonemeOutput = "";
		}

		private void OnDrawGizmosSelected()
		{
			if(testViewDir)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(viewEyePos, 0.02f);
				Gizmos.DrawWireSphere(viewTargetPos, 0.2f);

				Gizmos.color = Color.green;
				Gizmos.DrawLine(viewEyePos, viewTargetPos);
			}
		}

		void Update()
		{
			// Read out current phoneme and send it to the controller:
			prevPhoneme = curPhoneme;
			curPhoneme = FABasePhonemes.Group0_AI;
			if(reader is FAAudioReaderPhoneme)
			{
				curPhoneme = (reader as FAAudioReaderPhoneme).Phoneme;
			}
			controller.setPhoneme(curPhoneme, reader.Energy);

			// Output phonemes as strings, if required:
			if (prevPhoneme != curPhoneme)
			{
				phonemePosted = false;
				phonemeStartTime = Time.time;
			}
			if(writePhonemes && !phonemePosted && Time.time > phonemeStartTime + minPhonemeDuration)
			{
				phonemePosted = true;
				phonemeOutput += getPhonemeChar(curPhoneme);
			}

			// Continuously change view direction, if required:
			if(testViewDir)
			{
				float viewTargetPhase = Time.time * 0.5f;
				viewTargetPos = controller.transform.TransformPoint(new Vector3(Mathf.Cos(viewTargetPhase) * 3, 1.6f, Mathf.Sin(viewTargetPhase) * 3 + 6));
				viewEyePos = controller.transform.TransformPoint(Vector3.up * 1.55f);
				viewDirection = (viewTargetPos - viewEyePos).normalized;

				controller.setViewDirection(viewDirection);
			}

			// Update controller animation in realtime:
			controller.update(Time.deltaTime);
		}

		private char getPhonemeChar(FABasePhonemes phoneme)
		{
			switch (phoneme)
			{
				case FABasePhonemes.Group0_AI:
					return 'a';
				case FABasePhonemes.Group1_E:
					return 'e';
				case FABasePhonemes.Group2_U:
					return 'u';
				case FABasePhonemes.Group3_O:
					return 'o';
				case FABasePhonemes.Group4_CDGK:
					return 'c';
				case FABasePhonemes.Group5_FV:
					return 'f';
				case FABasePhonemes.Group6_LTh:
					return 'l';
				case FABasePhonemes.Group7_MBP:
					return 'm';
				case FABasePhonemes.Group8_WQ:
					return 'w';
				case FABasePhonemes.Group9_Rest:
					return '_';
				default:
					return ' ';
			}
		}

		#endregion
	}
}
