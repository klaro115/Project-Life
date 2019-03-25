using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim
{
	/* TIPP: Joint Mass
	 * 
	 * The body's mass distribution across the individual joints should be somewhat realistic.
	 * As such, here's a handy list of the approximate mass ratio of each body segment according
	 * to a US airforce study. (https://apps.dtic.mil/dtic/tr/fulltext/u2/710622.pdf p.59)
	 * 
	 * Segments:	Head	Trunk	UpArm	Forearm	Hand	Thigh	Calf	Foot
	 * Mass [%]:	7.3		50.7	2.6		1.6		0.7		10.3	4.3		1.5
	 */

	public class PAJoint : MonoBehaviour
	{
		#region Fields
		
		public PAJointType type = PAJointType.Ball;

		[Header("Hinge Joint")]
		public Vector3 hingeAxis = Vector3.right;
		public float hingeLimitMin = 0.0f;
		public float hingeLimitMax = 120.0f;

		[Header("Physics")]
		public float mass = 1.0f;
		[Range(0.01f, 1.0f)]
		public float urgency = PAConstants.jointDefaultUrgency;
		public float maxAngularSpeed = 180.0f;  // degrees per second.
		private float curAngularSpeed = 0.0f;
		public float maxAngularAccel = 100.0f;	// degrees per second squared.

		private Vector3 targetPosition = Vector3.zero;
		private Quaternion targetRotation = Quaternion.identity;
		private Vector3 defaultTargetPosition = Vector3.zero;
		private Quaternion defaultTargetRotation = Quaternion.identity;

		#endregion
		#region Methods

		private void OnDrawGizmosSelected()
		{
			Vector3 position = transform.position;

			if(type == PAJointType.Hinge)
			{
				Gizmos.color = Color.red;
				Vector3 axis = transform.TransformDirection(hingeAxis);
				Gizmos.DrawRay(position - axis * 0.1f, axis * 0.2f);
			}

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(position, new Vector3(0.015f, 0.015f, 0.015f));
		}

		public void Reset()
		{
			urgency = PAConstants.jointDefaultUrgency;

			targetPosition = defaultTargetPosition;
			targetRotation = defaultTargetRotation;
		}

		public void SetTargets(Vector3 newTargetPosition, Quaternion newTargetRotation, float newUrgency = PAConstants.jointDefaultUrgency)
		{
			urgency = Mathf.Clamp01(newUrgency);

			targetPosition = newTargetPosition;
			targetRotation = newTargetRotation;
		}

		public void UpdateJoint()
		{
			float targetAngularSpeed = maxAngularSpeed * urgency;
			curAngularSpeed = Mathf.Lerp(curAngularSpeed, targetAngularSpeed, Time.deltaTime * maxAngularAccel);
			float maxDegreesDelta = Time.deltaTime * curAngularSpeed;

			// todo: not a very good solution; Handles speed changes fine, but changes of direction will be immediate!
			// In other words, inertia is only simulated, but there is no bouncing or over-travel in here!!!

			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
		}

		#endregion
	}
}
