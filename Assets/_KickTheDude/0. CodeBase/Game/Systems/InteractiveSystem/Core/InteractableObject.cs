using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Linq;
using System;

namespace Game.InteractiveSystem
{
    public class InteractableObject : SerializedMonoBehaviour, IInteractable
    {
        public event Action<InteractableObject> Destroyed;

        [OdinSerialize, FoldoutGroup("SETUP", 0)]
        public Transform Root { get; private set; }

        [OdinSerialize, FoldoutGroup("SETUP", 0)]
        public Rigidbody RootRigidbody { get; private set; }

        [OdinSerialize, FoldoutGroup("SETUP", 0)]
        public IReadOnlyList<CollidersContainer> CollidersContainers { get; private set; } = new List<CollidersContainer>();

        [OdinSerialize, FoldoutGroup("SETUP", 0)]
        public IReadOnlyList<MeshRenderersContainer> MeshRenderersContainers { get; private set; } = new List<MeshRenderersContainer>();

        [OdinSerialize, FoldoutGroup("SETUP", 0)]
        public IReadOnlyList<Rigidbody> Rigidbodies { get; private set; } = new List<Rigidbody>();

        [OdinSerialize, FoldoutGroup("INTERACTIVES"), ListDrawerSettings(DraggableItems = false, ListElementLabelName = "Name", ShowFoldout = true, HideRemoveButton = true)]
        public IReadOnlyList<IInteractive<IInteractable>> Interactives { get; private set; } = new List<IInteractive<IInteractable>>();

        [OdinSerialize, FoldoutGroup("INTERACT ACTIONS"), ListDrawerSettings(DraggableItems = false, ListElementLabelName = "ActionName", ShowFoldout = true, HideRemoveButton = true)]
        public IReadOnlyList<IInteractiveAction> InteractiveActions { get; private set; } = new List<IInteractiveAction>();

        [OdinSerialize, FoldoutGroup("DEBUG", 1), ReadOnly]
        private List<IUpdateable> _updateables = new List<IUpdateable>();
        [OdinSerialize, FoldoutGroup("DEBUG", 1), ReadOnly]
        private List<IFixedUpdateable> _fixedUpdateables = new List<IFixedUpdateable>();

        private void OnValidate()
        {
            if (Root == null) Root = GetComponent<Transform>();
            if (RootRigidbody == null) RootRigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            foreach (var interactive in Interactives)
            {
                interactive.Init(this);

                if (interactive is IUpdateable)
                    _updateables.Add((IUpdateable)interactive);
                if (interactive is IFixedUpdateable)
                    _fixedUpdateables.Add((IFixedUpdateable)interactive);
            }

            foreach (var action in InteractiveActions)
                action.Init();
        }

        private void OnDisable()
        {
            foreach (var interactive in Interactives)
                interactive.Dispose();

            foreach (var action in InteractiveActions)
                action.Dispose();
        }

        public T GetInteractive<T>()
        {
            return (T)Interactives.FirstOrDefault(x => x is T);
        }

        public List<T> GetAllInteractivesByType<T>()
        {
            var listOfT = new List<T>();

            foreach (var interactive in Interactives)
                if (interactive is T)
                    listOfT.Add((T)interactive);

            return listOfT;
        }

        public bool TryGetInteractive<T>(out T result)
        {
            result = GetInteractive<T>();

            return result != null;
        }

        public bool HasInteractive<T>()
        {
            return Interactives.FirstOrDefault(x => x is T) != null;
        }

        public void StopInteract()
        {
            foreach (var collidersContainer in CollidersContainers)
            {
                collidersContainer.DeactivateAllColliders();
                collidersContainer.DeactivateAllTriggers();
            }

            foreach (var meshRendererContainer in MeshRenderersContainers)
                meshRendererContainer.Hide();

            foreach (var interactive in Interactives)
                interactive.StopInteract();

            RootRigidbody.isKinematic = true;
            RootRigidbody.detectCollisions = false;
        }

        private void Update()
        {
            foreach (var updateable in _updateables)
                updateable.Update();
        }

        private void FixedUpdate()
        {
            foreach (var fixedUpdateable in _fixedUpdateables)
                fixedUpdateable.FixedUpdate();
        }

        public void Destroy()
        {
            StopInteract();

            foreach (var interactive in Interactives)
                interactive.Dispose();

            Destroyed?.Invoke(this);

            Destroy(gameObject);
        }

        public void IgnoreOtherInteractable(InteractableObject interactableObject)
        {
            foreach(var collidersContainer in CollidersContainers)
                foreach (var otherCollidersContainer in interactableObject.CollidersContainers)
                    otherCollidersContainer.ChangeColliderIgnoreStatusWithOtherContainer(collidersContainer, false);
        }

#if UNITY_EDITOR
        [Button("TRY FIND COLLIDER CONTAINERS", ButtonSizes.Large), BoxGroup("ACTIONS", true, false, 2)]
        private void TryFindCollidersContainers()
        {
            CollidersContainers = Root.GetComponentsInChildren<CollidersContainer>();
        }

        [Button("TRY FIND MESH CONTAINERS", ButtonSizes.Large), BoxGroup("ACTIONS", true, false, 2)]
        private void TryFindMeshRenderersContainers()
        {
            MeshRenderersContainers = Root.GetComponentsInChildren<MeshRenderersContainer>();
        }

        [Button("TRY FIND RIGIDBODIES", ButtonSizes.Large), BoxGroup("ACTIONS", true, false, 2)]
        private void TryFindRigidbodies()
        {
            Rigidbodies = Root.GetComponentsInChildren<Rigidbody>();
        }
#endif
    }
}