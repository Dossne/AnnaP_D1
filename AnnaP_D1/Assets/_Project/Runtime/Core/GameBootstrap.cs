using FarmMerger.Board;
using FarmMerger.Pieces;
using UnityEngine;

namespace FarmMerger.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private BoardConfig boardConfig;
        private BoardModel boardModel;
        private BoardView boardView;
        private PieceLibrary pieceLibrary;
        private PieceDefinition currentPiece;
        private Camera targetCamera;
        private int paletteCycleIndex;

        private void Awake()
        {
            boardConfig = BoardConfig.CreateDefault();
            boardModel = new BoardModel(boardConfig.Width, boardConfig.Height);
            pieceLibrary = new PieceLibrary();

            CreateBoardView();
            ConfigureCamera();
            RollNextPiece();
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryFillCellFromPointer();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                boardModel.FillAll();
                boardView.Refresh();
                CompleteBoardCycle();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RollNextPiece();
            }
        }

        private void TryFillCellFromPointer()
        {
            if (targetCamera == null)
            {
                return;
            }

            Vector3 pointerPosition = Input.mousePosition;
            pointerPosition.z = Mathf.Abs(targetCamera.transform.position.z);

            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(pointerPosition);

            if (!boardView.TryGetCellPosition(worldPosition, out Vector2Int cellPosition))
            {
                return;
            }

            if (!boardModel.TryFillCell(cellPosition.x, cellPosition.y))
            {
                return;
            }

            boardView.Refresh();

            if (boardModel.IsFull)
            {
                CompleteBoardCycle();
            }
        }

        private void CompleteBoardCycle()
        {
            paletteCycleIndex++;
            boardModel.Clear();
            boardView.AdvancePalette(paletteCycleIndex);
            RollNextPiece();
        }

        private void CreateBoardView()
        {
            GameObject boardObject = new GameObject("Board");
            boardObject.transform.SetParent(transform, false);

            boardView = boardObject.AddComponent<BoardView>();
            boardView.Initialize(boardConfig, boardModel);
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
            targetCamera.backgroundColor = new Color(0.93f, 0.96f, 0.98f, 1f);
            targetCamera.orthographicSize = Mathf.Max(boardConfig.TotalHeight * 0.65f, boardConfig.TotalWidth * 0.42f) + 0.8f;
        }

        private void RollNextPiece()
        {
            currentPiece = pieceLibrary.GetRandom();
            Debug.Log($"Next piece prepared: {currentPiece.Id} ({currentPiece.Size} cells)");
        }
    }
}
