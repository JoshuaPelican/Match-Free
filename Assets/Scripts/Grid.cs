using System.Collections.Generic;
using UnityEngine;

public class Grid<T>
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }
    public Vector3 OriginPosition { get; private set; }

    public delegate void GridEvent(int x, int y);
    public event GridEvent OnGridValueChanged;

    T[,] grid;

    public Grid(int width, int height, float cellSize = 1, Vector3 originPosition = default)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        OriginPosition = originPosition;

        grid = new T[Width, Height];
    }

    public T this[int x, int y]
    {
        get { return GetValue(x, y); }

        set { SetValue(x, y, value); }
    }

    public T this[Vector3 pos]
    {
        get { return GetValue(pos); }

        set { SetValue(pos, value); }
    }

    public override string ToString()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int y = Height - 1; y >= 0; y--)
        {
            builder.Append("{");
            for (int x = 0; x < Width; x++)
                builder.Append($"{grid[x, y]}");
            builder.Append("}\n");
        }
        return builder.ToString();
    }

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool IsValidCell(Vector3 worldPosition)
    {
        int x, y;
        GetCoordinates(worldPosition, out x, out y);
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public bool IsAdjacent(int x1, int y1, int x2, int y2, bool vonNeumann, int range)
    {
        if (x1 == x2 && y1 == y2)
            return false;

        if (!IsValidCell(x1, y1) || !IsValidCell(x2, y2))
            return false;

        for (int i = -range; i < range + 1; i++)
        {
            for (int j = -range; j < range + 1; j++)
            {
                if (vonNeumann)
                    if (i != 0 && j != 0)
                        continue;

                if (x1 + i == x2 && y1 + j == y2)
                    return true;
            }
        }

        return false;
    }

    public List<Vector2Int> GetAdjacent(int x, int y, bool vonNeumann, int range)
    {
        List<Vector2Int> adjacent = new List<Vector2Int>();

        for (int i = -range; i < range + 1; i++)
        {
            for (int j = -range; j < range + 1; j++)
            {
                if (vonNeumann)
                    if (i != 0 && j != 0)
                        continue;

                if (i == 0 && j == 0) // same cell
                    continue;

                if (!IsValidCell(x + i, y + j))
                    continue;

                adjacent.Add(new Vector2Int(x + i, y + j));
            }
        }

        return adjacent;
    }

    public Vector3 GetWorldPosition(int x, int y, bool centered)
    {
        Vector3 worldPos = new Vector3(x - (Width / 2f), y - (Height / 2f)) * CellSize + OriginPosition;
        if (centered)
            worldPos += new Vector3(CellSize / 2f, CellSize / 2f);
        return worldPos;
    }

    public Vector3 GetWorldPosition(Vector3 worldPositon, bool centered)
    {
        GetCoordinates(worldPositon, out int x, out int y);
        return GetWorldPosition(x, y, centered);
    }

    public void GetCoordinates(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(((worldPosition - OriginPosition).x / CellSize) + (Width / 2f));
        y = Mathf.FloorToInt(((worldPosition - OriginPosition).y / CellSize) + (Height / 2f));
    }

    public bool IsEmpty(int x, int y)
    {
        return grid[x, y] == null;
    }

    public int GetCellAlternation(int x, int y, bool invert)
    {
        if ((x + y) % 2 == 0)
            return invert ? 0 : 1;
        return invert ? 1 : 0;
    }

    public void SetOriginAndCellSize(Vector3 origin, float cellSize)
    {
        OriginPosition = origin;
        CellSize = cellSize;
    }

    public void Resize(int newWidth, int newHeight)
    {
        T[,] resizedGrid = new T[newWidth, newHeight];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                resizedGrid[x, y] = grid[x, y];
            }
        }

        Width = newWidth;
        Height = newHeight;

        grid = resizedGrid;

    }

    void SetValue(int x, int y, T value)
    {
        if (!IsValidCell(x, y))
            return;

        grid[x, y] = value;
        OnGridValueChanged?.Invoke(x, y);
    }

    void SetValue(Vector3 worldPosition, T value)
    {
        GetCoordinates(worldPosition, out int x, out int y);
        SetValue(x, y, value);
    }

    T GetValue(int x, int y)
    {
        if (!IsValidCell(x, y))
            return default;

        return grid[x, y];
    }

    T GetValue(Vector3 worldPosition)
    {
        GetCoordinates(worldPosition, out int x, out int y);
        return GetValue(x, y);
    }
}
