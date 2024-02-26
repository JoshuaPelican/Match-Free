using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleBoard : Grid<OrbData>
{
    public Shape BoardShape => new Shape(Width, Height, true);

    //Static Events
    public UnityEvent<int, int> OnDataUpdated = new UnityEvent<int, int>();
    public UnityEvent<int, int> OnDataMatched = new UnityEvent<int, int>();
    public UnityEvent<int, int, int, int> OnDataMoved = new UnityEvent<int, int, int, int>();
    public UnityEvent OnSwap = new UnityEvent();

    public PuzzleBoard(int width, int height, float cellSize = 1, Vector3 originPosition = default) : base(width, height, cellSize, originPosition)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                this[x, y] = OrbData.Empty;
            }
        }
    }

    public PuzzleBoard(PuzzleBoard board) : base(board.Width, board.Height, board.CellSize, board.OriginPosition)
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                this[x, y] = new OrbData(board[x, y].Type);
            }
        }
    }

    public void Set(int x, int y, OrbData newOrbData)
    {
        if (!IsValidCell(x, y))
            return;

        this[x, y] = newOrbData;
        OnDataUpdated.Invoke(x, y);
    }

    public bool TrySwap(int x1, int y1, int x2, int y2)
    {
        if (!IsValidCell(x1, y1) || !IsValidCell(x2, y2))
            return false;

        if (!IsAdjacent(x1, y1, x2, y2, true, 1))
            return false;

        bool isMatch = false;

        //Swap
        OrbData temp = this[x1, y1];
        Set(x1, y1, this[x2, y2]);
        Set(x2, y2, temp);

        if (IsPartOfValidMatch(x1, y1) || IsPartOfValidMatch(x2, y2))
            isMatch = true;

        //Swap Back
        temp = this[x1, y1];
        Set(x1, y1, this[x2, y2]);
        Set(x2, y2, temp);

        return isMatch;
    }

    public void Swap(int x1, int y1, int x2, int y2, bool causeUpdate = true)
    {
        OrbData temp = this[x1, y1];
        Set(x1, y1, this[x2, y2]);
        Set(x2, y2, temp);

        if (!causeUpdate)
            return;

        OnSwap.Invoke();
        OnDataMoved.Invoke(x1, y1, x2, y2);
    }

    public void Replace(int x, int y, Shape shape, OrbData value)
    {
        for (int w = 0; w < shape.Width; w++)
        {
            for (int h = 0; h < shape.Height; h++)
            {
                if (shape[w, h])
                    Set(x + w, y + h, value);
            }
        }
    }

    public void Match(int x, int y, Shape shape)
    {
        //Replace(x, y, shape, OrbData.Empty); //Probably should have custom logic here for matches vs replacing

        for (int w = 0; w < shape.Width; w++) //This is that custom logic for now
        {
            for (int h = 0; h < shape.Height; h++)
            {
                if (shape[w, h])
                {
                    Set(x + w, y + h, OrbData.Empty);
                    OnDataMatched.Invoke(x + w, y + h);
                }
            }
        }
    }

    public void Randomize(int x, int y, Shape shape, bool preventMatches = false)
    {
        for (int w = 0; w < shape.Width; w++)
        {
            for (int h = 0; h < shape.Height; h++)
            {
                this[x + w, y + h] = OrbData.Random;

                while (IsPartOfValidMatch(x + w, y + h) && preventMatches) //Prevent matches on board from randomization
                    this[x + w, y + h] = OrbData.Random;

                Set(x + w, y + h, this[x + w, y + h]);
            }
        }
    }

    public List<MatchData> GetMatches()
    {
        List<MatchData> matchList = new List<MatchData>();
        List<Vector2Int> checkedCoords = new List<Vector2Int>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                //Check extensive neighbors for connections using flood fill

                if (checkedCoords.Contains(new Vector2Int(x, y)))
                    continue;

                checkedCoords.Add(new Vector2Int(x, y));

                if (this[x, y].IsEmpty)
                    continue;

                if (!IsPartOfValidMatch(x, y))
                    continue;

                List<Vector2Int> matchingCoords = GetMatchingFloodFill(x, y);

                foreach (Vector2Int coords in matchingCoords) //For every matching coord
                    if (!checkedCoords.Contains(coords)) //If not already checked, then add to checked
                        checkedCoords.Add(coords);

                if (matchingCoords.Count < 3)
                    continue;

                MatchData newMatch = CreateMatchFromCoords(matchingCoords); //Create the match and add it to the match list
                matchList.Add(newMatch);
            }
        }

        return matchList;
    }

    public void ApplyGravity()
    {
        for (int y = 1; y < Height; y++) //Bottom to top loop instead of left to right
        {
            for (int x = 0; x < Width; x++)
            {
                int currentY = y;

                while (currentY >= 1 && this[x, currentY - 1].IsEmpty) //Need something better here for matched/empty spaces
                {
                    Swap(x, currentY, x, currentY - 1, false);
                    currentY--; //Apply gravity to y pointer
                }

                OnDataMoved.Invoke(x, y, x, currentY);
            }
        }
    }

    public bool IsPartOfValidMatch(int x, int y)
    {
        OrbType matchingType = this[x, y].Type;

        //Horizontal
        int h = 1;
        int horizontalMatches = 1;

        while (IsValidCell(x + h, y) && this[x + h, y].Type == matchingType)
        {
            h++;
            horizontalMatches++;
        }

        h = -1;

        while (IsValidCell(x + h, y) && this[x + h, y].Type == matchingType)
        {
            h--;
            horizontalMatches++;
        }

        //Vertical
        int v = 1;
        int verticalMatches = 1;

        while (IsValidCell(x, y + v) && this[x, y + v].Type == matchingType)
        {
            v++;
            verticalMatches++;
        }

        v = -1;

        while (IsValidCell(x, y + v) && this[x, y + v].Type == matchingType)
        {
            v--;
            verticalMatches++;
        }

        //Check if part of valid match
        if (horizontalMatches >= 3 || verticalMatches >= 3)
        {
            return true;
        }

        return false;
    }

    List<Vector2Int> GetMatchingFloodFill(int x, int y, OrbType matchingType = OrbType.None, List<Vector2Int> checkedCoords = null, List<Vector2Int> matchingCoords = null)
    {
        matchingType = matchingType == OrbType.None ? this[x, y].Type : matchingType;

        checkedCoords ??= new List<Vector2Int>();
        matchingCoords ??= new List<Vector2Int>();

        for (int i = -1; i < 2; i++) //Adjacency Loop
        {
            for (int j = -1; j < 2; j++)
            {
                Vector2Int current = new Vector2Int(x + i, y + j);

                if (Mathf.Abs(i) == Mathf.Abs(j))
                    continue;

                if (!IsValidCell(current.x, current.y))
                    continue;

                if (checkedCoords.Contains(current))
                    continue;
                checkedCoords.Add(current);

                if (this[current.x, current.y].Type != matchingType)
                    continue;

                if (!IsPartOfValidMatch(current.x, current.y)) //If not part of a valid match, continue
                    continue;

                matchingCoords.Add(current);
                GetMatchingFloodFill(current.x, current.y, matchingType, checkedCoords, matchingCoords);
            }
        }

        return matchingCoords;
    }

    MatchData CreateMatchFromCoords(List<Vector2Int> coords)
    {
        int minX = int.MaxValue;
        int maxX = 0;

        int minY = int.MaxValue;
        int maxY = 0;

        foreach (Vector2Int coord in coords)
        {
            if (coord.x < minX) minX = coord.x;
            if (coord.x > maxX) maxX = coord.x;
            if (coord.y < minY) minY = coord.y;
            if (coord.y > maxY) maxY = coord.y;
        }

        int shapeWidth = maxX - minX;
        int shapeHeight = maxY - minY;

        bool[,] shapeGrid = new bool[shapeWidth + 1, shapeHeight + 1];

        foreach (Vector2Int coord in coords)
        {
            shapeGrid[coord.x - minX, coord.y - minY] = true;
        }

        return new MatchData(minX, minY, new Shape(shapeGrid), this[coords[0].x, coords[0].y].Type);
    }
}