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

        private const string BlackCatResourcePath = "MiniGame/Cats/Черная кошка с зелеными глазами";

        private static Sprite fallbackSprite;
        private static Sprite blackCatSprite;
        private static Texture2D blackCatTexture;

        private RectTransform rootRect;
        private RectTransform spriteRect;
        private CanvasGroup canvasGroup;
        private Image catImage;
        private bool isVisible;
        private float animationTime;
        private float baseSize;

        private void Update()
        {
            if (!isVisible || spriteRect == null)
            {
                return;
            }

            animationTime += Time.deltaTime;

            float sway = Mathf.Sin(animationTime * 3.6f) * 6f;
            float bob = Mathf.Sin(animationTime * 4.2f) * 5f;

            spriteRect.localRotation = Quaternion.Euler(0f, 0f, sway);
            spriteRect.anchoredPosition = new Vector2(0f, bob);
        }

        public void Initialize(CatStyle style, float size)
        {
            rootRect = GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, 24f);
            rootRect.sizeDelta = new Vector2(size * 0.88f, size * 0.92f);

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            baseSize = size;
            CreateSpriteVisual(style);
        }

        public void Show()
        {
            isVisible = true;
            animationTime = 0f;
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            rootRect.anchoredPosition = new Vector2(0f, 24f);
            spriteRect.anchoredPosition = Vector2.zero;
            spriteRect.localRotation = Quaternion.identity;
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

        private void CreateSpriteVisual(CatStyle style)
        {
            GameObject imageObject = new GameObject("CatPhoto", typeof(RectTransform));
            imageObject.transform.SetParent(transform, false);

            spriteRect = imageObject.GetComponent<RectTransform>();
            spriteRect.anchorMin = new Vector2(0.5f, 0.5f);
            spriteRect.anchorMax = new Vector2(0.5f, 0.5f);
            spriteRect.anchoredPosition = Vector2.zero;
            spriteRect.sizeDelta = new Vector2(baseSize * 0.88f, baseSize * 1.24f);

            catImage = imageObject.AddComponent<Image>();
            catImage.sprite = GetSprite(style);
            catImage.preserveAspect = true;
            catImage.raycastTarget = false;
            catImage.maskable = true;

            RectMask2D mask = gameObject.AddComponent<RectMask2D>();
            mask.padding = new Vector4(0f, 0f, 0f, -baseSize * 0.42f);
        }

        private static Sprite GetSprite(CatStyle style)
        {
            switch (style)
            {
                case CatStyle.Black:
                    if (blackCatSprite == null)
                    {
                        blackCatSprite = Resources.Load<Sprite>(BlackCatResourcePath);
                    }

                    if (blackCatSprite != null)
                    {
                        return blackCatSprite;
                    }

                    if (blackCatTexture == null)
                    {
                        blackCatTexture = Resources.Load<Texture2D>(BlackCatResourcePath);
                    }

                    if (blackCatTexture != null)
                    {
                        blackCatSprite = Sprite.Create(
                            blackCatTexture,
                            new Rect(0f, 0f, blackCatTexture.width, blackCatTexture.height),
                            new Vector2(0.5f, 0.5f),
                            100f);
                        return blackCatSprite;
                    }

                    break;
            }

            return GetFallbackSprite();
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackSprite != null)
            {
                return fallbackSprite;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }

            texture.Apply();
            fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
            return fallbackSprite;
        }
    }
}
