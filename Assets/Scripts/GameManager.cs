using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject CardPrefab;
    [HideInInspector]
    public static GameManager current;
    [Header("Cards Piles:")]
    public Pile[] piles;
    public Stock stock;

    List<string> History;

    void Start()
    {
        if (current == null) current = this;
        else Destroy(gameObject);
        History = new List<string>();
        for (int i = 0; i < piles.Length; i++) piles[i].pileNumber = i.ToString();
        GenerateCards();
    }

    void GenerateCards()
    {
        //remove all existing cards
        List<Card> deck = new List<Card>();
        for (int t = 0; t < 4; t++) //Loop 4 times for the type
            for (int n = 1; n <= 13; n++) //each type has 13 cards
            {
                Card card = Instantiate(CardPrefab, transform).GetComponent<Card>();
                card.SetCard(n, (CardType)t);
                deck.Add(card);
            }
        //shuffle the deck
        for (int i = 0; i < deck.Count; i++)
        {
            int j = Random.Range(i, deck.Count);
            Card tmp = deck[i];
            deck[i] = deck[j];
            deck[j] = tmp;
        }
        //putting cards in the right place
        float cardYPosition = 0;
        foreach (Card c in deck)
        {
            c.transform.eulerAngles = new Vector3(-90, Random.Range(-5,5), 0);
            c.transform.position = stock.transform.position + Vector3.up * cardYPosition;
            cardYPosition += 0.01f;
        }
        //Distribute cards in piles
        for (int i = 0; i < 7; i++)
        {
            for (int j = i; j < 7; j++)
            {
                piles[j].AddCard(deck[deck.Count - 1]);
                deck[deck.Count - 1].inStock = false;
                deck.RemoveAt(deck.Count - 1);
                if (i == j) piles[j].RevealLastCard();
            }
        }
        deck.Reverse();
        stock.cardList.AddRange(deck);
    }

    public void RegisterMove(string move)
    {
        print(move);
        History.Add(move);
    }

    public void Undo()
    {
        if (History.Count == 0) return;
        string[] move = History[History.Count - 1].Split(' ');
        if (move[0] == "SM") stock.Undo(int.Parse(move[1]));
        else if (move[0] == "S")
        {
            piles[int.Parse(move[1])].ReturnCardToStock();
        }
        else
            piles[int.Parse(move[1])].Undo(piles[int.Parse(move[0])], int.Parse(move[2]), move.Length == 4);
        History.RemoveAt(History.Count - 1);
    }
}
