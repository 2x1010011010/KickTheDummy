using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;
using RootMotion.FinalIK;

namespace RootMotion.Demos {
	
	// Code example for picking up/dropping props with AimIK applied.
	public class AimIKDemo : MonoBehaviour {
		
		[Tooltip("The prop you wish to pick up.")] 
		public PuppetMasterProp prop;
		
		[Tooltip("The PropMuscle to attach it to.")] 
		public PropMuscle connectTo;
		
		[Tooltip("If true, the prop will be picked up when PuppetMaster initiates")]
		public bool pickUpOnStart;

		public Animator animator;

		public AimIK aimGun;

		public AimIK aimHead;

		[Tooltip("AimIK will presume the character aims at this direction in the animation. This enables for using recoil, reload and gesture animations without weighing out AimIK.")]
		public Vector3 animatedAimingDirection = Vector3.forward;

		private float aimWeightV;
		private float upperbodyLayerWeightV;

		void Start() {
			if (pickUpOnStart) connectTo.currentProp = prop;

			connectTo.puppetMaster.OnRead += OnPuppetMasterRead;
		}

		void OnPuppetMasterRead() {
			// This enables for using recoil, reload and gesture animations without weighing out AimIK.
			aimGun.solver.axis = aimGun.solver.transform.InverseTransformVector(animator.transform.rotation * animatedAimingDirection);
		}
		
		void Update () {
			if (Input.GetKeyDown(KeyCode.P)) {
				// Makes the prop root drop any existing props and pick up the newly assigned one.
				connectTo.currentProp = prop;
			}
			
			if (Input.GetKeyDown(KeyCode.X)) {
				// By setting the prop root's currentProp to null, the prop connected to it will be dropped.
				connectTo.currentProp = null;
			}

			// Blend the AimIK weights
			float weightTarget = connectTo.currentProp != null? 1f: 0f;

			aimGun.solver.IKPositionWeight = Mathf.SmoothDamp(aimGun.solver.IKPositionWeight, weightTarget, ref aimWeightV, connectTo.currentProp != null? 0.1f: 0.2f);
			aimHead.solver.IKPositionWeight = aimGun.solver.IKPositionWeight;

			// Match animator layer weight with AimIK
			animator.SetLayerWeight(1, aimGun.solver.IKPositionWeight);
		}
	}
}

