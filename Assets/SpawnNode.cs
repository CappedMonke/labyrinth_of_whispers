using UnityEditor;
using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    public string text = "Spawn Node";

    void OnDrawGizmos()
    {
        Handles.Label(transform.position, text);
    }

    void OnDestroy()
    {
        SpawnSystem spawnSystem = FindFirstObjectByType<SpawnSystem>();
        if (spawnSystem != null)
        {
            spawnSystem.RemoveSpawnNode(this);
        }
        else
        {
            Debug.LogWarning("SpawnSystem not found in the scene.");
        }
    }
}

