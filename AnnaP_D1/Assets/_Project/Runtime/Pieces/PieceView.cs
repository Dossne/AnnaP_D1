using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceView : MonoBehaviour
    {
        private const float CellSize = 0.46f;
        private const float CellGap = 0.05f;
        private const float HitPadding = 0.22f;
        private const float BottomLineY = -0.42f;
        private const float ShadowOffsetY = -0.08f;
        private const float ShadowSquash = 0.82f;
        private const float BounceSpeed = 14f;

        private SpriteRenderer[] blockRenderers = new SpriteRenderer[0];
        private SpriteRenderer[] shadowRenderers = new SpriteRenderer[0];
        private Vector3[] blockBasePositions = new Vector3[0];
        private Sprite blockSprite;
        private Color activeColor;
        private PieceDefinition currentPiece;
        private bool isSelected;
        private bool isDragging;
        private Vector2 boundsSize;
        private float animationTime = 1f;
        private Vector3 animatedScale = Vector3.one;
        private Vector3 animatedOffset = Vector3.zero;

        private void Update()
        {
            UpdateSelectionAnimation();
        }

        public void Initialize(Color color)
        {
            activeColor = color;
            CreateSharedSprite();
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
            float startY = BottomLineY;

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
                Vector3 blockPosition = new Vector3(
                    startX + (cell.x * step),
                    startY + (cell.y * step),
                    0f);
                blockBasePositions[index] = blockPosition;
                renderer.transform.localPosition = blockPosition;
                renderer.transform.localScale = new Vector3(CellSize, CellSize, 1f);
                renderer.color = GetDisplayColor();
            }

            RefreshShadowColors();
            ApplyAnimatedTransforms();
        }

        public void SetSelected(bool selected)
        {
            if (selected && !isSelected)
            {
                animationTime = 0f;
            }

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

            RefreshShadowColors();
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
            SpriteRenderer[] newShadowRenderers = new SpriteRenderer[requiredCount];
            Vector3[] newBasePositions = new Vector3[requiredCount];

            for (int index = 0; index < newRenderers.Length; index++)
            {
                if (index < blockRenderers.Length)
                {
                    newRenderers[index] = blockRenderers[index];
                    newShadowRenderers[index] = shadowRenderers[index];
                    newBasePositions[index] = blockBasePositions[index];
                    continue;
                }

                GameObject shadowObject = new GameObject($"PieceShadow_{index}");
                shadowObject.transform.SetParent(transform, false);

                SpriteRenderer shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
                shadowRenderer.sprite = blockSprite;
                shadowRenderer.sortingOrder = 0;
                newShadowRenderers[index] = shadowRenderer;

                GameObject blockObject = new GameObject($"PieceCell_{index}");
                blockObject.transform.SetParent(transform, false);

                SpriteRenderer renderer = blockObject.AddComponent<SpriteRenderer>();
                renderer.sprite = blockSprite;
                renderer.sortingOrder = 1;
                newRenderers[index] = renderer;
            }

            blockRenderers = newRenderers;
            shadowRenderers = newShadowRenderers;
            blockBasePositions = newBasePositions;
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

        private void UpdateSelectionAnimation()
        {
            if (animationTime < 1f)
            {
                animationTime = Mathf.Min(1f, animationTime + (Time.deltaTime * 3.2f));
            }

            float press = 1f - Mathf.Clamp01(animationTime * BounceSpeed);
            float bounce = Mathf.Sin(Mathf.Clamp01(animationTime) * Mathf.PI) * 0.16f;
            float selectedBoost = isSelected ? 0.04f : 0f;
            float scaleValue = 1f - (press * 0.12f) + bounce + selectedBoost;

            animatedScale = new Vector3(scaleValue, scaleValue, 1f);
            animatedOffset = new Vector3(0f, -(press * 0.08f), 0f);

            ApplyAnimatedTransforms();
        }

        private void ApplyAnimatedTransforms()
        {
            for (int index = 0; index < blockRenderers.Length; index++)
            {
                if (!blockRenderers[index].gameObject.activeSelf)
                {
                    continue;
                }

                blockRenderers[index].transform.localScale = new Vector3(
                    CellSize * animatedScale.x,
                    CellSize * animatedScale.y,
                    1f);

                Vector3 baseLocalPosition = blockBasePositions[index];
                blockRenderers[index].transform.localPosition = baseLocalPosition + animatedOffset;

                shadowRenderers[index].transform.localPosition = new Vector3(baseLocalPosition.x, baseLocalPosition.y + ShadowOffsetY, 0f);
                shadowRenderers[index].transform.localScale = new Vector3(
                    CellSize * animatedScale.x,
                    CellSize * ShadowSquash,
                    1f);
            }
        }

        private void RefreshShadowColors()
        {
            for (int index = 0; index < shadowRenderers.Length; index++)
            {
                bool isActive = index < blockRenderers.Length && blockRenderers[index].gameObject.activeSelf;
                shadowRenderers[index].gameObject.SetActive(isActive);

                if (!isActive)
                {
                    continue;
                }

                Color shadowColor = new Color(0.34f, 0.23f, 0.10f, isDragging ? 0.16f : 0.24f);
                shadowRenderers[index].color = shadowColor;
            }
        }
    }
}
