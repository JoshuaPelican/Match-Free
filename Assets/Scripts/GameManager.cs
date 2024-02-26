using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static UnityEvent OnPuzzleStarted = new();
    public static UnityEvent OnPlayerTurnStart = new();
    public static UnityEvent OnPlayerTurnEnd = new();
    public static UnityEvent OnPuzzlerTurnStart = new();
    public static UnityEvent OnPuzzlerTurnEnd = new();
    public static UnityEvent OnGameOver = new();
    public static UnityEvent OnWin = new();

    static GameManager instance;

    const float TURN_DELAY = 0.5f;
    public static int CurrentTurn = 0;
    public const int TURNS_TO_WIN = 12;

    static GameState gameState = GameState.None;

    public static Dictionary<OrbType, int> ChosenColors = new Dictionary<OrbType, int>();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) SetGameState(GameState.Win);
    }

    public enum GameState
    {
        PuzzleStarted,
        PlayerTurnStart,
        PlayerTurnEnd,
        PuzzlerTurnStart,
        PuzzlerTurnEnd,
        GameOver,
        Win,
        None,
    }

    public static void SetGameState(GameState state)
    {
        if (state == gameState)
            return;

        gameState = state;

        switch (gameState)
        {
            case GameState.PuzzleStarted:
                CurrentTurn = 0;
                ChosenColors = new Dictionary<OrbType, int>();
                OnPuzzleStarted.Invoke();
                break;
            case GameState.PlayerTurnStart:
                instance.StartCoroutine(PlayerTurnStart());
                break;
            case GameState.PlayerTurnEnd:
                instance.StartCoroutine(PlayerTurnEnd());
                break;
            case GameState.PuzzlerTurnStart:
                instance.StartCoroutine(PuzzlerTurnStart());
                break;
            case GameState.PuzzlerTurnEnd:
                instance.StartCoroutine(PuzzlerTurnEnd());
                break;
            case GameState.GameOver:
                instance.StartCoroutine(GameOver());
                break;
            case GameState.Win:
                instance.StartCoroutine(Win());
                break;
        }
    }

    public static IEnumerator PlayerTurnStart()
    {
        yield return new WaitForSeconds(TURN_DELAY);

        OnPlayerTurnStart.Invoke();
    }

    public static IEnumerator PlayerTurnEnd()
    {
        OnPlayerTurnEnd.Invoke();

        yield return new WaitForSeconds(TURN_DELAY);

        SetGameState(GameState.PuzzlerTurnStart);
    }

    public static IEnumerator PuzzlerTurnStart()
    {
        yield return new WaitForSeconds(TURN_DELAY);

        OnPuzzlerTurnStart.Invoke();
    }

    public static IEnumerator PuzzlerTurnEnd()
    {
        CurrentTurn++;

        if (CurrentTurn >= TURNS_TO_WIN)
        {
            SetGameState(GameState.Win);
            yield return null;
        }

        OnPuzzlerTurnEnd.Invoke();

        yield return new WaitForSeconds(TURN_DELAY);

        SetGameState(GameState.PlayerTurnStart);
    }

    public static IEnumerator GameOver()
    {
        yield return new WaitForSeconds(TURN_DELAY);

        OnGameOver.Invoke();
    }

    public static IEnumerator Win()
    {
        yield return new WaitForSeconds(TURN_DELAY);

        OnWin.Invoke();
    }

    public static OrbType GetFavoriteColor()
    {
        int most = 0;
        OrbType mostType = OrbType.None;
        foreach (KeyValuePair<OrbType, int> chosenType in ChosenColors)
        {
            if (chosenType.Value > most)
            {
                mostType = chosenType.Key;
            }
        }

        return mostType;
    }

}
