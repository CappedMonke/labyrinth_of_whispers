using UnityEditor;
using UnityEngine;

public class SpawnNode : MonoBehaviour
{
    public string text = "Spawn Node";

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.Label(transform.position, text);
#endif
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

