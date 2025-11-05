using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Events;
using UnityEngine;
using Unity.Cinemachine;

namespace FourFatesStudios.ProjectWarden.Camera
{
    public enum CombatCamView {
        Default,
        CharacterSelect,
        ActionSelect,
        SkillSelect
    }

    // Perspective enum kept for legacy support, but we won't use perspective anymore
    public enum CameraPerspective {
        Exploration, // formerly Isometric
        Combat       // formerly TwoPointFiveD
    }
    
    [System.Serializable]
    public struct CameraViewData {
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }
    
    [System.Serializable]
    public class ViewEntry {
        public CombatCamView view;
        public CameraViewData data;
    }
    
    public class CameraController : MonoBehaviour {
        [Header("Cinemachine Virtual Cameras")]
        [SerializeField] private CinemachineCamera explorationCam; // formerly isoCam
        [SerializeField] private CinemachineCamera combatCam;      // formerly twoPointFiveDCam
        [SerializeField] private UnityEngine.Camera mainCam;

        [Header("Transition Settings")] 
        [SerializeField] private float blendDuration = 1f;
        [SerializeField] AnimationCurve blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Projection Settings")] 
        [SerializeField] private float orthoSize = 5f;
        [SerializeField] private float perspectiveFOV = 60f;

        [Header("Combat Camera Views (Legacy perspective system retained)")] 
        [SerializeField] private List<ViewEntry> viewEntries = new();
        private Dictionary<CombatCamView, CameraViewData> viewData;

        private float blendTimer;
        private CameraPerspective currentPerspective;
        private CameraPerspective targetPerspective;

        private void Start() {
            if (!mainCam) mainCam = UnityEngine.Camera.main;
            
            viewData = viewEntries.ToDictionary(v => v.view, v => v.data);
            
            // We always use orthographic now
            SetProjection(false, instant: true);
            SetActiveCam(explorationCam);

            currentPerspective = targetPerspective = CameraPerspective.Exploration;
        }

        private void OnEnable() {
            CameraEvents.OnSetCameraPerspective += SetPerspective;
            CameraEvents.OnChangeCameraTwoPointFiveDView += StartViewTransition;
        }

        private void OnDisable() {
            CameraEvents.OnSetCameraPerspective -= SetPerspective;
            CameraEvents.OnChangeCameraTwoPointFiveDView -= StartViewTransition;
        }
        
        private void Update()
        {
            // Debug test keys kept for dev
            if (Input.GetKeyDown(KeyCode.Tab)) SetPerspective(CameraPerspective.Exploration);
            if (Input.GetKeyDown(KeyCode.Q)) SetPerspective(CameraPerspective.Combat);

            if (Input.GetKeyDown(KeyCode.Alpha1)) StartViewTransition(CombatCamView.Default);
            if (Input.GetKeyDown(KeyCode.Alpha2)) StartViewTransition(CombatCamView.CharacterSelect);
            if (Input.GetKeyDown(KeyCode.Alpha3)) StartViewTransition(CombatCamView.ActionSelect);
            if (Input.GetKeyDown(KeyCode.Alpha4)) StartViewTransition(CombatCamView.SkillSelect);

            // Transition block kept but effectively dormant
            if (currentPerspective != targetPerspective)
            {
                blendTimer += Time.deltaTime;
                float t = blendCurve.Evaluate(Mathf.Clamp01(blendTimer / blendDuration));

                mainCam.fieldOfView = Mathf.Lerp(perspectiveFOV, orthoSize, t);

                if (t >= 1f)
                {
                    mainCam.orthographic = true; // Always orthographic now
                    currentPerspective = targetPerspective;
                }
            }
        }
        
        private void SetPerspective(CameraPerspective perspective) {
            if (targetPerspective == perspective) return;
            targetPerspective = perspective;
            blendTimer = 0f;

            // Both cams are isometric — only swap priority
            SetActiveCam(perspective == CameraPerspective.Exploration ? explorationCam : combatCam);
        }
        
        private void StartViewTransition(CombatCamView view) {
            StopAllCoroutines();
            StartCoroutine(LerpCamera(combatCam, viewData.GetValueOrDefault(view), 0.5f));
        }

        private IEnumerator LerpCamera(CinemachineCamera cam, CameraViewData data, float duration)
        {
            var bodyComp = cam.GetCinemachineComponent(CinemachineCore.Stage.Body);
            var composer = bodyComp as CinemachinePositionComposer;
            if (composer == null)
                yield break;

            Vector3 startOffset = composer.TargetOffset;
            Vector3 endOffset = data.positionOffset;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, duration);
                composer.TargetOffset = Vector3.Lerp(startOffset, endOffset, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            composer.TargetOffset = endOffset;
        }

        private void SetActiveCam(CinemachineCamera cam) {
            explorationCam.Priority = (cam == explorationCam) ? 10 : 0;
            combatCam.Priority = (cam == combatCam) ? 10 : 0;
        }

        private void SetProjection(bool perspective, bool instant = false) {
            // Keep the option, but default to ortho
            mainCam.orthographic = !perspective;
            mainCam.fieldOfView = perspective ? perspectiveFOV : orthoSize;
            if (instant) blendTimer = blendDuration;
        }
    }
}
