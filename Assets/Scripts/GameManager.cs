using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public GameObject CardPrefab;
    [HideInInspector]
    public static GameManager current;
    public List<Card> cardReferences;
    [Header("Cards Piles:")]
    public Pile[] piles;
    public Stock stock;
    [Space]
    public int playerScore = 0;
    public ParticleSystem playerWonPS;
    public bool playing = false;
    public UIManager uiManager;
    List<string> moveHistory;
    int numberOfCardsInFoundation = 0;
    [Header("Orientaion Settings")]
    public Vector3 cameraLandscapePos;
    public Vector3 stockLandscapePos;
    Vector3 cameraNormalPos, stockNormalPos;
    public Vector3[] foundationsLandScapePos;
    Vector3[] foundationsNormalPos;
    DeviceOrientation lastKnownDeviceOrientaion;
    bool orientaionLocked = false;

    void Start()
    {
        if (current == null) current = this;
        else Destroy(gameObject);
        moveHistory = new List<string>();
        cardReferences = new List<Card>();
        cameraNormalPos = Camera.main.transform.position;
        stockNormalPos = stock.transform.position;
        foundationsNormalPos = new Vector3[4];
        int fnp = 0;
        for (int i = 0; i < piles.Length; i++)
        {
            piles[i].pileNumber = i.ToString();
            if (piles[i].isFoundation) foundationsNormalPos[fnp++] = piles[i].transform.position;
        }
        lastKnownDeviceOrientaion = DeviceOrientation.Portrait;
    }

    private void Update()
    {
        if (!orientaionLocked &&
            lastKnownDeviceOrientaion != Input.deviceOrientation
            && (Input.deviceOrientation != DeviceOrientation.FaceDown||
            Input.deviceOrientation != DeviceOrientation.FaceDown))
        {
            SetOrientation();
            lastKnownDeviceOrientaion = Input.deviceOrientation;
        }
    }

    public void StartNewGame(bool TurnThreeMode)
    {
        if (cardReferences.Count == 0) GenerateCards();
        ShuffleCards();
        ResetGame();
        stock.TurnThreeMode = TurnThreeMode;
    }

    public void ResetGame()
    {
        foreach (Card card in cardReferences) card.ResetCard();
        foreach (Pile pile in piles) pile.ClearPile();
        stock.ClearStock();
        PutCardsInPlace();
        uiManager.HideEverything();
        uiManager.ShowInGameScreen(true);
        numberOfCardsInFoundation = 0;
        uiManager.UpdateScore(playerScore, moveHistory.Count);
        playing = true;
        moveHistory.Clear();
        playerScore = 0;
    }

    void GenerateCards()
    {
        //remove all existing cards
        for (int t = 0; t < 4; t++) //Loop 4 times for the type
            for (int n = 1; n <= 13; n++) //each type has 13 cards
            {
                Card card = Instantiate(CardPrefab, transform).GetComponent<Card>();
                card.SetCard(n, (CardType)t);
                cardReferences.Add(card);
            }
    }

    void ShuffleCards()
    {
        for (int i = 0; i < cardReferences.Count; i++)
        {
            int j = Random.Range(i, cardReferences.Count);
            Card tmp = cardReferences[i];
            cardReferences[i] = cardReferences[j];
            cardReferences[j] = tmp;
        }
    }

    void PutCardsInPlace()
    {
        List<Card> cards = new List<Card>(cardReferences);
        float cardYPosition = 0;
        foreach (Card c in cards)
        {
            c.transform.eulerAngles = new Vector3(-90, Random.Range(-5, 5), 0);
            c.transform.position = stock.transform.position + Vector3.up * cardYPosition;
            cardYPosition += 0.01f;
        }
        //Distribute cards in piles
        for (int i = 0; i < 7; i++)
        {
            for (int j = i; j < 7; j++)
            {
                piles[j].AddCard(cards[cards.Count - 1]);
                cards[cards.Count - 1].inStock = false;
                cards.RemoveAt(cards.Count - 1);
                if (i == j) piles[j].RevealLastCard();
            }
        }
        cards.Reverse();
        stock.cardList.AddRange(cards);
    }

    public void RegisterMove(string move, int score)
    {
        move += " " + playerScore;
        playerScore += score;
        if (playerScore < 0) playerScore = 0;
        print(move);
        moveHistory.Add(move);
        uiManager.UpdateScore(playerScore, moveHistory.Count);
    }

    public void Undo()
    {
        if (moveHistory.Count == 0) return;
        string[] move = moveHistory[moveHistory.Count - 1].Split(' ');
        int lastScore = 0;
        if (move[0] == "SM")
        {
            stock.Undo(int.Parse(move[1]));
            lastScore = int.Parse(move[2]);
        }
        else if (move[0] == "S")
        {
            piles[int.Parse(move[1])].ReturnCardToStock();
            lastScore = int.Parse(move[2]);
        }
        else
        {
            piles[int.Parse(move[1])].Undo(piles[int.Parse(move[0])], int.Parse(move[2]), move[3] == "F");
            lastScore = int.Parse(move[4]);
        }
        playerScore = lastScore;
        moveHistory.RemoveAt(moveHistory.Count - 1);
        uiManager.UpdateScore(playerScore, moveHistory.Count);
    }

    public void CardAddedToFoundation(bool added = true)
    {
        numberOfCardsInFoundation += added ? 1 : -1;
        if (numberOfCardsInFoundation == 52) PlayerWon();
    }

    void PlayerWon()
    {
        playing = false;
        playerWonPS.Play(true);
        uiManager.PlayerWon();
    }

    public void StopWinningParticles()
    {
        playerWonPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void SetOrientation()
    {
        Vector3[] foundationsNewPos = null;
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft ||
            Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            foundationsNewPos = foundationsLandScapePos;
            stock.transform.position = stockLandscapePos;
            Camera.main.transform.position = cameraLandscapePos;
        }
        else if (Input.deviceOrientation == DeviceOrientation.Portrait ||
                 Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
        {
            foundationsNewPos = foundationsNormalPos;
            stock.transform.position = stockNormalPos;
            Camera.main.transform.position = cameraNormalPos;
        }
        else return;
        int counter = 0;
        foreach (Pile pile in piles)
            if (pile.isFoundation)
            {
                pile.transform.position = foundationsNewPos[counter++];
                pile.RefreshFoundationPositions();
            }
        stock.RefreshStockPositions();
    }

    public void LockOrientation()
    {
        orientaionLocked = !orientaionLocked;
        Screen.orientation = ScreenOrientation.AutoRotation;
        if (orientaionLocked)
        {
            switch (lastKnownDeviceOrientaion)
            {
                case DeviceOrientation.Portrait:
                case DeviceOrientation.PortraitUpsideDown:
                    Screen.orientation = ScreenOrientation.Portrait;
                    break;
                case DeviceOrientation.LandscapeLeft:
                case DeviceOrientation.LandscapeRight:
                    Screen.orientation = ScreenOrientation.Landscape;
                    break;
                default:
                    print(lastKnownDeviceOrientaion);
                    break;
            }
        }
        
    }
}
