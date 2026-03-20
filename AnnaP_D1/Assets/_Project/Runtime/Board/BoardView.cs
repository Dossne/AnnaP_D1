using UnityEngine;

namespace FarmMerger.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        private SpriteRenderer[,] cellRenderers;
        private BoardConfig config;
        private BoardModel model;
        private Sprite cellSprite;
        private Color activePaletteColor;

        public void Initialize(BoardConfig boardConfig, BoardModel boardModel)
        {
            config = boardConfig;
            model = boardModel;
            activePaletteColor = config.Palette[0];

            CreateSharedSprite();
            BuildGrid();
            Refresh();
        }

        public void AdvancePalette(int cycleIndex)
        {
            activePaletteColor = config.Palette[cycleIndex % config.Palette.Length];
            Refresh();
        }

        public void Refresh()
        {
            for (int y = 0; y < config.Height; y++)
            {
                for (int x = 0; x < config.Width; x++)
                {
                    var renderer = cellRenderers[x, y];
                    renderer.color = model.IsCellFilled(x, y)
                        ? Color.Lerp(activePaletteColor, config.FilledColor, 0.35f)
                        : Color.Lerp(config.BaseColor, activePaletteColor, 0.2f);
                }
            }
        }

        public bool TryGetCellPosition(Vector3 worldPosition, out Vector2Int cellPosition)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPosition);
            float step = config.CellSize + config.CellGap;
            float minX = -(config.TotalWidth * 0.5f);
            float minY = -(config.TotalHeight * 0.5f);

            float offsetX = localPoint.x - minX;
            float offsetY = localPoint.y - minY;

            if (offsetX < 0f || offsetY < 0f)
            {
                cellPosition = default;
                return false;
            }

            int x = Mathf.FloorToInt(offsetX / step);
            int y = Mathf.FloorToInt(offsetY / step);

            if (x < 0 || y < 0 || x >= config.Width || y >= config.Height)
            {
                cellPosition = default;
                return false;
            }

            float insideCellX = offsetX - (x * step);
            float insideCellY = offsetY - (y * step);

            if (insideCellX > config.CellSize || insideCellY > config.CellSize)
            {
                cellPosition = default;
                return false;
            }

            cellPosition = new Vector2Int(x, y);
            return true;
        }

        private void CreateSharedSprite()
        {
            if (cellSprite != null)
            {
                return;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            cellSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private void BuildGrid()
        {
            cellRenderers = new SpriteRenderer[config.Width, config.Height];
            float step = config.CellSize + config.CellGap;
            float startX = -((config.TotalWidth - config.CellSize) * 0.5f);
            float startY = -((config.TotalHeight - config.CellSize) * 0.5f);

            for (int y = 0; y < config.Height; y++)
            {
                for (int x = 0; x < config.Width; x++)
                {
                    GameObject cellObject = new GameObject($"Cell_{x}_{y}");
                    cellObject.transform.SetParent(transform, false);
                    cellObject.transform.localPosition = new Vector3(startX + (x * step), startY + (y * step), 0f);
                    cellObject.transform.localScale = new Vector3(config.CellSize, config.CellSize, 1f);

                    SpriteRenderer renderer = cellObject.AddComponent<SpriteRenderer>();
                    renderer.sprite = cellSprite;
                    renderer.sortingOrder = 0;

                    cellRenderers[x, y] = renderer;
                }
            }
        }
    }
}
