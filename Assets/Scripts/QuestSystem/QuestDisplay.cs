using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using FourFatesStudios.ProjectWarden.QuestSystem;

public class QuestDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Transform questContainer; // Parent object for quest UI elements
    [SerializeField] private GameObject questPrefab;   // Prefab for individual quest display
    [SerializeField] private ScrollRect scrollRect;    // Optional: for scrolling through quests
    
    [Header("Quest System")]
    [SerializeField] private QuestManager questManager; // Reference to quest manager
    
    private List<GameObject> activeQuestDisplays = new List<GameObject>();
    private List<QuestSO> lastKnownQuests = new List<QuestSO>(); // Track what we last displayed
    
    void Start()
    {
        // Find QuestManager if not assigned
        if (questManager == null)
        {
            questManager = FindFirstObjectByType<QuestManager>();
        }
        
        // Subscribe to quest changes event
        QuestManager.OnQuestsChanged += RefreshQuestDisplay;
        
        // Subscribe to task completion events for instant updates
        QuestTask.OnTaskCompleted += OnTaskCompleted;
        
        // Initial display update
        UpdateQuestDisplay();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        QuestManager.OnQuestsChanged -= RefreshQuestDisplay;
        QuestTask.OnTaskCompleted -= OnTaskCompleted;
    }

    /// <summary>
    /// Called when any task is completed - triggers instant UI update
    /// </summary>
    private void OnTaskCompleted(QuestTask completedTask)
    {
        Debug.Log($"QuestDisplay: Task completed, refreshing UI - {completedTask.TaskName}");
        RefreshQuestDisplay();
    }

    // Remove or comment out the Update method since we're using events now
    /*
    void Update()
    {
        // Only update if the quest list has actually changed
        // You can remove this entirely if you implement the event system in QuestManager
        if (HasQuestListChanged())
        {
            UpdateQuestDisplay();
        }
    }
    */
    
    /// <summary>
    /// Checks if the quest list has changed since last update
    /// </summary>
    private bool HasQuestListChanged()
    {
        if (questManager == null) return false;
        
        List<QuestSO> currentQuests = questManager.GetActiveQuests();
        
        // Check if count is different
        if (currentQuests.Count != lastKnownQuests.Count)
            return true;
            
        // Check if any quest is different
        for (int i = 0; i < currentQuests.Count; i++)
        {
            if (currentQuests[i] != lastKnownQuests[i])
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Updates the quest display to show all currently active quests
    /// </summary>
    public void UpdateQuestDisplay()
    {
        if (questManager == null || questContainer == null) return;
        
        // Get current quests
        List<QuestSO> activeQuests = questManager.GetActiveQuests();
        
        // Clear existing displays
        ClearQuestDisplays();
        
        // Create UI elements for each active quest
        foreach (QuestSO quest in activeQuests)
        {
            CreateQuestDisplayElement(quest);
        }
        
        // Update our tracking list
        lastKnownQuests.Clear();
        lastKnownQuests.AddRange(activeQuests);
    }
    
    /// <summary>
    /// Creates a UI element for a single quest
    /// </summary>
    private void CreateQuestDisplayElement(QuestSO quest)
    {
        if (questPrefab == null) return;
        
        // Instantiate quest display prefab
        GameObject questDisplay = Instantiate(questPrefab, questContainer);
        activeQuestDisplays.Add(questDisplay);
        
        // Configure the quest display
        ConfigureQuestDisplay(questDisplay, quest);
    }
    
    /// <summary>
    /// Configures a quest display element with quest data
    /// </summary>
    private void ConfigureQuestDisplay(GameObject questDisplay, QuestSO quest)
    {
        Debug.Log($"Configuring quest display for: {quest.questName}");
        
        // Find and set quest name
        TextMeshProUGUI titleText = questDisplay.transform.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = quest.questName;
            Debug.Log($"Set title text: {quest.questName}");
        }
        else
        {
            Debug.LogWarning("QuestTitle not found in prefab");
        }
        
        // Find and set quest description
        TextMeshProUGUI descriptionText = questDisplay.transform.Find("QuestDescription")?.GetComponent<TextMeshProUGUI>();
        if (descriptionText != null)
        {
            descriptionText.text = quest.description;
            Debug.Log($"Set description text: {quest.description}");
        }
        else
        {
            Debug.LogWarning("QuestDescription not found in prefab");
        }
        
        // Find tasks container and display tasks
        Transform tasksContainer = questDisplay.transform.Find("TasksContainer");
        if (tasksContainer != null)
        {
            Debug.Log($"Found TasksContainer, displaying {quest.tasks.Count} tasks");
            DisplayQuestTasks(tasksContainer, quest.tasks);
        }
        else
        {
            Debug.LogWarning("TasksContainer not found in prefab - tasks will not be displayed");
            Debug.Log("Available children in prefab:");
            for (int i = 0; i < questDisplay.transform.childCount; i++)
            {
                Debug.Log($"  Child {i}: {questDisplay.transform.GetChild(i).name}");
            }
        }
        
        // Find and set progress text
        TextMeshProUGUI progressText = questDisplay.transform.Find("QuestProgress")?.GetComponent<TextMeshProUGUI>();
        if (progressText != null)
        {
            int completedTasks = GetCompletedTaskCount(quest.tasks);
            progressText.text = $"Progress: {completedTasks}/{quest.tasks.Count}";
            Debug.Log($"Set progress text: {completedTasks}/{quest.tasks.Count}");
        }
        else
        {
            Debug.LogWarning("QuestProgress not found in prefab");
        }
        
        // Optional: Add click functionality
        Button questButton = questDisplay.GetComponent<Button>();
        if (questButton != null)
        {
            questButton.onClick.AddListener(() => OnQuestClicked(quest));
        }
    }
    
    /// <summary>
    /// Displays quest tasks in the tasks container
    /// </summary>
    private void DisplayQuestTasks(Transform tasksContainer, List<QuestTask> tasks)
    {
        Debug.Log($"DisplayQuestTasks called with {tasks.Count} tasks in container: {tasksContainer.name}");
        
        // Clear existing task displays
        foreach (Transform child in tasksContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create a simple text display for each task
        for (int i = 0; i < tasks.Count; i++)
        {
            QuestTask task = tasks[i];
            Debug.Log($"Creating task display for: {task.TaskName}");
            
            GameObject taskObject = new GameObject($"Task_{i}", typeof(RectTransform));
            taskObject.transform.SetParent(tasksContainer, false);
            
            // Add and configure TextMeshPro component
            TextMeshProUGUI taskText = taskObject.AddComponent<TextMeshProUGUI>();
            taskText.text = $"{(task.IsCompleted ? "✓" : "○")} {task.TaskName}";
            taskText.fontSize = 12;
            taskText.color = task.IsCompleted ? Color.green : Color.white;
            taskText.fontStyle = FontStyles.Normal;
            taskText.alignment = TextAlignmentOptions.Left;
            taskText.textWrappingMode = TextWrappingModes.Normal;
            
            Debug.Log($"Task text set to: {taskText.text}");
            
            // Properly configure rect transform for layout
            RectTransform taskRect = taskObject.GetComponent<RectTransform>();
            taskRect.anchorMin = new Vector2(0, 1);
            taskRect.anchorMax = new Vector2(0, 1);
            taskRect.pivot = new Vector2(0, 1);
            taskRect.sizeDelta = new Vector2(180, 25); // Leave space for button
            taskRect.anchoredPosition = new Vector2(10, -i * 30); // Vertical spacing of 30 units
            
            // Configure the TextMeshPro RectTransform
            RectTransform textRect = taskText.rectTransform;
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.sizeDelta = new Vector2(180, 25);
            textRect.anchoredPosition = Vector2.zero;
            
            // Add completion button for incomplete tasks
            if (!task.IsCompleted)
            {
                GameObject buttonObject = new GameObject("CompleteButton", typeof(RectTransform));
                buttonObject.transform.SetParent(tasksContainer, false);
                
                Button completeButton = buttonObject.AddComponent<Button>();
                Image buttonImage = buttonObject.AddComponent<Image>();
                buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.7f); // Semi-transparent green
                
                // Button positioning
                RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0, 1);
                buttonRect.anchorMax = new Vector2(0, 1);
                buttonRect.pivot = new Vector2(0, 1);
                buttonRect.sizeDelta = new Vector2(60, 20);
                buttonRect.anchoredPosition = new Vector2(190, -i * 30 - 2); // Next to the task text
                
                // Button text
                GameObject buttonTextObj = new GameObject("Text", typeof(RectTransform));
                buttonTextObj.transform.SetParent(buttonObject.transform, false);
                TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "Complete";
                buttonText.fontSize = 10;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
                
                RectTransform buttonTextRect = buttonText.rectTransform;
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.sizeDelta = Vector2.zero;
                buttonTextRect.anchoredPosition = Vector2.zero;
                
                // Button functionality - capture the task in a closure
                QuestTask capturedTask = task;
                completeButton.onClick.AddListener(() => {
                    capturedTask.CompleteTask();
                    Debug.Log($"Completed task via UI button: {capturedTask.TaskName}");
                });
            }
            
            Debug.Log($"Task object created and configured: {taskObject.name}");
        }
        
        Debug.Log($"Finished creating {tasks.Count} task displays");
        
        // Force layout rebuild
        if (tasksContainer.GetComponent<ContentSizeFitter>() != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tasksContainer as RectTransform);
        }
    }
    
    /// <summary>
    /// Gets the count of completed tasks
    /// </summary>
    private int GetCompletedTaskCount(List<QuestTask> tasks)
    {
        int count = 0;
        foreach (QuestTask task in tasks)
        {
            if (task.IsCompleted)
            {
                count++;
            }
        }
        return count;
    }
    
    /// <summary>
    /// Handles quest click events
    /// </summary>
    private void OnQuestClicked(QuestSO quest)
    {
        Debug.Log($"Clicked on quest: {quest.questName}");
        // You can implement quest tracking, details popup, etc. here
        // questManager.SetActiveQuest(quest);
    }
    
    /// <summary>
    /// Clears all existing quest display elements
    /// </summary>
    private void ClearQuestDisplays()
    {
        foreach (GameObject questDisplay in activeQuestDisplays)
        {
            if (questDisplay != null)
            {
                Destroy(questDisplay);
            }
        }
        activeQuestDisplays.Clear();
    }
    
    /// <summary>
    /// Public method to force refresh the quest display
    /// </summary>
    public void RefreshQuestDisplay()
    {
        UpdateQuestDisplay();
    }
    
    /// <summary>
    /// Method to be called when quests change (can be called by QuestManager)
    /// </summary>
    public void OnQuestsChanged()
    {
        RefreshQuestDisplay();
    }
}
