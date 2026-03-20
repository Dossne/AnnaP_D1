using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceView : MonoBehaviour
    {
        private const float CellSize = 0.46f;
        private const float CellGap = 0.05f;
        private const float HitPadding = 0.22f;
        private const float FrameWidth = 1.9f;
        private const float FrameHeight = 1.7f;
        private const float FrameThickness = 0.08f;

        private SpriteRenderer[] blockRenderers = new SpriteRenderer[0];
        private SpriteRenderer[] frameRenderers = new SpriteRenderer[0];
        private Sprite blockSprite;
        private Color activeColor;
        private PieceDefinition currentPiece;
        private bool isSelected;
        private bool isDragging;
        private Vector2 boundsSize;

        public void Initialize(Color color)
        {
            activeColor = color;
            CreateSharedSprite();
            CreateFrame();
        }

        public void ShowPiece(PieceDefinition piece)
        {
            currentPiece = piece;
            EnsureRendererCount(piece.Size);

            float step = CellSize + CellGap;
            float totalWidth = (piece.Width * CellSize) + ((piece.Width - 1) * CellGap);
            float totalHeight = (piece.Height * CellSize) + ((piece.Height - 1) * CellGap);
            boundsSize = new Vector2(totalWidth, totalHeight);
            float startX = -((totalWidth - CellSize) * 0.5f);
            float startY = -((totalHeight - CellSize) * 0.5f);

            for (int index = 0; index < blockRenderers.Length; index++)
            {
                bool isActive = index < piece.Size;
                SpriteRenderer renderer = blockRenderers[index];
                renderer.gameObject.SetActive(isActive);

                if (!isActive)
                {
                    continue;
                }

                Vector2Int cell = piece.Cells[index];
                renderer.transform.localPosition = new Vector3(
                    startX + (cell.x * step),
                    startY + (cell.y * step),
                    0f);
                renderer.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                renderer.color = GetDisplayColor();
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            RefreshColors();
        }

        public void SetDragging(bool dragging)
        {
            isDragging = dragging;
            RefreshColors();

            for (int index = 0; index < blockRenderers.Length; index++)
            {
                if (!blockRenderers[index].gameObject.activeSelf)
                {
                    continue;
                }

                blockRenderers[index].sortingOrder = isDragging ? 5 : 1;
            }
        }

        private void RefreshColors()
        {
            for (int index = 0; index < blockRenderers.Length; index++)
            {
                if (!blockRenderers[index].gameObject.activeSelf)
                {
                    continue;
                }

                blockRenderers[index].color = GetDisplayColor();
            }
        }

        public bool ContainsWorldPoint(Vector3 worldPoint)
        {
            if (currentPiece == null)
            {
                return false;
            }

            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            float halfWidth = (boundsSize.x * 0.5f) + HitPadding;
            float halfHeight = (boundsSize.y * 0.5f) + HitPadding;

            return localPoint.x >= -halfWidth
                && localPoint.x <= halfWidth
                && localPoint.y >= -halfHeight
                && localPoint.y <= halfHeight;
        }

        private void EnsureRendererCount(int requiredCount)
        {
            if (blockRenderers.Length >= requiredCount)
            {
                return;
            }

            SpriteRenderer[] newRenderers = new SpriteRenderer[requiredCount];

            for (int index = 0; index < newRenderers.Length; index++)
            {
                if (index < blockRenderers.Length)
                {
                    newRenderers[index] = blockRenderers[index];
                    continue;
                }

                GameObject blockObject = new GameObject($"PieceCell_{index}");
                blockObject.transform.SetParent(transform, false);

                SpriteRenderer renderer = blockObject.AddComponent<SpriteRenderer>();
                renderer.sprite = blockSprite;
                renderer.sortingOrder = 1;
                newRenderers[index] = renderer;
            }

            blockRenderers = newRenderers;
        }

        private void CreateSharedSprite()
        {
            if (blockSprite != null)
            {
                return;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            blockSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private Color GetDisplayColor()
        {
            Color color = isSelected
                ? Color.Lerp(activeColor, Color.white, 0.28f)
                : activeColor;

            color.a = isDragging ? 0.82f : 1f;
            return color;
        }

        private void CreateFrame()
        {
            frameRenderers = new SpriteRenderer[4];

            for (int index = 0; index < frameRenderers.Length; index++)
            {
                GameObject frameObject = new GameObject($"Frame_{index}");
                frameObject.transform.SetParent(transform, false);

                SpriteRenderer renderer = frameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = blockSprite;
                renderer.color = Color.white;
                renderer.sortingOrder = 0;
                frameRenderers[index] = renderer;
            }

            frameRenderers[0].transform.localPosition = new Vector3(0f, FrameHeight * 0.5f, 0f);
            frameRenderers[0].transform.localScale = new Vector3(FrameWidth, FrameThickness, 1f);

            frameRenderers[1].transform.localPosition = new Vector3(0f, -(FrameHeight * 0.5f), 0f);
            frameRenderers[1].transform.localScale = new Vector3(FrameWidth, FrameThickness, 1f);

            frameRenderers[2].transform.localPosition = new Vector3(-(FrameWidth * 0.5f), 0f, 0f);
            frameRenderers[2].transform.localScale = new Vector3(FrameThickness, FrameHeight + FrameThickness, 1f);

            frameRenderers[3].transform.localPosition = new Vector3(FrameWidth * 0.5f, 0f, 0f);
            frameRenderers[3].transform.localScale = new Vector3(FrameThickness, FrameHeight + FrameThickness, 1f);
        }
    }
}
