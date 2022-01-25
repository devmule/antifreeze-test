using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace antifreeze_server
{
    class Game
    {

        private Timer timer = null;

        public ServerListener server { get; set; }


        public Game(int gridSize, int unitsCount, double unitSpeed)
        {

        }

        public void Start(int tps)
        {

            if (timer != null) return;

            timer = new Timer();
            timer.Elapsed += (sender, args) => Tick();
            timer.AutoReset = true;
            timer.Interval = 1000 / tps;
            timer.Start();

        }

        private void Tick()
        {
        }

    }
}
