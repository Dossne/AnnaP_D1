using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceView : MonoBehaviour
    {
        private const float CellSize = 0.46f;
        private const float CellGap = 0.05f;

        private SpriteRenderer[] blockRenderers = new SpriteRenderer[0];
        private Sprite blockSprite;
        private Color activeColor;

        public void Initialize(Color color)
        {
            activeColor = color;
            CreateSharedSprite();
        }

        public void ShowPiece(PieceDefinition piece)
        {
            EnsureRendererCount(piece.Size);

            float step = CellSize + CellGap;
            float totalWidth = (piece.Width * CellSize) + ((piece.Width - 1) * CellGap);
            float totalHeight = (piece.Height * CellSize) + ((piece.Height - 1) * CellGap);
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
                renderer.color = activeColor;
            }
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
    }
}
