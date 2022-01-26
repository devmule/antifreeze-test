using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AntifreezeServer.AntiGame
{


    class Unit
    {

        public Vector2 Coords = new Vector2();
        public Cell? DestinationCell = null;
        public Cell? NeighbourCell = null;
        public Cell? CurrentCell = null;
        public float PathToNeighbourFactor = 0.0f;

        public void Tick(float speed)
        {

            if (NeighbourCell == null || DestinationCell == null) return;

            PathToNeighbourFactor += speed;
            if (PathToNeighbourFactor >= 1)
            {

                Coords = (CurrentCell.coords + NeighbourCell.coords) / 2 * PathToNeighbourFactor;

                PathToNeighbourFactor = 0;
                CurrentCell = NeighbourCell;
                NeighbourCell = null;

                if (CurrentCell == DestinationCell)
                {
                    DestinationCell = null;
                    PathToNeighbourFactor = 0;
                }

            }
        }

        public bool GetIsMoving()
        {
            return DestinationCell != null;
        }

    }
}
