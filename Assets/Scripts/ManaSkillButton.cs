using UnityEngine;
using UnityEngine.UI;

public class ManaSkillButton : MonoBehaviour
{
    [SerializeField] int ManaCost;
    Button button;

    private void OnEnable()
    {
        Player.OnManaChanged.AddListener(UpdateButton);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    void UpdateButton(int currentMana)
    {
        button.interactable = currentMana >= ManaCost;
    }
}
