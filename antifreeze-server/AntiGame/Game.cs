using System;
using System.Collections.Generic;
using System.Timers;

namespace AntifreezeServer.AntiGame
{
    class Game
    {

        private Timer _timer = null;
        private Grid _grid;
        private List<Unit> _units = new List<Unit>();


        public Game(int gridSize, int unitsCount)
        {


            _grid = new Grid(gridSize);



            int cellsCount = _grid.Size * _grid.Size;

            if (unitsCount > cellsCount)
            {
                Console.WriteLine("Can not to create more units than the cell grid contains!");
                unitsCount = cellsCount;
            }

            // randomly place units
            Random rnd = new Random();
            int emptyPositionIndex, positionIndex, i, j;
            Cell cell;
            for (i = 0; i < unitsCount; i++)
            {
                emptyPositionIndex = rnd.Next(cellsCount - i);
                positionIndex = 0;
                for (j = 0; j < cellsCount; j++)
                {
                    positionIndex++;
                    cell = _grid.Cells[positionIndex];
                    if (cell.IsOccupied) positionIndex++;
                    if (emptyPositionIndex == positionIndex) break;
                }

                cell = _grid.Cells[positionIndex];
                Unit unit = new Unit(i, cell);
                _units.Add(unit);


            }

        }

        public void ApplyUserUnput(Message msg)
        {

            try
            {

                if (msg.UnitsDestinationOrders == null) return;

                for (int i = 0; i < msg.UnitsDestinationOrders.Count; i++)
                {

                    var order = msg.UnitsDestinationOrders[i];

                    var unit = _units[order.UnitUid];
                    var cell = _grid.Cells[order.CellUid];

                    unit.SetDestinationCell(cell);

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("user input error:\n{0}", e.ToString());
            }

        }

        public Message GetGameState()
        {

            Message msg = new Message();

            msg.GameState = new GameStateDTO();
            msg.GameState.GridSize = _grid.Size;
            msg.GameState.UnitsCount = _units.Count;

            msg.UnitsPositions = new List<UnitStatusDTO>();

            for (int i = 0; i < _units.Count; i++)
            {

                var unit = _units[i];
                var unitStatus = new UnitStatusDTO();

                unitStatus.Uid = unit.Uid;
                unitStatus.Coords = unit.Coords;
                unitStatus.IsMoving = unit.IsMoving;

                msg.UnitsPositions.Add(unitStatus);

            }

            return msg;

        }

        public void Start(int tps)
        {

            // timer runs in different thread !

            if (_timer != null) return;

            _timer = new Timer();
            _timer.Elapsed += (sender, args) => _tick();
            _timer.AutoReset = true;
            _timer.Interval = 1000 / tps;
            _timer.Start();

        }

        public event EventHandler<MessageEventArgs> OnTick;

        private void _tick()
        {

            for (int i = 0; i < _units.Count; i++)
            {
                _units[i].Tick(_grid, (float)_timer.Interval);
            }



            Message msg = new Message();
            var e = new MessageEventArgs();
            e.Message = msg;
            OnTick(this, e);

        }

    }
}
