using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class Cache
    {
        // map original mesh to mesh with secondary UV transformations
        private static readonly Dictionary<Mesh, Mesh> meshCache = new Dictionary<Mesh, Mesh>();
        private static readonly Dictionary<ValueTuple<Mesh, UVSet>, StitchResult[]> stitchCache = new Dictionary<ValueTuple<Mesh, UVSet>, StitchResult[]>();

        public static void AddCache(FFModelCache cache)
        {
            var secondaryUVMeshCaches = cache.SecondaryUVMeshCaches;
            for (var i = secondaryUVMeshCaches.Count - 1; i >= 0; i--)
                AddMesh(secondaryUVMeshCaches[i].Source, secondaryUVMeshCaches[i].Cache);

            var stitchCaches = cache.StitchCaches;
            for (var i = stitchCaches.Count - 1; i >= 0; i--)
                AddStitch(stitchCaches[i].GenerationSource, stitchCaches[i].UVSet, stitchCaches[i].Stitches);
        }

        private static void AddMesh(Mesh source, Mesh cache)
        {
            if (!meshCache.ContainsKey(source))
                meshCache.Add(source, cache);
            if (!meshCache.ContainsKey(cache))
                meshCache.Add(cache, cache);    // map cache to itself for simplicity   
        }

        private static void AddStitch(Mesh source, UVSet uvSet, StitchResult[] stitches)
        {
            var key = new ValueTuple<Mesh, UVSet>(source, uvSet);
            if (!stitchCache.ContainsKey(key))
                stitchCache.Add(key, stitches);
        }

        public static bool TryGetStitches(Mesh mesh, UVSet uvSet, out StitchResult[] stitches)
        {
            if (uvSet == UVSet.UV1 && !mesh.HasVertexAttribute(VertexAttribute.TexCoord1)) {
                if (meshCache.TryGetValue(mesh, out var cache)) {
                    mesh = cache;
                } else {
                    Debug.LogError("FluidFlow: Mesh does not have UV1! Unable to access stitches for UV1.");
                    stitches = null;
                    return false;
                }
            }
            return stitchCache.TryGetValue(new ValueTuple<Mesh, UVSet>(mesh, uvSet), out stitches);
        }

        public static bool TryGetMesh(Mesh mesh, out Mesh cache)
            => meshCache.TryGetValue(mesh, out cache);

        private static readonly List<Gravity.GenerationHandle> meshHandles = new List<Gravity.GenerationHandle>();
        private static readonly List<Stitcher.GenerationHandle> stitchHandles = new List<Stitcher.GenerationHandle>();
        private static List<(JobHandle[], Action)> callbacks = new List<(JobHandle[], Action)>();
        private static NativeArray<JobHandle> jobHandles;
        private static void EnsureJobHandlesCacheSize(int count)
        {
            if (jobHandles.IsCreated && jobHandles.Length >= count)
                return;
            if (jobHandles.IsCreated)
                jobHandles.Dispose();
            jobHandles = new NativeArray<JobHandle>(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            // when domain reloading is disabled for faster playmode enter, static fields are not cleared properly (https://docs.unity3d.com/Manual/DomainReloading.html)
            meshCache.Clear();
            stitchCache.Clear();
            meshHandles.Clear();
            stitchHandles.Clear();
            callbacks.Clear();
            Application.quitting += Dispose;
        }

        private static void Dispose()
        {
            Application.quitting -= Dispose;
            CompleteMeshRequests(meshHandles);
            CompleteStitchRequests(stitchHandles);
            if (jobHandles.IsCreated)
                jobHandles.Dispose();
        }

        public static void UpdateAsyncRequests()
        {
            var callbackCountBefore = callbacks.Count;
            for (var i = callbackCountBefore - 1; i >= 0; i--) {
                if (callbacks[i].Item1.AllDone()) {
                    ApplyCompletedMeshRequests();
                    ApplyCompletedStitchRequests();
                    callbacks[i].Item2.Invoke();
                    callbacks.RemoveAt(i);
                }
            }
            if (callbackCountBefore > 0 && callbacks.Count == 0)
                Application.onBeforeRender -= UpdateAsyncRequests;
        }

        private static void AddAsyncRequest(ValueTuple<JobHandle[], Action> callback)
        {
            if (callbacks.Count == 0)   // attach update hook, until async request is finished
                Application.onBeforeRender += UpdateAsyncRequests;
            callbacks.Add(callback);
        }

        public static Optional<Gravity.GenerationHandle> Request(Mesh mesh)
        {
            if (!mesh.isReadable) {
                Debug.LogErrorFormat("FluidFlow: {0} is not readable! Enable Read/Write in the import settings of the model.", mesh);
                return Optional<Gravity.GenerationHandle>.None;
            }
            if (meshCache.ContainsKey(mesh))    // already generated
                return Optional<Gravity.GenerationHandle>.None;
            if (!mesh.HasVertexAttribute(VertexAttribute.TexCoord1)) {
                Debug.LogErrorFormat("FluidFlow: '{0}' does not have UV1!", mesh);
                return Optional<Gravity.GenerationHandle>.None;
            }
            if (mesh.HasVertexAttribute(VertexAttribute.TexCoord2)) {
                Debug.LogErrorFormat("FluidFlow: '{0}' already has UV2!", mesh);
                return Optional<Gravity.GenerationHandle>.None;
            }
            var existingHandleIndex = meshHandles.FindIndex(handle => handle.Source == mesh || handle.Mesh == mesh);
            if (existingHandleIndex > -1) {
                // generation has already been requested, and generation is still running
                return new Optional<Gravity.GenerationHandle>(meshHandles[existingHandleIndex]);
            } else {
                var cache = Utility.CopyMesh(mesh);
                var generationHandle = new Gravity.GenerationHandle(mesh, cache);
                meshHandles.Add(generationHandle);
                return new Optional<Gravity.GenerationHandle>(generationHandle);
            }
        }

        public static Optional<Stitcher.GenerationHandle> Request(Mesh mesh, UVSet uvSet)
        {
            if (!mesh.isReadable) {
                Debug.LogErrorFormat("FluidFlow: {0} is not readable! Enable Read/Write in the import settings of the model.", mesh);
                return Optional<Stitcher.GenerationHandle>.None;
            }
            if (uvSet == UVSet.UV1 && !mesh.HasVertexAttribute(VertexAttribute.TexCoord1)) {
                if (meshCache.TryGetValue(mesh, out var cache)) {
                    mesh = cache;
                } else {
                    Debug.LogError("FluidFlow: Mesh does not have UV1! Unable to generate stitches for UV1.");
                    return Optional<Stitcher.GenerationHandle>.None;
                }
            }
            if (stitchCache.ContainsKey((mesh, uvSet)))   // already generated
                return Optional<Stitcher.GenerationHandle>.None;

            if (stitchCache.ContainsKey(new ValueTuple<Mesh, UVSet>(mesh, uvSet))) {
                return Optional<Stitcher.GenerationHandle>.None;
            }
            var existingHandleIndex = stitchHandles.FindIndex(handle => handle.SourceMesh == mesh && handle.UVSet == uvSet);
            if (existingHandleIndex > -1) {
                // generation has already been requested, and generation is still running
                return new Optional<Stitcher.GenerationHandle>(stitchHandles[existingHandleIndex]);
            } else {
                var handle = Stitcher.Generate(mesh, uvSet);
                stitchHandles.Add(handle);
                return new Optional<Stitcher.GenerationHandle>(handle);
            }
        }

        private static void ApplyCompletedMeshRequests()
        {
            for (var i = meshHandles.Count - 1; i >= 0; i--) {
                if (meshHandles[i].Handle.IsCompleted) {
                    meshHandles[i].Handle.Complete();
                    meshHandles[i].Apply();
                    AddMesh(meshHandles[i].Source, meshHandles[i].Mesh);
                    meshHandles[i].Dispose();
                    meshHandles.RemoveAt(i);
                }
            }
        }
        private static void ApplyCompletedStitchRequests()
        {
            for (var i = stitchHandles.Count - 1; i >= 0; i--) {
                if (stitchHandles[i].Handle.IsCompleted) {
                    stitchHandles[i].Handle.Complete();
                    var result = stitchHandles[i].Get();
                    AddStitch(result.Mesh, result.UVSet, result.Stitches);
                    stitchHandles[i].Dispose();
                    stitchHandles.RemoveAt(i);
                }
            }
        }

        public static void CompleteMeshRequests(List<Gravity.GenerationHandle> handles)
        {
            JobHandle.ScheduleBatchedJobs();
            EnsureJobHandlesCacheSize(handles.Count);
            for (var i = handles.Count - 1; i >= 0; i--)
                jobHandles[i] = handles[i].Handle;
            JobHandle.CompleteAll(jobHandles.GetSubArray(0, handles.Count));
            ApplyCompletedMeshRequests();
        }
        public static void CompleteStitchRequests(List<Stitcher.GenerationHandle> handles)
        {
            JobHandle.ScheduleBatchedJobs();
            EnsureJobHandlesCacheSize(handles.Count);
            for (var i = handles.Count - 1; i >= 0; i--)
                jobHandles[i] = handles[i].Handle;
            JobHandle.CompleteAll(jobHandles.GetSubArray(0, handles.Count));
            ApplyCompletedStitchRequests();
        }

        public static void CompleteMeshRequestsAsync(List<Gravity.GenerationHandle> handles, Action callback)
        {
            JobHandle.ScheduleBatchedJobs();
            var activeHandles = new JobHandle[handles.Count]; // cache active handles at the time of calling this coroutine
            for (var i = handles.Count - 1; i >= 0; i--)
                activeHandles[i] = handles[i].Handle;
            if (activeHandles.AllDone()) {
                ApplyCompletedMeshRequests();
                callback.Invoke();
            } else {
                AddAsyncRequest((activeHandles, callback));
            }
        }
        public static void CompleteStitchRequestsAsync(List<Stitcher.GenerationHandle> handles, Action callback)
        {
            JobHandle.ScheduleBatchedJobs();
            var activeHandles = new JobHandle[handles.Count]; // cache active handles at the time of calling this coroutine
            for (var i = handles.Count - 1; i >= 0; i--)
                activeHandles[i] = handles[i].Handle;
            if (activeHandles.AllDone()) {
                ApplyCompletedStitchRequests();
                callback.Invoke();
            } else {
                AddAsyncRequest((activeHandles, callback));
            }
        }

        private static bool AllDone(this JobHandle[] handles)
        {
            var allDone = true;
            for (var i = handles.Length - 1; i >= 0 && allDone; i--)
                allDone &= handles[i].IsCompleted;
            return allDone;
        }
    }

    public class CommandBufferCache : IDisposable
    {
        public readonly CommandBuffer Buffer;
        public CommandBufferCache()
        {
            Buffer = new CommandBuffer {
                name = "FFSharedCommandBuffer"
            };
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }

    public static class Shared
    {
        private static List<Material> Materials;
        private static CommandBufferCache cbCache;
        private static MaterialPropertyBlock mpbCache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Materials = new List<Material>();
            cbCache = new CommandBufferCache();
            mpbCache = new MaterialPropertyBlock();

            Application.quitting += Dispose;
        }

        private static void Dispose()
        {
            Application.quitting -= Dispose;
            cbCache.Dispose();
        }

        public static List<Material> MaterialList()
        {
            Materials.Clear();
            return Materials;
        }

        /// Caller is responsible for clearing the buffer after use!
        public static CommandBuffer CommandBuffer()
        {
            return cbCache.Buffer;
        }

        public static MaterialPropertyBlock MaterialPropertyBlock()
        {
            mpbCache.Clear();
            return mpbCache;
        }
    }
}
