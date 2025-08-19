using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Interfaces;
using FourFatesStudios.ProjectWarden.ScriptableObjects.LootTable;
using UnityEngine;
using UnityEngine.VFX;


namespace FourFatesStudios.ProjectWarden.Loot
{
    public class LootGenerator : MonoBehaviour {
        [SerializeField] private LootTable lootTable;
        [SerializeField] private Transform lootGenerationOrigin;
        [SerializeField] private VisualEffect lootVisualEffect;
        [SerializeField] private GameObject lootInstance;
        private float initialVelocityStrength = 4.0f;

        public void GenerateLoot() {
            var generatedItems = LootTable.GenerateRandomArray(lootTable);

            foreach (var generatedItem in generatedItems) {
                var lootInstance = Instantiate(this.lootInstance, lootGenerationOrigin.position, Quaternion.identity);
                lootInstance.GetComponent<Loot>().Initialize(generatedItem);
                
                var rb = lootInstance.GetComponent<Rigidbody>();

                var randomDirection = Random.onUnitSphere; 
                randomDirection.y = Mathf.Abs(randomDirection.y);
                
                rb.AddForce(randomDirection * initialVelocityStrength, ForceMode.Impulse);
            }
            
            Destroy(gameObject);
        }

        private void OnEnable() {
            SetLootColorToHighestRarity();
        }
        
        private void SetLootColorToHighestRarity() {
            if (lootVisualEffect.HasVector4("Color")) {
                lootVisualEffect.SetVector4("Color", RarityColors.Colors[lootTable.GetHighestRarity()]);
            } else {
                Debug.Log("Color property not found in LootOrb");
            }
        }
    }
}