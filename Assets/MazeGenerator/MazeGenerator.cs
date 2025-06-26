using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int mazeSize = new(10, 10);
    [SerializeField] private float scale = 1f;
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
            List<int> unvisitedNeighbors = new List<int>();
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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(currentMaze.transform);
                floor.transform.localScale = new Vector3(scale, 0.1f * scale, scale);
                floor.transform.localPosition = new Vector3(x * scale, -0.45f * scale, y * scale);

                for (int i = 0; i < 4; i++)
                {
                    if (maze[x, y].walls[i])
                    {
                        Vector3 wallPos = Vector3.zero;
                        Vector3 wallScale = Vector3.one * scale;
                        wallScale.y = scale;
                        wallScale.x = (i % 2 == 0) ? scale : 0.1f * scale;
                        wallScale.z = (i % 2 == 1) ? scale : 0.1f * scale;

                        if (i == 0)
                            wallPos = new Vector3(x * scale, 0, (y + 0.5f) * scale);
                        else if (i == 1)
                            wallPos = new Vector3((x + 0.5f) * scale, 0, y * scale);
                        else if (i == 2)
                            wallPos = new Vector3(x * scale, 0, (y - 0.5f) * scale);
                        else if (i == 3)
                            wallPos = new Vector3((x - 0.5f) * scale, 0, y * scale);

                        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        wall.transform.SetParent(currentMaze.transform);
                        wall.transform.localScale = wallScale;
                        wall.transform.localPosition = wallPos;
                    }
                }
            }
        }
    }

    public void DeleteCurrentMaze()
    {
        if (currentMaze != null)
        {
            DestroyImmediate(currentMaze);
            currentMaze = null;
        }
    }

    public void SaveCurrentMaze()
    {
        if (currentMaze != null)
        {
            currentMaze.name = "Maze_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
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