using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace AntifreezeServer
{
    class Game
    {

        private Timer timer = null;
        private AntiGame.Grid grid;


        public Game(int gridSize, int unitsCount, double unitSpeed)
        {

            grid = new AntiGame.Grid(gridSize, unitsCount, unitSpeed);
            grid.Test_BFSFindPath();

        }

        public void ApplyUserUnput(Message msg)
        {

        }

        public Message GetGameState()
        {

            Message msg = new Message();

            msg.state = new GameStateDTO();
            msg.state.grid = grid.Size;
            msg.state.units = grid.Units.Count;

            msg.positions = new List<UnitPositioningDTO>();

            // todo

            return msg;

        }

        public void Start(int tps)
        {

            // timer runs in different thread !

            if (timer != null) return;

            timer = new Timer();
            timer.Elapsed += (sender, args) => Tick();
            timer.AutoReset = true;
            timer.Interval = 1000 / tps;
            timer.Start();

        }

        public event EventHandler<OnTickEventArgs> OnTick;

        private void Tick()
        {

            Message msg = new Message();




            OnTickEventArgs e = new OnTickEventArgs();
            e.Message = msg;
            OnTick(this, e);

        }

    }

    public class OnTickEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}
