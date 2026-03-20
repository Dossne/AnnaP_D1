using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceDefinition
    {
        public PieceDefinition(string id, params Vector2Int[] cells)
        {
            Id = id;
            Cells = cells;
        }

        public string Id { get; }
        public Vector2Int[] Cells { get; }
        public int Size => Cells.Length;
    }
}
