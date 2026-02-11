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

    public void InitializeCard(CardData data)
    {
        cardData = data;
        UpdateCardVisuals();
    }

    void UpdateCardVisuals()
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        costText.text = cardData.energyCost.ToString();

        if (cardData.artwork != null)
        {
            artworkImage.sprite = cardData.artwork;
        }

        // Color según tipo de carta
        switch (cardData.cardType)
        {
            case CardType.Attack:
                cardBackground.color = new Color(0.8f, 0.2f, 0.2f); // Rojo
                break;
            case CardType.Skill:
                cardBackground.color = new Color(0.2f, 0.6f, 0.8f); // Azul
                break;
            case CardType.Power:
                cardBackground.color = new Color(0.3f, 0.8f, 0.3f); // Verde
                break;
        }
    }

    public void PlayCard()
    {
        Debug.Log($"Jugando carta: {cardData.cardName}");
    }
}