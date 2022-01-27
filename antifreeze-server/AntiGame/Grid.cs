using System;
using System.Collections.Generic;
using System.Numerics;

namespace AntifreezeServer.AntiGame
{
    /// <summary>
    /// 
    /// </summary>
    class Grid
    {
       
        public int Size { private set; get; }
        public List<Cell> Cells = new List<Cell>();

        public Grid(int gridSize)
        {

            Size = gridSize;

            int cellsCount = Size * Size;

            for (int i = 0; i < cellsCount; i++)
            {
                Cells.Add(new Cell(i, new Vector2(i % Size, i / Size)));
            }

        }

        public Cell GetCellByCoords(Vector2 coords)
        {

            int x = (int)Math.Round(coords.X);
            int y = (int)Math.Round(coords.Y);
            int index;

            if (x < 0) x = 0;
            if (x > (Size - 1)) x = Size - 1;
            if (y < 0) y = 0;
            if (y > (Size - 1)) y = Size - 1;

            index = y * Size + x;

            return Cells[index];

        }

        public List<Cell> GetCellNeighbours(Cell cell)
        {

            var nbrs = new List<Cell>();
            int x = cell.Uid % Size;
            int y = cell.Uid / Size;

            if (x > 0) nbrs.Add(Cells[cell.Uid - 1]);
            if (x < (Size - 1)) nbrs.Add(Cells[cell.Uid + 1]);
            if (y > 0) nbrs.Add(Cells[cell.Uid - Size]);
            if (y < (Size - 1)) nbrs.Add(Cells[cell.Uid + Size]);

            return nbrs;

        }


    }

}
