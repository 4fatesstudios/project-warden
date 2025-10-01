using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class GridDemoGameplay : MonoBehaviour
    {
        [Header("Game Rules")]
        public int maxIngredientsPerType = 3;
        public float reactionBonus = 1.5f;
        public bool allowIngredientStacking = false;
        
        [Header("Scoring")]
        public int baseScorePerIngredient = 10;
        public int aspectMatchBonus = 25;
        public int symmetryBonus = 50;
        public int completionBonus = 100;
        
        [Header("Visual Feedback")]
        public GameObject reactionEffectPrefab;
        public AudioClip placementSound;
        public AudioClip reactionSound;
        public AudioClip completionSound;
        
        private GridGameManager gridManager;
        private Dictionary<Aspect, int> aspectCounts = new Dictionary<Aspect, int>();
        private int currentScore = 0;
        private bool gameCompleted = false;
        
        public System.Action<int> OnScoreChanged;
        public System.Action<string> OnGameStateChanged;
        public System.Action OnGameCompleted;
        
        private void Start()
        {
            gridManager = GetComponent<GridGameManager>();
            InitializeGameplay();
        }
        
        private void InitializeGameplay()
        {
            ResetGame();
            
            // Subscribe to grid events (you'd need to add these events to GridGameManager)
            // gridManager.OnIngredientPlaced += OnIngredientPlaced;
            // gridManager.OnIngredientRemoved += OnIngredientRemoved;
            
            OnGameStateChanged?.Invoke("Game Started - Place ingredients to begin!");
        }
        
        public void ResetGame()
        {
            currentScore = 0;
            gameCompleted = false;
            aspectCounts.Clear();
            gridManager.ClearGrid();
            
            OnScoreChanged?.Invoke(currentScore);
            OnGameStateChanged?.Invoke("Grid cleared - Ready to start!");
        }
        
        public void OnIngredientPlaced(Ingredient ingredient, Vector2Int position)
        {
            if (gameCompleted) return;
            
            // Update aspect counts
            if (aspectCounts.ContainsKey(ingredient.IngredientAspect))
                aspectCounts[ingredient.IngredientAspect]++;
            else
                aspectCounts[ingredient.IngredientAspect] = 1;
            
            // Calculate placement score
            int placementScore = CalculatePlacementScore(ingredient, position);
            AddScore(placementScore);
            
            // Check for reactions
            CheckReactions(ingredient, position);
            
            // Check for patterns
            CheckPatterns();
            
            // Check win condition
            CheckWinCondition();
            
            // Play sound effect
            PlaySound(placementSound);
            
            UpdateGameState();
        }
        
        public void OnIngredientRemoved(Ingredient ingredient, Vector2Int position)
        {
            if (gameCompleted) return;
            
            // Update aspect counts
            if (aspectCounts.ContainsKey(ingredient.IngredientAspect))
            {
                aspectCounts[ingredient.IngredientAspect]--;
                if (aspectCounts[ingredient.IngredientAspect] <= 0)
                    aspectCounts.Remove(ingredient.IngredientAspect);
            }
            
            // Deduct score
            int penaltyScore = -Mathf.RoundToInt(baseScorePerIngredient * 0.5f);
            AddScore(penaltyScore);
            
            UpdateGameState();
        }
        
        private int CalculatePlacementScore(Ingredient ingredient, Vector2Int position)
        {
            int score = baseScorePerIngredient * ingredient.Potency;
            
            // Bonus for placing near edges (strategic placement)
            if (IsEdgePosition(position))
                score += 5;
            
            // Bonus for placing near center (balance bonus)
            if (IsCenterPosition(position))
                score += 10;
            
            return score;
        }
        
        private void CheckReactions(Ingredient newIngredient, Vector2Int position)
        {
            List<Vector2Int> reactionPositions = new List<Vector2Int>();
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int adjacentPos = position + dir;
                GridCell adjacentCell = gridManager.GetCell(adjacentPos.x, adjacentPos.y);
                
                if (adjacentCell != null && adjacentCell.IsOccupied)
                {
                    if (HasReaction(newIngredient, adjacentCell.OccupiedByIngredient))
                    {
                        reactionPositions.Add(adjacentPos);
                    }
                }
            }
            
            if (reactionPositions.Count > 0)
            {
                ProcessReactions(newIngredient, position, reactionPositions);
            }
        }
        
        private bool HasReaction(Ingredient ingredient1, Ingredient ingredient2)
        {
            // Opposing elements
            // Opposing elements react
            if ((ingredient1.IngredientAspect == Aspect.Scorch && ingredient2.IngredientAspect == Aspect.Frigid) ||
                (ingredient1.IngredientAspect == Aspect.Frigid && ingredient2.IngredientAspect == Aspect.Scorch))
            {
                return true;
                //Debug.Log($"Thermal reaction between {ingredient1.ItemName} and {ingredient2.ItemName}!");
            }
            
            // Life and Death reaction
            if ((ingredient1.IngredientAspect == Aspect.Corporeal && ingredient2.IngredientAspect == Aspect.Divine) ||
                (ingredient1.IngredientAspect == Aspect.Divine && ingredient2.IngredientAspect == Aspect.Corporeal))
            {
                return true;
                //Debug.Log($"Life/Death reaction between {ingredient1.ItemName} and {ingredient2.ItemName}!");
            }
            
            // Order and Chaos reaction
            if ((ingredient1.IngredientAspect == Aspect.Arc && ingredient2.IngredientAspect == Aspect.Caustic) ||
                (ingredient1.IngredientAspect == Aspect.Caustic && ingredient2.IngredientAspect == Aspect.Arc))
            {
                return true;
            }
            
            return false;
        }
        
        private void ProcessReactions(Ingredient centerIngredient, Vector2Int centerPos, List<Vector2Int> reactionPositions)
        {
            int reactionScore = Mathf.RoundToInt(aspectMatchBonus * reactionBonus * reactionPositions.Count);
            AddScore(reactionScore);
            
            // Create visual effects
            foreach (Vector2Int pos in reactionPositions)
            {
                CreateReactionEffect(centerPos, pos);
            }
            
            PlaySound(reactionSound);
            
            OnGameStateChanged?.Invoke($"Reaction! +{reactionScore} points from {reactionPositions.Count} adjacent ingredients!");
        }
        
        private void CreateReactionEffect(Vector2Int pos1, Vector2Int pos2)
        {
            if (reactionEffectPrefab == null) return;
            
            Vector3 worldPos1 = gridManager.GridToWorldPosition(pos1);
            Vector3 worldPos2 = gridManager.GridToWorldPosition(pos2);
            Vector3 midpoint = (worldPos1 + worldPos2) * 0.5f;
            
            GameObject effect = Instantiate(reactionEffectPrefab, midpoint, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        private void CheckPatterns()
        {
            // Check for symmetrical placement
            if (IsSymmetrical())
            {
                AddScore(symmetryBonus);
                OnGameStateChanged?.Invoke($"Symmetrical pattern detected! +{symmetryBonus} points!");
            }
            
            // Check for aspect clusters
            CheckAspectClusters();
        }
        
        private bool IsSymmetrical()
        {
            GridCell[,] cells = gridManager.GetAllCells();
            
            // Check horizontal symmetry
            for (int x = 0; x < gridManager.gridWidth / 2; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    GridCell leftCell = cells[x, y];
                    GridCell rightCell = cells[gridManager.gridWidth - 1 - x, y];
                    
                    if (leftCell.IsOccupied != rightCell.IsOccupied)
                        return false;
                    
                    if (leftCell.IsOccupied && rightCell.IsOccupied)
                    {
                        if (leftCell.CellAspect != rightCell.CellAspect)
                            return false;
                    }
                }
            }
            
            return true;
        }
        
        private void CheckAspectClusters()
        {
            foreach (var aspectCount in aspectCounts)
            {
                if (aspectCount.Value >= 3)
                {
                    int clusterBonus = aspectCount.Value * 5;
                    AddScore(clusterBonus);
                    OnGameStateChanged?.Invoke($"{aspectCount.Key} cluster formed! +{clusterBonus} points!");
                }
            }
        }
        
        private void CheckWinCondition()
        {
            // Win condition: Fill a certain percentage of the grid
            int totalCells = gridManager.gridWidth * gridManager.gridHeight;
            int occupiedCells = 0;
            
            GridCell[,] cells = gridManager.GetAllCells();
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    if (cells[x, y].IsOccupied)
                        occupiedCells++;
                }
            }
            
            float occupancyRate = (float)occupiedCells / totalCells;
            
            if (occupancyRate >= 0.75f && !gameCompleted) // 75% filled
            {
                CompleteGame();
            }
        }
        
        private void CompleteGame()
        {
            gameCompleted = true;
            AddScore(completionBonus);
            
            PlaySound(completionSound);
            OnGameCompleted?.Invoke();
            OnGameStateChanged?.Invoke($"Game Complete! Final Score: {currentScore}");
        }
        
        private bool IsEdgePosition(Vector2Int position)
        {
            return position.x == 0 || position.x == gridManager.gridWidth - 1 ||
                   position.y == 0 || position.y == gridManager.gridHeight - 1;
        }
        
        private bool IsCenterPosition(Vector2Int position)
        {
            int centerX = gridManager.gridWidth / 2;
            int centerY = gridManager.gridHeight / 2;
            
            return Mathf.Abs(position.x - centerX) <= 1 && Mathf.Abs(position.y - centerY) <= 1;
        }
        
        private void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, UnityEngine.Camera.main.transform.position);
            }
        }
        
        private void UpdateGameState()
        {
            if (gameCompleted) return;
            
            int occupiedCells = 0;
            GridCell[,] cells = gridManager.GetAllCells();
            
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    if (cells[x, y].IsOccupied)
                        occupiedCells++;
                }
            }
            
            float progress = (float)occupiedCells / (gridManager.gridWidth * gridManager.gridHeight) * 100f;
            OnGameStateChanged?.Invoke($"Progress: {progress:F1}% | Score: {currentScore}");
        }
        
        public Dictionary<Aspect, int> GetAspectCounts()
        {
            return new Dictionary<Aspect, int>(aspectCounts);
        }
        
        public int GetCurrentScore()
        {
            return currentScore;
        }
        
        public bool IsGameCompleted()
        {
            return gameCompleted;
        }
    }
}