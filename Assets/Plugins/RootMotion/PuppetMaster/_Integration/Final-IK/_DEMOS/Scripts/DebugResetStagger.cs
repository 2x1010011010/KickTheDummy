using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	public class DebugResetStagger : MonoBehaviour {

		public BehaviourBipedStagger stagger;

		public void Reset() {
			StopAllCoroutines();
			StartCoroutine(ResetPuppet());
		}

		private IEnumerator ResetPuppet() {
			yield return new WaitForSeconds(2f);
			
			foreach (Muscle m in stagger.puppetMaster.muscles) m.Reset();
			stagger.puppetMaster.targetAnimator.Play("Idle", 0);
			
			yield return new WaitForSeconds(0.25f);

			stagger.enabled = true;
		}
	}
}
