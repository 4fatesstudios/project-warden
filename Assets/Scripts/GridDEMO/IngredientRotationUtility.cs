using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Utility class for rotating ingredient shapes at runtime 
    /// Provides methods to rotate IngredientShapeData for placement preview and validation
    /// </summary>
    public static class IngredientRotationUtility
    {
        /// <summary>
        /// Rotate ingredient shape data by specified degrees (must be multiple of 90)
        /// </summary>
        /// <param name="shapeData">The original shape data</param>
        /// <param name="degrees">Degrees to rotate (90, 180, 270, etc.)</param>
        /// <returns>New shape data with rotated offsets</returns>
        public static IngredientShapeData RotateShape(IngredientShapeData shapeData, int degrees)
        {
            if (shapeData == null || !shapeData.rotatable) return shapeData;
            
            // Create a copy of the shape data
            var rotatedShape = new IngredientShapeData();
            
            // Copy basic properties
            rotatedShape.id = shapeData.id + $"_rotated_{degrees}";
            rotatedShape.icon = shapeData.icon;
            rotatedShape.pivot = shapeData.pivot;
            rotatedShape.expansionRotatesWithIngredient = shapeData.expansionRotatesWithIngredient;
            rotatedShape.rotatable = shapeData.rotatable;
            rotatedShape.expansionColor = shapeData.expansionColor;
            
            // Rotate the offsets
            rotatedShape.occupiedOffsets = RotateOffsets(shapeData.occupiedOffsets, degrees);
            
            if (shapeData.expansionRotatesWithIngredient)
            {
                rotatedShape.expansionOffsets = RotateOffsets(shapeData.expansionOffsets, degrees);
            }
            else
            {
                rotatedShape.expansionOffsets = shapeData.expansionOffsets;
            }
            
            return rotatedShape;
        }
        
        /// <summary>
        /// Rotate an array of Vector2Int offsets by specified degrees
        /// </summary>
        /// <param name="offsets">Original offsets</param>
        /// <param name="degrees">Degrees to rotate (must be multiple of 90)</param>
        /// <returns>Rotated offsets</returns>
        public static Vector2Int[] RotateOffsets(Vector2Int[] offsets, int degrees)
        {
            if (offsets == null || offsets.Length == 0) return offsets;
            
            var rotated = new Vector2Int[offsets.Length];
            int rotations = ((degrees % 360) / 90) % 4;
            if (rotations < 0) rotations += 4;
            
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int offset = offsets[i];
                
                for (int r = 0; r < rotations; r++)
                {
                    // Rotate 90 degrees clockwise: (x, y) -> (y, -x)
                    offset = new Vector2Int(offset.y, -offset.x);
                }
                
                rotated[i] = offset;
            }
            
            return rotated;
        }
        
        /// <summary>
        /// Get the rotated offsets for a specific rotation amount
        /// </summary>
        /// <param name="originalOffsets">Original offsets</param>
        /// <param name="rotation">Rotation in degrees</param>
        /// <returns>Rotated offsets</returns>
        public static Vector2Int[] GetRotatedOffsets(Vector2Int[] originalOffsets, int rotation)
        {
            return RotateOffsets(originalOffsets, rotation);
        }
        
        /// <summary>
        /// Normalize rotation to be between 0 and 270 degrees
        /// </summary>
        /// <param name="rotation">Input rotation</param>
        /// <returns>Normalized rotation (0, 90, 180, or 270)</returns>
        public static int NormalizeRotation(int rotation)
        {
            rotation = rotation % 360;
            if (rotation < 0) rotation += 360;
            return (rotation / 90) * 90;
        }
        
        /// <summary>
        /// Test method to validate rotation functionality
        /// </summary>
        public static void TestRotation()
        {
            // Test rotation normalization
            Debug.Log($"Rotation test: 90° -> {NormalizeRotation(90)}°");
            Debug.Log($"Rotation test: 450° -> {NormalizeRotation(450)}°");
            Debug.Log($"Rotation test: -90° -> {NormalizeRotation(-90)}°");
            
            // Test offset rotation
            Vector2Int[] testOffsets = { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) };
            var rotated90 = RotateOffsets(testOffsets, 90);
            
            Debug.Log("Original offsets: " + string.Join(", ", testOffsets));
            Debug.Log("Rotated 90°: " + string.Join(", ", rotated90));
        }
    }
}