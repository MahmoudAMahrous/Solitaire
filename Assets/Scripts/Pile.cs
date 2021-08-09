using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    public int pileNumber = 0;
    public Vector2 spaceBetweenCards = new Vector2(0.3f, 0.01f); //x for z space & y for y space
    Vector3 pileStartPosition;
    List<Card> cardList;
    //var for moving stack
    int cardIndexToFollow;
    //vars for Foundations
    public bool isFoundation = false;
    public CardType foundationType;

    void Start()
    {
        pileStartPosition = transform.position;
        if (!isFoundation) pileStartPosition.z += transform.localScale.z / 2 + 0.5f;
        cardList = new List<Card>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Card card = other.GetComponent<Card>();
        if (!card.beingDragged) return;
        if (!isFoundation)
        {
            if (!cardList.Contains(card) && //if it's a new card
                ((cardList.Count == 0 && card.number == 13) || //you start with a king
                (cardList.Count != 0 && cardList[cardList.Count - 1].number == card.number + 1 && //the number is appropriate
                (Mathf.Abs(card.type - cardList[cardList.Count - 1].type) != 2 && card.type != cardList[cardList.Count - 1].type)))) //the color is appropriate
                card.CardWelcome(true, this);
            else card.CardWelcome(false);
        }
        else
        {
            if (!cardList.Contains(card) && //if it's a new card
                (card.inStock || card.currentPile.IsLastCard(card)) &&
                ((cardList.Count == 0 && card.number == 1) || //you start with an Ace
                (cardList.Count != 0 && cardList[cardList.Count - 1].number == card.number - 1) && //the number is appropriate
                card.type == foundationType)) //the type is appropriate
                card.CardWelcome(true, this);
            else card.CardWelcome(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Card card = other.GetComponent<Card>();
        if (!card.beingDragged) return;
        card.CardWelcome(false);
    }

    public void MoveCardToPile(Card card, Pile newPile, bool moveStack = false)
    {
        cardList.Remove(card);
        newPile.AddCard(card);
        //move the stack to new pile
        if (moveStack)
            while (cardIndexToFollow < cardList.Count)
            {
                MoveCardToPile(cardList[cardIndexToFollow], newPile);
            }
        RevealLastCard();
    }

    public void AddCard(Card card)
    {
        cardList.Add(card);
        card.MoveToPosition(pileStartPosition + Vector3.back * cardList.Count * spaceBetweenCards.x + Vector3.up * cardList.Count * spaceBetweenCards.y);
        card.currentPile = this;
    }

    public void RevealLastCard()
    {
        if (cardList.Count != 0 && !cardList[cardList.Count - 1].cardFacingUp)
            cardList[cardList.Count - 1].RotateCard();
    }

    public void MoveStack(Card card)
    {
        cardIndexToFollow = cardList.IndexOf(card);
        if (cardIndexToFollow < 0 || cardIndexToFollow == cardList.Count - 1) return;
        for (int i = cardIndexToFollow + 1; i < cardList.Count; i++)
        {
            cardList[i].MoveToPosition(cardList[cardIndexToFollow].transform.position +
                Vector3.back * (i - cardIndexToFollow) * spaceBetweenCards.x + Vector3.up * (i - cardIndexToFollow) * spaceBetweenCards.y);
        }
    }

    public void StopMovingStack()
    {
        if (cardIndexToFollow == cardList.Count - 1) return;
        for (int i = cardIndexToFollow + 1; i < cardList.Count; i++)
        {
            cardList[i].MoveToPosition(pileStartPosition +
                Vector3.back * (i+1) * spaceBetweenCards.x + Vector3.up * (i+1) * spaceBetweenCards.y);
        }
    }

    public bool IsLastCard(Card card)
    {
        if (cardList.IndexOf(card) == cardList.Count - 1) return true;
        return false;
    }

    //only for foundations
    public bool CanCardMove(Card card)
    {
        if (!isFoundation || cardList.IndexOf(card) == cardList.Count - 1) return true; //making sure the player can only take the card on the top
        return false;
    }
}
