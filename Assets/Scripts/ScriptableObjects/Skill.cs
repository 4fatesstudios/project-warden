using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Skill")]
    public class Skill : ScriptableObject {
        [Header("Damage")]
        [SerializeField] bool doesDamage;
        [SerializeField, Tooltip("Base Damage"), Range(1, 9999)] private int baseDamage;
        [SerializeField, Tooltip("Stagger Damage"), Range(1, 999)] private int stagger;
        [SerializeField, Tooltip("Exposure Damage"), Range(1, 999)] private int exposure;
        [SerializeField, Tooltip("Damage Type")] private DamageType damageType;
        [SerializeField, Tooltip("Aspect")] private Aspect aspect;
        
        [Header("Buffing")]
        [SerializeField] bool doesBuffing;
        
        [Header("Debuffing")]
        [SerializeField] bool doesDebuffing;
        
        [Header("Common Attributes")]
        [SerializeField, Tooltip("Target Team")] private Team targetTeam;
        [SerializeField, Tooltip("Targeting")] private Targeting targeting;
        [SerializeField, Tooltip("Effect Timing")] private EffectTiming effectTiming;
    }
}