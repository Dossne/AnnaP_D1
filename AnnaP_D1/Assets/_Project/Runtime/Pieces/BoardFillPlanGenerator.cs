using System.Collections.Generic;
using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class BoardFillPlanGenerator
    {
        private readonly IReadOnlyList<PieceDefinition> pieces;
        private readonly List<PieceDefinition> workingPlan = new List<PieceDefinition>();

        public BoardFillPlanGenerator(IReadOnlyList<PieceDefinition> availablePieces)
        {
            pieces = availablePieces;
        }

        public bool TryGeneratePlan(int width, int height, out List<PieceDefinition> plan)
        {
            bool[,] occupiedCells = new bool[width, height];
            workingPlan.Clear();

            bool success = TryFillBoard(width, height, occupiedCells);
            plan = success ? new List<PieceDefinition>(workingPlan) : null;

            if (success)
            {
                Shuffle(plan);
            }

            return success;
        }

        private bool TryFillBoard(int width, int height, bool[,] occupiedCells)
        {
            if (!TryFindFirstEmptyCell(width, height, occupiedCells, out Vector2Int emptyCell))
            {
                return true;
            }

            List<PlacementCandidate> candidates = BuildCandidates(width, height, occupiedCells, emptyCell);

            foreach (PlacementCandidate candidate in candidates)
            {
                SetOccupied(candidate.Piece, candidate.Origin, occupiedCells, true);
                workingPlan.Add(candidate.Piece);

                if (TryFillBoard(width, height, occupiedCells))
                {
                    return true;
                }

                workingPlan.RemoveAt(workingPlan.Count - 1);
                SetOccupied(candidate.Piece, candidate.Origin, occupiedCells, false);
            }

            return false;
        }

        private List<PlacementCandidate> BuildCandidates(
            int width,
            int height,
            bool[,] occupiedCells,
            Vector2Int emptyCell)
        {
            List<PlacementCandidate> candidates = new List<PlacementCandidate>();

            for (int pieceIndex = 0; pieceIndex < pieces.Count; pieceIndex++)
            {
                PieceDefinition piece = pieces[pieceIndex];

                for (int cellIndex = 0; cellIndex < piece.Cells.Length; cellIndex++)
                {
                    Vector2Int origin = emptyCell - piece.Cells[cellIndex];

                    if (!CanPlace(piece, origin, width, height, occupiedCells))
                    {
                        continue;
                    }

                    candidates.Add(new PlacementCandidate(piece, origin));
                }
            }

            Shuffle(candidates);
            candidates.Sort((left, right) => right.Piece.Size.CompareTo(left.Piece.Size));
            return candidates;
        }

        private static bool CanPlace(
            PieceDefinition piece,
            Vector2Int origin,
            int width,
            int height,
            bool[,] occupiedCells)
        {
            for (int index = 0; index < piece.Cells.Length; index++)
            {
                Vector2Int targetCell = origin + piece.Cells[index];

                if (targetCell.x < 0 || targetCell.y < 0 || targetCell.x >= width || targetCell.y >= height)
                {
                    return false;
                }

                if (occupiedCells[targetCell.x, targetCell.y])
                {
                    return false;
                }
            }

            return true;
        }

        private static void SetOccupied(
            PieceDefinition piece,
            Vector2Int origin,
            bool[,] occupiedCells,
            bool value)
        {
            for (int index = 0; index < piece.Cells.Length; index++)
            {
                Vector2Int targetCell = origin + piece.Cells[index];
                occupiedCells[targetCell.x, targetCell.y] = value;
            }
        }

        private static bool TryFindFirstEmptyCell(
            int width,
            int height,
            bool[,] occupiedCells,
            out Vector2Int emptyCell)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (occupiedCells[x, y])
                    {
                        continue;
                    }

                    emptyCell = new Vector2Int(x, y);
                    return true;
                }
            }

            emptyCell = default;
            return false;
        }

        private static void Shuffle<T>(IList<T> items)
        {
            for (int index = items.Count - 1; index > 0; index--)
            {
                int swapIndex = Random.Range(0, index + 1);
                T temp = items[index];
                items[index] = items[swapIndex];
                items[swapIndex] = temp;
            }
        }

        private readonly struct PlacementCandidate
        {
            public PlacementCandidate(PieceDefinition piece, Vector2Int origin)
            {
                Piece = piece;
                Origin = origin;
            }

            public PieceDefinition Piece { get; }
            public Vector2Int Origin { get; }
        }
    }
}
