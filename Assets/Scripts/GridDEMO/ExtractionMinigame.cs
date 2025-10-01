using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Sliding block puzzle minigame for extracting specific ingredients
    /// </summary>
    public class ExtractionMinigame : MonoBehaviour
    {
        [Header("Minigame Configuration")]
        [SerializeField] private int puzzleWidth = 6;
        [SerializeField] private int puzzleHeight = 6;
        [SerializeField] private bool enableMinigame = true;
        
        [Header("Extraction Settings")]
        [SerializeField] private Ingredient targetIngredient;
        [SerializeField] private Vector2Int targetPosition = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int exitPosition = new Vector2Int(5, 2);
        
        [Header("Puzzle Ingredients")]
        [SerializeField] private List<PlacedPuzzleIngredient> puzzleIngredients = new List<PlacedPuzzleIngredient>();
        
        private bool isMinigameActive = false;
        private int moveCount = 0;
        private float startTime = 0f;
        
        [System.Serializable]
        public class PlacedPuzzleIngredient
        {
            public Ingredient ingredient;
            public Vector2Int position;
            public Vector2Int size = Vector2Int.one;
            public bool isTarget = false;
            public bool isMovable = true;
            public SlideDirection allowedDirections = SlideDirection.All;
        }
        
        [System.Flags]
        public enum SlideDirection
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            Horizontal = Left | Right,
            Vertical = Up | Down,
            All = Horizontal | Vertical
        }
        
        public struct ExtractionResult
        {
            public bool success;
            public float timeElapsed;
            public int moveCount;
            public float efficiencyScore;
            public Ingredient extractedIngredient;
        }
        
        private bool[,] puzzleGrid;
        private Dictionary<Ingredient, PlacedPuzzleIngredient> ingredientLookup;
        
        private void Awake()
        {
            InitializePuzzle();
        }
        
        /// <summary>
        /// Initialize the extraction puzzle
        /// </summary>
        private void InitializePuzzle()
        {
            puzzleGrid = new bool[puzzleWidth, puzzleHeight];
            ingredientLookup = new Dictionary<Ingredient, PlacedPuzzleIngredient>();
            
            foreach (var placedIngredient in puzzleIngredients)
            {
                if (placedIngredient.ingredient != null)
                {
                    ingredientLookup[placedIngredient.ingredient] = placedIngredient;
                }
            }
        }
        
        /// <summary>
        /// Start the extraction minigame
        /// </summary>
        public bool StartExtractionMinigame(Ingredient target = null)
        {
            if (!enableMinigame) return false;
            
            if (target != null)
            {
                targetIngredient = target;
            }
            
            if (targetIngredient == null)
            {
                Debug.LogError("No target ingredient specified for extraction");
                return false;
            }
            
            isMinigameActive = true;
            moveCount = 0;
            startTime = Time.time;
            
            // Generate or load puzzle layout
            GeneratePuzzleLayout();
            
            Debug.Log($"🧩 === EXTRACTION MINIGAME STARTED ===");
            Debug.Log($"🎯 Target: {targetIngredient.ItemName}");
            Debug.Log($"🎯 Goal: Slide {targetIngredient.ItemName} to the exit position");
            
            return true;
        }
        
        /// <summary>
        /// Generate a random puzzle layout
        /// </summary>
        private void GeneratePuzzleLayout()
        {
            // Clear existing layout
            puzzleIngredients.Clear();
            ClearPuzzleGrid();
            
            // Place target ingredient in center-left area
            var targetPlacement = new PlacedPuzzleIngredient
            {
                ingredient = targetIngredient,
                position = targetPosition,
                size = new Vector2Int(targetIngredient.GridWidth, targetIngredient.GridHeight),
                isTarget = true,
                isMovable = true,
                allowedDirections = SlideDirection.Horizontal // Target can only move horizontally to exit
            };
            puzzleIngredients.Add(targetPlacement);
            PlaceIngredientOnGrid(targetPlacement);
            
            // Generate blocking ingredients
            GenerateBlockingIngredients();
            
            Debug.Log($"🧩 Generated puzzle with {puzzleIngredients.Count} ingredients");
        }
        
        /// <summary>
        /// Generate blocking ingredients to create puzzle complexity
        /// </summary>
        private void GenerateBlockingIngredients()
        {
            var availableIngredients = FindFirstObjectByType<GridGameManager>()?.availableIngredients;
            if (availableIngredients == null || availableIngredients.Count == 0) return;
            
            int blockersToPlace = Random.Range(3, 8);
            
            for (int i = 0; i < blockersToPlace; i++)
            {
                var randomIngredient = availableIngredients[Random.Range(0, availableIngredients.Count)];
                if (randomIngredient == targetIngredient) continue; // Don't place duplicate targets
                
                Vector2Int randomPos = FindValidPlacementPosition(randomIngredient);
                if (randomPos.x >= 0) // Valid position found
                {
                    var blocker = new PlacedPuzzleIngredient
                    {
                        ingredient = randomIngredient,
                        position = randomPos,
                        size = new Vector2Int(randomIngredient.GridWidth, randomIngredient.GridHeight),
                        isTarget = false,
                        isMovable = true,
                        allowedDirections = GetRandomSlideDirection()
                    };
                    
                    puzzleIngredients.Add(blocker);
                    PlaceIngredientOnGrid(blocker);
                }
            }
        }
        
        /// <summary>
        /// Find a valid position to place an ingredient without overlap
        /// </summary>
        private Vector2Int FindValidPlacementPosition(Ingredient ingredient)
        {
            int attempts = 50;
            while (attempts > 0)
            {
                int x = Random.Range(0, puzzleWidth - ingredient.GridWidth + 1);
                int y = Random.Range(0, puzzleHeight - ingredient.GridHeight + 1);
                Vector2Int pos = new Vector2Int(x, y);
                
                if (CanPlaceIngredientAt(pos, ingredient))
                {
                    return pos;
                }
                
                attempts--;
            }
            
            return new Vector2Int(-1, -1); // No valid position found
        }
        
        /// <summary>
        /// Check if ingredient can be placed at position
        /// </summary>
        private bool CanPlaceIngredientAt(Vector2Int position, Ingredient ingredient)
        {
            // Check bounds
            if (position.x + ingredient.GridWidth > puzzleWidth ||
                position.y + ingredient.GridHeight > puzzleHeight)
                return false;
            
            // Check for overlap
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    if (puzzleGrid[position.x + x, position.y + y])
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Place ingredient on puzzle grid
        /// </summary>
        private void PlaceIngredientOnGrid(PlacedPuzzleIngredient placement)
        {
            for (int x = 0; x < placement.size.x; x++)
            {
                for (int y = 0; y < placement.size.y; y++)
                {
                    Vector2Int gridPos = placement.position + new Vector2Int(x, y);
                    if (IsValidGridPosition(gridPos))
                    {
                        puzzleGrid[gridPos.x, gridPos.y] = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// Remove ingredient from puzzle grid
        /// </summary>
        private void RemoveIngredientFromGrid(PlacedPuzzleIngredient placement)
        {
            for (int x = 0; x < placement.size.x; x++)
            {
                for (int y = 0; y < placement.size.y; y++)
                {
                    Vector2Int gridPos = placement.position + new Vector2Int(x, y);
                    if (IsValidGridPosition(gridPos))
                    {
                        puzzleGrid[gridPos.x, gridPos.y] = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Get random slide direction
        /// </summary>
        private SlideDirection GetRandomSlideDirection()
        {
            var directions = new SlideDirection[]
            {
                SlideDirection.Horizontal,
                SlideDirection.Vertical,
                SlideDirection.All
            };
            
            return directions[Random.Range(0, directions.Length)];
        }
        
        /// <summary>
        /// Attempt to slide an ingredient in a direction
        /// </summary>
        public bool TrySlideIngredient(Ingredient ingredient, SlideDirection direction)
        {
            if (!isMinigameActive || !ingredientLookup.ContainsKey(ingredient))
                return false;
            
            var placement = ingredientLookup[ingredient];
            if (!placement.isMovable)
                return false;
            
            // Check if direction is allowed
            if ((placement.allowedDirections & direction) == 0)
            {
                Debug.Log($"❌ {ingredient.ItemName} cannot move in direction {direction}");
                return false;
            }
            
            Vector2Int moveVector = GetMoveVector(direction);
            Vector2Int newPosition = placement.position + moveVector;
            
            // Check if move is valid
            if (!CanMoveIngredientTo(placement, newPosition))
            {
                Debug.Log($"❌ Cannot move {ingredient.ItemName} to {newPosition} - blocked");
                return false;
            }
            
            // Perform the move
            RemoveIngredientFromGrid(placement);
            placement.position = newPosition;
            PlaceIngredientOnGrid(placement);
            
            moveCount++;
            Debug.Log($"✅ Moved {ingredient.ItemName} to {newPosition} (Move #{moveCount})");
            
            // Check for completion
            if (placement.isTarget && HasReachedExit(placement))
            {
                CompleteMinigame();
            }
            
            return true;
        }
        
        /// <summary>
        /// Get movement vector for direction
        /// </summary>
        private Vector2Int GetMoveVector(SlideDirection direction)
        {
            return direction switch
            {
                SlideDirection.Up => Vector2Int.up,
                SlideDirection.Down => Vector2Int.down,
                SlideDirection.Left => Vector2Int.left,
                SlideDirection.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }
        
        /// <summary>
        /// Check if ingredient can move to new position
        /// </summary>
        private bool CanMoveIngredientTo(PlacedPuzzleIngredient placement, Vector2Int newPosition)
        {
            // Check bounds
            if (newPosition.x < 0 || newPosition.y < 0 ||
                newPosition.x + placement.size.x > puzzleWidth ||
                newPosition.y + placement.size.y > puzzleHeight)
                return false;
            
            // Temporarily remove ingredient from grid
            RemoveIngredientFromGrid(placement);
            
            // Check for overlap with other ingredients
            bool canMove = true;
            for (int x = 0; x < placement.size.x; x++)
            {
                for (int y = 0; y < placement.size.y; y++)
                {
                    Vector2Int checkPos = newPosition + new Vector2Int(x, y);
                    if (puzzleGrid[checkPos.x, checkPos.y])
                    {
                        canMove = false;
                        break;
                    }
                }
                if (!canMove) break;
            }
            
            // Restore ingredient to grid
            PlaceIngredientOnGrid(placement);
            
            return canMove;
        }
        
        /// <summary>
        /// Check if target ingredient has reached the exit
        /// </summary>
        private bool HasReachedExit(PlacedPuzzleIngredient placement)
        {
            if (!placement.isTarget) return false;
            
            // Check if any part of the target ingredient covers the exit position
            for (int x = 0; x < placement.size.x; x++)
            {
                for (int y = 0; y < placement.size.y; y++)
                {
                    Vector2Int checkPos = placement.position + new Vector2Int(x, y);
                    if (checkPos == exitPosition)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Complete the minigame successfully
        /// </summary>
        private void CompleteMinigame()
        {
            isMinigameActive = false;
            float timeElapsed = Time.time - startTime;
            
            var result = new ExtractionResult
            {
                success = true,
                timeElapsed = timeElapsed,
                moveCount = moveCount,
                efficiencyScore = CalculateEfficiencyScore(timeElapsed, moveCount),
                extractedIngredient = targetIngredient
            };
            
            Debug.Log($"🎉 === EXTRACTION MINIGAME COMPLETED ===");
            Debug.Log($"✅ Successfully extracted {targetIngredient.ItemName}!");
            Debug.Log($"⏱️ Time: {timeElapsed:F1}s");
            Debug.Log($"🔢 Moves: {moveCount}");
            Debug.Log($"⭐ Efficiency: {result.efficiencyScore:F1}%");
            
            // Award skill points based on efficiency
            var skillTree = FindFirstObjectByType<AlchemySkillTree>();
            if (skillTree != null)
            {
                int pointsAwarded = Mathf.RoundToInt(result.efficiencyScore / 25f); // 1 point per 25% efficiency
                if (pointsAwarded > 0)
                {
                    skillTree.AwardSkillPoints(pointsAwarded, "Extraction minigame");
                }
            }
        }
        
        /// <summary>
        /// Calculate efficiency score based on performance
        /// </summary>
        private float CalculateEfficiencyScore(float timeElapsed, int moves)
        {
            // Optimal performance targets
            float targetTime = 30f; // 30 seconds
            int targetMoves = 10;    // 10 moves
            
            // Calculate score components
            float timeScore = Mathf.Clamp01(targetTime / timeElapsed) * 50f;
            float moveScore = Mathf.Clamp01((float)targetMoves / moves) * 50f;
            
            return timeScore + moveScore;
        }
        
        /// <summary>
        /// Clear the puzzle grid
        /// </summary>
        private void ClearPuzzleGrid()
        {
            for (int x = 0; x < puzzleWidth; x++)
            {
                for (int y = 0; y < puzzleHeight; y++)
                {
                    puzzleGrid[x, y] = false;
                }
            }
        }
        
        /// <summary>
        /// Check if grid position is valid
        /// </summary>
        private bool IsValidGridPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < puzzleWidth &&
                   position.y >= 0 && position.y < puzzleHeight;
        }
        
        /// <summary>
        /// Get all movable ingredients in the puzzle
        /// </summary>
        public List<PlacedPuzzleIngredient> GetMovableIngredients()
        {
            return puzzleIngredients.Where(p => p.isMovable).ToList();
        }
        
        /// <summary>
        /// Get possible moves for an ingredient
        /// </summary>
        public List<SlideDirection> GetPossibleMoves(Ingredient ingredient)
        {
            var possibleMoves = new List<SlideDirection>();
            
            if (!ingredientLookup.ContainsKey(ingredient))
                return possibleMoves;
            
            var placement = ingredientLookup[ingredient];
            if (!placement.isMovable)
                return possibleMoves;
            
            // Check each direction
            var directions = new SlideDirection[] { SlideDirection.Up, SlideDirection.Down, SlideDirection.Left, SlideDirection.Right };
            
            foreach (var direction in directions)
            {
                if ((placement.allowedDirections & direction) != 0)
                {
                    Vector2Int moveVector = GetMoveVector(direction);
                    Vector2Int newPosition = placement.position + moveVector;
                    
                    if (CanMoveIngredientTo(placement, newPosition))
                    {
                        possibleMoves.Add(direction);
                    }
                }
            }
            
            return possibleMoves;
        }
        
        /// <summary>
        /// Debug visualization of puzzle state
        /// </summary>
        [ContextMenu("Debug Puzzle State")]
        public void DebugPuzzleState()
        {
            Debug.Log("🧩 === EXTRACTION PUZZLE STATE ===");
            Debug.Log($"Grid size: {puzzleWidth}x{puzzleHeight}");
            Debug.Log($"Target: {targetIngredient?.ItemName ?? "None"}");
            Debug.Log($"Exit position: {exitPosition}");
            Debug.Log($"Active: {isMinigameActive}");
            Debug.Log($"Moves: {moveCount}");
            
            if (puzzleIngredients.Count > 0)
            {
                Debug.Log("Puzzle ingredients:");
                foreach (var ingredient in puzzleIngredients)
                {
                    string type = ingredient.isTarget ? "TARGET" : "BLOCKER";
                    Debug.Log($"  - {ingredient.ingredient.ItemName} ({type}) at {ingredient.position}, moves: {ingredient.allowedDirections}");
                }
            }
            
            // Visual grid representation
            Debug.Log("Grid layout:");
            for (int y = puzzleHeight - 1; y >= 0; y--)
            {
                string row = $"Y={y}: ";
                for (int x = 0; x < puzzleWidth; x++)
                {
                    row += puzzleGrid[x, y] ? "[#]" : "[ ]";
                }
                Debug.Log(row);
            }
        }
        
        /// <summary>
        /// Test the extraction minigame
        /// </summary>
        [ContextMenu("Test Extraction Minigame")]
        public void TestExtractionMinigame()
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager?.availableIngredients.Count > 0)
            {
                var testIngredient = gridManager.availableIngredients[0];
                StartExtractionMinigame(testIngredient);
            }
            else
            {
                Debug.LogError("No ingredients available for testing");
            }
        }
    }
}