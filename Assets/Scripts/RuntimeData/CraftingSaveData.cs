using UnityEngine;
using System;
using System.Collections.Generic;

namespace FourFatesStudios.ProjectWarden.RuntimeData
{
    [Serializable]              // required for JsonUtility
    public class CraftingSaveData
    {
        public List<SavedPotion> potions = new();
        public List<SavedComponent> components = new();
    }

    [Serializable]
    public class SavedPotion
    {
        public string name;
        public bool upgraded;
        public List<string> effectNames = new();   // store PotionEffect asset names
    }

    [Serializable]
    public class SavedComponent
    {
        public string name;
        public string base1Name;   // Ingredient asset names
        public string base2Name;
    }
}
