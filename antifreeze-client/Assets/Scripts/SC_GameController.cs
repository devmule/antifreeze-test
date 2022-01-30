using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SC_GameController : MonoBehaviour
{


    [System.Serializable]
    public class OnDestinationOrderEvent : UnityEvent<int, int> { }


    [SerializeField] public GameObject SelectionArea;
    [SerializeField] public OnDestinationOrderEvent OnDestinationOrder;

    private Vector3 _startPosition = new Vector3();
    private Vector3 _endPosition = new Vector3();

    private List<SC_AntiGameUnit> _selectedUnits = new List<SC_AntiGameUnit>();
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
            selectionMesh = _generateMesh();

            selectionMeshCollider = gameObject.AddComponent<MeshCollider>();
            selectionMeshCollider.sharedMesh = selectionMesh;
            selectionMeshCollider.convex = true;
            selectionMeshCollider.isTrigger = true;

            selectionMeshCollider.gameObject.transform.SetParent(this.gameObject.transform);
            Destroy(selectionMeshCollider, 0.5f); // todo refactor
        }

        if (rightMousePressed)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            SC_AntiGameCell cellComponent = null;
            if (Physics.Raycast(ray, out hit, 50000.0f))
            {
                cellComponent = hit.transform?.gameObject?.GetComponent<SC_AntiGameCell>();
            }

            if (cellComponent != null)
            {
                _dispatchDestinationOrder(cellComponent);
            }

        }

    }

    private void OnTriggerEnter(Collider other)
    {

        var unitComponent = other.gameObject.GetComponent<SC_AntiGameUnit>();

        if (unitComponent == null) return;
        if (_selectedUnits.Contains(unitComponent)) return;

        _selectedUnits.Add(unitComponent);

    }

    private void _dispatchDestinationOrder(SC_AntiGameCell cellComponent) 
    {

        int destinationCellUid = cellComponent.Uid;
        List<int> unitUids = new List<int>();

        for (int i = 0; i < _selectedUnits.Count; i++)
        {
            var unit = _selectedUnits[i];
            unitUids.Add(unit.Uid);
            OnDestinationOrder.Invoke(unit.Uid, destinationCellUid);
        }

        _selectedUnits.Clear();

    }

    /// <summary>
    /// generate a MeshCollider by mouse positions
    /// mesh is a pyramid with rectangular base
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

        var verts = new Vector3[4];
        var vecs = new Vector3[4];

        // generating vertices of pyramid mesh
        for (int i = 0; i < boundingBox.Count; i++)
        {
            var corner = boundingBox[i];
            verts[i] = Camera.main.ScreenToWorldPoint(new Vector3(corner.x, corner.y, 0.01f));
            vecs[i] = Camera.main.ScreenToWorldPoint(new Vector3(corner.x, corner.y, 50000f));
        }

        var vertices = new Vector3[8];
        int[] tris = { 0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3, 7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7 };

        for (int i = 0; i < 4; i++)
        { vertices[i] = verts[i]; }

        for (int j = 4; j < 8; j++)
        { vertices[j] = verts[j - 4] + vecs[j - 4]; }


        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = vertices;
        selectionMesh.triangles = tris;

        return selectionMesh;

        /*
        var lowerLeft = new Vector3(Mathf.Min(_startPosition.x, _endPosition.x), Mathf.Min(_startPosition.y, _endPosition.y));
        var upperRight = new Vector3(Mathf.Max(_startPosition.x, _endPosition.x), Mathf.Max(_startPosition.y, _endPosition.y));

        var boundingBox = new List<Vector2> {
            // order is important
            new Vector2(lowerLeft.x, upperRight.y),
            new Vector2(upperRight.x, upperRight.y),
            new Vector2(lowerLeft.x, lowerLeft.y),
            new Vector2(upperRight.x, lowerLeft.y),
        };

        Vector3[] vertices = new Vector3[5];

        // generating vertices of pyramid mesh
        vertices[0] = Camera.main.transform.position;
        for (int i = 0; i < boundingBox.Count; i++)
        {
            var corner = boundingBox[i];
            vertices[i + 1] = Camera.main.ScreenToWorldPoint(new Vector3(corner.x, corner.y, 50000f));
        }

        Mesh selectionMesh = new Mesh();
        selectionMesh.vertices = vertices;
        selectionMesh.triangles = new int[] {
            0,1,2, 0,2,3, 0,3,4, 0,4,1,
            1,2,3, 1,3,4
        };

        return selectionMesh;*/

    }

}
