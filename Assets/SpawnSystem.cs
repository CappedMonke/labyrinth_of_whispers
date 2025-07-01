using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    public List<SpawnNode> spawnNodes;
    public string nodeName = "SpawnNode";
    public string lastSpawnNodeName = "";
    private Player player;

    void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene");
        }
    }

    public void SpawnPlayerAtLastSpawn()
    {
        if (string.IsNullOrEmpty(lastSpawnNodeName))
        {
            Debug.LogWarning("Last spawn node name is not set.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player object is not assigned.");
            return;
        }

        if (spawnNodes.Count == 0)
        {
            Debug.LogWarning("No spawn nodes available to spawn player at.");
            return;
        }

        SpawnNode targetNode = spawnNodes.Find(node => node.name == lastSpawnNodeName);
        if (targetNode == null)
        {
            Debug.LogWarning($"No spawn node found with name: {lastSpawnNodeName}");
            return;
        }

        player.transform.position = targetNode.transform.position;
    }

    public void RemoveSpawnNode(SpawnNode node)
    {
        if (spawnNodes.Contains(node))
        {
            spawnNodes.Remove(node);
            Destroy(node.gameObject);
            Debug.Log($"Spawn node '{node.name}' removed.");
        }
        else
        {
            Debug.LogWarning($"Spawn node '{node.name}' not found in the list.");
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(SpawnSystem))]
class SpawnSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpawnSystem spawnSystem = (SpawnSystem)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Spawn Node"))
        {
            GenerateSpawnNode();
        }

        if (GUILayout.Button("Spawn Player at Last Spawn"))
        {
            spawnSystem.SpawnPlayerAtLastSpawn();
        }
    }

    private void GenerateSpawnNode()
    {
        SpawnSystem spawnSystem = (SpawnSystem)target;

        GameObject newNode = new(spawnSystem.nodeName);
        SpawnNode spawnNodeComponent = newNode.AddComponent<SpawnNode>();
        spawnNodeComponent.text = spawnSystem.nodeName;
        newNode.transform.position = Vector3.zero;
        newNode.transform.SetParent(spawnSystem.transform);

        spawnSystem.spawnNodes.Add(spawnNodeComponent);
        spawnSystem.lastSpawnNodeName = spawnSystem.nodeName;

        EditorUtility.SetDirty(spawnSystem);
    }
}

#endif