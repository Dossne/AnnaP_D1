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

        private void Awake()
        {
            boardConfig = BoardConfig.CreateDefault();
            boardModel = new BoardModel(boardConfig.Width, boardConfig.Height);
            pieceLibrary = new PieceLibrary();

            CreateBoardView();
            CreatePieceView();
            ConfigureCamera();
            RollNextPiece();
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RollNextPiece();
            }
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
                currentPieces[index] = pieceLibrary.GetRandom();
                pieceViews[index].ShowPiece(currentPieces[index]);
            }

            Debug.Log(
                $"Current pieces: {currentPieces[0].Id}, {currentPieces[1].Id}, {currentPieces[2].Id}");
        }
    }
}
