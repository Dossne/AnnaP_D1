using FarmMerger.Pieces;
using UnityEngine;

namespace FarmMerger.Board
{
    public sealed class BoardView : MonoBehaviour
    {
        private const float BorderPadding = 0.16f;
        private const float BackdropPaddingX = 1.05f;
        private const float BackdropPaddingY = 0.85f;
        private static readonly Color BorderColor = new Color(0.79f, 0.63f, 0.17f, 1f);
        private static readonly Color WoodBaseColor = new Color(0.57f, 0.37f, 0.20f, 1f);
        private static readonly Color WoodPlankLightColor = new Color(0.64f, 0.42f, 0.23f, 1f);
        private static readonly Color WoodPlankDarkColor = new Color(0.49f, 0.31f, 0.17f, 1f);
        private static readonly Color WoodSeamColor = new Color(0.31f, 0.19f, 0.10f, 1f);
        private static readonly Color WoodPixelHighlightColor = new Color(0.74f, 0.51f, 0.29f, 1f);
        private static readonly Color WoodPixelShadowColor = new Color(0.40f, 0.25f, 0.14f, 1f);
        private static readonly Color ScratchColor = new Color(0.82f, 0.72f, 0.58f, 0.28f);
        private static readonly Color NailColor = new Color(0.23f, 0.19f, 0.16f, 1f);
        private static readonly Color ValidPreviewColor = new Color(0.34f, 0.78f, 0.40f, 0.65f);
        private static readonly Color InvalidPreviewColor = new Color(0.86f, 0.28f, 0.28f, 0.65f);

        private SpriteRenderer[,] cellRenderers;
        private SpriteRenderer[] previewRenderers = new SpriteRenderer[0];
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
            CreateWoodBackdrop();
            CreateBoardFrame();
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

        public void ShowPlacementPreview(PieceDefinition piece, Vector2Int originCell, bool isValid)
        {
            EnsurePreviewRendererCount(piece.Size);

            Color previewColor = isValid ? ValidPreviewColor : InvalidPreviewColor;
            float step = config.CellSize + config.CellGap;
            float startX = -((config.TotalWidth - config.CellSize) * 0.5f);
            float startY = -((config.TotalHeight - config.CellSize) * 0.5f);

            for (int index = 0; index < previewRenderers.Length; index++)
            {
                bool isActive = index < piece.Size;
                SpriteRenderer renderer = previewRenderers[index];
                renderer.gameObject.SetActive(isActive);

                if (!isActive)
                {
                    continue;
                }

                Vector2Int targetCell = originCell + piece.Cells[index];
                renderer.transform.localPosition = new Vector3(
                    startX + (targetCell.x * step),
                    startY + (targetCell.y * step),
                    0f);
                renderer.transform.localScale = new Vector3(config.CellSize, config.CellSize, 1f);
                renderer.color = previewColor;
            }
        }

