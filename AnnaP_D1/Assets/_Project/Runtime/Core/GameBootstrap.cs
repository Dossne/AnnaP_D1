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

        private BoardConfig boardConfig;
        private BoardModel boardModel;
        private BoardView boardView;
        private PieceLibrary pieceLibrary;
        private PieceView[] pieceViews;
        private PieceDefinition[] currentPieces;
        private Camera targetCamera;
        private int paletteCycleIndex;
        private int selectedPieceIndex;

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
                HandlePrimaryClick();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RollNextPiece();
                SelectPiece(0);
            }
        }

        private void HandlePrimaryClick()
        {
            if (targetCamera == null)
            {
                return;
            }

            Vector3 pointerPosition = Input.mousePosition;
            pointerPosition.z = Mathf.Abs(targetCamera.transform.position.z);

            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(pointerPosition);

            if (TrySelectPiece(worldPosition))
            {
                return;
            }

            TryPlaceSelectedPiece(worldPosition);
        }

        private void CreateBoardView()
        {
            GameObject boardObject = new GameObject("Board");
            boardObject.transform.SetParent(transform, false);

            boardView = boardObject.AddComponent<BoardView>();
            boardView.Initialize(boardConfig, boardModel);
        }

        private void CreatePieceView()
        {
            pieceViews = new PieceView[VisiblePieceCount];
            currentPieces = new PieceDefinition[VisiblePieceCount];

            float baseY = -((boardConfig.TotalHeight * 0.5f) + 1.2f);
            float centerOffset = (VisiblePieceCount - 1) * 0.5f;

            for (int index = 0; index < VisiblePieceCount; index++)
            {
                GameObject pieceObject = new GameObject($"CurrentPiece_{index}");
                pieceObject.transform.SetParent(transform, false);
                pieceObject.transform.localPosition = new Vector3((index - centerOffset) * PieceRowSpacing, baseY, 0f);

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
            targetCamera.transform.position = new Vector3(0f, 0f, -10f);
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

        private void TryPlaceSelectedPiece(Vector3 worldPosition)
        {
            if (!boardView.TryGetCellPosition(worldPosition, out Vector2Int originCell))
            {
                return;
            }

            PieceDefinition selectedPiece = currentPieces[selectedPieceIndex];

            if (!boardModel.TryPlacePiece(selectedPiece, originCell))
            {
                Debug.Log("Piece cannot be placed there.");
                return;
            }

            boardView.Refresh();
            ReplacePiece(selectedPieceIndex);
            pieceViews[selectedPieceIndex].SetSelected(true);

            if (!boardModel.IsFull)
            {
                return;
            }

            paletteCycleIndex++;
            boardModel.Clear();
            boardView.AdvancePalette(paletteCycleIndex);
        }
    }
}
