using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    public class TestData : BaseDataSO {
        [SerializeField] private int quantityTwo;
        [SerializeField] private string info;
        [SerializeField] private bool tested;
        [SerializeField] private Aspect aspect;

        public int QuantityTwo { get => quantityTwo; set => quantityTwo = value; }
        public string Info { get => info; set => info = value; }
        public bool Tested { get => tested; set => tested = value; }
        public Aspect Aspect { get => aspect; set => aspect = value; }
    }
}