using UnityEngine;

using UFaceAnim.Audio;

namespace UFaceAnim.Test
{
	public class FADemoSpeechEnergy : MonoBehaviour
	{
		#region Fields

		public FAAudioReader reader = null;
		public FAController controller = null;

		#endregion
		#region Methods

		void Start()
		{
			controller.initialize();
		}

		void Update()
		{
			FABasePhonemes phoneme = FABasePhonemes.Group0_AI;
			if(reader is FAAudioReaderPhoneme)
			{
				phoneme = (reader as FAAudioReaderPhoneme).Phoneme;
			}

			controller.setPhoneme(phoneme, reader.Energy);
			controller.update(Time.deltaTime);
		}

		#endregion
	}
}
