using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    
    public class GameManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }
    
        // Update is called once per frame
        void Update()
        {
            
        }

        [SerializeField] private PotionEffectDatabase potionEffectDb;
        [SerializeField] private ItemDatabase itemDb;

        private void Awake()
        {
        }

        private void OnApplicationQuit()
        {
        }
    }
    
}