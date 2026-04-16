using UnityEngine;

/// <summary>
/// Scales a SpriteRenderer to always cover the full camera view.
/// Uses non-uniform scaling to stretch and fill the entire viewport.
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

        // Use non-uniform scale to stretch-fill the entire camera view
        float scaleX = (cameraWidth / spriteWidth) * 1.02f; // tiny margin
        float scaleY = (cameraHeight / spriteHeight) * 1.02f;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        // Center on camera position
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }
}
