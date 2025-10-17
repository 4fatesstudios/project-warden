using System;
using FourFatesStudios.ProjectWarden.Camera;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Events
{
    public static class CameraEvents {
        public static Action<CameraPerspective> OnSetCameraPerspective;
        public static Action<CombatCamView> OnChangeCameraTwoPointFiveDView;
    }
}