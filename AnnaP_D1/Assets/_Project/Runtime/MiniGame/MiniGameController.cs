using UnityEngine;

namespace FarmMerger.MiniGame
{
    public sealed class MiniGameController : MonoBehaviour
    {
        private const int ColumnCount = 3;
        private const int RowCount = 4;
        private const float PlayfieldSize = 640f;
        private const float HoleSize = 158f;
        private const float HoleSpacing = 52f;
        private const float PlayfieldOffsetY = 96f;
        private const float BlackCatVisibleDuration = 2f;

        private readonly MiniGameHoleView[,] holes = new MiniGameHoleView[ColumnCount, RowCount];

        private RectTransform rootRect;
        private bool isInitialized;
        private float blackCatTimer;
        private MiniGameHoleView activeBlackCatHole;

        public void Initialize(RectTransform parentRect)
        {
            if (isInitialized)
            {
                return;
            }

            ResetHoleReferences();

            rootRect = CreateRect("MiniGamePlayfield", parentRect);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, PlayfieldOffsetY);
            rootRect.sizeDelta = new Vector2(PlayfieldSize, PlayfieldSize);

            BuildHoleGrid();
            isInitialized = true;
            SpawnBlackCatInRandomHole();
        }

        private void Update()
        {
            if (!isInitialized || activeBlackCatHole == null)
            {
                return;
            }

            blackCatTimer -= Time.deltaTime;
            if (blackCatTimer > 0f)
            {
                return;
            }

            activeBlackCatHole.HideBlackCat();
            SpawnBlackCatInRandomHole();
        }

        private void BuildHoleGrid()
        {
            float totalWidth = (ColumnCount * HoleSize) + ((ColumnCount - 1) * HoleSpacing);
            float totalHeight = (RowCount * HoleSize) + ((RowCount - 1) * HoleSpacing);
            float startX = -(totalWidth * 0.5f) + (HoleSize * 0.5f);
            float startY = (totalHeight * 0.5f) - (HoleSize * 0.5f);

            for (int row = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++)
                {
                    RectTransform holeRect = CreateRect($"Hole_{column}_{row}", rootRect);
                    holeRect.anchorMin = new Vector2(0.5f, 0.5f);
                    holeRect.anchorMax = new Vector2(0.5f, 0.5f);
                    holeRect.sizeDelta = new Vector2(HoleSize, HoleSize);
                    holeRect.anchoredPosition = new Vector2(
                        startX + (column * (HoleSize + HoleSpacing)),
                        startY - (row * (HoleSize + HoleSpacing)));

                    MiniGameHoleView holeView = holeRect.gameObject.AddComponent<MiniGameHoleView>();
                    holeView.Initialize(HoleSize);
                    holes[column, row] = holeView;
                }
            }
        }

        private void SpawnBlackCatInRandomHole()
        {
            MiniGameHoleView nextHole = FindRandomFreeHole();
            if (nextHole == null)
            {
                activeBlackCatHole = null;
                return;
            }

            activeBlackCatHole = nextHole;
            activeBlackCatHole.ShowBlackCat();
            blackCatTimer = BlackCatVisibleDuration;
        }

        private MiniGameHoleView FindRandomFreeHole()
        {
            int totalHoleCount = ColumnCount * RowCount;
            int startIndex = Random.Range(0, totalHoleCount);

            for (int offset = 0; offset < totalHoleCount; offset++)
            {
                int flatIndex = (startIndex + offset) % totalHoleCount;
                int column = flatIndex % ColumnCount;
                int row = flatIndex / ColumnCount;

                MiniGameHoleView hole = holes[column, row];
                if (hole == null)
                {
                    continue;
                }

                if (hole.IsOccupied)
                {
                    continue;
                }

                return hole;
            }

            return null;
        }

        private void ResetHoleReferences()
        {
            activeBlackCatHole = null;
            blackCatTimer = 0f;

            for (int row = 0; row < RowCount; row++)
            {
                for (int column = 0; column < ColumnCount; column++)
                {
                    holes[column, row] = null;
                }
            }
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }
    }
}
