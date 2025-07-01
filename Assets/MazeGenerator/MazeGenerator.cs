using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Added for .Where() and .ToList()

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int mazeSize = new(10, 10);
    [SerializeField] private float scale = 1f;
    [SerializeField, Range(0, 100)] private float missingWallPercentage = 5f;
    private GameObject currentMaze = null;

    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        new(0, 1),
        new(1, 0),
        new(0, -1),
        new(-1, 0)
    };

    private class Cell
    {
        public bool visited = false;
        public bool[] walls = new bool[4] { true, true, true, true };
    }

    private readonly List<GameObject> wallObjects = new();
    private readonly List<GameObject> floorObjects = new();

    public void GenerateMaze()
    {
        DeleteCurrentMaze();

        currentMaze = new GameObject("Maze");
        currentMaze.transform.SetParent(transform);

        int width = mazeSize.x;
        int height = mazeSize.y;
        Cell[,] maze = new Cell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = new Cell();

        Stack<Vector2Int> stack = new();
        Vector2Int current = new(0, 0);
        maze[0, 0].visited = true;
        stack.Push(current);

        while (stack.Count > 0)
        {
            current = stack.Peek();
            List<int> unvisitedNeighbors = new();
            for (int i = 0; i < 4; i++)
            {
                Vector2Int next = current + directions[i];
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height && !maze[next.x, next.y].visited)
                {
                    unvisitedNeighbors.Add(i);
                }
            }

            if (unvisitedNeighbors.Count > 0)
            {
                int dir = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                Vector2Int next = current + directions[dir];

                maze[current.x, current.y].walls[dir] = false;
                maze[next.x, next.y].walls[(dir + 2) % 4] = false;

                maze[next.x, next.y].visited = true;
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }

        wallObjects.Clear();
        floorObjects.Clear();

        List<(int x, int y, int dir)> allWallIndices = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int i = 0; i < 4; i++)
                {
                    int nx = x + directions[i].x;
                    int ny = y + directions[i].y;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;
                    if (x < nx || (x == nx && y < ny))
                    {
                        if (maze[x, y].walls[i])
                            allWallIndices.Add((x, y, i));
                    }
                }
            }
        }

        int wallsToRemove = Mathf.FloorToInt(allWallIndices.Count * (missingWallPercentage / 100f));
        if (wallsToRemove > 0 && allWallIndices.Count > 0)
        {
            List<int> indices = new(allWallIndices.Count);
            for (int i = 0; i < allWallIndices.Count; i++) indices.Add(i);
            for (int i = 0; i < wallsToRemove && indices.Count > 0; i++)
            {
                int idx = Random.Range(0, indices.Count);
                var (x, y, dir) = allWallIndices[indices[idx]];
                maze[x, y].walls[dir] = false;
                Vector2Int neighbor = new Vector2Int(x, y) + directions[dir];
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height)
                {
                    maze[neighbor.x, neighbor.y].walls[(dir + 2) % 4] = false;
                }
                indices.RemoveAt(idx);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(currentMaze.transform);
                floor.transform.localScale = new Vector3(scale, 0.1f * scale, scale);
                floor.transform.localPosition = new Vector3(x * scale, -0.45f * scale, y * scale);
                floorObjects.Add(floor);

                for (int i = 0; i < 4; i++)
                {
                    bool drawThisWall = false;
                    Vector3 wallPos = Vector3.zero;
                    Vector3 wallScale = Vector3.one * scale;
                    wallScale.y = scale;

                    if (i == 0)
                    {
                        if (maze[x, y].walls[0])
                        {
                            drawThisWall = true;
                            wallScale.x = scale;
                            wallScale.z = 0.1f * scale;
                            wallPos = new Vector3(x * scale, 0, (y + 0.5f) * scale);
                        }
                    }
                    else if (i == 1)
                    {
                        if (maze[x, y].walls[1])
                        {
                            drawThisWall = true;
                            wallScale.x = 0.1f * scale;
                            wallScale.z = scale;
                            wallPos = new Vector3((x + 0.5f) * scale, 0, y * scale);
                        }
                    }
                    else if (i == 2)
                    {
                        if (y == 0 && maze[x, y].walls[2])
                        {
                            drawThisWall = true;
                            wallScale.x = scale;
                            wallScale.z = 0.1f * scale;
                            wallPos = new Vector3(x * scale, 0, (y - 0.5f) * scale);
                        }
                    }
                    else if (i == 3)
                    {
                        if (x == 0 && maze[x, y].walls[3])
                        {
                            drawThisWall = true;
                            wallScale.x = 0.1f * scale;
                            wallScale.z = scale;
                            wallPos = new Vector3((x - 0.5f) * scale, 0, y * scale);
                        }
                    }

                    if (drawThisWall)
                    {
                        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        wall.transform.SetParent(currentMaze.transform);
                        wall.transform.localScale = wallScale;
                        wall.transform.localPosition = wallPos;
                        wallObjects.Add(wall);
                    }
                }
            }
        }
    }

    private void CombineAndReplace(List<GameObject> objects, string name, bool addCollider)
    {
        List<GameObject> validObjects = objects.Where(obj => obj != null).ToList();

        if (validObjects.Count == 0) return;

        GameObject combined = new(name);
        combined.transform.SetParent(currentMaze.transform);
        combined.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        List<MeshFilter> meshFilters = new();
        foreach (var obj in validObjects)
        {
            if (obj != null && obj.TryGetComponent<MeshFilter>(out var mf))
                meshFilters.Add(mf);
        }

        if (meshFilters.Count == 0)
        {
            Debug.LogWarning($"No valid MeshFilters found for {name} to combine.");
            DestroyImmediate(combined); 
            return;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        for (int i = 0; i < meshFilters.Count; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        combinedMesh.CombineMeshes(combine);

        MeshFilter combinedMF = combined.AddComponent<MeshFilter>();
        combinedMF.sharedMesh = combinedMesh;
        MeshRenderer combinedMR = combined.AddComponent<MeshRenderer>();

        if (validObjects[0] != null && validObjects[0].TryGetComponent<MeshRenderer>(out var mrComponent))
        {
            combinedMR.sharedMaterial = mrComponent.sharedMaterial;
        }
        else
        {
            Debug.LogWarning("Could not find MeshRenderer on the first valid object to get material from. Assigning default material.");
            combinedMR.sharedMaterial = new Material(Shader.Find("Standard"));
        }


        if (addCollider)
        {
            MeshCollider collider = combined.AddComponent<MeshCollider>();
            collider.sharedMesh = combinedMesh;
        }

        foreach (var obj in validObjects)
        {
            DestroyImmediate(obj);
        }
    }

    public void DeleteCurrentMaze()
    {
        if (currentMaze != null)
        {
            DestroyImmediate(currentMaze);
            currentMaze = null;
        }
        wallObjects.Clear();
        floorObjects.Clear();
    }

    public void SaveCurrentMaze()
    {
        if (currentMaze != null)
        {
            CombineAndReplace(wallObjects, "CombinedWalls", true);
            CombineAndReplace(floorObjects, "CombinedFloor", false);

            currentMaze.name = "Maze_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            wallObjects.Clear();
            floorObjects.Clear();
            currentMaze = null;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(MazeGenerator))]
class MazeGeneratorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        MazeGenerator mazeGenerator = (MazeGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Maze"))
        {
            mazeGenerator.GenerateMaze();
        }

        if (GUILayout.Button("Delete Current Maze"))
        {
            mazeGenerator.DeleteCurrentMaze();
        }

        if (GUILayout.Button("Save Current Maze"))
        {
            mazeGenerator.SaveCurrentMaze();
        }
    }
}

#endif