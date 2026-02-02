using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Necesario para lógica de listas

public class MapManager : MonoBehaviour
{
    [Header("Configuración de Mapa")]
    public GameObject nodePrefab; 
    public Transform mapParent;
    
    [Range(3, 10)] public int totalFloors = 5; // Cuántos pisos de profundidad
    [Range(2, 4)] public int nodesPerFloorMin = 2;
    [Range(2, 5)] public int nodesPerFloorMax = 4;

    [Header("Espaciado")]
    public float xSpacing = 2.0f; // Distancia horizontal entre nodos
    public float ySpacing = 2.5f; // Distancia vertical entre pisos

    [Header("Estado del Juego")]
    public MapNode currentNode;
    
    // Lista de Listas: Piso 0 -> [NodoA], Piso 1 -> [NodoB, NodoC], etc.
    private List<List<MapNode>> mapStructure = new List<List<MapNode>>();

    private void OnEnable() => MapNode.OnNodeClicked += OnNodeSelected;
    private void OnDisable() => MapNode.OnNodeClicked -= OnNodeSelected;

    void Start()
    {
        GenerateProceduralMap();
    }

    // --- GENERACIÓN DEL MAPA ---
    void GenerateProceduralMap()
    {
        // 1. Crear los Nodos
        for (int floor = 0; floor < totalFloors; floor++)
        {
            List<MapNode> currentFloorNodes = new List<MapNode>();
            
            // Regla: Primer y último piso siempre tienen 1 solo nodo
            int nodesCount = (floor == 0 || floor == totalFloors - 1) ? 1 : Random.Range(nodesPerFloorMin, nodesPerFloorMax + 1);
            
            // Calculamos ancho para centrar
            float floorWidth = (nodesCount - 1) * xSpacing;

            for (int i = 0; i < nodesCount; i++)
            {
                // Matemáticas para centrar los nodos en X
                float x = (-floorWidth / 2) + (i * xSpacing);
                float y = floor * ySpacing;

                // Añadir un poco de desorden (Jitter) para que no sea perfecto
                if (floor > 0 && floor < totalFloors - 1) x += Random.Range(-0.3f, 0.3f);

                MapNode newNode = CreateNode(i, floor, new Vector3(x, y-5, 0));
                currentFloorNodes.Add(newNode);
            }
            mapStructure.Add(currentFloorNodes);
        }

        // 2. Conectar los Nodos (La lógica inteligente)
        ConnectFloors();

        // 3. Dibujar Líneas
        foreach (var list in mapStructure)
            foreach (var node in list)
                node.ShowConnections();

        // 4. Iniciar Juego en el primer nodo (Piso 0, único nodo)
        SetCurrentNode(mapStructure[0][0]);
    }

    MapNode CreateNode(int xIndex, int yIndex, Vector3 pos)
    {
        GameObject obj = Instantiate(nodePrefab, pos, Quaternion.identity, mapParent);
        MapNode node = obj.GetComponent<MapNode>();
        node.Init(xIndex, yIndex, System.Guid.NewGuid().ToString().Substring(0, 5));
        return node;
    }

    void ConnectFloors()
    {
        for (int i = 0; i < mapStructure.Count - 1; i++)
        {
            var currentFloor = mapStructure[i];
            var nextFloor = mapStructure[i + 1];

            // Paso A: Cada nodo actual debe tener AL MENOS 1 salida
            foreach (var node in currentFloor)
            {
                var target = GetRandomNode(nextFloor);
                ConnectNodes(node, target);
            }

            // Paso B: Cada nodo del siguiente piso debe tener AL MENOS 1 entrada
            // (Evitamos islas a las que no se puede llegar)
            foreach (var nextNode in nextFloor)
            {
                if (nextNode.incomingNodes.Count == 0)
                {
                    var parent = GetRandomNode(currentFloor);
                    ConnectNodes(parent, nextNode);
                }
            }
        }
    }

    MapNode GetRandomNode(List<MapNode> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    void ConnectNodes(MapNode from, MapNode to)
    {
        // Evitamos duplicados
        if (!from.outgoingNodes.Contains(to))
        {
            from.outgoingNodes.Add(to);
            to.incomingNodes.Add(from);
        }
    }

    // --- LÓGICA DE JUEGO (MOVIMIENTO) ---

    void OnNodeSelected(MapNode selectedNode)
    {
        SetCurrentNode(selectedNode);
    }

    void SetCurrentNode(MapNode newNode)
    {
        // 1. Nodo anterior pasa a Visited
        if (currentNode != null)
        {
            currentNode.ChangeState(NodeState.Visited);
            currentNode.GetComponent<NodeView>().RefreshVisuals(); // Actualizar color
        }

        // 2. Nuevo nodo pasa a Active
        currentNode = newNode;
        currentNode.ChangeState(NodeState.Active);
        currentNode.GetComponent<NodeView>().RefreshVisuals();

        // 3. Bloquear todo el mapa para limpiar
        foreach(var list in mapStructure)
        {
            foreach(var node in list)
            {
                if (node.currentState != NodeState.Visited && node != currentNode)
                {
                    node.ChangeState(NodeState.Locked);
                    node.GetComponent<NodeView>().RefreshVisuals();
                }
            }
        }

        // 4. Desbloquear solo vecinos
        foreach(var neighbor in currentNode.outgoingNodes)
        {
            neighbor.ChangeState(NodeState.Attainable);
            neighbor.GetComponent<NodeView>().RefreshVisuals();
        }
    }
}