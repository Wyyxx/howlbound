using UnityEngine;

/// <summary>
/// Scales a SpriteRenderer to always cover the full camera view.
/// Attach to the MapBackground GameObject.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private void Start()
    {
        FitToCamera();
    }

    public void FitToCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        // Get the camera's visible area in world units
        float cameraHeight = 2f * cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // Get the sprite's native size in world units
        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        // Calculate scale to cover the entire camera view (no black bars)
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;
        float scale = Mathf.Max(scaleX, scaleY) * 1.05f; // 5% margin

        transform.localScale = new Vector3(scale, scale, 1f);

        // Center on camera position
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }
}
