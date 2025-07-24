using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    public class TestData : BaseDataSO {
        [SerializeField] private int quantityTwo;
        [SerializeField] private string info;
        [SerializeField] private bool tested;
        [SerializeField] private Aspect aspect;
        [SerializeField] private StatModifier statModifier;
        [SerializeField] private List<StatModifier> statModifierList;

        public int QuantityTwo { get => quantityTwo; set => quantityTwo = value; }
        public string Info { get => info; set => info = value; }
        public bool Tested { get => tested; set => tested = value; }
        public Aspect Aspect { get => aspect; set => aspect = value; }
        public StatModifier StatModifier { get => statModifier; set => statModifier = value; }
        public List<StatModifier> StatModifierList { get => statModifierList; set => statModifierList = value; }
    }
}