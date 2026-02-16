using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    public CardData cardData;

    [Header("UI Referencias")]
    public Image cardBackground;
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    private bool isSelected = false;
    private Vector3 originalPosition;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        button.onClick.AddListener(OnCardClicked);
    }

    public void InitializeCard(CardData data)
    {
        cardData = data;
        isSelected = false;
        UpdateCardVisuals();
    }

    void OnCardClicked()
    {
        if (!CombatManager.Instance.isPlayerTurn) return;

        if (isSelected)
        {
            // Si ya está seleccionada, deseleccionar
            Deselect();
        }
        else
        {
            // Si hay otra carta seleccionada, no permitir
            if (CombatManager.Instance.GetSelectedCount() >= 1)
            {
                Debug.Log("Ya tienes una carta seleccionada, juégala primero!");
                return;
            }

            if (CombatManager.Instance.currentEnergy < cardData.energyCost)
            {
                Debug.Log("No tienes suficiente energía!");
                return;
            }

            Select();
        }
    }

    public void Select()
    {
        isSelected = true;
        originalPosition = transform.localPosition;

        transform.localPosition = new Vector3(
            originalPosition.x,
            originalPosition.y + 40f,
            originalPosition.z
        );

        cardBackground.color = new Color(
            Mathf.Min(cardBackground.color.r + 0.3f, 1f),
            Mathf.Min(cardBackground.color.g + 0.3f, 1f),
            cardBackground.color.b
        );

        CombatManager.Instance.SelectCard(cardData);
    }

    public void Deselect()
    {
        isSelected = false;
        transform.localPosition = originalPosition;
        UpdateCardVisuals();
        CombatManager.Instance.DeselectCard(cardData);
    }

    void UpdateCardVisuals()
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        costText.text = cardData.energyCost.ToString();

        if (cardData.artwork != null)
            artworkImage.sprite = cardData.artwork;

        switch (cardData.cardType)
        {
            case CardType.Attack:
                cardBackground.color = new Color(0.8f, 0.2f, 0.2f);
                break;
            case CardType.Skill:
                cardBackground.color = new Color(0.2f, 0.6f, 0.8f);
                break;
            case CardType.Power:
                cardBackground.color = new Color(0.3f, 0.8f, 0.3f);
                break;
        }
    }
}