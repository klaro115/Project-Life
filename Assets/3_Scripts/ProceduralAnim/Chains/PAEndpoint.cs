using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim
{
	public class PAEndpoint : MonoBehaviour
	{
		#region Fields

		[Header("Endpoint Behaviour")]
		public PAEndpointType type = PAEndpointType.Contact;
		public PABodyMapping bodyMapping = PABodyMapping.None;

		[Header("Contact Settings")]
		public Vector3 contactOffset = Vector3.zero;
		public Vector3 contactEuler = Vector3.zero;
		private Quaternion contactRotation = Quaternion.identity;

		[Header("Targeting")]
		public Vector3 targetPoint = Vector3.zero;
		public Quaternion targetRotation = Quaternion.identity;
		private Vector3 defaultTargePoint = Vector3.zero;
		public Quaternion defaulTargetRotation = Quaternion.identity;
		private IPATarget targetInterface = null;
		private Transform targetTransform = null;
		private PATargetMode mode = PATargetMode.Coordinate;

		#endregion
		#region Properties

		public PATargetMode Mode => mode;
		public IPATarget Target { get => targetInterface; set => SetTarget(value); }

		public Vector3 ContactPoint => transform.TransformPoint(contactOffset);
		public Vector3 ContactOffset => transform.TransformDirection(contactOffset);
		public Quaternion ContactRotation => transform.rotation * contactRotation;
		public Quaternion ContactRotationOffset => contactRotation;

		#endregion
		#region Methods

		private void OnDrawGizmosSelected()
		{
			Vector3 position = ContactPoint;
			contactRotation = Quaternion.Euler(contactEuler);
			Quaternion rotation = ContactRotation;

			const float axisLength = 0.1f;
			float axisXYLength = axisLength;
			float axisZLength = axisLength;
			if(type == PAEndpointType.Pointer)
			{
				axisXYLength *= 0.5f;
				axisZLength *= 2;
			}

			Gizmos.color = Color.red;
			Gizmos.DrawRay(position, rotation * Vector3.right * axisXYLength);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(position, rotation * Vector3.up * axisXYLength);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(position, rotation * Vector3.forward * axisZLength);

			if(type == PAEndpointType.Root)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(position, 0.05f);
			}
		}

		public void ResetTarget()
		{
			mode = PATargetMode.Coordinate;

			targetInterface = null;
			targetTransform = null;
			targetPoint = defaultTargePoint;
			targetRotation = defaulTargetRotation;
		}

		public void SetTarget(Vector3 newPosition, Quaternion newRotation)
		{
			SetTarget_internal(newPosition, newRotation, null, null);
		}
		public void SetTarget(Transform newTarget)
		{
			if (newTarget != null)
				SetTarget_internal(newTarget.position, newTarget.rotation, newTarget, null);
			else
				SetTarget_internal(defaultTargePoint, defaulTargetRotation, null, null);
		}
		public void SetTarget(IPATarget newTarget)
		{
			Vector3 newPoint = defaultTargePoint;
			Quaternion newRotation = defaulTargetRotation;

			if (newTarget != null)
				newTarget.GetTargetContact(this, ref targetPoint, ref targetRotation);

			SetTarget_internal(newPoint, newRotation, newTarget?.TargetTransform, newTarget);
		}
		private void SetTarget_internal(Vector3 newTargetPt, Quaternion newTargetRot, Transform newTargetTrans, IPATarget newTargetInt)
		{
			if(newTargetInt != targetInterface && targetInterface != null) targetInterface.RequestUnsetTargetUser(this);

			if (newTargetInt != null) mode = PATargetMode.Target;
			else if (newTargetTrans != null) mode = PATargetMode.Transform;
			else mode = PATargetMode.Coordinate;

			targetInterface = newTargetInt;
			targetTransform = newTargetTrans;
			targetPoint = newTargetPt;
			targetRotation = newTargetRot;
		}

		#endregion
	}
}
