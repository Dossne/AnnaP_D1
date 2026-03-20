using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceLibrary
    {
        private readonly PieceDefinition[] pieces =
        {
            new PieceDefinition("Single", new Vector2Int(0, 0)),
            new PieceDefinition("Domino", new Vector2Int(0, 0), new Vector2Int(1, 0)),
            new PieceDefinition("Bar3", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)),
            new PieceDefinition("Square2", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1)),
            new PieceDefinition("L3", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0))
        };

        public PieceDefinition GetRandom()
        {
            return pieces[Random.Range(0, pieces.Length)];
        }
    }
}
