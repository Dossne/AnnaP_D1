using UnityEngine;
using UnityEngine.UI;

namespace FarmMerger.MiniGame
{
    public sealed class MiniGameController : MonoBehaviour
    {
        private const int GridSize = 3;
        private const float PlayfieldSize = 560f;
        private const float HoleSize = 142f;
        private const float HoleSpacing = 34f;
        private const float PlayfieldOffsetY = 46f;

        private readonly MiniGameHoleView[,] holes = new MiniGameHoleView[GridSize, GridSize];

        private RectTransform rootRect;
        private bool isInitialized;

        public void Initialize(RectTransform parentRect)
        {
            if (isInitialized)
            {
                return;
            }

            rootRect = CreateRect("MiniGamePlayfield", parentRect);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, PlayfieldOffsetY);
            rootRect.sizeDelta = new Vector2(PlayfieldSize, PlayfieldSize);

            BuildHoleGrid();
            isInitialized = true;
        }

        private void BuildHoleGrid()
        {
            float totalWidth = (GridSize * HoleSize) + ((GridSize - 1) * HoleSpacing);
            float startX = -(totalWidth * 0.5f) + (HoleSize * 0.5f);
            float startY = (totalWidth * 0.5f) - (HoleSize * 0.5f);

            for (int row = 0; row < GridSize; row++)
            {
                for (int column = 0; column < GridSize; column++)
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

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }
    }
}
