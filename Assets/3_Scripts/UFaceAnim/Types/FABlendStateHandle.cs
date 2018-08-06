
namespace UFaceAnim
{
	[System.Serializable]
	public struct FABlendStateHandle
	{
		#region Fields

		public string key;
		public FABlendOverwrite overwrite;
		public FABlendState state;

		#endregion
		#region Properties

		public static FABlendStateHandle Default
		{
			get
			{
				FABlendStateHandle bsh = new FABlendStateHandle();

				bsh.key = null;
				bsh.overwrite = FABlendOverwrite.Emotion;
				bsh.state = FABlendState.Default;

				return bsh;
			}
		}

		#endregion
	}
}
