using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AntifreezeServer.AntiGame
{
    class Grid
    {


        ///
        ///  0   1   2   3   4   5  ...
        /// 0+w 1+w 2+w 3+w 4+w 5+w ...
        /// ... ... ... ... ... ... ...
        /// 
       
        public int Size { private set; get; }
        public List<Cell> Cells = new List<Cell>();
        public List<Unit> Units = new List<Unit>();

        public Grid(int gridSize, int unitsCount, double unitSpeed)
        {

            Size = gridSize;

            int cellsCount = Size * Size;
            for (int i = 0; i < cellsCount; i++)
            {
                Cells.Add(new Cell(i, new Vector2(i % Size, i / Size)));
            }


            Random rnd = new Random();
            for (int i = 0; i < unitsCount; i++)
            {
                Unit unit = new Unit();
                Units.Add(unit);

                int positionIndex = rnd.Next(Size * Size - i);

                bool foundEmpty = false;
                while (!foundEmpty)
                {
                    if (IsCellOccupied(Cells[positionIndex]))
                    {
                        positionIndex++;
                    }
                    else
                    {
                        foundEmpty = true;
                    }
                }

                unit.CurrentCell = Cells[positionIndex];

            }

        }

        public bool IsCellOccupied(Cell cell)
        { 

            foreach (Unit unit in Units)
            {
                if (unit.CurrentCell == cell)
                {
                    return true;
                }
            }

            return false;

        }

        public Cell? GetCellByCoords(Vector2 coords)
        {

            int x = (int)Math.Round(coords.X);
            int y = (int)Math.Round(coords.Y);

            if (x < 0 || x >= Size || y < 0 || y >= Size) return null;

            int index = y * Size + x;

            return Cells[index];

        }

        public List<Cell> GetCellNeighbours(Cell cell)
        {

            var nbrs = new List<Cell>();

            int x = cell.uid % Size;
            int y = cell.uid / Size;

            if (x > 0) nbrs.Add(Cells[cell.uid - 1]);
            if (x < (Size - 1)) nbrs.Add(Cells[cell.uid + 1]);
            if (y > 0) nbrs.Add(Cells[cell.uid - Size]);
            if (y < (Size - 1)) nbrs.Add(Cells[cell.uid + Size]);

            return nbrs;

        }


        public void Test_BFSFindPath()
        {

            var fromCell = GetCellByCoords(new Vector2(0, 0));
            var toCell = GetCellByCoords(new Vector2(Size - 1, Size - 1));

            var path = BFSFindPath(fromCell, toCell);


            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {

                    string val = " - ";
                    int index = x + y * Size;
                    var cell = Cells[index];

                    if (cell == fromCell) val = " S ";
                    if (cell == toCell) val = " F ";
                    if (IsCellOccupied(cell)) val = " 0 ";
                    if (path.Contains(cell)) val = " * ";

                    Console.Write(val);
                }
                Console.WriteLine();
            }

        }

        public List<Cell> BFSFindPath(Cell fromCell, Cell toCell)
        {

            var frontierNodes = new List<Cell> { fromCell };
            if (fromCell == toCell) return frontierNodes;

            var parentConnections = new Dictionary<Cell, Cell>();
            var checkedNodes = new List<Cell> { fromCell };


            while (frontierNodes.Count > 0)
            {
                var cur = frontierNodes[0];
                frontierNodes.RemoveAt(0);

                var neighbours = GetCellNeighbours(cur);

                for (int i = 0; i < neighbours.Count; i++)
                {
                    var nbr = neighbours[i];

                    if (checkedNodes.IndexOf(nbr) >= 0) continue;

                    checkedNodes.Add(nbr);

                    if (IsCellOccupied(nbr)) continue;

                    frontierNodes.Add(nbr);
                    parentConnections.Add(nbr, cur);

                    if (nbr != toCell) continue;

                    var path = new List<Cell> { toCell };
                    var curPathNode = toCell;
                    while (parentConnections.ContainsKey(curPathNode))
                    {
                        var curPathNeighbour = parentConnections[curPathNode];
                        path.Add(curPathNeighbour);
                        curPathNode = curPathNeighbour;
                    }

                    return path;
                }
            }


            // path to destination not founded - find path to nearest
            float nearestDistance = float.MaxValue;
            Cell nearestNode = fromCell;
            foreach (var cur in parentConnections.Keys)
            {
                float distance = Vector2.Distance(toCell.coords, cur.coords);
                if (distance < nearestDistance)
                {
                    nearestNode = cur;
                    nearestDistance = distance;
                }
            }

            var neartestPath = new List<Cell> { nearestNode };
            var curNearestPathNode = nearestNode;
            while (parentConnections.ContainsKey(curNearestPathNode))
            {
                var curPathNeighbour = parentConnections[curNearestPathNode];
                neartestPath.Add(curPathNeighbour);
                curNearestPathNode = curPathNeighbour;
            }
            return neartestPath;


        }



    }

}
