using System.Collections.Generic;
using FarmMerger.Board;
using FarmMerger.Pieces;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace FarmMerger.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static readonly Color PieceColor = new Color(0.98f, 0.91f, 0.63f, 1f);
        private static readonly Color FrameColor = new Color(0.79f, 0.63f, 0.17f, 1f);
        private static readonly Color HudBannerColor = new Color(0.98f, 0.87f, 0.53f, 0.96f);
        private static readonly Color HudPanelColor = new Color(1f, 0.97f, 0.86f, 0.98f);
        private static readonly Color HudOverlayColor = new Color(0.28f, 0.18f, 0.08f, 0.52f);
        private static readonly Color HudAccentColor = new Color(0.79f, 0.63f, 0.17f, 1f);
        private static readonly Color HudTextColor = new Color(0.36f, 0.23f, 0.08f, 1f);
        private const float TargetPortraitAspect = 720f / 1280f;
        private const int VisiblePieceCount = 3;
        private const float PieceRowSpacing = 1.95f;
        private const float BoardOffsetY = 1.3f;
        private const float PieceTrayDistanceBelowBoard = 1.55f;
        private const float PieceTrayWidth = 6.9f;
        private const float PieceTrayHeight = 2.0f;
        private const float PieceTrayThickness = 0.08f;

        private BoardConfig boardConfig;
        private BoardModel boardModel;
        private BoardView boardView;
        private PieceLibrary pieceLibrary;
        private BoardFillPlanGenerator fillPlanGenerator;
        private PieceView[] pieceViews;
        private PieceDefinition[] currentPieces;
        private Vector3[] pieceSlotPositions;
        private Camera targetCamera;
        private Sprite sharedSprite;
        private Font sharedUiFont;
        private Queue<PieceDefinition> plannedPieces = new Queue<PieceDefinition>();
        private Canvas hudCanvas;
        private GameObject miniGameWindow;
        private Button miniGameBannerButton;
        private int paletteCycleIndex;
        private int selectedPieceIndex;
        private bool isDraggingPiece;
        private bool isMiniGameWindowOpen;
        private int draggedPieceIndex = -1;

        private void Awake()
        {
            boardConfig = BoardConfig.CreateDefault();
            boardModel = new BoardModel(boardConfig.Width, boardConfig.Height);
            pieceLibrary = new PieceLibrary();
            fillPlanGenerator = new BoardFillPlanGenerator(pieceLibrary.Pieces);

            CreateBoardView();
            CreatePieceView();
            ConfigureCamera();
            CreateHud();
            GeneratePlannedPieces();
            FillVisiblePieces();
            SelectPiece(0);
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (isMiniGameWindowOpen)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverUi())
                {
                    return;
                }

                TryBeginPieceDrag();
            }

            if (isDraggingPiece && Input.GetMouseButton(0))
            {
                UpdateDraggedPiecePosition();
            }

            if (isDraggingPiece && Input.GetMouseButtonUp(0))
            {
                ReleaseDraggedPiece();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                GeneratePlannedPieces();
                FillVisiblePieces();
                SelectPiece(0);
            }
        }

        private void CreateBoardView()
        {
            GameObject boardObject = new GameObject("Board");
            boardObject.transform.SetParent(transform, false);
            boardObject.transform.localPosition = new Vector3(0f, BoardOffsetY, 0f);

            boardView = boardObject.AddComponent<BoardView>();
            boardView.Initialize(boardConfig, boardModel);
        }

        private void CreatePieceView()
        {
            pieceViews = new PieceView[VisiblePieceCount];
            currentPieces = new PieceDefinition[VisiblePieceCount];
            pieceSlotPositions = new Vector3[VisiblePieceCount];

            float baseY = BoardOffsetY - ((boardConfig.TotalHeight * 0.5f) + PieceTrayDistanceBelowBoard + (PieceTrayHeight * 0.5f));
            float slotWidth = PieceTrayWidth / VisiblePieceCount;
            float leftEdge = -(PieceTrayWidth * 0.5f);

            CreatePieceTrayFrame(baseY);

            for (int index = 0; index < VisiblePieceCount; index++)
            {
                GameObject pieceObject = new GameObject($"CurrentPiece_{index}");
                pieceObject.transform.SetParent(transform, false);
                float slotCenterX = leftEdge + (slotWidth * (index + 0.5f));
                pieceSlotPositions[index] = new Vector3(slotCenterX, baseY, 0f);
                pieceObject.transform.localPosition = pieceSlotPositions[index];

                PieceView pieceView = pieceObject.AddComponent<PieceView>();
                pieceView.Initialize(PieceColor);
                pieceViews[index] = pieceView;
            }
        }

        private void ConfigureCamera()
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
            {
                return;
            }

            targetCamera.orthographic = true;
            targetCamera.transform.position = new Vector3(0f, 0.35f, -10f);
            targetCamera.backgroundColor = new Color(0.55f, 0.38f, 0.24f, 1f);

            float contentTop = BoardOffsetY + (boardConfig.TotalHeight * 0.5f) + 0.45f;
            float trayCenterY = BoardOffsetY - ((boardConfig.TotalHeight * 0.5f) + PieceTrayDistanceBelowBoard + (PieceTrayHeight * 0.5f));
            float contentBottom = trayCenterY - (PieceTrayHeight * 0.5f) - 0.45f;
            float requiredVerticalHalfSize = (contentTop - contentBottom) * 0.5f;
            float requiredHorizontalHalfSize = (Mathf.Max(boardConfig.TotalWidth, PieceTrayWidth) * 0.5f) + 0.35f;

            targetCamera.orthographicSize = Mathf.Max(requiredVerticalHalfSize, requiredHorizontalHalfSize / TargetPortraitAspect);
        }

        private void CreateHud()
        {
            EnsureEventSystem();

            GameObject canvasObject = new GameObject("HudCanvas");
            canvasObject.transform.SetParent(transform, false);

            hudCanvas = canvasObject.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 100;

            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(720f, 1280f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            miniGameBannerButton = CreateButton(
                "MiniGameBanner",
                canvasObject.transform,
                "mini game",
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -118f),
                new Vector2(420f, 108f),
                HudBannerColor,
                HudTextColor,
                34);
            miniGameBannerButton.onClick.AddListener(OpenMiniGameWindow);

            miniGameWindow = CreateWindow(canvasObject.transform);
            miniGameWindow.SetActive(false);
        }

        private GameObject CreateWindow(Transform parent)
        {
            GameObject overlay = CreateUiObject("MiniGameWindow", parent);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.90f, 0.85f, 0.96f, 1f);

            GameObject panel = CreateUiObject("WindowPanel", overlay.transform);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(620f, 1120f);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.90f, 0.85f, 0.96f, 1f);

            Button closeButton = CreateButton(
                "CloseButton",
                panel.transform,
                "Close",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 88f),
                new Vector2(240f, 84f),
                HudAccentColor,
                Color.white,
                30);
            closeButton.onClick.AddListener(CloseMiniGameWindow);

            return overlay;
        }

        private Button CreateButton(
            string objectName,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            Color backgroundColor,
            Color textColor,
            int fontSize)
        {
            GameObject buttonObject = CreateUiObject(objectName, parent);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = buttonObject.AddComponent<Image>();
            image.color = backgroundColor;

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = Color.Lerp(backgroundColor, Color.white, 0.08f);
            colors.pressedColor = Color.Lerp(backgroundColor, Color.black, 0.08f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.55f);
            button.colors = colors;

            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = HudAccentColor;
            outline.effectDistance = new Vector2(4f, -4f);

            CreateText(
                "Label",
                buttonObject.transform,
                label,
                fontSize,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                size - new Vector2(40f, 18f),
                textColor);

            return button;
        }

        private GameObject CreateText(
            string objectName,
            Transform parent,
            string textValue,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            GameObject textObject = CreateUiObject(objectName, parent);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.AddComponent<Text>();
            text.text = textValue;
            text.font = GetSharedUiFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            return textObject;
        }

        private GameObject CreateUiObject(string objectName, Transform parent)
        {
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private Font GetSharedUiFont()
        {
            if (sharedUiFont != null)
            {
                return sharedUiFont;
            }

            sharedUiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return sharedUiFont;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.transform.SetParent(transform, false);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void OpenMiniGameWindow()
        {
            isMiniGameWindowOpen = true;
            miniGameWindow.SetActive(true);

            if (isDraggingPiece && draggedPieceIndex >= 0)
            {
                pieceViews[draggedPieceIndex].SetDragging(false);
                pieceViews[draggedPieceIndex].transform.localPosition = pieceSlotPositions[draggedPieceIndex];
                boardView.HidePlacementPreview();
                isDraggingPiece = false;
                draggedPieceIndex = -1;
            }
        }

        private void CloseMiniGameWindow()
        {
            isMiniGameWindowOpen = false;
            miniGameWindow.SetActive(false);
        }

        private bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }

            return EventSystem.current.IsPointerOverGameObject();
        }

        private void FillVisiblePieces()
        {
            for (int index = 0; index < VisiblePieceCount; index++)
            {
                ReplacePiece(index);
            }

            Debug.Log(
                $"Current pieces: {currentPieces[0].Id}, {currentPieces[1].Id}, {currentPieces[2].Id}");
        }

        private void ReplacePiece(int index)
        {
            if (plannedPieces.Count == 0)
            {
                GeneratePlannedPieces();
            }

            currentPieces[index] = plannedPieces.Dequeue();
            pieceViews[index].ShowPiece(currentPieces[index]);
            pieceViews[index].transform.localPosition = pieceSlotPositions[index];
            pieceViews[index].SetDragging(false);
        }

        private bool TrySelectPiece(Vector3 worldPosition)
        {
            for (int index = 0; index < pieceViews.Length; index++)
            {
                if (!pieceViews[index].ContainsWorldPoint(worldPosition))
                {
                    continue;
                }

                SelectPiece(index);
                return true;
            }

            return false;
        }

        private void SelectPiece(int index)
        {
            selectedPieceIndex = index;

            for (int pieceIndex = 0; pieceIndex < pieceViews.Length; pieceIndex++)
            {
                pieceViews[pieceIndex].SetSelected(pieceIndex == selectedPieceIndex);
            }
        }

        private void TryBeginPieceDrag()
        {
            Vector3 worldPosition = GetPointerWorldPosition();

            if (!TrySelectPiece(worldPosition))
            {
                return;
            }

            isDraggingPiece = true;
            draggedPieceIndex = selectedPieceIndex;
            pieceViews[draggedPieceIndex].SetDragging(true);
            UpdateDraggedPiecePosition();
        }

        private void UpdateDraggedPiecePosition()
        {
            if (!isDraggingPiece)
            {
                return;
            }

            Vector3 worldPosition = GetPointerWorldPosition();
            pieceViews[draggedPieceIndex].transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
            UpdatePlacementPreview(worldPosition);
        }

        private void ReleaseDraggedPiece()
        {
            Vector3 worldPosition = GetPointerWorldPosition();
            int pieceIndex = draggedPieceIndex;

            isDraggingPiece = false;
            draggedPieceIndex = -1;
            boardView.HidePlacementPreview();

            if (TryPlacePieceAtWorldPosition(pieceIndex, worldPosition))
            {
                return;
            }

            pieceViews[pieceIndex].SetDragging(false);
            pieceViews[pieceIndex].transform.localPosition = pieceSlotPositions[pieceIndex];
        }

        private bool TryPlacePieceAtWorldPosition(int pieceIndex, Vector3 worldPosition)
        {
            PieceDefinition selectedPiece = currentPieces[pieceIndex];

            if (!TryResolvePlacementOrigin(worldPosition, selectedPiece, out Vector2Int originCell, out _))
            {
                return false;
            }

            if (!boardModel.TryPlacePiece(selectedPiece, originCell))
            {
                Debug.Log("Piece cannot be placed there.");
                return false;
            }

            boardView.Refresh();

            if (boardModel.IsFull)
            {
                paletteCycleIndex++;
                boardModel.Clear();
                boardView.AdvancePalette(paletteCycleIndex);
                GeneratePlannedPieces();
                FillVisiblePieces();
                SelectPiece(0);
            }
            else
            {
                ReplacePiece(pieceIndex);
                pieceViews[pieceIndex].SetSelected(true);
            }

            return true;
        }

        private void UpdatePlacementPreview(Vector3 worldPosition)
        {
            PieceDefinition draggedPiece = currentPieces[draggedPieceIndex];

            if (!boardView.TryGetCellPosition(worldPosition, out Vector2Int hoveredCell))
            {
                boardView.HidePlacementPreview();
                return;
            }

            if (TryResolvePlacementOrigin(worldPosition, draggedPiece, out Vector2Int resolvedCell, out bool resolvedIsValid))
            {
                boardView.ShowPlacementPreview(draggedPiece, resolvedCell, resolvedIsValid);
                return;
            }

            boardView.ShowPlacementPreview(draggedPiece, hoveredCell, false);
        }

        private Vector3 GetPointerWorldPosition()
        {
            if (targetCamera == null)
            {
                return Vector3.zero;
            }

            Vector3 pointerPosition = Input.mousePosition;
            pointerPosition.z = Mathf.Abs(targetCamera.transform.position.z);
            return targetCamera.ScreenToWorldPoint(pointerPosition);
        }

        private void CreatePieceTrayFrame(float centerY)
        {
            CreateFrameSegment("PieceTrayTop", new Vector3(0f, centerY + (PieceTrayHeight * 0.5f), 0f), new Vector3(PieceTrayWidth, PieceTrayThickness, 1f));
            CreateFrameSegment("PieceTrayBottom", new Vector3(0f, centerY - (PieceTrayHeight * 0.5f), 0f), new Vector3(PieceTrayWidth, PieceTrayThickness, 1f));
            CreateFrameSegment("PieceTrayLeft", new Vector3(-(PieceTrayWidth * 0.5f), centerY, 0f), new Vector3(PieceTrayThickness, PieceTrayHeight + PieceTrayThickness, 1f));
            CreateFrameSegment("PieceTrayRight", new Vector3(PieceTrayWidth * 0.5f, centerY, 0f), new Vector3(PieceTrayThickness, PieceTrayHeight + PieceTrayThickness, 1f));
        }

        private void CreateFrameSegment(string objectName, Vector3 localPosition, Vector3 localScale)
        {
            GameObject frameObject = new GameObject(objectName);
            frameObject.transform.SetParent(transform, false);
            frameObject.transform.localPosition = localPosition;
            frameObject.transform.localScale = localScale;

            SpriteRenderer renderer = frameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.color = FrameColor;
            renderer.sortingOrder = 0;
        }

        private Sprite GetSharedSprite()
        {
            if (sharedSprite != null)
            {
                return sharedSprite;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return sharedSprite;
        }

        private bool TryResolvePlacementOrigin(
            Vector3 worldPosition,
            PieceDefinition piece,
            out Vector2Int resolvedOrigin,
            out bool isValid)
        {
            if (!boardView.TryGetCellPosition(worldPosition, out Vector2Int hoveredCell))
            {
                resolvedOrigin = default;
                isValid = false;
                return false;
            }

            if (CanPlacePieceAndRemainSolvable(piece, hoveredCell))
            {
                resolvedOrigin = hoveredCell;
                isValid = true;
                return true;
            }

            const int searchRadius = 2;
            float bestDistance = float.MaxValue;
            bool foundValidPlacement = false;
            Vector2Int bestOrigin = hoveredCell;

            for (int offsetY = -searchRadius; offsetY <= searchRadius; offsetY++)
            {
                for (int offsetX = -searchRadius; offsetX <= searchRadius; offsetX++)
                {
                    Vector2Int candidateOrigin = new Vector2Int(hoveredCell.x + offsetX, hoveredCell.y + offsetY);

                    if (!CanPlacePieceAndRemainSolvable(piece, candidateOrigin))
                    {
                        continue;
                    }

                    float distance = new Vector2(offsetX, offsetY).sqrMagnitude;
                    if (distance >= bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    bestOrigin = candidateOrigin;
                    foundValidPlacement = true;
                }
            }

            resolvedOrigin = foundValidPlacement ? bestOrigin : hoveredCell;
            isValid = foundValidPlacement;
            return foundValidPlacement;
        }

        private void GeneratePlannedPieces()
        {
            if (fillPlanGenerator.TryGeneratePlan(boardModel.CreateSnapshot(), out List<PieceDefinition> plan))
            {
                plannedPieces = new Queue<PieceDefinition>(plan);
                return;
            }

            Debug.LogWarning("Failed to build guaranteed fill plan. Falling back to random pieces.");

            List<PieceDefinition> fallbackPieces = new List<PieceDefinition>();
            int targetCellCount = boardConfig.Width * boardConfig.Height;
            int totalCells = 0;

            while (totalCells < targetCellCount)
            {
                PieceDefinition piece = pieceLibrary.GetRandom();

                if (totalCells + piece.Size > targetCellCount)
                {
                    continue;
                }

                fallbackPieces.Add(piece);
                totalCells += piece.Size;
            }

            plannedPieces = new Queue<PieceDefinition>(fallbackPieces);
        }

        private bool CanPlacePieceAndRemainSolvable(PieceDefinition piece, Vector2Int origin)
        {
            if (!boardModel.CanPlacePiece(piece, origin))
            {
                return false;
            }

            bool[,] snapshot = boardModel.CreateSnapshot();

            for (int index = 0; index < piece.Cells.Length; index++)
            {
                Vector2Int targetCell = origin + piece.Cells[index];
                snapshot[targetCell.x, targetCell.y] = true;
            }

            return fillPlanGenerator.TryGeneratePlan(snapshot, out _);
        }
    }
}
