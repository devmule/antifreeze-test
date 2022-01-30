using System;
using System.Collections.Generic;
using System.Numerics;

namespace AntifreezeServer.AntiGame
{
    /// <summary>
    /// Data shared between game Units
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

        public List<Cell> GetCellNeighbours(Cell cell)
        {
            var nbrs = new List<Cell>();
            int x = cell.Uid % Size;
            int y = cell.Uid / Size;
            int s = Size - 1;

            if (x > 0) nbrs.Add(Cells[(x - 1) + y * Size]);
            if (x < s) nbrs.Add(Cells[(x + 1) + y * Size]);
            if (y > 0) nbrs.Add(Cells[x + (y - 1) * Size]);
            if (y < s) nbrs.Add(Cells[x + (y + 1) * Size]);

            return nbrs;
        }


        /// <summary>
        /// BFS algorithm implementation with "nearest available" modification
        /// </summary>
        public List<Cell> FindPath(Cell fromCell, Cell destinationCell, Func<Cell, bool> isCellSolid = null)
        {

            var frontierCells = new List<Cell> { fromCell };

            if (fromCell == destinationCell)
            {
                return frontierCells;
            }

            var pathConnections = new Dictionary<Cell, Cell>();
            var checkedNodes = new List<Cell> { fromCell };

            var nearestAvailableCell = fromCell;
            float nearestDistance = float.MaxValue;
            float currentDistance;

            Func<Cell, List<Cell>> buildPathTo = (choosedDestinationCell) =>
            {
                Cell curPathNeighbour;
                var path = new List<Cell> { choosedDestinationCell };
                var curPathNode = choosedDestinationCell;
                while (pathConnections.ContainsKey(curPathNode))
                {
                    curPathNeighbour = pathConnections[curPathNode];
                    path.Insert(0, curPathNeighbour);
                    curPathNode = curPathNeighbour;
                }
                return path;
            };


            while (frontierCells.Count > 0)
            {
                var currentCell = frontierCells[0];
                frontierCells.RemoveAt(0);

                currentDistance = Vector2.Distance(destinationCell.Coords, currentCell.Coords); // System.NullReferenceException: 'Object reference not set to an instance of an object.'
                if (currentDistance < nearestDistance)
                {
                    nearestAvailableCell = currentCell;
                    nearestDistance = currentDistance;
                }

                var cellNeighbours = GetCellNeighbours(currentCell);

                for (int i = 0; i < cellNeighbours.Count; i++)
                {
                    var neighbour = cellNeighbours[i];

                    if (checkedNodes.IndexOf(neighbour) >= 0) continue;

                    checkedNodes.Add(neighbour);

                    if (isCellSolid != null && isCellSolid(neighbour)) continue;

                    frontierCells.Add(neighbour);
                    pathConnections.Add(neighbour, currentCell);

                    if (neighbour != destinationCell) continue;

                    return buildPathTo(destinationCell);
                }
            }

            return buildPathTo(nearestAvailableCell);

        }

    }

}
