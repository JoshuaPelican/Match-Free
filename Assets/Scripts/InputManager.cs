using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour //Strictly Handles Player Puzzle Input
{
    [SerializeField] LayerMask SelectableLayerMask;

    public static UnityEvent OnDragStart = new();
    public static UnityEvent OnDrag = new();
    public static UnityEvent OnDragEnd = new();

    Orb selectedOrb;
    bool isMoving;
    bool isBlockingInput;

    [SerializeField] AudioClip PickupClip;
    [SerializeField] AudioClip PutDownClip;

    private void OnEnable()
    {
        GameManager.OnPuzzlerTurnStart.AddListener(BlockInput);
        GameManager.OnPlayerTurnStart.AddListener(UnblockInput);
        GameManager.OnGameOver.AddListener(BlockInput);
    }

    private void Update()
    {
        if (isBlockingInput) //Unless input is blocked
            return;

        HandleInput(); //Get input every frame
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) //Click or Tap Down
        {
            TrySelect();
        }

        else if (Input.GetMouseButton(0)) //Drag or Swipe
        {
            TryMove();
        }

        else if (Input.GetMouseButtonUp(0)) //End Click or Remove Finger
        {
            TryDeselect();
        }
    }

    void TrySelect()
    {
        RaycastHit2D hit = Physics2D.Raycast(InputUtility.MousePosition, Vector2.zero, 10, SelectableLayerMask); //Raycast the current pressed location

        if (!hit) //If hit nothing
            return;

        if (!hit.transform.TryGetComponent(out Orb hitOrb)) //If hit something but not orb for some reason
            return;

        selectedOrb = hitOrb; //Select the orb
        //selectedOrb.Select();
        OnDragStart.Invoke();

        AudioManager.PlayEffect(PickupClip, 0.2f, 1.1f);
    }

    void TryMove()
    {
        if (!selectedOrb)
            return;

        selectedOrb.Move();
        isMoving = true;
        OnDrag.Invoke();
    }

    void TryDeselect()
    {
        if (!selectedOrb)
            return;

        selectedOrb.Deselect();
        ClearSelection();

        if (!isMoving)
            return;

        //BlockInput();
        //OnDragEnd.Invoke();
        isMoving = false;

        AudioManager.PlayEffect(PutDownClip, 0.2f, 1.1f);
    }

    void ClearSelection()
    {
        selectedOrb = null;
    }

    void BlockInput()
    {
        //if (selectedOrb)
        //    TryDeselect();

        isBlockingInput = true;
    }

    void UnblockInput()
    {
        isBlockingInput = false;
    }
}
