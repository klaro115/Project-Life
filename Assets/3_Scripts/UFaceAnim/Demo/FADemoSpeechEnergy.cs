using UnityEngine;

using UFaceAnim.Audio;

namespace UFaceAnim.Test
{
	public class FADemoSpeechEnergy : MonoBehaviour
	{
		#region Fields

		public FAAudioReader reader = null;
		public FAController controller = null;

		public bool writePhonemes = false;
		public string phonemeOutput = null;
		public float minPhonemeDuration = 0.03f;

		private FABasePhonemes prevPhoneme = FABasePhonemes.None;
		private FABasePhonemes curPhoneme = FABasePhonemes.None;
		private float phonemeStartTime = 0.0f;
		private bool phonemePosted = false;

		#endregion
		#region Methods

		void Start()
		{
			controller.initialize();

			phonemeStartTime = 0.0f;
			if (writePhonemes) phonemeOutput = "";
		}

		void Update()
		{
			prevPhoneme = curPhoneme;
			curPhoneme = FABasePhonemes.Group0_AI;
			if(reader is FAAudioReaderPhoneme)
			{
				curPhoneme = (reader as FAAudioReaderPhoneme).Phoneme;
			}
			if(prevPhoneme != curPhoneme)
			{
				phonemePosted = false;
				phonemeStartTime = Time.time;
			}

			if(writePhonemes && !phonemePosted && Time.time > phonemeStartTime + minPhonemeDuration)
			{
				phonemePosted = true;
				phonemeOutput += getPhonemeChar(curPhoneme);
			}

			controller.setPhoneme(curPhoneme, reader.Energy);
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
