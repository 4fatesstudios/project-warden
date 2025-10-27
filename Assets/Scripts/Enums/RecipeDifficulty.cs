namespace FourFatesStudios.ProjectWarden.Enums
{
    /// <summary>
    /// Difficulty levels for alchemy recipes affecting success thresholds and requirements
    /// </summary>
    public enum RecipeDifficulty
    {
        /// <summary>
        /// Beginner recipes with relaxed success criteria and helpful hints
        /// </summary>
        Beginner = 0,
        
        /// <summary>
        /// Standard difficulty recipes for normal gameplay
        /// </summary>
        Standard = 1,
        
        /// <summary>
        /// Advanced recipes requiring better spatial arrangement and efficiency
        /// </summary>
        Advanced = 2,
        
        /// <summary>
        /// Master level recipes with strict requirements and complex patterns
        /// </summary>
        Master = 3
    }
}