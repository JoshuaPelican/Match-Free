public struct Shape
{
    public int Width, Height;
    public bool[,] Grid;

    public int Count;

    public Shape(int width, int height, bool defaultValue = false)
    {
        Width = width;
        Height = height;

        Count = 0;

        Grid = new bool[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Grid[x, y] = defaultValue;

                if (Grid[x, y]) Count++;
            }
        }
    }

    public Shape(bool[,] grid)
    {
        Width = grid.GetLength(0);
        Height = grid.GetLength(1);

        Count = 0;

        Grid = grid;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Grid[x, y]) Count++;
            }
        }
    }

    public bool this[int x, int y]
    {
        get { return Grid[x, y]; }
        set { Grid[x, y] = value; }
    }

    public override string ToString()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        for (int y = 0; y < Height; y++)
        {
            builder.Append("{");
            for (int x = 0; x < Width; x++)
            {
                builder.Append(Grid[x, y] == true ? 'O' : "__");
            }
            builder.Append("}\n");
        }

        return builder.ToString();
    }

    public static readonly Shape single = new Shape(new bool[1, 1]
    {
        { true }
    });
    public static readonly Shape cross = new Shape(new bool[3, 3]
    {
        { false, true, false },
        { true , true, true  },
        { false, true, false }
    });
    public static readonly Shape square = new Shape(new bool[3, 3]
    {
        { true, true, true },
        { true, true, true },
        { true, true, true }
    });
    //public static Shape board => new Shape(PuzzleGrid.Grid.Width, PuzzleGrid.Grid.Height, true);

    public Shape GetInvert()
    {
        Shape invertedShape = new Shape(Width, Height);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                invertedShape.Grid[x, y] = !Grid[x, y];
            }
        }

        return invertedShape;
    }
}