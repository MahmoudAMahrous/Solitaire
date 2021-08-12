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
    public int playerScore = 0;
    public TextMeshProUGUI scoreText;
    public Help help;
    public ParticleSystem playerWonPS;
    public bool playing = false;
    List<string> moveHistory;
    int numberOfCardsInFoundation = 0;

    void Start()
    {
        if (current == null) current = this;
        else Destroy(gameObject);
        moveHistory = new List<string>();
        cardReferences = new List<Card>();
        numberOfCardsInFoundation = 0;
        for (int i = 0; i < piles.Length; i++) piles[i].pileNumber = i.ToString();
        GenerateCards();
        UpdateScoreText();
        playing = true;
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
        //shuffle the deck
        for (int i = 0; i < cardReferences.Count; i++)
        {
            int j = Random.Range(i, cardReferences.Count);
            Card tmp = cardReferences[i];
            cardReferences[i] = cardReferences[j];
            cardReferences[j] = tmp;
        }
        PutCardsInPlace();
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
        UpdateScoreText();
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
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + playerScore + "      Moves: " + moveHistory.Count;
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
    }
}
