using UnityEngine;
using UnityEngine.UI;

namespace FarmMerger.MiniGame
{
    public sealed class MiniGameCatView : MonoBehaviour
    {
        public enum CatStyle
        {
            Black,
        }

        private static readonly Color BlackFurColor = new Color(0.05f, 0.05f, 0.07f, 1f);
        private static readonly Color GreenEyeColor = new Color(0.38f, 0.96f, 0.46f, 1f);
        private static readonly Color EyePupilColor = new Color(0.02f, 0.02f, 0.02f, 1f);
        private static readonly Color PawColor = new Color(0.07f, 0.07f, 0.09f, 1f);
        private static readonly Color MuzzleColor = new Color(0.15f, 0.15f, 0.17f, 1f);

        private static Sprite circleSprite;
        private static Sprite triangleSprite;

        private RectTransform rootRect;
        private RectTransform wavingPawRect;
        private CanvasGroup canvasGroup;
        private bool isVisible;
        private float animationTime;

        private void Update()
        {
            if (!isVisible || wavingPawRect == null)
            {
                return;
            }

            animationTime += Time.deltaTime;
            float wave = Mathf.Sin(animationTime * 8f);
            wavingPawRect.localRotation = Quaternion.Euler(0f, 0f, 22f + (wave * 20f));
            wavingPawRect.anchoredPosition = new Vector2(42f, 34f + Mathf.Abs(wave) * 8f);
        }

        public void Initialize(CatStyle style, float size)
        {
            EnsureSprites();

            rootRect = GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, 18f);
            rootRect.sizeDelta = new Vector2(size, size);

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            CreateCatVisual(style, size);
        }

        public void Show()
        {
            isVisible = true;
            animationTime = 0f;
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            rootRect.anchoredPosition = new Vector2(0f, 18f);
            wavingPawRect.localRotation = Quaternion.Euler(0f, 0f, 22f);
        }

        public void Hide()
        {
            isVisible = false;
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void HideImmediate()
        {
            Hide();
        }

        private void CreateCatVisual(CatStyle style, float size)
        {
            Color furColor = style == CatStyle.Black ? BlackFurColor : Color.white;

            CreateImage(
                "Body",
                transform,
                new Vector2(0f, -8f),
                new Vector2(size * 0.54f, size * 0.60f),
                circleSprite,
                furColor);

            CreateImage(
                "Head",
                transform,
                new Vector2(0f, 18f),
                new Vector2(size * 0.52f, size * 0.48f),
                circleSprite,
                furColor);

            CreateImage(
                "EarLeft",
                transform,
                new Vector2(-28f, 56f),
                new Vector2(size * 0.18f, size * 0.18f),
                triangleSprite,
                furColor);

            CreateImage(
                "EarRight",
                transform,
                new Vector2(28f, 56f),
                new Vector2(size * 0.18f, size * 0.18f),
                triangleSprite,
                furColor);

            CreateImage(
                "Muzzle",
                transform,
                new Vector2(0f, 6f),
                new Vector2(size * 0.20f, size * 0.12f),
                circleSprite,
                MuzzleColor);

            CreateEye(new Vector2(-20f, 22f), GreenEyeColor);
            CreateEye(new Vector2(20f, 22f), GreenEyeColor);

            CreateImage(
                "PawLeft",
                transform,
                new Vector2(-32f, -2f),
                new Vector2(size * 0.12f, size * 0.18f),
                circleSprite,
                PawColor);

            wavingPawRect = CreateImage(
                "PawWave",
                transform,
                new Vector2(42f, 34f),
                new Vector2(size * 0.12f, size * 0.22f),
                circleSprite,
                PawColor).GetComponent<RectTransform>();
            wavingPawRect.pivot = new Vector2(0.5f, 0.1f);
        }

        private void CreateEye(Vector2 position, Color irisColor)
        {
            CreateImage(
                "EyeWhite",
                transform,
                position,
                new Vector2(18f, 24f),
                circleSprite,
                irisColor);

            CreateImage(
                "EyePupil",
                transform,
                position,
                new Vector2(5f, 13f),
                circleSprite,
                EyePupilColor);
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

        private static void EnsureSprites()
        {
            if (circleSprite != null && triangleSprite != null)
            {
                return;
            }

            circleSprite = CreateCircleSprite(128);
            triangleSprite = CreateTriangleSprite(128);
        }

        private static Sprite CreateCircleSprite(int resolution)
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
                    texture.SetPixel(x, y, normalizedDistance <= 1f ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        }

        private static Sprite CreateTriangleSprite(int resolution)
        {
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < resolution; y++)
            {
                float normalizedY = (float)y / (resolution - 1);
                float halfWidth = normalizedY * 0.5f;

                for (int x = 0; x < resolution; x++)
                {
                    float normalizedX = ((float)x / (resolution - 1)) - 0.5f;
                    bool isInside = Mathf.Abs(normalizedX) <= halfWidth;
                    texture.SetPixel(x, y, isInside ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0f), resolution);
        }
    }
}
