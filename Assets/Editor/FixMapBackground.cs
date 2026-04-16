using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixMapBackground
{
    [MenuItem("Tools/Fix Map Background")]
    public static void Execute()
    {
        // 1. Delete the old UI Fondo from Canvas
        var canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            var fondoTransform = canvasGO.transform.Find("Fondo");
            if (fondoTransform != null)
            {
                Undo.DestroyObjectImmediate(fondoTransform.gameObject);
                Debug.Log("Deleted old Canvas/Fondo UI element.");
            }
        }

        // 2. Create a new world-space background using SpriteRenderer
        var bgGO = new GameObject("MapBackground");
        Undo.RegisterCreatedObjectUndo(bgGO, "Create MapBackground");

        var sr = bgGO.AddComponent<SpriteRenderer>();

        // Load the sprite from the map background image
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/map_01.png");
        if (sprite != null)
        {
            sr.sprite = sprite;
            Debug.Log("Assigned map_01.png sprite to MapBackground.");
        }
        else
        {
            Debug.LogError("Could not load sprite at Assets/Sprites/Fondos/map_01.png");
            return;
        }

        // 3. Set sorting order to be behind everything (nodes use default SpriteRenderer)
        sr.sortingOrder = -10;

        // 4. Position the background at the center of where nodes spawn
        // Nodes spawn from y=-5 (floor 0) to y = (totalFloors-1)*ySpacing - 5
        // With defaults: totalFloors=8, ySpacing=2.5 => y goes from -5 to 12.5
        // Center Y = (-5 + 12.5) / 2 = 3.75
        // Center X = 0 (nodes are centered around x=0)
        // Z = 0 (same plane, sorting order handles layering)
        bgGO.transform.position = new Vector3(0f, 3.75f, 0f);

        // 5. Scale the background to cover the map area nicely
        // The sprite's native size in world units depends on its pixels-per-unit
        // We'll calculate the needed scale to cover the map area
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;

        // Map area: roughly 12 units wide (nodes spread ~-4 to 4 with some margin) x 20 units tall
        float desiredWidth = 14f;
        float desiredHeight = 22f;

        float scaleX = desiredWidth / spriteWidth;
        float scaleY = desiredHeight / spriteHeight;
        float scale = Mathf.Max(scaleX, scaleY); // Use max to ensure full coverage

        bgGO.transform.localScale = new Vector3(scale, scale, 1f);

        Debug.Log($"MapBackground created. Sprite size: {spriteWidth}x{spriteHeight}, Scale: {scale}");

        // 6. Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Map background fix complete!");
    }
}
