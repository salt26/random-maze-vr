﻿using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator
{
    // Adjustable Maze Generator
    private static readonly int[,] _permutation;
    private static readonly int[] _dx;
    private static readonly int[] _dy;
    private float _ratio = .5f;

    static MazeGenerator()
    {
        _permutation = new int[24, 4]
        {
            {0, 1, 2, 3 },
            {0, 1, 3, 2 },
            {0, 2, 1, 3 },
            {0, 2, 3, 1 },
            {0, 3, 1, 2 },
            {0, 3, 2, 1 },
            {1, 0, 2, 3 },
            {1, 0, 3, 2 },
            {1, 2, 0, 3 },
            {1, 2, 3, 0 },
            {1, 3, 0, 2 },
            {1, 3, 2, 0 },
            {2, 0, 1, 3 },
            {2, 0, 3, 1 },
            {2, 1, 0, 3 },
            {2, 1, 3, 0 },
            {2, 3, 0, 1 },
            {2, 3, 1, 0 },
            {3, 0, 1, 2 },
            {3, 0, 2, 1 },
            {3, 1, 0, 2 },
            {3, 1, 2, 0 },
            {3, 2, 0, 1 },
            {3, 2, 1, 0 }
        };
        _dx = new int[4] { -1, 0, 1, 0 };
        _dy = new int[4] { 0, 1, 0, -1 };
    }

    public void SetRatio(float r)
    {
        _ratio = r;
    }

    public float GetRatio()
    {
        return _ratio;
    }

    public int[,] FromDimensions(int rows, int cols, int innerRows, int innerCols, float dropProb = 0f)
    {
        if (innerRows == 0 || innerCols == 0)
        {
            return FromDimensions(rows, cols, dropProb);
        }
        else
        {
            int[,] maze = new int[2 * rows + 1, 2 * cols + 1];
            bool[,] vis = new bool[rows, cols];
            List<int> list = new List<int>();
            InitializeMaze(maze);

            int x1 = (rows - innerRows) / 2, x2 = (rows + innerRows) / 2;
            int y1 = (cols - innerCols) / 2, y2 = (cols + innerCols) / 2;
            for(int i = x1; i < x2; i++)
            {
                for(int j = y1; j < y2; j++)
                {
                    list.Add(i * cols + j);
                    vis[i, j] = true;
                }
            }
            for(int i = x1 + 1; i < x2; i++)
            {
                for(int j = y1 + 1; j < y2; j++)
                {
                    maze[2 * i, 2 * j] = 0;
                }
            }
            for(int i = x1; i < x2; i++)
            {
                for(int j = y1 + 1; j < y2; j++)
                {
                    maze[2 * i + 1, 2 * j] = 0;
                }
            }
            for (int i = x1 + 1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    maze[2 * i, 2 * j + 1] = 0;
                }
            }
            UseAlgorithm(maze, vis, list);

            if (dropProb > 0f)
            {
                for (int i = 2; i < 2 * rows; i += 2)
                {
                    for (int j = 1; j < 2 * cols; j += 2)
                    {
                        float p = Random.Range(0f, 1f);
                        if (p < dropProb)
                            maze[i, j] = 0;
                    }
                }

                for (int i = 1; i < 2 * rows; i += 2)
                {
                    for (int j = 2; j < 2 * cols; j += 2)
                    {
                        float p = Random.Range(0f, 1f);
                        if (p < dropProb)
                            maze[i, j] = 0;
                    }
                }
            }

            return maze;
        }
    }

    public int[,] FromDimensions(int rows, int cols, float dropProb = 0f)
    {
        int[,] maze = new int[2 * rows + 1, 2 * cols + 1];
        bool[,] vis = new bool[rows, cols];
        List<int> list = new List<int>();
        InitializeMaze(maze);

        int x = Random.Range(0, rows);
        int y = Random.Range(0, cols);
        list.Add(x * cols + y);
        vis[x, y] = true;
        UseAlgorithm(maze, vis, list);

        if (dropProb > 0f)
        {
            for (int i = 2; i < 2 * rows; i += 2)
            {
                for (int j = 1; j < 2 * cols; j += 2)
                {
                    float p = Random.Range(0f, 1f);
                    if (p < dropProb)
                        maze[i, j] = 0;
                }
            }

            for (int i = 1; i < 2 * rows; i += 2)
            {
                for (int j = 2; j < 2 * cols; j += 2)
                {
                    float p = Random.Range(0f, 1f);
                    if (p < dropProb)
                        maze[i, j] = 0;
                }
            }
        }

        return maze;
    }

    private void UseAlgorithm(int[,] maze, bool[,] vis, List<int> list)
    {
        int rows = maze.GetLength(0) / 2;
        int cols = maze.GetLength(1) / 2;
        while (list.Count > 0)
        {
            bool hasFind = false;
            int permutationIndex = Random.Range(0, 24);
            int index = Random.Range(0f, 1f) < _ratio ? list.Count - 1 : Random.Range(0, list.Count);
            for (int i = 0; i < 4; i++)
            {
                int dir = _permutation[permutationIndex, i];
                int x = list[index] / cols + _dx[dir];
                int y = list[index] % cols + _dy[dir];
                if (0 <= x && x < rows && 0 <= y && y < cols && !vis[x, y])
                {
                    list.Add(x * cols + y);
                    vis[x, y] = true;
                    hasFind = true;
                    maze[2 * x - _dx[dir] + 1, 2 * y - _dy[dir] + 1] = 0;
                    break;
                }
            }
            if (!hasFind)
            {
                list.RemoveAt(index);
            }
        }
    }

    private void InitializeMaze(int[,] maze)
    {
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                maze[i, j] = (i % 2 == 1 && j % 2 == 1) ? 0 : 1;
            }
        }
    }

    public string ConvertToString(int[,] maze)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                sb.Append(maze[i, j] == 0 ? "." : "#");
            }
            sb.Append("\n");
        }

        return sb.ToString();
    }
}
