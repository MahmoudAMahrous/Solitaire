using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    public string pileNumber = "";
    public Vector2 spaceBetweenCards = new Vector2(0.3f, 0.01f); //x for z space & y for y space
    public GameObject pilePlace;
    Vector3 pileStartPosition;
    List<Card> cardList;
    //var for moving stack
    int cardIndexToFollow;
    //vars for Foundations
    public bool isFoundation = false;

    public void SetUpPile(int pileNum)
    {
        pileNumber = pileNum.ToString();
        pileStartPosition = transform.position;
        if (!isFoundation) pileStartPosition.z += transform.localScale.z / 2 + 0.5f;
        cardList = new List<Card>();
        Instantiate(pilePlace, pileStartPosition - Vector3.up * spaceBetweenCards.y, Quaternion.Euler(90, 0, 0)).transform.SetParent(transform, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Card card = other.GetComponent<Card>();
        if (!card.beingDragged) return;
        card.SetNewPile(IsCardWelcome(card), this);
    }

    private void OnTriggerExit(Collider other)
    {
        Card card = other.GetComponent<Card>();
        if (!card.beingDragged) return;
        card.SetNewPile(false);
    }

    public bool IsCardWelcome(Card card)
    {
        if (!isFoundation)
        {
            if (!cardList.Contains(card) && //if it's a new card
                ((cardList.Count == 0 && card.number == 13) || //you start with a king
                (cardList.Count != 0 && cardList[cardList.Count - 1].number == card.number + 1 && //the number is appropriate
                (Mathf.Abs(card.type - cardList[cardList.Count - 1].type) != 2 && card.type != cardList[cardList.Count - 1].type)))) //the color is appropriate
                return true;
        }
        else
        {
            if (!cardList.Contains(card) && //if it's a new card
                (card.inStock || card.currentPile.IsLastCard(card)) &&
                ((cardList.Count == 0 && card.number == 1) || //you start with an Ace
                ((cardList.Count != 0 && cardList[cardList.Count - 1].number == card.number - 1) && //the number is appropriate
                card.type == cardList[0].type))) //the type is appropriate
                return true;
        }
        return false;
    }

    public void MoveCardToPile(Card card, Pile newPile, bool hasStack = false)
    {
        string move = pileNumber + " " + newPile.pileNumber + " " + GetStackLength(card);
        int score = 0;
        cardList.Remove(card);
        if (isFoundation) GameManager.current.CardAddedToFoundation(false);
        newPile.AddCard(card);
        if (hasStack)
        {
            //move the stack to new pile 
            while (cardIndexToFollow < cardList.Count)
                MoveCardToPile(cardList[cardIndexToFollow], newPile);
            if (RevealLastCard())
            {
                move += " F"; //last card flipped
                score += 5;
            }
            else move += " N"; //last card wasn't flipped
            if (!isFoundation && newPile.isFoundation) score = 10;
            else if (isFoundation && !newPile.isFoundation) score = -15;
            if (card.beingDragged) GameManager.current.RegisterMove(move, score); //only register the move if made by the player
        }
    }

    public void AddCard(Card card)
    {
        if (isFoundation) GameManager.current.CardAddedToFoundation();
        card.MoveToPosition(GetNewCardPosition());
        cardList.Add(card);
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
        cardIndexToFollow = GetCardIndex(card);
        if (cardIndexToFollow < 0 || cardIndexToFollow == cardList.Count - 1) return;
        for (int i = cardIndexToFollow + 1; i < cardList.Count; i++)
        {
            cardList[i].MoveToPosition(cardList[cardIndexToFollow].transform.position +
                Vector3.back * (i - cardIndexToFollow) * spaceBetweenCards.x + Vector3.up * (i - cardIndexToFollow) * spaceBetweenCards.y);
        }
    }

    public void ReturnCards()
    {
        for (int i = cardIndexToFollow; i < cardList.Count; i++)
        {
            cardList[i].MoveToPosition(pileStartPosition +
                Vector3.back * (i) * spaceBetweenCards.x + Vector3.up * (i) * spaceBetweenCards.y);
        }
    }

    public bool IsLastCard(Card card)
    {
        if (GetCardIndex(card) == cardList.Count - 1) return true;
        return false;
    }

    //only for foundations
    public bool CanCardMove(Card card)
    {
        if (!isFoundation || GetCardIndex(card) == cardList.Count - 1) return true; //making sure the player can only take the card on the top
        return false;
    }

    public int GetStackLength(Card card)
    {
        return cardList.Count - GetCardIndex(card);
    }

    public void Undo(Pile pile, int numberOfCardsToReturn, bool hideLastCard)
    {
        cardIndexToFollow = cardList.Count - numberOfCardsToReturn;
        if (hideLastCard)
        {
            pile.HideLastCard();
        }
        MoveCardToPile(cardList[cardIndexToFollow], pile, true);
    }

    public void ReturnCardToStock()
    {
        cardList[cardList.Count - 1].inStock = true;
        cardList[cardList.Count - 1].RemovePileReferences();
        cardList.RemoveAt(cardList.Count - 1);
        GameManager.current.stock.RefreshAfterUndoFromPile();
    }

    public Card GetLastCard()
    {
        return cardList.Count == 0 ? null : cardList[cardList.Count - 1];
    }

    public Vector3 GetNewCardPosition()
    {
        return pileStartPosition + Vector3.back * cardList.Count * spaceBetweenCards.x + Vector3.up * cardList.Count * spaceBetweenCards.y;
    }

    public bool OnTopOfASimilarCard(Card card)
    {
        int i = GetCardIndex(card);
        if (i != 0 && cardList[i - 1].cardFacingUp) return true;
        else return false;
    }

    public int GetCardIndex(Card card)
    {
        return cardList.IndexOf(card);
    }

    public void ClearPile()
    {
        cardList.Clear();
    }

    public void RefreshFoundationPositions()
    {
        pileStartPosition = transform.position;
        for (int i = 0; i < cardList.Count; i++)
        {
            cardList[i].MoveToPosition(pileStartPosition + Vector3.back * i * spaceBetweenCards.x +
                Vector3.up * i * spaceBetweenCards.y);
        }
    }
}
