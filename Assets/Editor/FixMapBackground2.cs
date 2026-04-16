using UnityEngine;
using UnityEditor;

public class FixMapBackground2
{
    [MenuItem("Tools/Fix Map Background 2")]
    public static void Execute()
    {
        // 1. Delete the Regeneracion button from Canvas
        var canvasGO = GameObject.Find("Canvas");
        if (canvasGO != null)
        {
            var regenTransform = canvasGO.transform.Find("Regeneracion");
            if (regenTransform != null)
            {
                Undo.DestroyObjectImmediate(regenTransform.gameObject);
                Debug.Log("Deleted Regeneracion button.");
            }
        }

        // 2. Resize MapBackground to cover the full camera view
        var bgGO = GameObject.Find("MapBackground");
        if (bgGO == null)
        {
            Debug.LogError("MapBackground not found!");
            return;
        }

        var sr = bgGO.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("MapBackground has no SpriteRenderer or sprite!");
            return;
        }

        // Camera info: orthographic, size=15, position=(0, 5, -10)
        // Visible area: height = 2 * orthoSize = 30 units
        // Width = height * aspect = 30 * (16/9) ≈ 53.33 units
        // Camera center at Y=5, so visible Y range: -10 to 20
        // Camera center at X=0, so visible X range: -26.67 to 26.67

        float cameraOrthoSize = 15f;
        float cameraY = 5f;
        float visibleHeight = 2f * cameraOrthoSize; // 30
        float aspectRatio = 16f / 9f;
        float visibleWidth = visibleHeight * aspectRatio; // ~53.33

        // Add some margin to ensure full coverage
        float margin = 1.05f;
        float desiredWidth = visibleWidth * margin;
        float desiredHeight = visibleHeight * margin;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        float scaleX = desiredWidth / spriteWidth;
        float scaleY = desiredHeight / spriteHeight;
        float scale = Mathf.Max(scaleX, scaleY); // Cover mode - no black bars

        Undo.RecordObject(bgGO.transform, "Resize MapBackground");
        bgGO.transform.localScale = new Vector3(scale, scale, 1f);

        // Center on camera position
        bgGO.transform.position = new Vector3(0f, cameraY, 0f);

        Debug.Log($"MapBackground resized. Desired: {desiredWidth}x{desiredHeight}, Sprite: {spriteWidth}x{spriteHeight}, Scale: {scale}, Position: (0, {cameraY}, 0)");

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Map background fix 2 complete!");
    }
}
