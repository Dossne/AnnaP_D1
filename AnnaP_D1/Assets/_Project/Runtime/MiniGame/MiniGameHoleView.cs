using UnityEngine;
using UnityEngine.UI;

namespace FarmMerger.MiniGame
{
    public sealed class MiniGameHoleView : MonoBehaviour
    {
        private static readonly Color RimColor = new Color(0.31f, 0.14f, 0.42f, 1f);
        private static readonly Color InnerShadowColor = new Color(0f, 0f, 0f, 0.94f);
        private static readonly Color FrontCoverColor = new Color(0f, 0f, 0f, 0.98f);

        private static Sprite circleSprite;
        private static Sprite radialShadowSprite;

        private bool isOccupied;
        private MiniGameCatView blackCatView;

        public bool IsOccupied => isOccupied;

        public void Initialize(float size)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(size, size);

            EnsureSprites();

            CreateImage(
                "HoleRim",
                transform,
                Vector2.zero,
                new Vector2(size, size),
                circleSprite,
                RimColor);

            CreateImage(
                "HoleInner",
                transform,
                Vector2.zero,
                new Vector2(size * 0.72f, size * 0.72f),
                radialShadowSprite,
                InnerShadowColor);

            GameObject catObject = new GameObject("BlackCat", typeof(RectTransform));
            catObject.transform.SetParent(transform, false);
            blackCatView = catObject.AddComponent<MiniGameCatView>();
            blackCatView.Initialize(MiniGameCatView.CatStyle.Black, size * 0.86f);
            blackCatView.HideImmediate();

            CreateImage(
                "HoleFrontCover",
                transform,
                new Vector2(0f, -(size * 0.18f)),
                new Vector2(size * 0.76f, size * 0.42f),
                circleSprite,
                FrontCoverColor);
        }

        public void SetOccupied(bool occupied)
        {
            isOccupied = occupied;
        }

        public void ShowBlackCat()
        {
            isOccupied = true;
            blackCatView.Show();
        }

        public void HideBlackCat()
        {
            isOccupied = false;
            blackCatView.Hide();
        }

        private static void EnsureSprites()
        {
            if (circleSprite != null && radialShadowSprite != null)
            {
                return;
            }

            circleSprite = CreateCircleSprite(128, false);
            radialShadowSprite = CreateCircleSprite(128, true);
        }

        private static GameObject CreateImage(
            string objectName,
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 size,
            Sprite sprite,
            Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform));
            imageObject.transform.SetParent(parent, false);

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;

            return imageObject;
        }

        private static Sprite CreateCircleSprite(int resolution, bool useRadialShadow)
        {
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            float half = (resolution - 1) * 0.5f;
            float radius = resolution * 0.5f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dx = x - half;
                    float dy = y - half;
                    float normalizedDistance = Mathf.Sqrt((dx * dx) + (dy * dy)) / radius;

                    if (normalizedDistance > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    if (!useRadialShadow)
                    {
                        texture.SetPixel(x, y, Color.white);
                        continue;
                    }

                    float alpha = Mathf.Lerp(0.42f, 1f, 1f - Mathf.Clamp01(normalizedDistance));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, resolution, resolution),
                new Vector2(0.5f, 0.5f),
                resolution);
        }
    }
}
