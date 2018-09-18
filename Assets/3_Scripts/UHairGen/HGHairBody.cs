using UnityEngine;

namespace UHairGen
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class HGHairBody : MonoBehaviour
	{
		#region Fields

		public HGHair hair = null;
		public float radiusX = 0.1f;
		public float radiusY = 0.095f;
		public float radiusZ = 0.12f;

//		private MeshRenderer rend = null;
		private MeshFilter filter = null;
		
		#endregion
		#region Methods

		[ContextMenu("Create hair mesh")]
		public void create()
		{
//			rend = GetComponent<MeshRenderer>();
			filter = GetComponent<MeshFilter>();

			if(hair == null)
			{
				destroy();
				return;
			}

			Mesh mesh = HGHairBuilder.build(hair);
			filter.sharedMesh = mesh;

			gameObject.SetActive(true);
		}
		[ContextMenu("Destroy hair mesh")]
		public void destroy()
		{
			if(filter != null)
			{
				Mesh mesh = filter.sharedMesh;

				filter.mesh = null;
				filter.sharedMesh = null;

				// Adjust object scale to match hair mesh to the head's dimensions:
				transform.localScale = new Vector3(radiusX, radiusY, radiusZ) * 2.0f;

				if (!Application.isEditor)
					Destroy(mesh);
				else
					DestroyImmediate(mesh);
			}

			gameObject.SetActive(false);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;

			Vector3 pos = transform.position;
			Vector3 x = transform.right * radiusX;
			Vector3 y = transform.up * radiusY;
			Vector3 z = transform.forward * radiusZ;

			Gizmos.DrawLine(pos - x, pos + x);
			Gizmos.DrawLine(pos - y, pos + y);
			Gizmos.DrawLine(pos - z, pos + z);
		}

		#endregion
	}
}
