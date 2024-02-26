using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    public static UnityEvent<MatchData> OnMatch = new();
    public static UnityEvent OnBoardResized = new();
    public static UnityEvent<int, int> OnHighlight = new();

    [Header("Settings")]
    [SerializeField] int BoardSizeX = 5, BoardSizeY = 5;
    [SerializeField] float BoardPadding = 0.05f;
    [SerializeField] float BoardSetupDelay = 0.25f;
    [SerializeField] float CascadeTime = 0.25f;
    [SerializeField] float ComboTime = 0.5f;

    [Header("References")]
    [SerializeField] Orb orbPrefab;
    [SerializeField] Transform orbContainer;
    [SerializeField] Player player;

    static PuzzleBoard board;

    List<MatchData> matches = new List<MatchData>();

    private void OnEnable()
    {
        GameManager.OnPuzzlerTurnEnd.AddListener(CheckIfPlayable);
    }

    private void Awake()
    {
        Invoke(nameof(Setup), BoardSetupDelay);
    }

    private void Start()
    {
        GameManager.SetGameState(GameManager.GameState.PuzzleStarted);
    }

    public void MakeMatch(int x1, int y1, int x2, int y2)
    {
        bool isMatch = board.TrySwap(x1, y1, x2, y2);

        if (!isMatch)
        {
            Debug.Log("Something went wrong! Not a valid match!");
            return;
        }

        board.Swap(x1, y1, x2, y2);
    }

    public void RandomizeBoard()
    {
        board.Randomize(0, 0, board.BoardShape, true);
    }

    public List<MoveData> GetAllPossibleMoves() // Kinda a mess but it works well. Also very slow for sure but its brute force
    {
        List<MoveData> possibleMoves = new List<MoveData>();
        PuzzleBoard copy = new PuzzleBoard(board);

        for (int x = 0; x < copy.Width; x++)
        {
            for (int y = 0; y < copy.Height; y++)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (Mathf.Abs(i) == Mathf.Abs(j))
                            continue;

                        bool isMatch = copy.TrySwap(x, y, x + i, y + j);

                        if (!isMatch)
                            continue;

                        copy.Swap(x, y, x + i, y + j, false);
                        List<MatchData> matches = copy.GetMatches();

                        PuzzleBoard deeperCopy = new PuzzleBoard(copy);
                        List<MatchData> deeperMatches = new List<MatchData>(matches);
                        for (int s = 0; s < 4; s++) //TODO: 4 is depth, should be variable instead
                        {
                            deeperCopy = new PuzzleBoard(deeperCopy);

                            ApplyMatches(deeperCopy, deeperMatches);
                            deeperCopy.ApplyGravity();

                            matches.AddRange(deeperCopy.GetMatches());
                            deeperMatches = new List<MatchData>(deeperCopy.GetMatches());

                            if (deeperMatches.Count == 0) //No more matches found so leave loop
                                break;
                        }

                        float score = 0;
                        float bonus = 0;

                        foreach (MatchData match in matches)
                        {

                            for (int a = 0; a < match.Shape.Width; a++)
                            {
                                for (int b = 0; b < match.Shape.Height; b++)
                                {
                                    if(player.x == (match.X + a) && player.y == (match.Y + b))
                                    {
                                        bonus = 6;
                                        goto done;
                                    }
                                    else if (board.IsAdjacent(player.x, player.y, match.X + a, match.Y + b, true, 1))
                                    {
                                        bonus = 3;
                                    }
                                    else if (board.IsAdjacent(player.x, player.y, match.X + a, match.Y + b, false, 1))
                                    {
                                        if(bonus < 1.25f)
                                            bonus = 1.25f;
                                    }
                                }
                            }
                        
                        done:
                            float distanceBonus = ((BoardSizeX + BoardSizeY) / 2) - Vector2Int.Distance(new Vector2Int(player.x, player.y), new Vector2Int(match.X, match.Y));
                            if (bonus == 0)
                                score += match.Shape.Count + distanceBonus / ((BoardSizeX + BoardSizeY) / 2);
                            else
                                score += match.Shape.Count * bonus;
                        }

                        possibleMoves.Add(new MoveData(x, y, x + i, y + j, matches, score));

                        copy.Swap(x, y, x + i, y + j, false);
                    }
                }
            }
        }

        return possibleMoves;
    }

    void Setup()
    {
        board = new PuzzleBoard(BoardSizeX, BoardSizeY);
        board.OnSwap.AddListener(StartProcessBoard);

        PositionAndScaleBoardToScreen();

        FillEmpty(true);

        player.Initialize(board, board.Width / 2, board.Height / 2);

        GameManager.SetGameState(GameManager.GameState.PlayerTurnStart);
    }

    void CheckIfPlayable()
    {
        List<MoveData> moves = GetAllPossibleMoves();

        while (moves.Count == 0)
        {
            RandomizeBoard();
            moves = GetAllPossibleMoves();
        }
    }

    void ResizeBoard(int newWidth, int newHeight)
    {
        board.Resize(newWidth, newHeight);
        PositionAndScaleBoardToScreen();

        OnBoardResized.Invoke();

        FillEmpty(true);
    }

    void PositionAndScaleBoardToScreen()
    {
        //Calculate board scale based on screen size and board size
        //float aspect = (float)Screen.width / Screen.height;
        float worldHeight = Camera.main.orthographicSize * 2;
        //float worldWidth = worldHeight * aspect;
        float boardScale = worldHeight / board.Height;

        //Calculate board position
        //float boardYOffset = (-worldHeight / 2) + (board.Height * boardScale / 2f);
        //Vector2 boardPosition = new Vector2(0, boardYOffset + BoardPadding);

        board.SetOriginAndCellSize(Vector3.zero, boardScale - BoardPadding * 2);
    }

    void MakeOrb(int x, int y)
    {
        Orb newOrb = Instantiate(
            orbPrefab,
            new Vector2(board.GetWorldPosition(x, y, true).x, (board.Height / 2) + board.CellSize + board.GetWorldPosition(x, y, true).y),
            Quaternion.identity, orbContainer);
        newOrb.Initialize(board, x, y);
    }

    void StartProcessBoard() // TODO: FIX THIS NAMING AND SETUP TO SOMETHING CLEANER
    {
        StartCoroutine(ProcessBoard());
    }

    IEnumerator ProcessBoard()
    {
        matches = board.GetMatches();

        yield return new WaitForSeconds(CascadeTime);

        if (matches.Count == 0) //If board no longer needs processing
        {
            StopAllCoroutines();
            GameManager.SetGameState(GameManager.GameState.PuzzlerTurnEnd);
            yield return null;
        }

        yield return ApplyMatchesSelf(); //Needs yield return to do it over time and wait till done
        yield return new WaitForSeconds(CascadeTime / 2);
        board.ApplyGravity();
        yield return new WaitForSeconds(CascadeTime);
        FillEmpty(false);
        yield return ProcessBoard();
    }

    IEnumerator ApplyMatchesSelf()
    {
        foreach (MatchData match in matches)
        {
            board.Match(match.X, match.Y, match.Shape);
            //OnMatch.Invoke(match);
            yield return new WaitForSeconds(ComboTime);
        }

        matches.Clear();

        yield return null;
    }

    void ApplyMatches(PuzzleBoard board, List<MatchData> matches)
    {
        foreach (MatchData match in matches)
        {
            board.Match(match.X, match.Y, match.Shape);
        }
    }

    void FillEmpty(bool preventMatches)
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board[x, y].IsEmpty)
                {
                    MakeOrb(x, y);
                    board.Randomize(x, y, Shape.single, preventMatches);
                }
            }
        }
    }
}

public struct MoveData
{
    public int X1, Y1, X2, Y2;
    public List<MatchData> Matches;
    public float Score;


    public MoveData(int x1, int y1, int x2, int y2, List<MatchData> matches, float score)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;

        Matches = matches;

        Score = score;
    }

    public override string ToString()
    {
        return $"From: ({X1},{Y1})  To: ({X2},{Y2})  Score: {Score}";
    }
}