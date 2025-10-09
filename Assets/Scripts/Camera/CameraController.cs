using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private CinemachineCamera isoCam; // orthographic iso for general gameplay
        [SerializeField] private CinemachineCamera perspectiveCam; // 2.5d for combat
        [SerializeField] private UnityEngine.Camera mainCam;

        [Header("Transition Settings")] 
        [SerializeField] private float blendDuration = 1f;
        [SerializeField] AnimationCurve blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Projection Settings")] 
        [SerializeField] private float orthoSize = 5f;
        [SerializeField] private float perspectiveFOV = 60f;

        [Header("Combat Camera Views (Perspective Only")] 
        [SerializeField] private List<ViewEntry> viewEntries = new();
        private Dictionary<CombatCamView, CameraViewData> viewData;

        private bool inPerspective = false;
        private float blendTimer;

        private CinemachineBrain brain;

        private void Start() {
            if (!mainCam) mainCam = UnityEngine.Camera.main;
            brain = mainCam.GetComponent<CinemachineBrain>();
            
            // set up dic
            viewData = viewEntries.ToDictionary(v => v.view, v => v.data);
            
            // start in ortho view
            SetProjection(false, instant: true);
            SetActiveCam(isoCam);
        }
        
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                TogglePerspective();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                StopAllCoroutines();
                StartCoroutine(LerpCamera(perspectiveCam, viewData.GetValueOrDefault(CombatCamView.Default), 0.5f));
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                StopAllCoroutines();
                StartCoroutine(LerpCamera(perspectiveCam, viewData.GetValueOrDefault(CombatCamView.CharacterSelect), 0.5f));
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                StopAllCoroutines();
                StartCoroutine(LerpCamera(perspectiveCam, viewData.GetValueOrDefault(CombatCamView.ActionSelect), 0.5f));
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                StopAllCoroutines();
                StartCoroutine(LerpCamera(perspectiveCam, viewData.GetValueOrDefault(CombatCamView.SkillSelect), 0.5f));
            }

            if (blendTimer < blendDuration) {
                blendTimer += Time.deltaTime;
                float t = blendCurve.Evaluate(Mathf.Clamp01(blendTimer / blendDuration));

                if (inPerspective) {
                    // transitioning *to* perspective
                    mainCam.fieldOfView = Mathf.Lerp(orthoSize, perspectiveFOV, t);
                    if (t >= 1f) mainCam.orthographic = false; // switch at end
                } 
                else {
                    // transitioning *to* orthographic
                    mainCam.fieldOfView = Mathf.Lerp(perspectiveFOV, orthoSize, t);
                    if (t >= 1f) mainCam.orthographic = true; // switch at end
                }
            }
        }


        public void TogglePerspective() {
            inPerspective = !inPerspective;
            blendTimer = 0f;

            SetActiveCam(inPerspective ? perspectiveCam : isoCam);
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
                float lerpT = Mathf.SmoothStep(0f, 1f, t);

                composer.TargetOffset = Vector3.Lerp(startOffset, endOffset, lerpT);
                yield return null;
            }

            composer.TargetOffset = endOffset;
        }

        private void SetActiveCam(CinemachineCamera cam) {
            isoCam.Priority = (cam == isoCam) ? 10 : 0;
            perspectiveCam.Priority = (cam == perspectiveCam) ? 10 : 0;
        }

        private void SetProjection(bool perspective, bool instant = false) {
            if (perspective) {
                mainCam.orthographic = false;
                mainCam.fieldOfView = perspectiveFOV;
            }
            else {
                mainCam.orthographic = true;
                mainCam.fieldOfView = orthoSize;
            }

            if (instant) blendTimer = blendDuration;
        }
    }
}