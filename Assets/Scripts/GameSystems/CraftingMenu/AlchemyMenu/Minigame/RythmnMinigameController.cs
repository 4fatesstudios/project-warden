using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    public class RhythmMinigameController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;  // assign in Inspector
        public UIDocument UIDocument => uiDocument;

        [SerializeField] private float speed = 300f; // pixels per second
        [SerializeField] private float maxTime = 10f; // seconds for the minigame

        public int requiredHits = 3;
        public int maxAttempts = 5;

        private VisualElement root;
        private VisualElement track;
        private VisualElement cursor;
        private VisualElement perfectZone;
        private VisualElement goodZone;
        private VisualElement badZone;
        private Label attemptsLabel;
        private Label timerLabel;

        private float trackWidth;
        private bool movingRight;
        private int successfulHits;
        private int attempts;
        private bool active;
        private float timer;

        public event Action<bool> OnMinigameEnd;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;
            track = root.Q<VisualElement>("track");
            cursor = root.Q<VisualElement>("cursor");
            perfectZone = root.Q<VisualElement>("perfectZone");
            goodZone = root.Q<VisualElement>("goodZone");
            badZone = root.Q<VisualElement>("badZone");
            attemptsLabel = root.Q<Label>("AttemptsLabel");
            timerLabel = root.Q<Label>("TimerLabel");

            if (track == null || cursor == null || perfectZone == null || goodZone == null || badZone == null)
            {
                Debug.LogError("RhythmMinigameController: Missing UI elements.");
                enabled = false;
                return;
            }

            track.RegisterCallback<GeometryChangedEvent>(_ => trackWidth = track.resolvedStyle.width);

            Hide();
        }

        private void Update()
        {
            if (!active || cursor == null || trackWidth <= 0f)
                return;

            MoveCursor();

            if (Input.GetKeyDown(KeyCode.Space))
                EvaluateHit();

            // Timer logic
            timer -= Time.deltaTime;
            if (timerLabel != null)
                timerLabel.text = $"Time: {Mathf.Max(0, Mathf.CeilToInt(timer))}s";

            if (timer <= 0f)
                FinishMiniGame(successfulHits >= requiredHits);

            // Update attempts label
            if (attemptsLabel != null)
                attemptsLabel.text = $"Attempts: {attempts}/{maxAttempts}";
        }

        private void MoveCursor()
        {
            float delta = speed * Time.deltaTime;
            float current = cursor.resolvedStyle.left;
            float next = current + (movingRight ? delta : -delta);

            if (next <= 0f)
            {
                next = 0f;
                movingRight = true;
            }
            else if (next >= trackWidth - cursor.resolvedStyle.width)
            {
                next = trackWidth - cursor.resolvedStyle.width;
                movingRight = false;
            }

            cursor.style.left = next;
        }

        private void EvaluateHit()
        {
            Vector2 center = cursor.worldBound.center;

            if (perfectZone.worldBound.Contains(center) || goodZone.worldBound.Contains(center))
                successfulHits++;
            else if (badZone.worldBound.Contains(center))
                successfulHits--;

            attempts++;

            if (attempts >= maxAttempts)
                FinishMiniGame(successfulHits >= requiredHits);
        }

        private void FinishMiniGame(bool success)
        {
            active = false;
            Hide();
            OnMinigameEnd?.Invoke(success);
        }

        public void Init(int hitsNeeded, int triesAllowed)
        {
            requiredHits = hitsNeeded;
            maxAttempts = triesAllowed;
            successfulHits = 0;
            attempts = 0;
            movingRight = true;
            cursor.style.left = 0f;
            timer = maxTime;

            // Initialize UI
            if (attemptsLabel != null)
                attemptsLabel.text = $"Attempts: {attempts}/{maxAttempts}";
            if (timerLabel != null)
                timerLabel.text = $"Time: {Mathf.CeilToInt(timer)}s";

            Show();
            active = true;
        }

        public void Show()
        {
            if (uiDocument?.rootVisualElement == null)
            {
                Debug.LogError("RhythmMinigameController: uiDocument or rootVisualElement is null");
                return;
            }

            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            enabled = true;
        }

        public void Hide()
        {
            if (uiDocument?.rootVisualElement != null)
                uiDocument.rootVisualElement.style.display = DisplayStyle.None;

            enabled = false;
        }
    }
}
