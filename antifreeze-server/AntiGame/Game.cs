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
            for (i = 0; i < unitsCount; i++)
            {
                Cell cell;
                emptyPositionIndex = rnd.Next(cellsCount - i);
                positionIndex = 0;
                for (j = 0; j < cellsCount - i; j++)
                {
                    cell = _grid.Cells[j];
                    if (cell.IsOccupied) continue;
                    if (emptyPositionIndex == positionIndex) break;
                    positionIndex++;
                }

                cell = _grid.Cells[positionIndex]; // System.ArgumentOutOfRangeException: 'Index was out of range. Must be non-negative and less than the size of the collection. Arg_ParamName_Name'
                Unit unit = new Unit(i, cell);
                _units.Add(unit);

            }

        }

        public void ApplyClientMessage(string stringMessage)
        {
            try
            {
                var message = MessageSerializator.Deserialize(stringMessage);
                var orders = message.UnitsDestinationOrders;
                ApplyUserUnput(orders);
            }
            catch (Exception e)
            {
                Console.WriteLine("user input deserialization error: {0}", e.ToString());
            }
        }

        public void ApplyUserUnput(List<UnitDestinationOrderDTO> unitsDestOrders)
        {
            try
            {
                if (unitsDestOrders == null) { return; }

                for (int i = 0; i < unitsDestOrders.Count; i++)
                {
                    var order = unitsDestOrders[i];

                    var unit = _units[order.UnitUid];
                    var cell = _grid.Cells[order.CellUid];

                    unit.SetDestinationCell(cell);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("user input error: {0}", e.ToString());
            }
        }

        public string GetSerializedGameState()
        {

            Message msg = new Message();

            msg.GameState = new GameStateDTO();
            msg.GameState.GridSize = _grid.Size;
            msg.GameState.UnitsCount = _units.Count;

            msg.UnitsStatuses = new List<UnitStatusDTO>();

            for (int i = 0; i < _units.Count; i++)
            {

                var unit = _units[i];
                var unitStatus = new UnitStatusDTO();

                unitStatus.Uid = unit.Uid;
                unitStatus.X = unit.Coords.X;
                unitStatus.Y = unit.Coords.Y;
                unitStatus.IsMoving = unit.IsMoving;

                msg.UnitsStatuses.Add(unitStatus);

            }

            string messageString = MessageSerializator.Serialize(msg);
            return messageString;

        }

        public void Start(int tps)
        {

            // timer runs in different thread !

            if (_timer != null) { return; }

            _timer = new Timer();
            _timer.Elapsed += (sender, args) => _tick();
            _timer.AutoReset = true;
            _timer.Interval = 1000 / tps;
            _timer.Start();

        }

        public event Action<string> OnTick;

        private void _tick()
        {

            var dt = (float)_timer.Interval / 1000;

            for (int i = 0; i < _units.Count; i++)
            {
                _units[i].Tick(_grid, dt);
            }


            Message msg = new Message();

            msg.UnitsStatuses = new List<UnitStatusDTO>();

            for (int i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                var unitStatus = new UnitStatusDTO();
                unitStatus.Uid = unit.Uid;
                unitStatus.IsMoving = unit.IsMoving;
                unitStatus.X = unit.Coords.X;
                unitStatus.Y = unit.Coords.Y;
                msg.UnitsStatuses.Add(unitStatus);
            }

            string messageString = MessageSerializator.Serialize(msg);
            OnTick(messageString);

        }

    }
}
