using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Debug tracker specifically for obstacle spawn system.
    /// Tracks spawn attempts, success rates, and provides detailed analytics.
    /// </summary>
    [System.Serializable]
    public class ObstacleSpawnDebugger : MonoBehaviour
    {
        [Header("🚧 Obstacle Spawn Analytics")]
        [SerializeField] private bool enableSpawnChanceDebug = true;
        [Tooltip("Log every spawn chance calculation and roll result")]
        
        [SerializeField] private bool enableSpawnStatistics = true;
        [Tooltip("Track and display spawn success rates and patterns")]
        
        [SerializeField] private bool enableSpawnDistributionAnalysis = true;
        [Tooltip("Analyze obstacle type distribution and placement patterns")]
        
        [Header("📊 Live Statistics")]
        [SerializeField] private int totalSpawnAttempts = 0;
        [SerializeField] private int successfulSpawns = 0;
        [SerializeField] private float currentSuccessRate = 0f;
        
        [Header("🎲 Spawn Chance Details")]
        [SerializeField] private float lastSpawnChance = 0f;
        [SerializeField] private float lastRandomRoll = 0f;
        [SerializeField] private bool lastSpawnSuccess = false;
        
        [Header("📈 Obstacle Type Distribution")]
        [SerializeField] private int corporealCount = 0;
        [SerializeField] private int frigidCount = 0;
        [SerializeField] private int scorchCount = 0;
        [SerializeField] private int causticCount = 0;
        [SerializeField] private int arcCount = 0;
        [SerializeField] private int divineCount = 0;
        
        // Runtime tracking
        private List<SpawnAttempt> spawnHistory = new List<SpawnAttempt>();
        private Dictionary<ObstacleType, int> obstacleTypeCounts = new Dictionary<ObstacleType, int>();
        
        [System.Serializable]
        public struct SpawnAttempt
        {
            public Vector2Int position;
            public float spawnChance;
            public float randomRoll;
            public bool success;
            public ObstacleType? spawnedType;
            public float timestamp;
            
            public SpawnAttempt(Vector2Int pos, float chance, float roll, bool spawned, ObstacleType? type = null)
            {
                position = pos;
                spawnChance = chance;
                randomRoll = roll;
                success = spawned;
                spawnedType = type;
                timestamp = Time.time;
            }
        }
        
        private void Awake()
        {
            InitializeObstacleTypeCounts();
        }
        
        private void InitializeObstacleTypeCounts()
        {
            obstacleTypeCounts.Clear();
            foreach (ObstacleType type in System.Enum.GetValues(typeof(ObstacleType)))
            {
                obstacleTypeCounts[type] = 0;
            }
        }
        
        /// <summary>
        /// Log and track a spawn chance calculation
        /// </summary>
        public void LogSpawnChanceCalculation(Vector2Int position, float spawnChance, float randomRoll)
        {
            totalSpawnAttempts++;
            lastSpawnChance = spawnChance;
            lastRandomRoll = randomRoll;
            lastSpawnSuccess = randomRoll < spawnChance;
            
            if (lastSpawnSuccess)
            {
                successfulSpawns++;
            }
            
            // Update success rate
            currentSuccessRate = totalSpawnAttempts > 0 ? (float)successfulSpawns / totalSpawnAttempts : 0f;
            
            // Create spawn attempt record
            var attempt = new SpawnAttempt(position, spawnChance, randomRoll, lastSpawnSuccess);
            spawnHistory.Add(attempt);
            
            // Keep history manageable (last 100 attempts)
            if (spawnHistory.Count > 100)
            {
                spawnHistory.RemoveAt(0);
            }
            
            if (enableSpawnChanceDebug)
            {
                string result = lastSpawnSuccess ? "SUCCESS" : "FAILED";
                string icon = lastSpawnSuccess ? "✅" : "❌";
                
                DebugSystemConfig.LogObstacleSpawn(
                    $"{icon} Spawn at ({position.x},{position.y}): Roll {randomRoll:F3} vs Chance {spawnChance:F3} = {result}"
                );
                
                if (enableSpawnStatistics)
                {
                    DebugSystemConfig.LogObstacleSpawn(
                        $"📊 Running Stats: {successfulSpawns}/{totalSpawnAttempts} spawns ({currentSuccessRate:P1} success rate)"
                    );
                }
            }
        }
        
        /// <summary>
        /// Log obstacle type selection and track distribution
        /// </summary>
        public void LogObstacleTypeSpawned(ObstacleType obstacleType, Vector2Int position)
        {
            // Update counters
            obstacleTypeCounts[obstacleType]++;
            
            // Update serialized fields for Inspector visibility
            switch (obstacleType)
            {
                case ObstacleType.Corporeal: corporealCount++; break;
                case ObstacleType.Frigid: frigidCount++; break;
                case ObstacleType.Scorch: scorchCount++; break;
                case ObstacleType.Caustic: causticCount++; break;
                case ObstacleType.Arc: arcCount++; break;
                case ObstacleType.Divine: divineCount++; break;
            }
            
            // Update the last spawn attempt with the type
            if (spawnHistory.Count > 0)
            {
                var lastAttempt = spawnHistory[spawnHistory.Count - 1];
                lastAttempt.spawnedType = obstacleType;
                spawnHistory[spawnHistory.Count - 1] = lastAttempt;
            }
            
            if (enableSpawnChanceDebug)
            {
                string obstacleIcon = GetObstacleIcon(obstacleType);
                DebugSystemConfig.LogObstacleSpawn(
                    $"{obstacleIcon} Spawned {obstacleType} obstacle at ({position.x},{position.y})"
                );
                
                if (enableSpawnDistributionAnalysis)
                {
                    int totalObstacles = obstacleTypeCounts.Values.Sum();
                    float percentage = totalObstacles > 0 ? (float)obstacleTypeCounts[obstacleType] / totalObstacles * 100f : 0f;
                    
                    DebugSystemConfig.LogObstacleSpawn(
                        $"📈 {obstacleType} Distribution: {obstacleTypeCounts[obstacleType]}/{totalObstacles} ({percentage:F1}%)"
                    );
                }
            }
        }
        
        /// <summary>
        /// Get emoji icon for obstacle type
        /// </summary>
        private string GetObstacleIcon(ObstacleType type)
        {
            return type switch
            {
                ObstacleType.Corporeal => "🪨",
                ObstacleType.Frigid => "❄️",
                ObstacleType.Scorch => "🔥",
                ObstacleType.Caustic => "☢️",
                ObstacleType.Arc => "⚡",
                ObstacleType.Divine => "✨",
                _ => "❓"
            };
        }
        
        /// <summary>
        /// Generate comprehensive spawn analytics report
        /// </summary>
        [ContextMenu("Generate Spawn Analytics Report")]
        public void GenerateSpawnAnalyticsReport()
        {
            if (!enableSpawnStatistics)
            {
                Debug.Log("🚧 Spawn statistics disabled - enable to generate report");
                return;
            }
            
            Debug.Log("🚧 === OBSTACLE SPAWN ANALYTICS REPORT ===");
            
            // Overall statistics
            Debug.Log($"📊 OVERALL STATISTICS:");
            Debug.Log($"   • Total spawn attempts: {totalSpawnAttempts}");
            Debug.Log($"   • Successful spawns: {successfulSpawns}");
            Debug.Log($"   • Success rate: {currentSuccessRate:P2}");
            Debug.Log($"   • Last spawn chance: {lastSpawnChance:F3}");
            Debug.Log($"   • Last random roll: {lastRandomRoll:F3}");
            
            // Recent spawn pattern
            if (spawnHistory.Count > 0)
            {
                var recentAttempts = spawnHistory.TakeLast(10).ToList();
                int recentSuccesses = recentAttempts.Count(a => a.success);
                float recentSuccessRate = (float)recentSuccesses / recentAttempts.Count;
                
                Debug.Log($"📈 RECENT PATTERN (last {recentAttempts.Count} attempts):");
                Debug.Log($"   • Recent success rate: {recentSuccessRate:P2}");
                Debug.Log($"   • Recent spawns: {recentSuccesses}/{recentAttempts.Count}");
            }
            
            // Obstacle type distribution
            int totalObstacles = obstacleTypeCounts.Values.Sum();
            if (totalObstacles > 0)
            {
                Debug.Log($"🎲 OBSTACLE TYPE DISTRIBUTION ({totalObstacles} total):");
                foreach (var kvp in obstacleTypeCounts.OrderByDescending(x => x.Value))
                {
                    float percentage = (float)kvp.Value / totalObstacles * 100f;
                    string icon = GetObstacleIcon(kvp.Key);
                    Debug.Log($"   {icon} {kvp.Key}: {kvp.Value} ({percentage:F1}%)");
                }
                
                // Check for even distribution
                float expectedPercentage = 100f / System.Enum.GetValues(typeof(ObstacleType)).Length;
                var deviations = obstacleTypeCounts.Values.Select(count => 
                    Mathf.Abs(((float)count / totalObstacles * 100f) - expectedPercentage)
                ).ToList();
                float averageDeviation = deviations.Average();
                
                Debug.Log($"📊 Distribution Analysis:");
                Debug.Log($"   • Expected per type: {expectedPercentage:F1}%");
                Debug.Log($"   • Average deviation: {averageDeviation:F1}%");
                
                if (averageDeviation < 5f)
                {
                    Debug.Log($"   ✅ Distribution is well-balanced");
                }
                else if (averageDeviation < 10f)
                {
                    Debug.Log($"   ⚠️ Distribution has some imbalance");
                }
                else
                {
                    Debug.Log($"   ❌ Distribution is significantly imbalanced");
                }
            }
            
            Debug.Log("🚧 === END ANALYTICS REPORT ===");
        }
        
        /// <summary>
        /// Test obstacle spawn probability with multiple rolls
        /// </summary>
        [ContextMenu("Test Spawn Probability (100 rolls)")]
        public void TestSpawnProbability()
        {
            var gridManager = GetComponent<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("🚧 No GridGameManager found for spawn testing");
                return;
            }
            
            float spawnChance = gridManager.obstacleSpawnChance;
            int testRolls = 100;
            int successCount = 0;
            
            Debug.Log($"🚧 === TESTING SPAWN PROBABILITY ===");
            Debug.Log($"🎲 Running {testRolls} spawn chance tests with {spawnChance:P1} chance");
            
            for (int i = 0; i < testRolls; i++)
            {
                float roll = Random.Range(0f, 1f);
                bool success = roll < spawnChance;
                if (success) successCount++;
                
                if (i < 10 || i >= testRolls - 10) // Log first and last 10
                {
                    string result = success ? "✅ SPAWN" : "❌ NO SPAWN";
                    Debug.Log($"   Roll {i + 1}: {roll:F3} → {result}");
                }
                else if (i == 10)
                {
                    Debug.Log($"   ... (rolling {testRolls - 20} more times) ...");
                }
            }
            
            float actualSuccessRate = (float)successCount / testRolls;
            float expectedSuccessRate = spawnChance;
            float deviation = Mathf.Abs(actualSuccessRate - expectedSuccessRate) * 100f;
            
            Debug.Log($"📊 TEST RESULTS:");
            Debug.Log($"   • Expected success rate: {expectedSuccessRate:P2}");
            Debug.Log($"   • Actual success rate: {actualSuccessRate:P2} ({successCount}/{testRolls})");
            Debug.Log($"   • Deviation: {deviation:F1} percentage points");
            
            if (deviation < 5f)
            {
                Debug.Log($"   ✅ Random generation is working correctly");
            }
            else if (deviation < 10f)
            {
                Debug.Log($"   ⚠️ Some variance detected (normal for small samples)");
            }
            else
            {
                Debug.Log($"   ❌ Significant deviation detected - check random generation");
            }
            
            Debug.Log("🚧 === END PROBABILITY TEST ===");
        }
        
        /// <summary>
        /// Reset all spawn statistics
        /// </summary>
        [ContextMenu("Reset Spawn Statistics")]
        public void ResetSpawnStatistics()
        {
            totalSpawnAttempts = 0;
            successfulSpawns = 0;
            currentSuccessRate = 0f;
            lastSpawnChance = 0f;
            lastRandomRoll = 0f;
            lastSpawnSuccess = false;
            
            corporealCount = 0;
            frigidCount = 0;
            scorchCount = 0;
            causticCount = 0;
            arcCount = 0;
            divineCount = 0;
            
            spawnHistory.Clear();
            InitializeObstacleTypeCounts();
            
            Debug.Log("🚧 Obstacle spawn statistics reset");
        }
        
        /// <summary>
        /// Get current spawn statistics as a formatted string
        /// </summary>
        public string GetSpawnStatsSummary()
        {
            return $"Spawns: {successfulSpawns}/{totalSpawnAttempts} ({currentSuccessRate:P1}) | " +
                   $"Last: {lastRandomRoll:F3} vs {lastSpawnChance:F3} = {(lastSpawnSuccess ? "✅" : "❌")}";
        }
    }
}