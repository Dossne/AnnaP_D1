using FarmMerger.Board;
using FarmMerger.Pieces;
using UnityEngine;

namespace FarmMerger.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static readonly Color PieceColor = new Color(0.89f, 0.62f, 0.28f, 1f);
        private const int VisiblePieceCount = 3;
        private const float PieceRowSpacing = 2.3f;
        private const float BoardOffsetY = 1.7f;
        private const float PieceRowOffsetY = -1.2f;
        private const float PieceTrayWidth = 7.7f;
        private const float PieceTrayHeight = 2.0f;
        private const float PieceTrayThickness = 0.08f;

        private BoardConfig boardConfig;
        private BoardModel boardModel;
        private BoardView boardView;
        private PieceLibrary pieceLibrary;
        private PieceView[] pieceViews;
        private PieceDefinition[] currentPieces;
        private Vector3[] pieceSlotPositions;
        private Camera targetCamera;
        private Sprite sharedSprite;
        private int paletteCycleIndex;
        private int selectedPieceIndex;
        private bool isDraggingPiece;
        private int draggedPieceIndex = -1;

        private void Awake()
        {
            boardConfig = BoardConfig.CreateDefault();
            boardModel = new BoardModel(boardConfig.Width, boardConfig.Height);
            pieceLibrary = new PieceLibrary();

            CreateBoardView();
            CreatePieceView();
            ConfigureCamera();
            RollNextPiece();
            SelectPiece(0);
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
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
                RollNextPiece();
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

            float baseY = -((boardConfig.TotalHeight * 0.5f) + PieceRowOffsetY);
            float centerOffset = (VisiblePieceCount - 1) * 0.5f;

            CreatePieceTrayFrame(baseY);

            for (int index = 0; index < VisiblePieceCount; index++)
            {
                GameObject pieceObject = new GameObject($"CurrentPiece_{index}");
                pieceObject.transform.SetParent(transform, false);
                pieceSlotPositions[index] = new Vector3((index - centerOffset) * PieceRowSpacing, baseY, 0f);
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
            targetCamera.transform.position = new Vector3(0f, 1.0f, -10f);
            targetCamera.backgroundColor = new Color(0.55f, 0.38f, 0.24f, 1f);
            targetCamera.orthographicSize = Mathf.Max(boardConfig.TotalHeight * 0.76f, boardConfig.TotalWidth * 0.55f) + 0.8f;
        }

        private void RollNextPiece()
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
            currentPieces[index] = pieceLibrary.GetRandom();
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
            ReplacePiece(pieceIndex);
            pieceViews[pieceIndex].SetSelected(true);

            if (boardModel.IsFull)
            {
                paletteCycleIndex++;
                boardModel.Clear();
                boardView.AdvancePalette(paletteCycleIndex);
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
            renderer.color = Color.white;
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

            if (boardModel.CanPlacePiece(piece, hoveredCell))
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

                    if (!boardModel.CanPlacePiece(piece, candidateOrigin))
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
    }
}
