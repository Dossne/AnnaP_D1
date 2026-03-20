using FarmMerger.Board;
using FarmMerger.Pieces;
using UnityEngine;

namespace FarmMerger.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static readonly Color PieceColor = new Color(0.89f, 0.62f, 0.28f, 1f);

        private BoardConfig boardConfig;
        private BoardModel boardModel;
        private BoardView boardView;
        private PieceLibrary pieceLibrary;
        private PieceView pieceView;
        private PieceDefinition currentPiece;
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
            GameObject pieceObject = new GameObject("CurrentPiece");
            pieceObject.transform.SetParent(transform, false);
            pieceObject.transform.localPosition = new Vector3(0f, -((boardConfig.TotalHeight * 0.5f) + 1.2f), 0f);

            pieceView = pieceObject.AddComponent<PieceView>();
            pieceView.Initialize(PieceColor);
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
            targetCamera.orthographicSize = Mathf.Max(boardConfig.TotalHeight * 0.76f, boardConfig.TotalWidth * 0.55f) + 0.8f;
        }

        private void RollNextPiece()
        {
            currentPiece = pieceLibrary.GetRandom();
            pieceView.ShowPiece(currentPiece);
            Debug.Log($"Current piece: {currentPiece.Id} ({currentPiece.Size} cells, {currentPiece.Width}x{currentPiece.Height})");
        }
    }
}
