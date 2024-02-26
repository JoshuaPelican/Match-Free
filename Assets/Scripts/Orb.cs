using UnityEngine;

public class Orb : MonoBehaviour
{
    //Public
    [SerializeField] protected float RepositionSpeed = 0.25f;

    //Data
    protected PuzzleBoard board;
    public int x { get; protected set; } 
    public int y { get; protected set; }
    protected OrbData data => board[x, y];

    //States
    protected bool isPositionDirty;
    protected bool isSelected;

    //Components
    protected SpriteRenderer rend;
    protected Rigidbody2D rig;
    [SerializeField] GameObject highlight;
    //BoxCollider2D col;

    public virtual void Initialize(PuzzleBoard board, int x, int y)
    {
        this.board = board;
        this.x = x;
        this.y = y;

        transform.localScale = board.CellSize * Vector3.one;

        name = $"({x},{y})";

        rend = GetComponent<SpriteRenderer>();
        rig = GetComponent<Rigidbody2D>();
        //col = GetComponent<BoxCollider2D>();

        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Random.Range(0, 360)));

        PuzzleManager.OnBoardResized.AddListener(OnBoardResized);
        PuzzleManager.OnHighlight.AddListener(OnHighlight);
        GameManager.OnPlayerTurnEnd.AddListener(DisableHighlight);
        board.OnDataUpdated.AddListener(OnDataUpdated);
        board.OnDataMatched.AddListener(OnDataMatched);
        board.OnDataMoved.AddListener(OnDataMoved);

        RefreshData();
    }

    protected virtual void OnBoardResized()
    {
        if (x >= board.Width || y >= board.Height)
        {
            Destroy(gameObject, 0.25f);
        }
        else
        {
            transform.localScale = board.CellSize * Vector3.one;
            SetPositionDirty();
        }
    }

    protected virtual void OnHighlight(int x, int y)
    {
        if (this.x != x || this.y != y)
            return;

        highlight.transform.rotation = Quaternion.identity;

        highlight.SetActive(true);
    }

    protected virtual void DisableHighlight()
    {
        highlight.SetActive(false);
    }

    protected virtual void OnDataUpdated(int x, int y)
    {
        if (this.x != x || this.y != y)
            return;

        RefreshData();
    }

    protected virtual void OnDataMatched(int x, int y)
    {
        if (this.x == x && this.y == y)
            Destroy(gameObject, 0.5f);
    }

    protected virtual void OnDataMoved(int x1, int y1, int x2, int y2)
    {
        if (x == x1 && y == y1)
        {
            x = x2;
            y = y2;
            RefreshData();
        }
        else if (x == x2 && y == y2)
        {
            x = x1;
            y = y1;
            RefreshData();
        }
    }

    private void FixedUpdate()
    {
        if (!isPositionDirty)
            return;

        SmoothToGrid();
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out Orb other)) //Hit Orb
            return;

        if (other.isSelected)
            return;

        if (!board.IsAdjacent(x, y, other.x, other.y)) //TODO: Change for player to move where able
            return;

        Swap(other);
    }
    */

    
    public virtual void Move()
    {
        rig.MovePosition(InputUtility.MousePosition);
    }

    public virtual void Select()
    {
        isSelected = true;
        //rend.sortingOrder++;
        //col.size = new Vector2(0.15f, 0.15f);
        UnsetPositionDirty();
    }

    public virtual void Deselect()
    {
        isSelected = false;
        //col.size = new Vector2(0.8f, 0.8f);
        //rend.sortingOrder--;
        SetPositionDirty();
    }

    protected void SmoothToGrid()
    {
        Vector2 targetPos = board.GetWorldPosition(x, y, true);
        Vector2 moveDir = targetPos - (Vector2)transform.position;

        rig.MovePosition(rig.position + (moveDir * RepositionSpeed));

        if (rig.position == targetPos)
            UnsetPositionDirty();
    }
    protected virtual void Swap(Orb other)
    {
        bool isMatch = board.TrySwap(x, y, other.x, other.y);

        if (isMatch)
        {
            int tempX, tempY;
            tempX = x;
            tempY = y;

            x = other.x;
            y = other.y;

            other.x = tempX;
            other.y = tempY;
        }

        SetPositionDirty();
    }

    protected virtual void RefreshData()
    {
        rend.color = OrbTypeUtility.TypeColors[(int)data.Type];
        name = $"({x},{y})";

        SetPositionDirty();
    }

    protected void SetPositionDirty()
    {
        if (isSelected)
            return;

        isPositionDirty = true;
    }

    protected void UnsetPositionDirty()
    {
        isPositionDirty = false;
    }
}
