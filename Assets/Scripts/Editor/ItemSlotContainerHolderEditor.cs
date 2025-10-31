using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(ItemSlotContainerHolder))]
public class ItemSlotContainerHolderEditor : Editor
{
    private bool showAdvancedOptions = false;
    private int defaultQuantity = 5;
    private bool includeIngredients = true;
    private bool includePotions = true;
    private bool includeTrinkets = true;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Auto-Population Tools", EditorStyles.boldLabel);
        
        // Advanced options foldout
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            
            defaultQuantity = EditorGUILayout.IntField("Default Quantity", defaultQuantity);
            defaultQuantity = Mathf.Max(1, defaultQuantity);
            
            EditorGUILayout.LabelField("Item Types to Include:", EditorStyles.boldLabel);
            includeIngredients = EditorGUILayout.Toggle("Ingredients", includeIngredients);
            includePotions = EditorGUILayout.Toggle("Potions", includePotions);
            includeTrinkets = EditorGUILayout.Toggle("Trinkets", includeTrinkets);
            
            EditorGUI.indentLevel--;
        }
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Populate from Resources", GUILayout.Height(30)))
        {
            PopulateFromResources();
        }
        
        if (GUILayout.Button("Clear All Items", GUILayout.Height(30)))
        {
            ClearAllItems();
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add Sample Items", GUILayout.Height(25)))
        {
            AddSampleItems();
        }
        
        if (GUILayout.Button("Sort Items by Name", GUILayout.Height(25)))
        {
            SortItemsByName();
        }
        
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Add some helpful info
        EditorGUILayout.HelpBox(
            "• 'Populate from Resources' will scan /Assets/Resources/Items/ and add all found items\n" +
            "• 'Clear All Items' will remove all items from the arrays\n" +
            "• 'Add Sample Items' adds a few test items for quick testing\n" +
            "• 'Sort Items by Name' alphabetically sorts the current item list\n" +
            "• Use Advanced Options to customize quantity and filter item types",
            MessageType.Info);
            
        // Show current stats
        SerializedProperty itemsProperty = serializedObject.FindProperty("initialItems");
        if (itemsProperty.arraySize > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Current Items: {itemsProperty.arraySize}", EditorStyles.helpBox);
        }
    }
    
    private void PopulateFromResources()
    {
        ItemSlotContainerHolder holder = (ItemSlotContainerHolder)target;
        
        // Load all items from Resources/Items folder
        List<Item> items = new List<Item>();
        List<int> quantities = new List<int>();
        
        // Load items from selected subfolders in Resources/Items
        if (includeIngredients)
        {
            Item[] ingredientItems = Resources.LoadAll<Item>("Items/Ingredients");
            foreach (Item item in ingredientItems)
            {
                if (item != null)
                {
                    items.Add(item);
                    quantities.Add(GetQuantityForItem(item));
                }
            }
        }
        
        if (includePotions)
        {
            Item[] potionItems = Resources.LoadAll<Item>("Items/Potions");
            foreach (Item item in potionItems)
            {
                if (item != null)
                {
                    items.Add(item);
                    quantities.Add(GetQuantityForItem(item));
                }
            }
        }
        
        if (includeTrinkets)
        {
            Item[] trinketItems = Resources.LoadAll<Item>("Items/Trinkets");
            foreach (Item item in trinketItems)
            {
                if (item != null)
                {
                    items.Add(item);
                    quantities.Add(GetQuantityForItem(item));
                }
            }
        }
        
        // Also load any items directly in the Items folder
        Item[] rootItems = Resources.LoadAll<Item>("Items");
        foreach (Item item in rootItems)
        {
            if (item != null && !items.Contains(item))
            {
                items.Add(item);
                quantities.Add(GetQuantityForItem(item));
            }
        }
        
        // Update the serialized properties
        SerializedProperty itemsProperty = serializedObject.FindProperty("initialItems");
        SerializedProperty quantitiesProperty = serializedObject.FindProperty("initialQuantities");
        
        itemsProperty.arraySize = items.Count;
        quantitiesProperty.arraySize = quantities.Count;
        
        for (int i = 0; i < items.Count; i++)
        {
            itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            quantitiesProperty.GetArrayElementAtIndex(i).intValue = quantities[i];
        }
        
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log($"Successfully populated inventory with {items.Count} items from Resources folder!");
        
        // Mark the object as dirty for saving
        EditorUtility.SetDirty(holder);
    }
    
    private void AddSampleItems()
    {
        ItemSlotContainerHolder holder = (ItemSlotContainerHolder)target;
        
        // Add a few sample items for testing
        List<Item> sampleItems = new List<Item>();
        List<int> sampleQuantities = new List<int>();
        
        // Try to load some common items
        Item fireEssence = Resources.Load<Item>("Items/Ingredients/FireEssence");
        Item earthShard = Resources.Load<Item>("Items/Ingredients/EarthShard");
        Item testPotion = Resources.Load<Item>("Items/Potions/NewPotion");
        
        if (fireEssence != null) { sampleItems.Add(fireEssence); sampleQuantities.Add(10); }
        if (earthShard != null) { sampleItems.Add(earthShard); sampleQuantities.Add(15); }
        if (testPotion != null) { sampleItems.Add(testPotion); sampleQuantities.Add(3); }
        
        if (sampleItems.Count == 0)
        {
            Debug.LogWarning("No sample items found in Resources folder!");
            return;
        }
        
        // Update the serialized properties
        SerializedProperty itemsProperty = serializedObject.FindProperty("initialItems");
        SerializedProperty quantitiesProperty = serializedObject.FindProperty("initialQuantities");
        
        itemsProperty.arraySize = sampleItems.Count;
        quantitiesProperty.arraySize = sampleQuantities.Count;
        
        for (int i = 0; i < sampleItems.Count; i++)
        {
            itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = sampleItems[i];
            quantitiesProperty.GetArrayElementAtIndex(i).intValue = sampleQuantities[i];
        }
        
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log($"Added {sampleItems.Count} sample items to inventory!");
        
        EditorUtility.SetDirty(holder);
    }
    
    private void SortItemsByName()
    {
        SerializedProperty itemsProperty = serializedObject.FindProperty("initialItems");
        SerializedProperty quantitiesProperty = serializedObject.FindProperty("initialQuantities");
        
        // Create lists to sort
        List<(Item item, int quantity)> itemPairs = new List<(Item, int)>();
        
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            Item item = itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue as Item;
            int quantity = quantitiesProperty.GetArrayElementAtIndex(i).intValue;
            
            if (item != null)
            {
                itemPairs.Add((item, quantity));
            }
        }
        
        // Sort by item name
        itemPairs.Sort((a, b) => string.Compare(a.item.ItemName, b.item.ItemName));
        
        // Update arrays with sorted data
        itemsProperty.arraySize = itemPairs.Count;
        quantitiesProperty.arraySize = itemPairs.Count;
        
        for (int i = 0; i < itemPairs.Count; i++)
        {
            itemsProperty.GetArrayElementAtIndex(i).objectReferenceValue = itemPairs[i].item;
            quantitiesProperty.GetArrayElementAtIndex(i).intValue = itemPairs[i].quantity;
        }
        
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log($"Sorted {itemPairs.Count} items alphabetically!");
        
        EditorUtility.SetDirty(target);
    }
    
    private void ClearAllItems()
    {
        ItemSlotContainerHolder holder = (ItemSlotContainerHolder)target;
        
        SerializedProperty itemsProperty = serializedObject.FindProperty("initialItems");
        SerializedProperty quantitiesProperty = serializedObject.FindProperty("initialQuantities");
        
        itemsProperty.arraySize = 0;
        quantitiesProperty.arraySize = 0;
        
        serializedObject.ApplyModifiedProperties();
        
        Debug.Log("Cleared all items from inventory!");
        
        EditorUtility.SetDirty(holder);
    }
    
    private int GetQuantityForItem(Item item)
    {
        // Use default quantity if Advanced Options is not expanded, otherwise use smart defaults
        if (!showAdvancedOptions)
        {
            return defaultQuantity;
        }
        
        // Smart quantity assignment based on item properties
        string itemName = item.name.ToLower();
        
        // Base quantity on rarity
        int baseQuantity = item.ItemRarity switch
        {
            Rarity.Common => 15,
            Rarity.Uncommon => 10,
            Rarity.Rare => 5,
            Rarity.Epic => 3,
            Rarity.Mythic => 1,
            _ => defaultQuantity
        };
        
        // Adjust based on item type
        if (itemName.Contains("essence") || itemName.Contains("herb"))
            return Mathf.Max(baseQuantity + 5, 10); // Essences and herbs are common
        else if (itemName.Contains("potion") || itemName.Contains("elixir"))
            return Mathf.Max(baseQuantity - 2, 1);  // Potions are more valuable
        else if (itemName.Contains("trinket") || itemName.Contains("artifact"))
            return Mathf.Max(baseQuantity - 3, 1);  // Trinkets are rare
        else if (itemName.Contains("test"))
            return 99; // Test items for debugging
        else
            return baseQuantity;
    }
}