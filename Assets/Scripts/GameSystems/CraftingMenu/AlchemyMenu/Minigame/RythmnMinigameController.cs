using System;
using UnityEngine;
using UnityEngine.UIElements;



namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.Minigame
{
    public class RhythmMinigameController : MonoBehaviour
    {
        private VisualElement root;
        public UIDocument uiDocument;
        public int requiredHits = 3;
        public int maxAttempts = 5;

        private VisualElement cursor;
        private VisualElement perfectZone;
        private VisualElement goodZone;
        private VisualElement badZone;

        private int successfulHits = 0;
        private int attempts = 0;
        private bool goingRight = true;

        public event Action<bool> OnMinigameEnd;

        private float speed = 300f;
        private float trackWidth;

        void OnEnable()
        {
            root = uiDocument.rootVisualElement;
            cursor = root.Q<VisualElement>("cursor");
            perfectZone = root.Q<VisualElement>("perfectZone");
            goodZone = root.Q<VisualElement>("goodZone");
            badZone = root.Q<VisualElement>("badZone");

            // Fix: Delay reading width until layout is ready
            var track = root.Q<VisualElement>("track");
            track.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                trackWidth = track.resolvedStyle.width;
            });

            successfulHits = 0;
            attempts = 0;
        }


        void Update()
        {
            if (cursor == null || trackWidth <= 0) return;

            MoveCursor();

            if (Input.GetKeyDown(KeyCode.Space))
                EvaluateHit();
        }


        private void MoveCursor()
        {
            float delta = speed * Time.deltaTime;
            float currentX = cursor.resolvedStyle.left;
            float newX = currentX + (goingRight ? delta : -delta);

            var track = root.Q<VisualElement>("track");
            track.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                trackWidth = track.resolvedStyle.width;
            });


            if (newX <= 0)
            {
                newX = 0;
                goingRight = true;
            }
            else if (newX >= trackWidth - cursor.resolvedStyle.width)
            {
                newX = trackWidth - cursor.resolvedStyle.width;
                goingRight = false;
            }

            cursor.style.left = newX;

        }

        private void EvaluateHit()
        {
            float cursorCenter = cursor.worldBound.center.x;

            if (perfectZone.worldBound.Contains(new Vector2(cursorCenter, perfectZone.worldBound.center.y)))
                successfulHits++;
            else if (goodZone.worldBound.Contains(new Vector2(cursorCenter, goodZone.worldBound.center.y)))
                successfulHits++;
            else if (badZone.worldBound.Contains(new Vector2(cursorCenter, badZone.worldBound.center.y)))
                successfulHits--;

            attempts++;

            if (attempts >= maxAttempts)
            {
                bool success = successfulHits >= requiredHits;
                OnMinigameEnd?.Invoke(success);
                enabled = false;
            }
        }

        public void Init(int hits, int tries)
        {
            requiredHits = hits;
            maxAttempts = tries;
            successfulHits = 0;
            attempts = 0;
            enabled = true;
        }
    }
}