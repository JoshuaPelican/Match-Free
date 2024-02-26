using System.Collections.Generic;
using UnityEngine;

public class Puzzler : MonoBehaviour
{
    [SerializeField] PuzzleManager puzzleManager;

    private void Start()
    {
        GameManager.OnPuzzlerTurnStart.AddListener(() => Invoke(nameof(MakeBestMatch), Random.Range(3f, 4f)));
    }

    void MakeBestMatch()
    {
        List<MoveData> moves = puzzleManager.GetAllPossibleMoves();

        moves.Sort((x, y) => y.Score.CompareTo(x.Score));

        MoveData bestMove = moves[0];

        //Debug.Log("Best Move: " + bestMove.ToString() + "\n" + bestMove.Matches[0].Shape.ToString());

        puzzleManager.MakeMatch(bestMove.X1, bestMove.Y1, bestMove.X2, bestMove.Y2);
    }
}
