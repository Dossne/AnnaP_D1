using UnityEngine;
using UnityEngine.UI;

namespace FarmMerger.MiniGame
{
    public sealed class MiniGameHoleView : MonoBehaviour
    {
        private static readonly Color RimShadowColor = new Color(0.18f, 0.08f, 0.25f, 0.92f);
        private static readonly Color RimBaseColor = new Color(0.31f, 0.14f, 0.42f, 1f);
        private static readonly Color RimHighlightColor = new Color(0.55f, 0.35f, 0.68f, 0.95f);
        private static readonly Color RimInnerColor = new Color(0.24f, 0.11f, 0.34f, 1f);
        private static readonly Color InnerShadowColor = new Color(0f, 0f, 0f, 0.98f);
        private static readonly Color InnerSoftShadowColor = new Color(0.06f, 0.02f, 0.08f, 0.88f);
        private static readonly Color FrontCoverColor = new Color(0.03f, 0.01f, 0.05f, 0.98f);

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
                "HoleDropShadow",
                transform,
                new Vector2(0f, -(size * 0.05f)),
                new Vector2(size * 1.03f, size * 0.95f),
                circleSprite,
                RimShadowColor);

            CreateImage(
                "HoleRimBase",
                transform,
                Vector2.zero,
                new Vector2(size, size),
                circleSprite,
                RimBaseColor);

            CreateImage(
                "HoleRimHighlight",
                transform,
                new Vector2(0f, size * 0.03f),
                new Vector2(size * 0.88f, size * 0.74f),
                circleSprite,
                RimHighlightColor);

            CreateImage(
                "HoleRimInner",
                transform,
                new Vector2(0f, -(size * 0.03f)),
                new Vector2(size * 0.84f, size * 0.72f),
                circleSprite,
                RimInnerColor);

            CreateImage(
                "HoleInnerSoft",
                transform,
                new Vector2(0f, -(size * 0.02f)),
                new Vector2(size * 0.70f, size * 0.60f),
                radialShadowSprite,
                InnerSoftShadowColor);

            CreateImage(
                "HoleInnerCore",
                transform,
                new Vector2(0f, -(size * 0.05f)),
                new Vector2(size * 0.60f, size * 0.50f),
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
                new Vector2(0f, -(size * 0.19f)),
                new Vector2(size * 0.78f, size * 0.40f),
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

                    float falloff = 1f - Mathf.Clamp01(normalizedDistance);
                    float alpha = Mathf.SmoothStep(0.18f, 1f, falloff * falloff);
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
