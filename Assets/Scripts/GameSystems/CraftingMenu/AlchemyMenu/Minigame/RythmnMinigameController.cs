using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.Minigame
{
    public class RhythmMinigameController : MonoBehaviour
    {
        [Header("References")]
        public UIDocument uiDocument;                // assign in Inspector

        [Header("Movement")]
        [SerializeField] private float speed = 300f; // px / sec

        [Header("Requirements (will be set by Init)")]
        public int requiredHits = 3;
        public int maxAttempts = 5;

        /* UI Elements */
        private VisualElement root;
        private VisualElement track;
        private VisualElement cursor;
        private VisualElement perfectZone;
        private VisualElement goodZone;
        private VisualElement badZone;

        /* State */
        private float trackWidth;
        private bool movingRight = true;
        private int successfulHits;
        private int attempts;
        private bool active;

        public event Action<bool> OnMinigameEnd;     // true = success

        /* ---------------------------------------------------------------- */

        private void OnEnable()
        {
            if (uiDocument == null)
            {
                Debug.LogError("RhythmMinigameController: UIDocument not assigned.");
                return;
            }

            root = uiDocument.rootVisualElement;
            track = root.Q<VisualElement>("track");
            cursor = root.Q<VisualElement>("cursor");
            perfectZone = root.Q<VisualElement>("perfectZone");
            goodZone = root.Q<VisualElement>("goodZone");
            badZone = root.Q<VisualElement>("badZone");

            if (track == null || cursor == null ||
                perfectZone == null || goodZone == null || badZone == null)
            {
                Debug.LogError("RhythmMinigameController: One or more UI elements missing.");
                return;
            }

            /* cache width once layout is ready */
            track.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                trackWidth = track.resolvedStyle.width;
            });

            /* start hidden */
            root.style.display = DisplayStyle.None;
            enabled = false;                // Update() off by default
        }

        /* ---------------------------------------------------------------- */

        private void Update()
        {
            if (!active || cursor == null || trackWidth <= 0f) return;

            MoveCursor();

            if (Input.GetKeyDown(KeyCode.Space))
                EvaluateHit();
        }

        /* ---------------------------------------------------------------- */

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

        /* ---------------------------------------------------------------- */

        private void EvaluateHit()
        {
            float cx = cursor.worldBound.center.x;
            Vector2 test = new(cx, cursor.worldBound.center.y);

            if (perfectZone.worldBound.Contains(test))
                successfulHits++;
            else if (goodZone.worldBound.Contains(test))
                successfulHits++;
            else if (badZone.worldBound.Contains(test))
                successfulHits--;

            attempts++;

            if (attempts >= maxAttempts)
            {
                FinishMiniGame(successfulHits >= requiredHits);
            }
        }

        private void FinishMiniGame(bool success)
        {
            active = false;
            root.style.display = DisplayStyle.None;
            OnMinigameEnd?.Invoke(success);
        }

        /* ---------------------------------------------------------------- */

        public void Init(int hitsNeeded, int triesAllowed)
        {
            requiredHits = hitsNeeded;
            maxAttempts = triesAllowed;
            successfulHits = 0;
            attempts = 0;
            movingRight = true;

            /* reset UI */
            cursor.style.left = 0f;
            root.style.display = DisplayStyle.Flex;

            active = true;
            enabled = true;
        }
    }
}
