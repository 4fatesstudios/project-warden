using FourFatesStudios.ProjectWarden.Interactions.Interactables;
using System;
using FourFatesStudios.ProjectWarden.Characters.Controllers;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Interactions
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField] private LayerMask interactLayer;

        private SphereCollider _interactionCollider;
        private Interactable _currentFocused;
        private static readonly Collider[] _overlapResults = new Collider[10];

        private void Awake() {
            _interactionCollider = GetComponent<SphereCollider>();
            _interactionCollider.isTrigger = true;
        }

        private void OnEnable() {
            PlayerController.OnExplorationInteractAction += TryInteract;
        }

        private void OnDisable() {
            PlayerController.OnExplorationInteractAction -= TryInteract;

            if (_currentFocused == null) return;
            _currentFocused.OnDestroyed -= HandleInteractableDestroyed;
            _currentFocused.OnUnfocus();
            _currentFocused = null;
        }

        private void Update() {
            UpdateFocus();
        }

        private void UpdateFocus() {
            var radius = _interactionCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            var count = Physics.OverlapSphereNonAlloc(transform.position, radius, _overlapResults, interactLayer);

            Interactable closest = null;
            var closestDist = float.MaxValue;

            for (var i = 0; i < count; i++) {
                var interactable = _overlapResults[i].GetComponentInParent<Interactable>();
                if (interactable == null || !interactable.CanInteract(gameObject))
                    continue;

                var dist = Vector3.Distance(transform.position, interactable.transform.position);
                
                if (!(dist < closestDist)) continue;
                
                closest = interactable;
                closestDist = dist;
            }

            if (_currentFocused == closest) return;
            
            if (_currentFocused != null) {
                _currentFocused.OnDestroyed -= HandleInteractableDestroyed;
                _currentFocused.OnUnfocus();
            }

            _currentFocused = closest;

            if (_currentFocused == null) return;
            
            _currentFocused.OnDestroyed += HandleInteractableDestroyed;
            _currentFocused.OnFocus();
        }

        private void HandleInteractableDestroyed(Interactable destroyed) {
            if (_currentFocused == destroyed) {
                _currentFocused = null;
            }
        }

        private void TryInteract() {
            if (_currentFocused != null && _currentFocused.CanInteract(gameObject)) {
                _currentFocused.Interact(gameObject);
            }
        }
    }
}
