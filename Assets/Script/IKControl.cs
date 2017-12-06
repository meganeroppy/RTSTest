using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))] 

public class IKControl : MonoBehaviour {

	protected Animator animator;

	public bool ikActive = false;

	public Transform rightHandObj = null;
	public Transform leftHandObj = null;
	public Transform rightFootObj = null;
	public Transform leftFootObj = null;
	public Transform bodyObj = null;

	public Transform lookObj = null;

	void Start () 
	{
		animator = GetComponent<Animator>();
	}

	//a callback for calculating IK
	void OnAnimatorIK()
	{
		if(animator) {

			//if the IK is active, set the position and rotation directly to the goal. 
			if(ikActive) {

				// Set the look target position, if one has been assigned
				if(lookObj != null) {
					animator.SetLookAtWeight(1);
					animator.SetLookAtPosition(lookObj.position);
				}    

				// Set the right hand target position and rotation, if one has been assigned
				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.RightHand, rightHandObj );

				//if(rightHandObj != null) {

				//	animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
				//	animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);  
				//	animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandObj.position);
				//	animator.SetIKRotation(AvatarIKGoal.RightHand,rightHandObj.rotation);
				//}

				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.LeftHand, leftHandObj );

				//if( leftHandObj )
				//{
				//	animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
				//	animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,1);  
				//	animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandObj.position);
				//	animator.SetIKRotation(AvatarIKGoal.LeftHand,leftHandObj.rotation);
				//}      

				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.RightFoot, rightFootObj );

				//if( rightFootObj )
				//{
				//	animator.SetIKPositionWeight(AvatarIKGoal.RightFoot,1);
				//	animator.SetIKRotationWeight(AvatarIKGoal.RightFoot,1);  
				//	animator.SetIKPosition(AvatarIKGoal.RightFoot,rightFootObj.position);
				//	animator.SetIKRotation(AvatarIKGoal.RightFoot,rightFootObj.rotation);
				//}    

				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.LeftFoot, leftFootObj );
				//if( leftFootObj )
				//{
				//	animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot,1);
				//	animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot,1);  
				//	animator.SetIKPosition(AvatarIKGoal.LeftFoot,leftFootObj.position);
				//	animator.SetIKRotation(AvatarIKGoal.LeftFoot,leftFootObj.rotation);
				//}  

				if( bodyObj )
				{
					animator.bodyPosition = bodyObj.transform.position;
					animator.bodyRotation = bodyObj.transform.rotation;
				}   
			}

			//if the IK is not active, set the position and rotation of the hand and head back to the original position
			else {          
				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.RightHand, null );
				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.LeftHand, null );
				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.RightFoot, null );
				SetIKPositionWeightAndRotationWeight( AvatarIKGoal.LeftFoot, null );

			//	animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
			//	animator.SetIKRotationWeight(AvatarIKGoal.RightHand,0); 
			//	animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,0);
			//	animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,0); 

				animator.SetLookAtWeight(0);
			}
		}
	}   

	private void SetIKPositionWeightAndRotationWeight( AvatarIKGoal avatarIKGoal, Transform target)
	{
		if( target != null )
		{
			animator.SetIKPositionWeight(avatarIKGoal, 1);
			animator.SetIKRotationWeight(avatarIKGoal, 1);  
			animator.SetIKPosition(avatarIKGoal, target.position);
			animator.SetIKRotation(avatarIKGoal, target.rotation);
		}
		else
		{
			animator.SetIKPositionWeight(avatarIKGoal, 0);
			animator.SetIKRotationWeight(avatarIKGoal, 0);  
		}
	}
}
