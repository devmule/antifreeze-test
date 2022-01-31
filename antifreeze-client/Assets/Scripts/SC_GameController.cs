using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SC_GameController : MonoBehaviour
{

    private class SelectionContainer
    {

        public List<SC_AntiGameUnit> SelectedUnits = new List<SC_AntiGameUnit>();

        public void ForEachSelected(Action<SC_AntiGameUnit> fn)
        {
            for (int i = 0; i < SelectedUnits.Count; i++) { fn(SelectedUnits[i]); }
        }

        public void Select(SC_AntiGameUnit unit) 
        {
            if (SelectedUnits.Contains(unit)) return;
            SelectedUnits.Add(unit);
            unit.SetSelected(true);
        }
        public void Diselect(SC_AntiGameUnit unit)
        {
            if (!SelectedUnits.Contains(unit)) return;
            SelectedUnits.Remove(unit);
            unit.SetSelected(false);
        }
        public void DiselectAll() 
        {
            for (int i = 0; i < SelectedUnits.Count; i++)
            {
                var unit = SelectedUnits[i];
                unit.SetSelected(false);
            }
            SelectedUnits.Clear();
        }
    }


    [System.Serializable]
    public class OnDestinationOrderEvent : UnityEvent<int, int> { }


    [SerializeField] public GameObject SelectionArea;
    [SerializeField] public OnDestinationOrderEvent OnDestinationOrder;

    private Vector3 _startPosition = new Vector3();
    private Vector3 _endPosition = new Vector3();

    private SelectionContainer _selectedUnits = new SelectionContainer();
    private RectTransform _selectionArea;

    MeshCollider selectionMeshCollider;
    Mesh selectionMesh;

    private void Awake() { }

    private void Start() 
    {
        _selectionArea = SelectionArea.GetComponent<RectTransform>();
        _selectionArea.gameObject.SetActive(false);
    }

    private void Update()
    {

        bool leftMousePressed = Input.GetMouseButtonDown(0);
        bool leftMouseReleased = Input.GetMouseButtonUp(0);
        bool leftMouseDown = Input.GetMouseButton(0);

        bool rightMousePressed = Input.GetMouseButtonDown(1);

        if (leftMousePressed)
        {
            _selectByClick();
            _startPosition = Input.mousePosition;
            _selectionArea.gameObject.SetActive(true);
        }

        if (leftMouseDown)
        {
            _endPosition = Input.mousePosition;
            var lowerLeft = new Vector3(Mathf.Min(_startPosition.x, _endPosition.x), Mathf.Min(_startPosition.y, _endPosition.y));
            var upperRight = new Vector3(Mathf.Max(_startPosition.x, _endPosition.x),  Mathf.Max(_startPosition.y, _endPosition.y));

            _selectionArea.position = lowerLeft;
            _selectionArea.localScale = upperRight - lowerLeft;
        }

        if (leftMouseReleased)
        {
            _selectionArea.gameObject.SetActive(false);

            if (Vector3.Distance(_startPosition, _endPosition) > 5)
            {
                _selectByArea();
            }
        }

        if (rightMousePressed)
        { 
            _calculateUnitsDestinationOrders(); 
        }

    }

    private void _calculateUnitsDestinationOrders()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 50000.0f)) return;
        var cellComponent = hit.transform?.gameObject?.GetComponent<SC_AntiGameCell>();
        if (cellComponent == null) return;

        _dispatchDestinationOrder(cellComponent);

    }

    private void _selectByClick()
    {

        _selectedUnits.DiselectAll();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 50000.0f)) return;

        var unitComponent = hit.transform.gameObject.GetComponent<SC_AntiGameUnit>();
        if (unitComponent == null) return;

        _selectedUnits.Select(unitComponent);

    }

    private void _selectByArea()
    {
        _selectedUnits.DiselectAll();

        selectionMesh = _generateMesh();

        selectionMeshCollider = gameObject.AddComponent<MeshCollider>();
        selectionMeshCollider.sharedMesh = selectionMesh;
        selectionMeshCollider.convex = true;
        selectionMeshCollider.isTrigger = true;

        selectionMeshCollider.gameObject.transform.SetParent(this.gameObject.transform);
        Destroy(selectionMeshCollider, 0.5f); // todo refactor

    }

    private void OnTriggerEnter(Collider other)
    {

        var unitComponent = other.gameObject.GetComponent<SC_AntiGameUnit>();

        if (unitComponent == null) return;
        _selectedUnits.Select(unitComponent);

    }

    private void _dispatchDestinationOrder(SC_AntiGameCell cellComponent) 
    {

        int destinationCellUid = cellComponent.Uid;
        _selectedUnits.ForEachSelected(unit => OnDestinationOrder.Invoke(unit.Uid, destinationCellUid));
        _selectedUnits.DiselectAll();

    }

    /// <summary>
    /// generate a MeshCollider by mouse positions
    /// </summary>
    private Mesh _generateMesh()
    {

        var lowerLeft = new Vector3(Mathf.Min(_startPosition.x, _endPosition.x), Mathf.Min(_startPosition.y, _endPosition.y));
        var upperRight = new Vector3(Mathf.Max(_startPosition.x, _endPosition.x), Mathf.Max(_startPosition.y, _endPosition.y));

        var boundingBox = new List<Vector2> {
            // order is important
            new Vector2(lowerLeft.x, upperRight.y),
            new Vector2(upperRight.x, upperRight.y),
            new Vector2(lowerLeft.x, lowerLeft.y),
            new Vector2(upperRight.x, lowerLeft.y),
        };

        var near = new Vector3[4];
        var far = new Vector3[4];

        for (int i = 0; i < boundingBox.Count; i++)
        {
            var corner = boundingBox[i];
            near[i] = Camera.main.ScreenToWorldPoint(new Vector3(corner.x, corner.y, 0.01f));
            far[i] = Camera.main.ScreenToWorldPoint(new Vector3(corner.x, corner.y, 50000f));
        }

        var vertices = new Vector3[8];
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };

        for (int i = 0; i < 4; i++) { vertices[i] = near[i]; }

        for (int i = 4; i < 8; i++) { vertices[i] = near[i - 4] + far[i - 4]; }


        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = vertices;
        selectionMesh.triangles = tris;

        return selectionMesh;

    }

}
