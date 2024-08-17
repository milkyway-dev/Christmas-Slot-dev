using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GambleController : MonoBehaviour
{
    // UI and Components References
    [Header("UI and Components")]
    [SerializeField] private GameObject gamble_game; // The main gamble game object
    [SerializeField] private Button doubleButton; // Button for doubling the bet
    [SerializeField] private SocketIOManager socketManager; // Reference to the SocketIO Manager
    [SerializeField] private AudioController audioController; // Reference to the Audio Controller
    [SerializeField] internal List<CardFlip> allcards = new List<CardFlip>(); // List of all card flip objects
    [SerializeField] private TMP_Text winamount; // Text to display the win amount
    [SerializeField] private SlotBehaviour slotController; // Reference to the Slot Controller
    [SerializeField] private Sprite[] HeartSpriteList; // List of heart suit sprites
    [SerializeField] private Sprite[] ClubSpriteList; // List of club suit sprites
    [SerializeField] private Sprite[] SpadeSpriteList; // List of spade suit sprites
    [SerializeField] private Sprite[] DiamondSpriteList; // List of diamond suit sprites
    [SerializeField] private Sprite cardCover; // Default card cover sprite
    [SerializeField] private CardFlip DealerCard_Script; // Reference to the dealer's card flip script

    // Gamble Section References
    [Header("Gamble Section References")]
    [SerializeField] private GameObject GambleEnd_Object; // Object to display when gamble ends
    [SerializeField] private Button m_Collect_Button; // Button for collecting winnings
    [SerializeField] private Button m_Double_Button; // Button for doubling winnings

    // Loading Screen References
    [Header("Loading Screen References")]
    [SerializeField] private GameObject loadingScreen; // Loading screen game object
    [SerializeField] private Image slider; // Slider for loading screen

    // Internal Variables
    private Sprite highcard_Sprite; // Sprite for the high card
    private Sprite lowcard_Sprite; // Sprite for the low card
    private Sprite spare1card_Sprite; // Sprite for the first spare card
    private Sprite spare2card_Sprite; // Sprite for the second spare card
    internal bool gambleStart = false; // Indicates if the gamble has started
    internal bool isResult = false; // Indicates if the result has been received

    private Tweener Gamble_Tween_Scale = null; // Tweener for scaling the double button

    #region Initialization

    private void Start()
    {
        // Setup event listeners for buttons
        if (doubleButton)
        {
            doubleButton.onClick.RemoveAllListeners();
            doubleButton.onClick.AddListener(delegate { StartGamblegame(false); });
        }

        // Collect Button Setup
        if (m_Collect_Button)
        {
            m_Collect_Button.onClick.RemoveAllListeners();
            m_Collect_Button.onClick.AddListener(OnReset);
        }

        // Double Button Setup
        if (m_Double_Button)
        {
            m_Double_Button.onClick.RemoveAllListeners();
            m_Double_Button.onClick.AddListener(delegate { NormalCollectFunction(); StartGamblegame(true); });
        }

        toggleDoubleButton(false); // Disable double button at start
    }

    #endregion

    #region Button Toggle

    // Toggles the interactability of the double button
    internal void toggleDoubleButton(bool toggle)
    {
        doubleButton.interactable = toggle;
    }

    #endregion

    #region Gamble Game

    // Starts the gamble game
    void StartGamblegame(bool isRepeat = false)
    {
        if (GambleEnd_Object) GambleEnd_Object.SetActive(false); // Hide end screen
        GambleTweeningAnim(false); // Stop animation
        slotController.DeactivateGamble(); // Deactivate the gamble slot
        winamount.text = "0"; // Reset win amount text

        if (!isRepeat) winamount.text = "0"; // Reset win amount on non-repeat

        if (audioController) audioController.PlayButtonAudio(); // Play button click audio
        if (gamble_game) gamble_game.SetActive(true); // Activate gamble game object
        loadingScreen.SetActive(true); // Show loading screen

        StartCoroutine(loadingRoutine()); // Start loading routine
        StartCoroutine(GambleCoroutine()); // Start gamble coroutine
    }

    // Resets the game and collects winnings
    private void OnReset()
    {
        if (slotController) slotController.GambleCollect(); // Collect winnings
        NormalCollectFunction(); // Reset the gamble game
    }

    // Normal collect function
    private void NormalCollectFunction()
    {
        gambleStart = false; // End gamble
        slotController.updateBalance(); // Update player balance

        if (gamble_game) gamble_game.SetActive(false); // Hide gamble game

        // Reset all card flip objects
        allcards.ForEach((element) =>
        {
            element.Card_Button.image.sprite = cardCover;
            element.Reset();
        });

        // Reset dealer's card
        DealerCard_Script.Card_Button.image.sprite = cardCover;
        DealerCard_Script.once = false;

        toggleDoubleButton(false); // Disable double button
    }

    #endregion

    #region Card Handling

    // Compute the card sprites based on the received message
    private void ComputeCards()
    {
        highcard_Sprite = CardSet(socketManager.myMessage.highCard.suit, socketManager.myMessage.highCard.value);
        lowcard_Sprite = CardSet(socketManager.myMessage.lowCard.suit, socketManager.myMessage.lowCard.value);
        spare1card_Sprite = CardSet(socketManager.myMessage.exCards[0].suit, socketManager.myMessage.exCards[0].value);
        spare2card_Sprite = CardSet(socketManager.myMessage.exCards[1].suit, socketManager.myMessage.exCards[1].value);
    }

    // Determines the sprite for a given card suit and value
    private Sprite CardSet(string suit, string value)
    {
        Sprite tempSprite = null;
        switch (suit.ToUpper())
        {
            case "HEARTS":
                tempSprite = GetCardSprite(HeartSpriteList, value);
                break;
            case "DIAMONDS":
                tempSprite = GetCardSprite(DiamondSpriteList, value);
                break;
            case "CLUBS":
                tempSprite = GetCardSprite(ClubSpriteList, value);
                break;
            case "SPADES":
                tempSprite = GetCardSprite(SpadeSpriteList, value);
                break;
            default:
                Debug.LogError("Invalid Suit: " + suit);
                break;
        }
        return tempSprite;
    }

    // Helper function to get the correct sprite from a sprite list based on value
    private Sprite GetCardSprite(Sprite[] spriteList, string value)
    {
        switch (value.ToUpper())
        {
            case "A": return spriteList[0];
            case "K": return spriteList[12];
            case "Q": return spriteList[11];
            case "J": return spriteList[10];
            default:
                int myval = int.Parse(value);
                return spriteList[myval - 1];
        }
    }

    #endregion

    #region Coroutines

    // Main coroutine for handling the gamble process
    IEnumerator GambleCoroutine()
    {
        // Reset all card states
        for (int i = 0; i < allcards.Count; i++)
        {
            allcards[i].once = false;
        }

        socketManager.OnGamble(); // Send gamble request

        yield return new WaitUntil(() => socketManager.isResultdone); // Wait for result
        ComputeCards(); // Compute card sprites
        gambleStart = true; // Mark gamble as started
    }

    // Coroutine for handling the loading screen
    IEnumerator loadingRoutine()
    {
        float fillAmount = 1;
        while (fillAmount > 0.1)
        {
            fillAmount -= Time.deltaTime;
            slider.fillAmount = fillAmount;
            if (fillAmount == 0.1) yield break;
            yield return null;
        }
        yield return new WaitUntil(() => gambleStart);
        slider.fillAmount = 0;
        yield return new WaitForSeconds(1f);
        loadingScreen.SetActive(false);
    }

    // Coroutine for collecting winnings
    private IEnumerator NewCollectRoutine()
    {
        isResult = false;
        socketManager.OnCollect(); // Send collect request

        yield return new WaitUntil(() => socketManager.isResultdone); // Wait for result
        isResult = true; // Mark result as received
    }

    // Coroutine for resetting the game after collection
    IEnumerator Collectroutine()
    {
        yield return new WaitForSeconds(2f);
        gambleStart = false;
        yield return new WaitForSeconds(2);
        slotController.updateBalance();
        if (gamble_game) gamble_game.SetActive(false);
        allcards.ForEach((element) =>
        {
            element.Card_Button.image.sprite = cardCover;
            element.Reset();
        });
        DealerCard_Script.Card_Button.image.sprite = cardCover;
        DealerCard_Script.once = false;
        toggleDoubleButton(false);
    }

    #endregion

    #region Gamble Actions

    // Get the correct card sprite based on the player's result
    internal Sprite GetCard()
    {
        if (socketManager.myMessage.playerWon)
        {
            if (DealerCard_Script) DealerCard_Script.cardImage = lowcard_Sprite;
            return highcard_Sprite;
        }
        else
        {
            if (DealerCard_Script) DealerCard_Script.cardImage = highcard_Sprite;
            return lowcard_Sprite;
        }
    }

    // Flip all the cards when the game ends
    internal void FlipAllCard()
    {
        int cardVal = 0;
        for (int i = 0; i < allcards.Count; i++)
        {
            if (allcards[i].once) continue;

            allcards[i].Card_Button.interactable = false;
            if (cardVal == 0)
            {
                allcards[i].cardImage = spare1card_Sprite;
                cardVal++;
            }
            else
            {
                allcards[i].cardImage = spare2card_Sprite;
            }
            allcards[i].FlipMyObject();
            allcards[i].Card_Button.interactable = false;
        }

        if (DealerCard_Script) DealerCard_Script.FlipMyObject();

        if (socketManager.myMessage.playerWon)
        {
            winamount.text = "YOU WIN\n" + socketManager.myMessage.currentWining.ToString();
            if (GambleEnd_Object) GambleEnd_Object.SetActive(true);
        }
        else
        {
            winamount.text = "YOU LOSE\n0";
            StartCoroutine(Collectroutine());
        }
    }

    // Starts the coroutine for collecting winnings
    internal void RunOnCollect()
    {
        StartCoroutine(NewCollectRoutine());
    }

    // Coroutine to handle the game over situation
    void OnGameOver()
    {
        StartCoroutine(Collectroutine());
    }

    #endregion

    #region Tweening Animations

    // Controls the scaling animation for the double button
    internal void GambleTweeningAnim(bool IsStart)
    {
        if (IsStart)
        {
            Gamble_Tween_Scale = doubleButton.gameObject.GetComponent<RectTransform>()
                .DOScale(new Vector2(1.18f, 1.18f), 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0);
        }
        else
        {
            Gamble_Tween_Scale.Kill();
            doubleButton.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    #endregion
}