        public void HidePlacementPreview()
        {
            for (int index = 0; index < previewRenderers.Length; index++)
            {
                previewRenderers[index].gameObject.SetActive(false);
            }
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

        private void EnsurePreviewRendererCount(int requiredCount)
        {
            if (previewRenderers.Length >= requiredCount)
            {
                return;
            }

            SpriteRenderer[] newRenderers = new SpriteRenderer[requiredCount];

            for (int index = 0; index < newRenderers.Length; index++)
            {
                if (index < previewRenderers.Length)
                {
                    newRenderers[index] = previewRenderers[index];
                    continue;
                }

                GameObject previewObject = new GameObject($"PreviewCell_{index}");
                previewObject.transform.SetParent(transform, false);

                SpriteRenderer renderer = previewObject.AddComponent<SpriteRenderer>();
                renderer.sprite = cellSprite;
                renderer.sortingOrder = 2;
                renderer.gameObject.SetActive(false);
                newRenderers[index] = renderer;
            }

            previewRenderers = newRenderers;
        }

        private void CreateBoardFrame()
        {
            CreateLayer(
                "Border",
                config.TotalWidth + (BorderPadding * 2f),
                config.TotalHeight + (BorderPadding * 2f),
                BorderColor,
                -2);

            CreateLayer(
                "GridBackground",
                config.TotalWidth,
                config.TotalHeight,
                BorderColor,
                -1);
        }

        private void CreateWoodBackdrop()
        {
            float backdropWidth = config.TotalWidth + (BackdropPaddingX * 2f);
            float backdropHeight = config.TotalHeight + (BackdropPaddingY * 2f);

            CreateLayer("WoodBase", backdropWidth, backdropHeight, WoodBaseColor, -6);

            float plankHeight = backdropHeight / 4f;
            for (int index = 0; index < 4; index++)
            {
                float y = (backdropHeight * 0.5f) - (plankHeight * 0.5f) - (index * plankHeight);
                CreateDecorLayer(
                    $"WoodPlank_{index}",
                    new Vector3(0f, y, 0f),
                    new Vector3(backdropWidth, plankHeight - 0.04f, 1f),
                    index % 2 == 0 ? WoodPlankLightColor : WoodPlankDarkColor,
                    -5,
                    0f);

                if (index < 3)
                {
                    float seamY = y - (plankHeight * 0.5f);
                    CreateDecorLayer(
                        $"WoodSeam_{index}",
                        new Vector3(0f, seamY, 0f),
                        new Vector3(backdropWidth, 0.06f, 1f),
                        WoodSeamColor,
                        -4,
                        0f);
                }
            }

            CreateDecorLayer("PixelBand_A", new Vector3(-0.9f, 1.55f, 0f), new Vector3(1.45f, 0.12f, 1f), WoodPixelHighlightColor, -4, 0f);
            CreateDecorLayer("PixelBand_B", new Vector3(1.2f, 0.3f, 0f), new Vector3(1.05f, 0.12f, 1f), WoodPixelShadowColor, -4, 0f);
            CreateDecorLayer("PixelBand_C", new Vector3(-1.1f, -0.9f, 0f), new Vector3(1.25f, 0.12f, 1f), WoodPixelHighlightColor, -4, 0f);
            CreateDecorLayer("PixelBand_D", new Vector3(0.8f, -1.7f, 0f), new Vector3(1.6f, 0.12f, 1f), WoodPixelShadowColor, -4, 0f);

            CreateDecorLayer("PixelKnot_A", new Vector3(-1.95f, 0.35f, 0f), new Vector3(0.22f, 0.22f, 1f), WoodPixelShadowColor, -4, 0f);
            CreateDecorLayer("PixelKnot_B", new Vector3(-1.72f, 0.35f, 0f), new Vector3(0.12f, 0.12f, 1f), WoodPixelHighlightColor, -4, 0f);
            CreateDecorLayer("PixelKnot_C", new Vector3(1.86f, -1.0f, 0f), new Vector3(0.20f, 0.20f, 1f), WoodPixelShadowColor, -4, 0f);
            CreateDecorLayer("PixelKnot_D", new Vector3(2.08f, -1.0f, 0f), new Vector3(0.10f, 0.10f, 1f), WoodPixelHighlightColor, -4, 0f);

            CreateDecorLayer("Scratch_A", new Vector3(-1.4f, 0.95f, 0f), new Vector3(1.2f, 0.04f, 1f), ScratchColor, -4, -18f);
            CreateDecorLayer("Scratch_B", new Vector3(1.15f, -0.35f, 0f), new Vector3(0.95f, 0.04f, 1f), ScratchColor, -4, 16f);
            CreateDecorLayer("Scratch_C", new Vector3(-0.35f, -1.15f, 0f), new Vector3(1.35f, 0.03f, 1f), ScratchColor, -4, 8f);

            CreateDecorLayer("Nail_TL", new Vector3(-(backdropWidth * 0.5f) + 0.28f, (backdropHeight * 0.5f) - 0.28f, 0f), new Vector3(0.12f, 0.12f, 1f), NailColor, -3, 0f);
            CreateDecorLayer("Nail_TR", new Vector3((backdropWidth * 0.5f) - 0.28f, (backdropHeight * 0.5f) - 0.28f, 0f), new Vector3(0.12f, 0.12f, 1f), NailColor, -3, 0f);
            CreateDecorLayer("Nail_BL", new Vector3(-(backdropWidth * 0.5f) + 0.28f, -(backdropHeight * 0.5f) + 0.28f, 0f), new Vector3(0.12f, 0.12f, 1f), NailColor, -3, 0f);
            CreateDecorLayer("Nail_BR", new Vector3((backdropWidth * 0.5f) - 0.28f, -(backdropHeight * 0.5f) + 0.28f, 0f), new Vector3(0.12f, 0.12f, 1f), NailColor, -3, 0f);
        }

        private void CreateLayer(string objectName, float width, float height, Color color, int sortingOrder)
        {
            GameObject layerObject = new GameObject(objectName);
            layerObject.transform.SetParent(transform, false);
            layerObject.transform.localPosition = Vector3.zero;
            layerObject.transform.localScale = new Vector3(width, height, 1f);

            SpriteRenderer renderer = layerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = cellSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private void CreateDecorLayer(
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            int sortingOrder,
            float rotationZ)
        {
            GameObject layerObject = new GameObject(objectName);
            layerObject.transform.SetParent(transform, false);
            layerObject.transform.localPosition = localPosition;
            layerObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            layerObject.transform.localScale = localScale;

            SpriteRenderer renderer = layerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = cellSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }
    }
}
