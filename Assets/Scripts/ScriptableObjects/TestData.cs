using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    public class TestData : BaseDataSO {
        [SerializeField] private int quantity;
        [SerializeField] private string info;
        [SerializeField] private bool tested;
        
        // public int Quantity { get => quantity; set => quantity = value; }
        // public string Info { get => info; set => info = value; }
        // public bool Tested { get => tested; set => tested = value; }
    }
}