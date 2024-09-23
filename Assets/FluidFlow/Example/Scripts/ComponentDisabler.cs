
using UnityEngine;

namespace FluidFlow
{
    // if FluidFlow fails to compile, this script will not run, so the targeted component will remain active.
    // This is used to inform the user of this package about package requirements of the asset, via UI Text
    public class ComponentDisabler : MonoBehaviour
    {
        public Behaviour component;

        private void Awake()
        {
            component.enabled = false;
        }
    }
}
