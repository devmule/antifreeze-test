using System.Numerics;

namespace AntifreezeServer.AntiGame
{
    class Cell
    {
        public int Uid { get; private set; }
        public Vector2 Coords { get; private set; }
        public bool IsOccupied = false;

        public Cell(int uid, Vector2 coords)
        {
            Uid = uid;
            Coords = coords;
        }

    }
}
