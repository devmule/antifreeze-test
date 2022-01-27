using System;
using System.Collections.Generic;
using System.Numerics;

namespace AntifreezeServer.AntiGame
{


    class Unit
    {

        private static float _speed = 0.5f;

        public int Uid { get; private set; }

        public readonly Vector2 Coords = new Vector2();

        public bool IsMoving { get { return _destinationCell != null; } }

        private Cell? _destinationCell = null;
        private Cell? _neighbourCell = null;
        private Cell? _currentCell = null;
        private float _pathToNeighbourFactor = 0.0f;

        public Unit(int uid, Cell initialPosition)
        {
            Uid = uid;
            _setCurrentCell(initialPosition);
        }

        public void Tick(Grid grid, float dt)
        {


            if (_destinationCell == null) return;


            if (_neighbourCell == null)
            {

                List<Cell> path = BFSFindPath(grid, _currentCell, _destinationCell);

                if (path.Count < 2)
                {
                    StopMoving();
                    return;
                }
                _neighbourCell = path[1];

            }

            if (_neighbourCell.IsOccupied)
            {

            }

            _pathToNeighbourFactor += _speed * dt;

            // Coords = (CurrentCell.Coords + NeighbourCell.Coords) / 2 * PathToNeighbourFactor;
            // Coords.CopyTo(_neighbourCell.Coords);


            if (_pathToNeighbourFactor < 1) return;

            _setCurrentCell(_neighbourCell);
            _neighbourCell = null;

            if (_currentCell == _destinationCell)
            {
                StopMoving();
            }

        }

        private void _setCurrentCell(Cell cell)
        {

            if (cell.IsOccupied) return;

            if (_currentCell != null)
            {
                _currentCell.IsOccupied = false;
            }

            _currentCell = cell;
            _currentCell.IsOccupied = true;

        }

        public void SetDestinationCell(Cell cell)
        {
            if (cell == _destinationCell) return;
            _destinationCell = cell;
            _pathToNeighbourFactor = 0;
        }

        public void StopMoving()
        {
            _neighbourCell = null;
            _destinationCell = null;
            _pathToNeighbourFactor = 0;
        }

        public static List<Cell> BFSFindPath(Grid grid, Cell fromCell, Cell toCell)
        {

            var frontierNodes = new List<Cell> { fromCell };
            if (fromCell == toCell) return frontierNodes;

            var parentConnections = new Dictionary<Cell, Cell>();
            var checkedNodes = new List<Cell> { fromCell };

            Cell curPathNeighbour;

            while (frontierNodes.Count > 0)
            {
                var cur = frontierNodes[0];
                frontierNodes.RemoveAt(0);

                var neighbours = grid.GetCellNeighbours(cur);

                for (int i = 0; i < neighbours.Count; i++)
                {
                    var nbr = neighbours[i];

                    if (checkedNodes.IndexOf(nbr) >= 0) continue;

                    checkedNodes.Add(nbr);

                    if (nbr.IsOccupied) continue;

                    frontierNodes.Add(nbr);
                    parentConnections.Add(nbr, cur);

                    if (nbr != toCell) continue;

                    var path = new List<Cell> { toCell };
                    var curPathNode = toCell;
                    while (parentConnections.ContainsKey(curPathNode))
                    {
                        curPathNeighbour = parentConnections[curPathNode];
                        path.Insert(0, curPathNeighbour);
                        curPathNode = curPathNeighbour;
                    }

                    return path;
                }
            }


            // path to destination not founded - find path to nearest

            var nearestDistance = float.MaxValue;
            var nearestNode = fromCell;
            float distance;

            var neartestPath = new List<Cell> { nearestNode };
            var curNearestPathNode = nearestNode;

            foreach (var cur in parentConnections.Keys)
            {
                distance = Vector2.Distance(toCell.Coords, cur.Coords);
                if (distance < nearestDistance)
                {
                    nearestNode = cur;
                    nearestDistance = distance;
                }
            }

            while (parentConnections.ContainsKey(curNearestPathNode))
            {
                curPathNeighbour = parentConnections[curNearestPathNode];
                neartestPath.Insert(0, curPathNeighbour);
                curNearestPathNode = curPathNeighbour;
            }
            return neartestPath;


        }

        public static void Test_BFSFindPath(Grid grid)
        {

            var fromCell = grid.GetCellByCoords(new Vector2(0, 0));
            var toCell = grid.GetCellByCoords(new Vector2(grid.Size - 1, grid.Size - 1));

            var path = BFSFindPath(grid, fromCell, toCell);


            for (int y = 0; y < grid.Size; y++)
            {
                for (int x = 0; x < grid.Size; x++)
                {

                    string val = " - ";
                    int index = x + y * grid.Size;
                    var cell = grid.Cells[index];

                    if (cell == fromCell) val = " S ";
                    if (cell == toCell) val = " F ";
                    if (cell.IsOccupied) val = " 0 ";
                    if (path.Contains(cell)) val = " * ";

                    Console.Write(val);
                }
                Console.WriteLine();
            }

        }


    }
}
