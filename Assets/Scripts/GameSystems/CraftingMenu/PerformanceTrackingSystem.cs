using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    /// <summary>
    /// Tracks player performance in minigames and manages bulk crafting eligibility
    /// </summary>
    public class PerformanceTrackingSystem : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private float sRankThreshold = 90f;
        [SerializeField] private float aRankThreshold = 80f;
        [SerializeField] private float bRankThreshold = 70f;
        [SerializeField] private float cRankThreshold = 60f;
        [SerializeField] private float dRankThreshold = 50f;
        
        [Header("Bulk Crafting Requirements")]
        [SerializeField] private bool requireSRankForBulkCrafting = true;
        [SerializeField] private int minimumAttemptsForRanking = 3;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        
        // Performance data storage
        private Dictionary<string, MinigamePerformance> performances = new Dictionary<string, MinigamePerformance>();
        
        // Events
        public event Action<string, PerformanceRank> OnRankAchieved;
        public event Action<string> OnBulkCraftingUnlocked;
        
        // Singleton pattern for easy access
        public static PerformanceTrackingSystem Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPerformanceData();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                SavePerformanceData();
            }
        }
        
        /// <summary>
        /// Records a minigame performance for a specific recipe
        /// </summary>
        public void RecordPerformance(AlchemyRecipe recipe, float score, float timeElapsed, bool completed)
        {
            if (recipe == null) return;
            
            string recipeId = recipe.name;
            
            // Get or create performance record
            if (!performances.TryGetValue(recipeId, out MinigamePerformance performance))
            {
                performance = new MinigamePerformance
                {
                    recipeId = recipeId,
                    recipeName = recipe.ItemName
                };
                performances[recipeId] = performance;
            }
            
            // Update performance data
            performance.attempts++;
            performance.lastScore = score;
            performance.lastCompleted = completed;
            
            if (completed)
            {
                performance.completions++;
                performance.totalScore += score;
                performance.averageScore = performance.totalScore / performance.completions;
                
                // Update best scores
                if (score > performance.bestScore)
                {
                    performance.bestScore = score;
                    performance.bestTime = timeElapsed;
                }
                else if (score == performance.bestScore && timeElapsed < performance.bestTime)
                {
                    performance.bestTime = timeElapsed;
                }
                
                // Calculate new rank
                PerformanceRank newRank = CalculateRank(performance.bestScore);
                PerformanceRank oldRank = performance.rank;
                
                if (newRank > oldRank)
                {
                    performance.rank = newRank;
                    OnRankImproved(recipeId, oldRank, newRank);
                }
            }
            
            // Update timestamps
            performance.lastAttemptTime = DateTime.Now;
            
            if (enableDebugLogging)
            {
                Debug.Log($"🎯 Performance recorded for {recipe.ItemName}: Score={score:F1}, Rank={performance.rank}, Attempts={performance.attempts}");
            }
            
            SavePerformanceData();
        }
        
        /// <summary>
        /// Calculates rank based on score
        /// </summary>
        private PerformanceRank CalculateRank(float score)
        {
            if (score >= sRankThreshold) return PerformanceRank.S;
            if (score >= aRankThreshold) return PerformanceRank.A;
            if (score >= bRankThreshold) return PerformanceRank.B;
            if (score >= cRankThreshold) return PerformanceRank.C;
            if (score >= dRankThreshold) return PerformanceRank.D;
            return PerformanceRank.F;
        }
        
        /// <summary>
        /// Handles rank improvements and unlocks
        /// </summary>
        private void OnRankImproved(string recipeId, PerformanceRank oldRank, PerformanceRank newRank)
        {
            OnRankAchieved?.Invoke(recipeId, newRank);
            
            // Check for bulk crafting unlock
            if (requireSRankForBulkCrafting && newRank == PerformanceRank.S && oldRank < PerformanceRank.S)
            {
                OnBulkCraftingUnlocked?.Invoke(recipeId);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"🎉 Bulk crafting unlocked for {GetRecipeName(recipeId)}!");
                }
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"📈 Rank improved for {GetRecipeName(recipeId)}: {oldRank} → {newRank}");
            }
        }
        
        /// <summary>
        /// Checks if bulk crafting is available for a recipe
        /// </summary>
        public bool CanBulkCraft(string recipeId)
        {
            if (!performances.TryGetValue(recipeId, out MinigamePerformance performance))
                return false;
                
            if (requireSRankForBulkCrafting)
            {
                return performance.rank == PerformanceRank.S && performance.attempts >= minimumAttemptsForRanking;
            }
            
            return performance.rank >= PerformanceRank.A && performance.attempts >= minimumAttemptsForRanking;
        }
        
        /// <summary>
        /// Checks if bulk crafting is available for a recipe object
        /// </summary>
        public bool CanBulkCraft(AlchemyRecipe recipe)
        {
            return recipe != null && CanBulkCraft(recipe.name);
        }
        
        /// <summary>
        /// Gets performance data for a recipe
        /// </summary>
        public MinigamePerformance GetPerformance(string recipeId)
        {
            return performances.TryGetValue(recipeId, out MinigamePerformance performance) ? performance : null;
        }
        
        /// <summary>
        /// Gets performance data for a recipe object
        /// </summary>
        public MinigamePerformance GetPerformance(AlchemyRecipe recipe)
        {
            return recipe != null ? GetPerformance(recipe.name) : null;
        }
        
        /// <summary>
        /// Gets all recipes with S rank
        /// </summary>
        public List<MinigamePerformance> GetSRankRecipes()
        {
            return performances.Values.Where(p => p.rank == PerformanceRank.S).ToList();
        }
        
        /// <summary>
        /// Gets all recipes available for bulk crafting
        /// </summary>
        public List<MinigamePerformance> GetBulkCraftableRecipes()
        {
            return performances.Values.Where(p => CanBulkCraft(p.recipeId)).ToList();
        }
        
        /// <summary>
        /// Gets recipe name from ID (for logging purposes)
        /// </summary>
        private string GetRecipeName(string recipeId)
        {
            if (performances.TryGetValue(recipeId, out MinigamePerformance performance))
            {
                return performance.recipeName ?? recipeId;
            }
            return recipeId;
        }
        
        /// <summary>
        /// Saves performance data to PlayerPrefs
        /// </summary>
        private void SavePerformanceData()
        {
            try
            {
                string json = JsonUtility.ToJson(new PerformanceDataWrapper { performances = performances.Values.ToArray() });
                PlayerPrefs.SetString("AlchemyPerformanceData", json);
                PlayerPrefs.Save();
                
                if (enableDebugLogging)
                {
                    Debug.Log($"💾 Saved performance data for {performances.Count} recipes");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save performance data: {e.Message}");
            }
        }
        
        /// <summary>
        /// Loads performance data from PlayerPrefs
        /// </summary>
        private void LoadPerformanceData()
        {
            try
            {
                string json = PlayerPrefs.GetString("AlchemyPerformanceData", "");
                if (!string.IsNullOrEmpty(json))
                {
                    PerformanceDataWrapper wrapper = JsonUtility.FromJson<PerformanceDataWrapper>(json);
                    if (wrapper?.performances != null)
                    {
                        performances.Clear();
                        foreach (var performance in wrapper.performances)
                        {
                            performances[performance.recipeId] = performance;
                        }
                        
                        if (enableDebugLogging)
                        {
                            Debug.Log($"📋 Loaded performance data for {performances.Count} recipes");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load performance data: {e.Message}");
                performances.Clear();
            }
        }
        
        /// <summary>
        /// Resets performance data for a specific recipe
        /// </summary>
        public void ResetPerformance(string recipeId)
        {
            if (performances.ContainsKey(recipeId))
            {
                performances.Remove(recipeId);
                SavePerformanceData();
                
                if (enableDebugLogging)
                {
                    Debug.Log($"🔄 Reset performance data for {GetRecipeName(recipeId)}");
                }
            }
        }
        
        /// <summary>
        /// Resets all performance data
        /// </summary>
        public void ResetAllPerformance()
        {
            performances.Clear();
            SavePerformanceData();
            
            if (enableDebugLogging)
            {
                Debug.Log("🔄 Reset all performance data");
            }
        }
        
        /// <summary>
        /// Gets summary statistics
        /// </summary>
        public PerformanceStats GetStats()
        {
            var stats = new PerformanceStats();
            
            if (performances.Count == 0)
                return stats;
            
            stats.totalRecipes = performances.Count;
            stats.sRankCount = performances.Values.Count(p => p.rank == PerformanceRank.S);
            stats.aRankCount = performances.Values.Count(p => p.rank == PerformanceRank.A);
            stats.bRankCount = performances.Values.Count(p => p.rank == PerformanceRank.B);
            stats.bulkCraftableCount = performances.Values.Count(p => CanBulkCraft(p.recipeId));
            stats.averageScore = performances.Values.Where(p => p.completions > 0).Average(p => p.averageScore);
            stats.totalAttempts = performances.Values.Sum(p => p.attempts);
            stats.totalCompletions = performances.Values.Sum(p => p.completions);
            
            return stats;
        }
    }
    
    /// <summary>
    /// Wrapper class for JSON serialization
    /// </summary>
    [System.Serializable]
    public class PerformanceDataWrapper
    {
        public MinigamePerformance[] performances;
    }
    
    /// <summary>
    /// Performance statistics summary
    /// </summary>
    [System.Serializable]
    public class PerformanceStats
    {
        public int totalRecipes;
        public int sRankCount;
        public int aRankCount;
        public int bRankCount;
        public int bulkCraftableCount;
        public float averageScore;
        public int totalAttempts;
        public int totalCompletions;
    }
    
    /// <summary>
    /// Individual recipe performance data
    /// </summary>
    [System.Serializable]
    public class MinigamePerformance
    {
        public string recipeId;
        public string recipeName;
        public PerformanceRank rank = PerformanceRank.F;
        public float bestScore = 0f;
        public float lastScore = 0f;
        public float totalScore = 0f;
        public float averageScore = 0f;
        public int attempts = 0;
        public int completions = 0;
        public float bestTime = float.MaxValue;
        public bool lastCompleted = false;
        public DateTime lastAttemptTime = DateTime.MinValue;
        
        public bool canBulkCraft => rank == PerformanceRank.S;
        public float completionRate => attempts > 0 ? (float)completions / attempts : 0f;
    }
    
    /// <summary>
    /// Performance ranking system
    /// </summary>
    public enum PerformanceRank
    {
        F = 0,  // 0-49%
        D = 1,  // 50-59%
        C = 2,  // 60-69%
        B = 3,  // 70-79%
        A = 4,  // 80-89%
        S = 5   // 90%+
    }
}