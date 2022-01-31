using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SC_AntiGame : MonoBehaviour
{
    [Serializable]
    public class OnUserGaveOrdersToUnitsEvent : UnityEvent<string> { }

    [SerializeField] public GameObject UnitPrefab;
    [SerializeField] public GameObject CellPrefab;
    [SerializeField] public float DistanceBetweenCells = 1.0f;
    [SerializeField] public OnUserGaveOrdersToUnitsEvent OnUserGaveOrdersToUnits;

    private List<GameUnitDestinationOrderDTO> _unitsDestinationOrders = new List<GameUnitDestinationOrderDTO>();

    private int _GridSize = 0;
    private GameObject _cellsContainer;
    private GameObject _unitsContainer;
    private List<SC_AntiGameCell> _cells = new List<SC_AntiGameCell>();
    private List<SC_AntiGameUnit> _units = new List<SC_AntiGameUnit>();


    private void Awake()
    {
        _cellsContainer = new GameObject("Cells");
        _cellsContainer.transform.parent = this.transform;

        _unitsContainer = new GameObject("Units");
        _unitsContainer.transform.parent = this.transform;

    }

    public void ApplyServerMessage(string serializedUpdates)
    {
        try
        {
            var updates = GameMessageSerializator.Deserialize(serializedUpdates);

            if (updates.GameState != null) { 
                _initGameState(updates.GameState); 
            }

            if (updates.UnitsStatuses != null) { 
                for (int i = 0; i < updates.UnitsStatuses.Count; i++)
                {
                    var unitStatus = updates.UnitsStatuses[i];
                    _updateUnitStatus(unitStatus);
                }
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

    }

    public void SetOrderToUnit(int unitUid, int destinsationCellUid)
    {
        lock (_unitsDestinationOrders)
        {
            GameUnitDestinationOrderDTO unitOrder = _unitsDestinationOrders.Find(udo => udo.UnitUid == unitUid);
            if (unitOrder == null)
            {
                unitOrder = new GameUnitDestinationOrderDTO();
            }
            unitOrder.UnitUid = unitUid;
            unitOrder.CellUid = destinsationCellUid;

            _unitsDestinationOrders.Add(unitOrder);
        }
    }

    private void _initGameState(GameStateDTO gameState)
    {

        // create new cells container, delete old
        if (_cellsContainer != null) { Destroy(_cellsContainer); }
        _cellsContainer = new GameObject("Cells");
        _cellsContainer.transform.parent = this.transform;

        _GridSize = gameState.GridSize;

        int cellsCount = _GridSize * _GridSize;

        for (int i = 0; i < cellsCount; i++)
        {
            var cellObjectInstance = Instantiate(CellPrefab);
            cellObjectInstance.transform.position = _convertGameCoordsToUnityCoords(i % _GridSize, i / _GridSize);
            cellObjectInstance.transform.parent = _cellsContainer.transform;
            var cell = cellObjectInstance.GetComponent<SC_AntiGameCell>();
            cell.SetUid(i);
            _cells.Add(cell);
        }


        // create new unity container, delete old
        if (_unitsContainer != null) { Destroy(_unitsContainer); }
        _unitsContainer = new GameObject("Units");
        _unitsContainer.transform.parent = this.transform;

        for (int i = 0; i < gameState.UnitsCount; i++)
        {
            var unitObjectInstance = Instantiate(UnitPrefab);
            unitObjectInstance.transform.parent = _unitsContainer.transform;
            var unit = unitObjectInstance.GetComponent<SC_AntiGameUnit>();
            unit.SetUid(i);
            _units.Add(unit);
        }
    }

    private Vector3 _convertGameCoordsToUnityCoords(float x, float y)
    {
        var gs = ((float)_GridSize - 1) / 2;
        return new Vector3( (x - gs) * DistanceBetweenCells, 0.0f, (gs - y) * DistanceBetweenCells );
    }

    private void _updateUnitStatus(GameUnitStatusDTO unitStatus)
    {
        var unit = _units.Find(u => u.Uid == unitStatus.Uid);
        if (unit == null) return;

        unit.gameObject.transform.position = _convertGameCoordsToUnityCoords(unitStatus.X, unitStatus.Y);
        unit.SetMoving(unitStatus.IsMoving);

    }

    // Update is called once per frame
    void Update()
    {

        lock (_unitsDestinationOrders)
        {
            if (_unitsDestinationOrders.Count > 0)
            {
                var message = new GameUpdateMessage();
                message.UnitsDestinationOrders = _unitsDestinationOrders;
                var messageString = GameMessageSerializator.Serialize(message);
                OnUserGaveOrdersToUnits?.Invoke(messageString);
                _unitsDestinationOrders.Clear();
                _unitsDestinationOrders.TrimExcess();
            }
        }
        

    }
}
