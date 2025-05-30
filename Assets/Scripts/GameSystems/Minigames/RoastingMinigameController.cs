using UnityEngine;
using UnityEngine.UIElements;

public class RoastingMinigameController : MonoBehaviour
{
    [Header("UI Toolkit Elements")]
    public UIDocument uiDocument;

    private ProgressBar[] foodBars;
    private Label resultLabel;

    [Header("Config")]
    public float cookDuration = 5f;
    public float burnDuration = 8f;
    public int foodCount = 3;

    private float[] cookTimers;
    private FoodState[] foodStates;
    private bool gameEnded;
    private float totalCookTime;

    private enum FoodState { Uncooked, Cooked, Burnt }

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        foodBars = new ProgressBar[foodCount];
        for (int i = 0; i < foodCount; i++)
        {
            foodBars[i] = root.Q<ProgressBar>($"foodBar{i + 1}");
        }

        resultLabel = root.Q<Label>("resultLabel");

        cookTimers = new float[foodCount];
        foodStates = new FoodState[foodCount];

        ResetGame();
    }

    void Update()
    {
        if (gameEnded) return;

        totalCookTime += Time.deltaTime;

        for (int i = 0; i < foodCount; i++)
        {
            cookTimers[i] += Time.deltaTime;

            if (cookTimers[i] < cookDuration)
                foodStates[i] = FoodState.Uncooked;
            else if (cookTimers[i] < burnDuration)
                foodStates[i] = FoodState.Cooked;
            else
                foodStates[i] = FoodState.Burnt;

            UpdateFoodBar(i);
        }

        if (totalCookTime > burnDuration + 2f) // Small buffer period
        {
            EvaluateGame();
        }
    }

    private void UpdateFoodBar(int index)
    {
        var bar = foodBars[index];
        float timer = cookTimers[index];

        if (timer < cookDuration)
        {
            bar.value = (timer / cookDuration) * 100f;
            bar.title = "Uncooked";
        }
        else if (timer < burnDuration)
        {
            bar.value = ((timer - cookDuration) / (burnDuration - cookDuration)) * 100f;
            bar.title = "Cooked";
        }
        else
        {
            bar.value = 100f;
            bar.title = "Burnt";
        }
    }

    private void EvaluateGame()
    {
        gameEnded = true;

        int cookedCount = 0;
        foreach (var state in foodStates)
        {
            if (state == FoodState.Cooked)
                cookedCount++;
        }

        if (cookedCount == foodCount)
            resultLabel.text = "Perfectly roasted!";
        else
            resultLabel.text = $"Only {cookedCount} cooked. Try again!";

        Debug.Log("Roasting Game Ended");
    }

    public void ResetGame()
    {
        totalCookTime = 0f;
        gameEnded = false;

        for (int i = 0; i < foodCount; i++)
        {
            cookTimers[i] = 0f;
            foodStates[i] = FoodState.Uncooked;
            UpdateFoodBar(i);
        }

        if (resultLabel != null)
            resultLabel.text = "Roast the food until it's just right!";
    }
}
