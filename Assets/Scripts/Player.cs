using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : Orb
{
    public static UnityEvent<int> OnHealthChanged = new();
    public static UnityEvent<int> OnManaChanged = new();

    [SerializeField] int maxLives = 3;
    [SerializeField] int range = 2;
    int currentLives;
    bool canTeleport;

    Animator animator;

    int currentMana = 5;
    [SerializeField] int MaxMana = 20;
    [SerializeField] int TeleportManaCost = 5;
    [SerializeField] int HealManaCost = 10;
    [SerializeField] AudioClip DamagedClip;
    [SerializeField] AudioClip SkillUsedClip;
    [SerializeField] AudioClip DeathClip;
    [SerializeField] AudioClip MatchClip;

    private void OnEnable()
    {
        GameManager.OnPlayerTurnStart.AddListener(() => HighlightInRange(2));
    }

    private void Awake()
    {
        currentLives = maxLives;

        animator = GetComponent<Animator>();
    }

    public override void Initialize(PuzzleBoard board, int x, int y)
    {
        base.Initialize(board, x, y);
        transform.rotation = Quaternion.identity;
        //transform.localScale *= 1.2f;

        OnHealthChanged.Invoke(currentLives);
        OnManaChanged.Invoke(currentMana);

        board.OnDataMatched.AddListener((x, y) => AudioSource.PlayClipAtPoint(MatchClip, Vector3.zero, 0.25f));
    }

    protected override void OnBoardResized()
    {
        if (x >= board.Width || y >= board.Height)
        {
            Die();
        }
        else
        {
            transform.localScale = board.CellSize * Vector3.one;
            SetPositionDirty();
        }
    }

    protected override void OnDataMatched(int x, int y)
    {
        if (this.x == x && this.y == y)
        {
            TakeDamage(currentLives);
        }
        else if (board.IsAdjacent(this.x, this.y, x, y, true, 1))
        {
            Debug.Log($"Ouch from ({x},{y})!");
            TakeDamage(1);
        }
    }

    protected override void OnDataUpdated(int x, int y)
    {
        base.OnDataUpdated(x, y);
    }

    protected override void Swap(Orb other)
    {
        // Do nothing
    }

    protected override void OnDataMoved(int x1, int y1, int x2, int y2)
    {
        base.OnDataMoved(x1, y1, x2, y2);
    }

    protected override void OnHighlight(int x, int y)
    {
        // Do nothing
    }

    protected override void DisableHighlight()
    {
        // Do nothing
    }

    protected override void RefreshData()
    {
        name = $"Player ({x},{y})";

        SetPositionDirty();
    }

    public override void Select()
    {
        base.Select();
    }

    public override void Deselect()
    {
        TryMove();
        base.Deselect();
    }

    public void TryMove()
    {
        int otherX, otherY;
        board.GetCoordinates(transform.position, out otherX, out otherY);

        if (!board.IsAdjacent(x, y, otherX, otherY, false, range) && !canTeleport) //TODO: Change for player to move where able
            return;

        x = otherX;
        y = otherY;

        OrbType chosenType = board[x, y].Type;
        if (GameManager.ChosenColors.ContainsKey(chosenType)) GameManager.ChosenColors[chosenType]++;
        else GameManager.ChosenColors.Add(chosenType, 1);

        canTeleport = false;

        TryGainMana();

        GameManager.SetGameState(GameManager.GameState.PlayerTurnEnd);
    }

    void TryGainMana()
    {
        List<Vector2Int> adjacent = board.GetAdjacent(x, y, false, 1);
        Dictionary<OrbType, int> foundTypes = new Dictionary<OrbType, int>();

        foreach (Vector2Int cell in adjacent)
        {
            OrbType type = board[cell.x, cell.y].Type;

            if (foundTypes.ContainsKey(type))
                foundTypes[type]++;
            else
                foundTypes.Add(type, 1);
        }

        int mostFound = 0;
        foreach (KeyValuePair<OrbType,int> foundType in foundTypes)
        {
            if(foundType.Value > mostFound)
            {
                mostFound = foundType.Value;
            }
        }

        GainMana(mostFound);
    }

    void GainMana(int manaToGain)
    {
        int actualManaGained = Mathf.Clamp(manaToGain, -currentMana, MaxMana - currentMana);
        currentMana += actualManaGained;
        Mathf.Clamp(currentMana, 0, MaxMana);

        OnManaChanged.Invoke(currentMana);
    }

    void HighlightInRange(int range)
    {
        List<Vector2Int> adjacent = board.GetAdjacent(x, y, false, range);
        foreach (Vector2Int cell in adjacent)
        {
            PuzzleManager.OnHighlight.Invoke(cell.x, cell.y);
        }
    }

    void TakeDamage(int damage)
    {
        currentLives -= damage;

        animator.SetTrigger("TakeDamage");
        OnHealthChanged.Invoke(currentLives);

        AudioManager.PlayEffect(DamagedClip, 0.10f, 1.25f);

        if (currentLives > 0)
            return;

        Die();
    }

    void Die()
    {
        AudioManager.PlayEffect(DamagedClip, 0.15f, 0.75f);
        animator.SetTrigger("Die");
        GameManager.SetGameState(GameManager.GameState.GameOver);
    }

    public void TeleportSkill()
    {
        if (currentMana < TeleportManaCost)
            return;

        GainMana(-TeleportManaCost);

        canTeleport = true;

        HighlightInRange(board.Width);

        AudioManager.PlayEffect(SkillUsedClip, 0.2f, 1);
    }

    public void HealSkill()
    {
        if (currentMana < HealManaCost)
            return;

        GainMana(-HealManaCost);

        currentLives = Mathf.Clamp(currentLives + 1, 0, maxLives);
        OnHealthChanged.Invoke(currentLives);

        AudioManager.PlayEffect(SkillUsedClip, 0.2f, 1);
    }
}
