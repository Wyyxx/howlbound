using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CleanDeathPanel
{
    public static void Execute()
    {
        // Find the DeathPanel
        GameObject deathPanel = GameObject.Find("Canvas/DeathPanel");
        if (deathPanel == null)
        {
            Debug.LogError("DeathPanel not found!");
            return;
        }

        // Collect all children and find duplicates
        var children = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < deathPanel.transform.childCount; i++)
        {
            children.Add(deathPanel.transform.GetChild(i));
        }

        // Track names we've seen - keep first, delete duplicates
        var seen = new System.Collections.Generic.HashSet<string>();
        var toDelete = new System.Collections.Generic.List<GameObject>();

        foreach (var child in children)
        {
            if (seen.Contains(child.name))
            {
                toDelete.Add(child.gameObject);
                Debug.Log($"Marking duplicate for deletion: {child.name}");
            }
            else
            {
                seen.Add(child.name);
            }
        }

        // Delete duplicates
        foreach (var obj in toDelete)
        {
            Object.DestroyImmediate(obj);
        }

        // Move DeathPanel to be the last sibling so it renders on top
        deathPanel.transform.SetAsLastSibling();

        EditorUtility.SetDirty(deathPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"Cleanup complete. Deleted {toDelete.Count} duplicates. DeathPanel moved to last sibling.");
    }
}
