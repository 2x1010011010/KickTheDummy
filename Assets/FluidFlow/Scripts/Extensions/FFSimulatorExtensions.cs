using System.Collections;
using UnityEngine;

namespace FluidFlow
{
    public static class FFSimulatorExtensions
    {
        public static void FadeOut(this FFSimulator simulator, float amountPerSecond = 5f, float duration = 2f)
        {
            IEnumerator fade()
            {
                if (!simulator.TextureChannelReference.IsValid)
                    yield break;
                var textureChannel = simulator.TextureChannelReference.Resolve();
                float time = duration;
                while (time >= 0) {
                    using (var paintScope = simulator.GravityMap.Canvas.BeginPaintScope(textureChannel, false)) {
                        if (paintScope.IsValid) {
                            var targetTex = paintScope.Target;
                            using (var tmp = new TmpRenderTexture(targetTex.descriptor)) {
                                Shader.SetGlobalFloat(FFEffectsUtil.FadeAmountPropertyID, amountPerSecond * Time.deltaTime);
                                InternalShaders.CopyTexture(targetTex, tmp);
                                Graphics.Blit(tmp, targetTex, FFEffectsUtil.FluidEffects.Get(Utility.SetBit(3, true)));
                            }
                        }
                    }
                    yield return null;
                    time -= Time.deltaTime;
                }
            };
            simulator.StartCoroutine(fade());
        }
    }
}
