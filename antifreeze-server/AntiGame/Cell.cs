using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AntifreezeServer.AntiGame
{
    class Cell
    {
        public int uid { get; private set; }
        public Vector2 coords { get; private set; }

        public Cell(int uid, Vector2 coords)
        {
            this.uid = uid;
            this.coords = coords;
        }

    }
}
