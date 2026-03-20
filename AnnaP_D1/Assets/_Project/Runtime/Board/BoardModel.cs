using FarmMerger.Pieces;
using UnityEngine;

namespace FarmMerger.Board
{
    public sealed class BoardModel
    {
        private readonly bool[,] cells;
        private int filledCellCount;

        public BoardModel(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new bool[width, height];
        }

        public int Width { get; }
        public int Height { get; }
        public bool IsFull => filledCellCount >= Width * Height;

        public bool IsCellFilled(int x, int y)
        {
            return cells[x, y];
        }

        public bool TryFillCell(int x, int y)
        {
            if (cells[x, y])
            {
                return false;
            }

            cells[x, y] = true;
            filledCellCount++;
            return true;
        }

        public bool CanPlacePiece(PieceDefinition piece, Vector2Int origin)
        {
            for (int index = 0; index < piece.Cells.Length; index++)
            {
                Vector2Int targetCell = origin + piece.Cells[index];

                if (targetCell.x < 0 || targetCell.y < 0 || targetCell.x >= Width || targetCell.y >= Height)
                {
                    return false;
                }

                if (cells[targetCell.x, targetCell.y])
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryPlacePiece(PieceDefinition piece, Vector2Int origin)
        {
            if (!CanPlacePiece(piece, origin))
            {
                return false;
            }

            for (int index = 0; index < piece.Cells.Length; index++)
            {
                Vector2Int targetCell = origin + piece.Cells[index];
                cells[targetCell.x, targetCell.y] = true;
            }

            filledCellCount += piece.Size;
            return true;
        }

        public void FillAll()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    cells[x, y] = true;
                }
            }

            filledCellCount = Width * Height;
        }

        public void Clear()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    cells[x, y] = false;
                }
            }

            filledCellCount = 0;
        }
    }
}
