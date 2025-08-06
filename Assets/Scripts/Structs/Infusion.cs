using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Structs
{
    [System.Serializable]
    public struct Infusion {
        [Header("Damage")] 
        [SerializeField, Tooltip("Damage Instances")] private DamageInstance[] damageInstances;
        
        [Header("Healing")]
        [SerializeField, Tooltip("Damage Instances")] private HealInstance[] healingInstances;
        
        [Header("Buffing")]
        [SerializeField] private BuffStatInstance[] buffStatInstances;
        [SerializeField] private BuffHealInstance[] buffHealInstances;
        [SerializeField] private BuffShieldInstance[] buffShieldInstances;
        
        [Header("Debuffing")]
        [SerializeField] private DebuffStatInstance[] debuffStatInstances;
        [SerializeField] private DebuffDOTInstance[] debuffDOTInstances;
        
        public DamageInstance[] DamageInstances => damageInstances;
        public HealInstance[] HealingInstances => healingInstances;
        public BuffStatInstance[] BuffStatInstances => buffStatInstances;
        public BuffHealInstance[] BuffHealInstances => buffHealInstances;
        public BuffShieldInstance[] BuffShieldInstances => buffShieldInstances;
        public DebuffStatInstance[] DebuffStatInstances => debuffStatInstances;
        public DebuffDOTInstance[] DebuffDOTInstances => debuffDOTInstances;
    }
}