using System.Collections.Generic;
using UnityEngine;

namespace FarmMerger.Pieces
{
    public sealed class PieceLibrary
    {
        private readonly PieceDefinition[] pieces;

        public PieceLibrary()
        {
            List<PieceDefinition> definitions = new List<PieceDefinition>();

            AddVariants(definitions, "Bar3", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0));
            AddVariants(definitions, "Bar4", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0));
            AddVariants(definitions, "Square2", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1));
            AddVariants(definitions, "L3", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 0));
            AddVariants(definitions, "L4", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 0));
            AddVariants(definitions, "T4", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(1, 1));
            AddVariants(definitions, "Z4", new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(2, 1));
            AddVariants(definitions, "Corner5", new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2));

            pieces = definitions.ToArray();
        }

        public IReadOnlyList<PieceDefinition> Pieces => pieces;

        public PieceDefinition GetRandom()
        {
            return pieces[Random.Range(0, pieces.Length)];
        }

        private static void AddVariants(List<PieceDefinition> definitions, string baseId, params Vector2Int[] cells)
        {
            HashSet<string> seenKeys = new HashSet<string>();
            List<Vector2Int[]> variants = BuildVariants(cells);

            for (int index = 0; index < variants.Count; index++)
            {
                string key = BuildKey(variants[index]);
                if (!seenKeys.Add(key))
                {
                    continue;
                }

                string id = seenKeys.Count == 1 ? baseId : $"{baseId}_V{seenKeys.Count}";
                definitions.Add(new PieceDefinition(id, variants[index]));
            }
        }

        private static List<Vector2Int[]> BuildVariants(Vector2Int[] cells)
        {
            List<Vector2Int[]> variants = new List<Vector2Int[]>();

            for (int rotation = 0; rotation < 4; rotation++)
            {
                Vector2Int[] rotated = Rotate(cells, rotation);
                variants.Add(rotated);
                variants.Add(MirrorX(rotated));
            }

            return variants;
        }

        private static Vector2Int[] Rotate(Vector2Int[] cells, int turns)
        {
            Vector2Int[] rotated = new Vector2Int[cells.Length];

            for (int index = 0; index < cells.Length; index++)
            {
                Vector2Int cell = cells[index];
                rotated[index] = turns switch
                {
                    1 => new Vector2Int(-cell.y, cell.x),
                    2 => new Vector2Int(-cell.x, -cell.y),
                    3 => new Vector2Int(cell.y, -cell.x),
                    _ => cell
                };
            }

            return rotated;
        }

        private static Vector2Int[] MirrorX(Vector2Int[] cells)
        {
            Vector2Int[] mirrored = new Vector2Int[cells.Length];

            for (int index = 0; index < cells.Length; index++)
            {
                mirrored[index] = new Vector2Int(-cells[index].x, cells[index].y);
            }

            return mirrored;
        }

        private static string BuildKey(Vector2Int[] cells)
        {
            PieceDefinition normalized = new PieceDefinition("Key", cells);
            Vector2Int[] sortedCells = (Vector2Int[])normalized.Cells.Clone();

            System.Array.Sort(sortedCells, (left, right) =>
            {
                int yCompare = left.y.CompareTo(right.y);
                return yCompare != 0 ? yCompare : left.x.CompareTo(right.x);
            });

            return string.Join("|", System.Array.ConvertAll(sortedCells, cell => $"{cell.x}:{cell.y}"));
        }
    }
}
