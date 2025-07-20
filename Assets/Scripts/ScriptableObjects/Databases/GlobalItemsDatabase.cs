using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    public class GlobalItemsDatabase : ScriptableObject {
        [SerializeField] private List<ItemDatabase<Item>> itemDatabases;
        
        
    }
}