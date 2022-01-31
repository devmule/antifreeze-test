using System;
using System.Collections.Generic;
using System.Timers;

namespace AntifreezeServer.AntiGame
{
    class AntiGame
    {

        private Timer _timer = null;
        private Grid _grid;
        private List<Unit> _units = new List<Unit>();


        public AntiGame(int gridSize, int unitsCount)
        {

            _grid = new Grid(gridSize);
            int cellsCount = _grid.Size * _grid.Size;

            if (unitsCount > cellsCount)
            {
                Console.WriteLine("Can not to create units more than cells count in grid!");
                unitsCount = cellsCount;
            }

            // randomly place units
            Random rnd = new Random();
            int randomEmptyIndex, selectedGridIndex, i, j;
            var nonEmptyIndices = new List<int>();

            for (i = 0; i < unitsCount; i++)
            {
                selectedGridIndex = 0;
                randomEmptyIndex = rnd.Next(cellsCount - i);
                for (j = 0; j < cellsCount - i; j++)
                {
                    if (nonEmptyIndices.Contains(j)) continue; // do not count occupied positions
                    if (selectedGridIndex == randomEmptyIndex) break;
                    selectedGridIndex++;
                }
                nonEmptyIndices.Add(selectedGridIndex);

                Unit unit = new Unit(i, _grid.Cells[selectedGridIndex]);
                _units.Add(unit);

            }

        }

        public void ApplyClientMessage(string stringMessage)
        {
            try
            {
                var message = GameMessageSerializator.Deserialize(stringMessage);
                var orders = message.UnitsDestinationOrders;
                ApplyUserUnput(orders);
            }
            catch (Exception e)
            {
                Console.WriteLine("user input deserialization error: {0}", e.ToString());
            }
        }

        public void ApplyUserUnput(List<GameUnitDestinationOrderDTO> unitsDestOrders)
        {
            try
            {
                lock (this)
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
            }
            catch (Exception e)
            {
                Console.WriteLine("user input error: {0}", e.ToString());
            }
        }

        public string GetSerializedGameState()
        {

            GameUpdateMessage msg = new GameUpdateMessage();

            msg.GameState = new GameStateDTO();
            msg.GameState.GridSize = _grid.Size;
            msg.GameState.UnitsCount = _units.Count;

            msg.UnitsStatuses = new List<GameUnitStatusDTO>();

            for (int i = 0; i < _units.Count; i++)
            {

                var unit = _units[i];
                var unitStatus = new GameUnitStatusDTO();

                unitStatus.Uid = unit.Uid;
                unitStatus.X = unit.Coords.X;
                unitStatus.Y = unit.Coords.Y;
                unitStatus.IsMoving = unit.IsMoving;

                msg.UnitsStatuses.Add(unitStatus);

            }

            string messageString = GameMessageSerializator.Serialize(msg);
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

        public event Action<string> OnGameUpdated;

        private void _tick()
        {

            // === update game ===
            lock (this)
            {
                var dt = (float)_timer.Interval / 1000;
                for (int i = 0; i < _units.Count; i++) { _units[i].Tick(_grid, dt); }
            }


            // === send update to clients === 

            GameUpdateMessage updates = new GameUpdateMessage();
            updates.UnitsStatuses = new List<GameUnitStatusDTO>();
            for (int i = 0; i < _units.Count; i++)
            {
                var unit = _units[i];
                if (!unit.IsUpdated) continue;

                unit.IsUpdated = false;

                var unitStatus = new GameUnitStatusDTO();
                unitStatus.Uid = unit.Uid;
                unitStatus.IsMoving = unit.IsMoving;
                unitStatus.X = unit.Coords.X;
                unitStatus.Y = unit.Coords.Y;
                updates.UnitsStatuses.Add(unitStatus);
            }

            string serializedUpdates = GameMessageSerializator.Serialize(updates);
            OnGameUpdated?.Invoke(serializedUpdates);

        }

    }
}
