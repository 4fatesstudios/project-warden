using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Structs
{
    [System.Serializable]
    public struct EffectTimingInfo {
        [SerializeField, Tooltip("Effect Timing")] private EffectTiming effectTiming;
        [SerializeField, Tooltip("Turn Duration")] private int turnDuration;
        
        public bool RequiresTurnDuration =>
            effectTiming == EffectTiming.Delay || 
            effectTiming == EffectTiming.OverTime || 
            effectTiming == EffectTiming.OnTurn;
        
        public EffectTiming EffectTiming { get => effectTiming; set => effectTiming = value; }
        public int TurnDuration { get => turnDuration; set => turnDuration = value; }
    }
}