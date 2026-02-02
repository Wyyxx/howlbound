using UnityEngine;

[RequireComponent(typeof(MapNode))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))] // Aseguramos el collider
public class NodeView : MonoBehaviour
{
    private MapNode mapNode;
    private SpriteRenderer sr;

    [Header("Paleta de Colores")]
    public Color colorLocked = new Color(0.5f, 0.5f, 0.5f);
    public Color colorAttainable = Color.white;
    public Color colorVisited = new Color(0.2f, 0.2f, 0.2f);
    public Color colorActive = Color.green;

    [Header("Animación")]
    public Color colorHover = Color.yellow;
    public float scaleHover = 1.2f;

    void Awake()
    {
        mapNode = GetComponent<MapNode>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Método público para refrescar el color cuando el Manager cambie el estado
    public void RefreshVisuals()
    {
        transform.localScale = Vector3.one; // Resetear escala

        switch (mapNode.currentState)
        {
            case NodeState.Locked: sr.color = colorLocked; break;
            case NodeState.Attainable: sr.color = colorAttainable; break;
            case NodeState.Visited: sr.color = colorVisited; break;
            case NodeState.Active: sr.color = colorActive; break;
        }
    }

    // --- MOUSE ---
    void OnMouseEnter()
    {
        if (mapNode.currentState == NodeState.Attainable)
        {
            sr.color = colorHover;
            transform.localScale = Vector3.one * scaleHover;
        }
    }

    void OnMouseExit()
    {
        // Al salir, volvemos al color que nos corresponde según el estado
        RefreshVisuals();
    }

    void OnMouseDown()
    {
        mapNode.ClickNode();
    }
}