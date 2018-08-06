using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[System.Serializable]
	[CreateAssetMenu(menuName="Face Anims/new Preset", fileName="new FAPreset")]
	public class FAPresets : ScriptableObject
	{
		#region Fields

		public FABlendSetupEmotion blendShapeSetup = FABlendSetupEmotion.Default;
		public FABlendSetupSpeech blendShapesSpeech = FABlendSetupSpeech.Default;
		public FABlendLibrary blendStateLibrary = FABlendLibrary.Empty;

		#endregion
	}
}
