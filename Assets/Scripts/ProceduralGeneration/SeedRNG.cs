using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    [System.Serializable]
    public class SeedRNG {
        [SerializeField, Tooltip("Enter 0 or leave blank for random seed")] private int randomSeed;
        private System.Random _rng;

        public int RandomSeed => randomSeed;
        public System.Random Rng => _rng;

        public void SetRandomSeed() {
            if (randomSeed == 0)
                randomSeed = Random.Range(int.MinValue, int.MaxValue);
            
            _rng = new System.Random(randomSeed);
        }
    }
}