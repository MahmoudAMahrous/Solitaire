using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    public string pileNumber = "";
    public Vector2 spaceBetweenCards = new Vector2(0.3f, 0.01f); //x for z space & y for y space
    Vector3 pileStartPosition;
    List<Card> cardList;
    //var for moving stack
    int cardIndexToFollow;
    //vars for Foundations
    public bool isFoundation = false;

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
                card.SetNewPile(true, this);
            else card.SetNewPile(false);
        }
        else
        {
            if (!cardList.Contains(card) && //if it's a new card
                (card.inStock || card.currentPile.IsLastCard(card)) &&
                ((cardList.Count == 0 && card.number == 1) || //you start with an Ace
                ((cardList.Count != 0 && cardList[cardList.Count - 1].number == card.number - 1) && //the number is appropriate
                card.type == cardList[0].type))) //the type is appropriate
                card.SetNewPile(true, this);
            else card.SetNewPile(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Card card = other.GetComponent<Card>();
        if (!card.beingDragged) return;
        card.SetNewPile(false);
    }

    public void MoveCardToPile(Card card, Pile newPile, bool moveStack = false)
    {
        string move = pileNumber + " " + newPile.pileNumber + " " + GetStackLength(card);
        cardList.Remove(card);
        newPile.AddCard(card);
        //move the stack to new pile
        if (moveStack)
        {
            while (cardIndexToFollow < cardList.Count)
                MoveCardToPile(cardList[cardIndexToFollow], newPile);
            if (RevealLastCard()) move += " F"; //last card flipped
            if (card.beingDragged) GameManager.current.RegisterMove(move); //only register the move is made by the player
        }
    }

    public void AddCard(Card card)
    {
        cardList.Add(card);
        card.MoveToPosition(pileStartPosition + Vector3.back * cardList.Count * spaceBetweenCards.x + Vector3.up * cardList.Count * spaceBetweenCards.y);
        card.currentPile = this;
    }

    public bool RevealLastCard()
    {
        if (cardList.Count != 0 && !cardList[cardList.Count - 1].cardFacingUp)
        {
            cardList[cardList.Count - 1].RevealHide(true);
            return true;
        }
        return false;
    }

    public void HideLastCard()
    {
        cardList[cardList.Count - 1].RevealHide(false);
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

    public int GetStackLength(Card card)
    {
        return cardList.Count - cardList.IndexOf(card);
    }

    public void Undo(Pile pile, int numberOfCardsToReturn, bool hideLastCard)
    {
        cardIndexToFollow = cardList.Count - numberOfCardsToReturn;
        if (hideLastCard) pile.HideLastCard();
        MoveCardToPile(cardList[cardIndexToFollow], pile, true);        
    }

    public void ReturnCardToStock()
    {
        cardList[cardList.Count - 1].inStock = true;
        cardList[cardList.Count - 1].RemovePileReferences();
        cardList.RemoveAt(cardList.Count - 1);
        GameManager.current.stock.RefreshAfterUndoFromPile();
    }
}
