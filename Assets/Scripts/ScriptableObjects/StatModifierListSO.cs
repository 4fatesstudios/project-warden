using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [CreateAssetMenu(fileName = "StatModifierList", menuName = "Stats/Stat Modifier List")]
    public class StatModifierListSO : ScriptableObject{
        [SerializeField] private List<StatModifier> statModifiers;
    }
}