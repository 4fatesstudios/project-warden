using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum RefiningType
    {
        None,
        Grinding,
        Distilling,
        Roasting
    }
    
    public enum RefiningResult
    {
        Success,
        Failed,
        Lost,
        CriticalSuccess
    }
    
    public enum RefiningDifficulty
    {
        Trivial,
        Easy,
        Moderate,
        Hard,
        Expert,
        Impossible
    }
}