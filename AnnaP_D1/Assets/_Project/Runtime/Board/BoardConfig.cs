using UnityEngine;

namespace FarmMerger.Board
{
    public sealed class BoardConfig
    {
        public BoardConfig(
            int width,
            int height,
            float cellSize,
            float cellGap,
            Color baseColor,
            Color filledColor,
            Color[] palette)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            CellGap = cellGap;
            BaseColor = baseColor;
            FilledColor = filledColor;
            Palette = palette;
        }

        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public float CellGap { get; }
        public Color BaseColor { get; }
        public Color FilledColor { get; }
        public Color[] Palette { get; }

        public float TotalWidth => (Width * CellSize) + ((Width - 1) * CellGap);
        public float TotalHeight => (Height * CellSize) + ((Height - 1) * CellGap);

        public static BoardConfig CreateDefault()
        {
            return new BoardConfig(
                width: 10,
                height: 10,
                cellSize: 0.56f,
                cellGap: 0.06f,
                baseColor: new Color(0.97f, 0.85f, 0.38f, 1f),
                filledColor: new Color(0.35f, 0.56f, 0.33f, 1f),
                palette: new[]
                {
                    new Color(0.66f, 0.82f, 0.52f, 1f),
                    new Color(0.96f, 0.74f, 0.39f, 1f),
                    new Color(0.92f, 0.58f, 0.46f, 1f),
                    new Color(0.55f, 0.77f, 0.84f, 1f)
                });
        }
    }
}
