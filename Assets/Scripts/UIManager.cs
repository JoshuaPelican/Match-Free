using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI HealthText;
    [SerializeField] TextMeshProUGUI ManaText;
    [SerializeField] TextMeshProUGUI TurnText;
    [SerializeField] TextMeshProUGUI FavoriteColorTextGameOver;
    [SerializeField] TextMeshProUGUI FavoriteColorTextWin;
    [SerializeField] TextMeshProUGUI TurnsSurvivedText;
    [SerializeField] Animator TurnAnimator;
    [SerializeField] Animator EndGameAnimator;

    private void OnEnable()
    {
        Player.OnHealthChanged.AddListener(UpdateHealth);
        Player.OnManaChanged.AddListener(UpdateMana);
        GameManager.OnPlayerTurnStart.AddListener(() => DisplayTurn(true));
        GameManager.OnPuzzlerTurnStart.AddListener(() => DisplayTurn(false));
        GameManager.OnPuzzlerTurnEnd.AddListener(UpdateTurn);
        GameManager.OnGameOver.AddListener(GameOver);
        GameManager.OnWin.AddListener(Win);
    }

    void UpdateHealth(int health)
    {
        string h = "Lives:";
        for (int i = 0; i < health; i++)
        {
            h += " X";
        }
        HealthText.SetText(h);
    }

    void UpdateMana(int totalMana)
    {
        ManaText.SetText($"Mana: {totalMana}/20");
    }

    void UpdateTurn()
    {
        TurnText.SetText($"Turns Remaining: {GameManager.TURNS_TO_WIN - GameManager.CurrentTurn}");
    }

    void DisplayTurn(bool isPlayerTurn)
    {
        TurnAnimator.SetTrigger("DisplayTurn" + (isPlayerTurn ? "Player" : "Puzzler"));
    }

    void GameOver()
    {
        FavoriteColorTextGameOver.SetText($"Your favorite color was: {GameManager.GetFavoriteColor()}");
        TurnsSurvivedText.SetText($"You survived {GameManager.CurrentTurn} turns, but died before you could escape the puzzle.");
        EndGameAnimator.SetTrigger("GameOver");
    }

    void Win()
    {
        FavoriteColorTextWin.SetText($"Your favorite color was: {GameManager.GetFavoriteColor()}");
        EndGameAnimator.SetTrigger("Win");
    }
}
