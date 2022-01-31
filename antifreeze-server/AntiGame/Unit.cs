using System;
using System.Collections.Generic;
using System.Numerics;

namespace AntifreezeServer.AntiGame
{


    class Unit
    {

        private static float _speed = 1f; // cells per second
        private static float _pathFindingPeriod = 0.2f; // seconds between re-searching path
        private static float _timeToOkWithNearestAvailableCell = 1f; // seconds unit continuing researching path to destination

        public int Uid { get; private set; }

        public Vector2 Coords { get; private set; } = new Vector2();

        public bool IsMoving { get { return _neighbourCell != null; } }

        private Cell _destinationCell = null;
        private Cell _neighbourCell = null;
        private Cell _currentCell = null;
        private float _pathToNeighbourFactor = 0.0f;
        private float _pathFindingPeriodValue = 0.0f;
        private float _timeToOkWithNearestCellValue = 0.0f;

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

                _pathFindingPeriodValue += dt;
                if (_pathFindingPeriodValue < _pathFindingPeriod) return;

                _pathFindingPeriodValue = 0;

                var path = grid.FindPath(_currentCell, _destinationCell, cell => cell.IsOccupied && cell != _currentCell);
                if (path.Count < 2)
                {
                    _timeToOkWithNearestCellValue += dt;
                    if (_timeToOkWithNearestCellValue >= _timeToOkWithNearestAvailableCell)
                    {
                        _destinationCell = null;
                    }
                    return;
                }

                _neighbourCell = path[1];
                _pathToNeighbourFactor = 0f;
                _timeToOkWithNearestCellValue = 0f;
                _pathFindingPeriodValue = _pathFindingPeriod;

                _currentCell.IsOccupied = false;
                _neighbourCell.IsOccupied = true;
            }

            _pathToNeighbourFactor += _speed * dt;
            if (_pathToNeighbourFactor > 1f) { _pathToNeighbourFactor = 1f; }

            Coords = _currentCell.Coords * (1 - _pathToNeighbourFactor) + _neighbourCell.Coords * _pathToNeighbourFactor;

            if (_pathToNeighbourFactor < 1) return;

            _pathToNeighbourFactor = 0f;
            _currentCell = _neighbourCell;
            _neighbourCell = null;

            if (_currentCell == _destinationCell) { _destinationCell = null; }
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
            Coords = _currentCell.Coords;

        }

        public void SetDestinationCell(Cell cell)
        {
            if (cell == _destinationCell) return;
            _destinationCell = cell;
            _timeToOkWithNearestCellValue = 0f;
            _pathFindingPeriodValue = _pathFindingPeriod;
        }


    }
}
