using UnityEngine;

namespace FluidFlow
{
    /// <summary>
    /// ScriptableObject wrapper around a FFDecal
    /// </summary>
    [CreateAssetMenu(fileName = "NewDecal", menuName = "Fluid Flow/Decal")]
    public class FFDecalSO : ScriptableObject
    {
        public FFDecal Decal = new FFDecal(
            new FFDecal.Mask() {
                Components = ComponentMask.A
            },
            new FFDecal.Channel() {
                TargetTextureChannel = "Color",
                ChannelType = FFDecal.Channel.Type.COLOR,
                Source = Color.white,
                Data = 1
            });

        // allow implicit conversion to a FFDecal
        public static implicit operator FFDecal(FFDecalSO wrapper)
        {
            return wrapper.Decal;
        }
    }
}