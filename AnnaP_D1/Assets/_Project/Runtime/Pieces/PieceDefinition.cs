using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceDefinition
    {
        public PieceDefinition(string id, params Vector2Int[] cells)
        {
            Id = id;
            Cells = NormalizeCells(cells);

            int maxX = 0;
            int maxY = 0;

            for (int index = 0; index < Cells.Length; index++)
            {
                Vector2Int cell = Cells[index];
                if (cell.x > maxX)
                {
                    maxX = cell.x;
                }

                if (cell.y > maxY)
                {
                    maxY = cell.y;
                }
            }

            Size = Cells.Length;
            Width = maxX + 1;
            Height = maxY + 1;
        }

        public string Id { get; }
        public Vector2Int[] Cells { get; }
        public int Size { get; }
        public int Width { get; }
        public int Height { get; }

        private static Vector2Int[] NormalizeCells(Vector2Int[] cells)
        {
            int minX = cells[0].x;
            int minY = cells[0].y;

            for (int index = 1; index < cells.Length; index++)
            {
                Vector2Int cell = cells[index];
                if (cell.x < minX)
                {
                    minX = cell.x;
                }

                if (cell.y < minY)
                {
                    minY = cell.y;
                }
            }

            Vector2Int[] normalizedCells = new Vector2Int[cells.Length];

            for (int index = 0; index < cells.Length; index++)
            {
                normalizedCells[index] = new Vector2Int(cells[index].x - minX, cells[index].y - minY);
            }

            return normalizedCells;
        }
    }
}
