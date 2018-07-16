using System.Collections;
using UnityEngine;

namespace UFaceAnim
{
	[AddComponentMenu("Scripts/UFaceAnim/Controller")]
	public class FAController : MonoBehaviour
	{
		#region Fields

		private bool initialized = false;

		[SerializeField]
		private FAPresets preset = null;
		private FARenderer[] renderers = null;

		private FABlendState currentBlendState = FABlendState.Default;

		#endregion
		#region Methods

		public void initialize()
		{
			initialized = true;

			renderers = GetComponentsInChildren<FARenderer>(true);
			for(int i = 0; i < renderers.Length; ++i)
			{
				renderers[i].initialize();
			}

			//...
		}

		public void update()
		{
			if(!initialized)
			{
				initialize();
			}

			//...

			// >>>	TODO: Create 'currentState' by interpolating between preset states using the current emotion!	<<<

			updateRenderers();
		}

		private void updateRenderers()
		{
			if (renderers == null) return;

			for (int i = 0; i < renderers.Length; ++i)
			{
				FARenderer rend = renderers[i];
				if(rend != null)
				{
					rend.update(ref currentBlendState);
				}
			}
		}

		#endregion
	}
}
