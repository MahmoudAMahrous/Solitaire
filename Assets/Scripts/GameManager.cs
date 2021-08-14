using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

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
    public Help help;

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
            piles[i].SetUpPile(i);
            if (piles[i].isFoundation) foundationsNormalPos[fnp++] = piles[i].transform.position;
        }
        lastKnownDeviceOrientaion = DeviceOrientation.Portrait;
        CheckGameData();
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
        stock.TurnThreeMode = TurnThreeMode;
        ShuffleCards();
        PlayerPrefs.SetInt("Draw3", TurnThreeMode ? 1 : 0);
        ResetGame();
    }

    public void ResetGame()
    {
        foreach (Card card in cardReferences) card.ResetCard();
        foreach (Pile pile in piles) pile.ClearPile();
        stock.ClearStock();
        PutCardsInPlace();
        numberOfCardsInFoundation = 0;
        playing = true;
        moveHistory.Clear();
        playerScore = 0;
        uiManager.UpdateScore(playerScore, moveHistory.Count);
        uiManager.HideEverything();
        uiManager.ShowInGameScreen(true);
        uiManager.ShowContinueButton(true);
    }

    void GenerateCards(string cardsString = "")
    {
        if(cardsString == "")
        for (int t = 0; t < 4; t++) //Loop 4 times for the type
            for (int n = 1; n <= 13; n++) //each type has 13 cards
            {
                CreateCard(n, t);
            }
        else
        {
            string[] cardData, cardsData = cardsString.Split('-');
            foreach (string cd in cardsData)
            {
                if (cd == "") continue;
                cardData = cd.Split(' ');
                CreateCard(int.Parse(cardData[0]), int.Parse(cardData[1]));
            }
        }
    }

    void CreateCard(int number, int type)
    {
        Card card = Instantiate(CardPrefab, transform).GetComponent<Card>();
        card.SetCard(number, (CardType)type);
        cardReferences.Add(card);
    }

    void ShuffleCards()
    {
        string cards = "";
        for (int i = 0; i < cardReferences.Count; i++)
        {
            int j = Random.Range(i, cardReferences.Count);
            Card tmp = cardReferences[i];
            cardReferences[i] = cardReferences[j];
            cardReferences[j] = tmp;
            cards += cardReferences[i].number + " " + ((int)cardReferences[i].type) + "-";
        }
        PlayerPrefs.SetString("Cards", cards);
    }

    void PutCardsInPlace()
    {
        List<Card> cards = new List<Card>(cardReferences);
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
        stock.stockCount = cards.Count;
        stock.RefreshStockPositions();
    }

    public void RegisterMove(string move, int score)
    {
        move += " " + playerScore;
        playerScore += score;
        if (playerScore < 0) playerScore = 0;
        print(move);
        moveHistory.Add(move);
        uiManager.UpdateScore(playerScore, moveHistory.Count);
        CheckIfAllCardsShown();
    }

    public void Undo()
    {
        if (moveHistory.Count == 0) return;
        UndoMove(moveHistory[moveHistory.Count - 1]);
        moveHistory.RemoveAt(moveHistory.Count - 1);
        uiManager.UpdateScore(playerScore, moveHistory.Count);
    }

    void UndoMove(string moveString)
    {
        print("undo : " + moveString);
        string[] move = moveString.Split(' ');
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
        PlayerPrefs.DeleteKey("Cards");
        uiManager.ShowContinueButton(false);
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

    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    void SaveGameData()
    {
        if (PlayerPrefs.HasKey("Cards"))
        {
            string moves = "";
            foreach (string move in moveHistory) moves += move + "/";
            print(moves);
            PlayerPrefs.SetString("Moves", moves);
            PlayerPrefs.SetInt("Score", playerScore);
        } 
        PlayerPrefs.Save();
    }

    void CheckGameData()
    {
        if (PlayerPrefs.HasKey("Cards"))
        {
            GenerateCards(PlayerPrefs.GetString("Cards"));
            ResetGame();
            playerScore = PlayerPrefs.GetInt("Score");
            stock.TurnThreeMode = (PlayerPrefs.GetInt("Draw3") == 1);
            print(PlayerPrefs.GetString("Moves"));
            string[] moves = PlayerPrefs.GetString("Moves").Split('/');
            foreach (string move in moves)
            {
                if (move == "") continue;
                moveHistory.Add(move);
                RepeatMove(move);
            }
            uiManager.ShowContinueButton(true);
            uiManager.UpdateScore(playerScore, moveHistory.Count);
        }
    }

    void RepeatMove(string moveString)
    {
        print(moveString);
        string[] move = moveString.Split(' ');
        if (move[0] == "SM")
        {
            stock.ClickStock(false);
        }
        else if (move[0] == "S")
        {
            stock.GetCurrentCard().MoveFromStock(piles[int.Parse(move[1])]);
        }
        else
        {
            piles[int.Parse(move[0])].Undo(piles[int.Parse(move[1])], int.Parse(move[2]), false);
        }
    }

    void CheckIfAllCardsShown()
    {
        if (stock.stockCount != 0) return;
        foreach (Card card in cardReferences) if (!card.cardFacingUp) return;
        AutoFinishTheGame();
    }

    void AutoFinishTheGame()
    {
        Card card;
        Pile pile;
        if (!help.FindMove(out card, out pile)) return;
        card.currentPile.MoveCardToPile(card, pile);
        card.MoveToPosition(card.transform.position); //to stop the card from moving
        card.transform.DOMove(pile.GetNewCardPosition(), 0.1f).OnComplete(() => AutoFinishTheGame()).SetEase(Ease.OutBack);
    }
}
