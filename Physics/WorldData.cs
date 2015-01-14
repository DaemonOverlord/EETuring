namespace EETuring.Physics
{
    public struct WorldData
    {
        public int[][] ForeGroundTiles { get; private set; }

        public int[][][] TileData { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public WorldData(int[][] foregroundTiles, int[][][] tileData, int width, int height) : this()
        {
            ForeGroundTiles = foregroundTiles;
            TileData = tileData;
            Width = width;
            Height = height;
        }
    }
}
