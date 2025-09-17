using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visual Configuration")]
        public GameObject cellPrefab;
        public Material baseCellMaterial;
        public Material highlightMaterial;
        public Material occupiedMaterial;
        
        [Header("Grid Lines")]
        public bool showGridLines = true;
        public Color gridLineColor = Color.gray;
        public float gridLineWidth = 0.02f;
        
        [Header("Particle Effects")]
        public GameObject placementEffectPrefab;
        public GameObject removalEffectPrefab;
        public GameObject aspectReactionEffectPrefab;
        
        private GridGameManager gridManager;
        private GameObject[,] cellVisuals;
        private MeshRenderer[,] cellRenderers;
        private List<LineRenderer> gridLines = new List<LineRenderer>();
        private ParticleSystem[] particleSystems;
        
        private void Awake()
        {
            gridManager = GetComponent<GridGameManager>();
        }
        
        private void Start()
        {
            CreateGridVisuals();
            CreateGridLines();
            SetupParticleEffects();
        }
        
        private void CreateGridVisuals()
        {
            // Prevent duplicate grid creation - preserve existing grid visuals
            if (cellVisuals != null && cellVisuals.Length > 0)
            {
                Debug.Log("🔄 GridVisualizer: Grid visuals already exist, preserving them.");
                return;
            }
            
            cellVisuals = new GameObject[gridManager.gridWidth, gridManager.gridHeight];
            cellRenderers = new MeshRenderer[gridManager.gridWidth, gridManager.gridHeight];
            
            // Create cell prefab if none provided
            if (cellPrefab == null)
            {
                cellPrefab = CreateDefaultCellPrefab();
            }
            
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    Vector3 position = gridManager.GridToWorldPosition(new Vector2Int(x, y));
                    GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                    cellObj.name = $"Cell_{x}_{y}";
                    
                    cellVisuals[x, y] = cellObj;
                    cellRenderers[x, y] = cellObj.GetComponent<MeshRenderer>();
                    
                    // Add collider for mouse interaction
                    if (cellObj.GetComponent<Collider>() == null)
                    {
                        BoxCollider collider = cellObj.AddComponent<BoxCollider>();
                        collider.size = new Vector3(gridManager.cellSize, 0.1f, gridManager.cellSize);
                    }
                }
            }
        }
        
        private GameObject CreateDefaultCellPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.transform.localScale = new Vector3(gridManager.cellSize * 0.95f, 0.05f, gridManager.cellSize * 0.95f);
            
            // Create material if none provided
            if (baseCellMaterial == null)
            {
                baseCellMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                baseCellMaterial.color = Color.white;
            }
            
            prefab.GetComponent<MeshRenderer>().material = baseCellMaterial;
            return prefab;
        }
        
        private void CreateGridLines()
        {
            if (!showGridLines) return;
            
            // Prevent duplicate grid line creation
            if (gridLines.Count > 0)
            {
                Debug.Log("🔄 GridVisualizer: Grid lines already exist, preserving them.");
                return;
            }
            
            // Vertical lines
            for (int x = 0; x <= gridManager.gridWidth; x++)
            {
                GameObject lineObj = new GameObject($"GridLine_V_{x}");
                lineObj.transform.parent = transform;
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                SetupLineRenderer(lr);
                
                Vector3 start = gridManager.gridStartPosition + new Vector3(x * gridManager.cellSize, 0.01f, 0);
                Vector3 end = gridManager.gridStartPosition + new Vector3(x * gridManager.cellSize, 0.01f, gridManager.gridHeight * gridManager.cellSize);
                
                lr.positionCount = 2;
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
                
                gridLines.Add(lr);
            }
            
            // Horizontal lines
            for (int y = 0; y <= gridManager.gridHeight; y++)
            {
                GameObject lineObj = new GameObject($"GridLine_H_{y}");
                lineObj.transform.parent = transform;
                
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                SetupLineRenderer(lr);
                
                Vector3 start = gridManager.gridStartPosition + new Vector3(0, 0.01f, y * gridManager.cellSize);
                Vector3 end = gridManager.gridStartPosition + new Vector3(gridManager.gridWidth * gridManager.cellSize, 0.01f, y * gridManager.cellSize);
                
                lr.positionCount = 2;
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
                
                gridLines.Add(lr);
            }
        }
        
        private void SetupLineRenderer(LineRenderer lr)
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.material.color = gridLineColor;
            lr.startWidth = gridLineWidth;
            lr.endWidth = gridLineWidth;
            lr.useWorldSpace = true;
            lr.sortingOrder = 1;
        }
        
        private void SetupParticleEffects()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
        
        public void RefreshGrid()
        {
            Debug.Log("🔄 GridVisualizer.RefreshGrid() - Updating all cell visuals");
            GridCell[,] cells = gridManager.GetAllCells();
            
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    UpdateCellVisual(x, y, cells[x, y]);
                }
            }
            
            Debug.Log("✅ Grid refresh complete - all cells updated to match their logical state");
        }
        
        public void UpdateHighlight(Vector2Int hoveredCell, Ingredient selectedIngredient)
        {
            // Clear all highlights first
            ClearHighlights();
            
            if (hoveredCell.x < 0 || hoveredCell.y < 0 || selectedIngredient == null)
                return;
            
            // Highlight cells for the selected ingredient using shape data
            bool canPlace = gridManager.CanPlaceIngredient(selectedIngredient, hoveredCell);
            
            // Get the cells that would be occupied by this ingredient
            var cellsToHighlight = gridManager.GetIngredientCells(selectedIngredient, hoveredCell);
            
            foreach (var cellPos in cellsToHighlight)
            {
                if (cellPos.x >= 0 && cellPos.x < gridManager.gridWidth && 
                    cellPos.y >= 0 && cellPos.y < gridManager.gridHeight)
                {
                    GridCell cell = gridManager.GetCell(cellPos.x, cellPos.y);
                    cell.SetHighlighted(true, canPlace);
                    UpdateCellVisual(cellPos.x, cellPos.y, cell);
                }
            }
        }
        
        public void ClearHighlights()
        {
            GridCell[,] cells = gridManager.GetAllCells();
            
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    if (cells[x, y].IsHighlighted)
                    {
                        cells[x, y].SetHighlighted(false);
                        UpdateCellVisual(x, y, cells[x, y]);
                    }
                }
            }
        }
        
        private void UpdateCellVisual(int x, int y, GridCell cell)
        {
            if (cellRenderers[x, y] == null) return;
            
            Material materialToUse = GetMaterialForCell(cell);
            cellRenderers[x, y].material = materialToUse;
            
            // Update material color using MaterialPropertyBlock
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_BaseColor", cell.CellColor);
            
            // Only add emission for highlights, not for occupied cells
            // This keeps occupied cells subtle while ingredient models provide the color
            if (cell.VisualState == CellVisualState.ValidHighlight || 
                cell.VisualState == CellVisualState.InvalidHighlight)
            {
                // Add subtle emission for highlights
                Color emissionColor = cell.CellColor * 0.3f;
                mpb.SetColor("_EmissionColor", emissionColor);
                mpb.SetFloat("_EmissionIntensity", 0.5f);
            }
            else
            {
                // Clear emission for empty and occupied cells
                mpb.SetColor("_EmissionColor", Color.black);
                mpb.SetFloat("_EmissionIntensity", 0f);
            }
            
            cellRenderers[x, y].SetPropertyBlock(mpb);
            
            // Animate height based on state
            AnimateCellHeight(cellVisuals[x, y], cell);
        }
        
        private Material GetMaterialForCell(GridCell cell)
        {
            switch (cell.VisualState)
            {
                case CellVisualState.ValidHighlight:
                case CellVisualState.InvalidHighlight:
                    return highlightMaterial ?? baseCellMaterial;
                    
                case CellVisualState.Occupied:
                    return occupiedMaterial ?? baseCellMaterial;
                    
                case CellVisualState.Obstacle:
                    return baseCellMaterial; // Use base material for obstacles, color handled by MaterialPropertyBlock
                    
                default:
                    return baseCellMaterial;
            }
        }
        
        private void AnimateCellHeight(GameObject cellObj, GridCell cell)
        {
            if (cellObj == null) return;
            
            float targetHeight = 0.05f;
            
            switch (cell.VisualState)
            {
                case CellVisualState.ValidHighlight:
                    targetHeight = 0.1f;
                    break;
                case CellVisualState.InvalidHighlight:
                    targetHeight = 0.08f;
                    break;
                case CellVisualState.Occupied:
                    targetHeight = 0.15f + (cell.CellIntensity * 0.1f);
                    break;
                case CellVisualState.Obstacle:
                    targetHeight = 0.12f; // Obstacles are slightly raised
                    break;
            }
            
            Vector3 currentScale = cellObj.transform.localScale;
            Vector3 targetScale = new Vector3(currentScale.x, targetHeight, currentScale.z);
            
            // Simple lerp animation - could be enhanced with DOTween or similar
            cellObj.transform.localScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * 10f);
        }
        
        public void PlayPlacementEffect(Vector2Int gridPosition, Ingredient ingredient)
        {
            if (placementEffectPrefab == null) return;
            
            Vector3 worldPosition = gridManager.GridToWorldPosition(gridPosition);
            GameObject effect = Instantiate(placementEffectPrefab, worldPosition, Quaternion.identity);
            
            // Customize effect based on ingredient
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = GetAspectColor(ingredient.IngredientAspect);
            }
            
            Destroy(effect, 2f);
        }
        
        public void PlayRemovalEffect(Vector2Int gridPosition)
        {
            if (removalEffectPrefab == null) return;
            
            Vector3 worldPosition = gridManager.GridToWorldPosition(gridPosition);
            GameObject effect = Instantiate(removalEffectPrefab, worldPosition, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Scorch: return Color.red;
                case Aspect.Frigid: return Color.cyan;
                case Aspect.Arc: return Color.yellow;
                case Aspect.Caustic: return new Color(0.5f, 0.3f, 0.1f); // Brown
                case Aspect.Corporeal: return Color.gray;
                case Aspect.Divine: return Color.white;
                default: return Color.gray;
            }
        }
    }
}