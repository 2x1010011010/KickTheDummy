using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    /// <summary>
    /// ScriptableObject wrapper around a FFBrush
    /// </summary>
    [CreateAssetMenu(fileName = "NewBrush", menuName = "Fluid Flow/Brush")]
    public class FFBrushSO : ScriptableObject
    {
        public FFBrush Brush = new FFBrush(FFBrush.Type.COLOR, Color.white, 1, .1f);

        // allow implicit conversion to a FFBrush
        public static implicit operator FFBrush(FFBrushSO wrapper)
        {
            return wrapper.Brush;
        }
    }
}