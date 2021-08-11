using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stock : MonoBehaviour
{
    [HideInInspector]
    public List<Card> cardList;
    public float spaceBetweenCards = 0.01f;
    int cardPointer = -1;
    public Vector3[] wastePositions; //waste cards position is relative to the stock object
    public bool TurnThreeMode = false;
    //vars for clicking the stock
    RaycastHit hit;

    void Start()
    {
        cardList = new List<Card>();
    }

    private void Update()
    {
        ClickStock();
    }

    void ClickStock()
    {
        if (Input.GetMouseButtonDown(0) &&
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) && hit.transform == transform)
        {
            string move = "SM ";
            int cardsShown = 0;
            for (int i = 0; i < (TurnThreeMode ? 3 : 1); i++)
            {
                cardPointer = GetNextLast(1);
                if (cardPointer == -2 && i == 0)
                {
                    cardsShown = -1;
                    ResetStock();
                    break;
                }
                else if (cardPointer != -2)
                {
                    RefreshStockPositions();
                    cardsShown++;
                }
            }
            move += cardsShown;
            GameManager.current.RegisterMove(move, cardsShown < 0 ? -100 : 0);
        }
    }

    void RefreshStockPositions()
    {
        int wastePos = 1;
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            if (!cardList[i].inStock) continue;
            if (i == cardPointer)
            {
                cardList[i].RevealHide(true);
                cardList[i].MoveToPosition(transform.position + wastePositions[0]);
            }
            else if (i > cardPointer)
            {
                cardList[i].RevealHide(false);
                cardList[i].MoveToPosition(transform.position + Vector3.up * (cardList.Count - 1 - i) * spaceBetweenCards);
            }
            else
            {
                cardList[i].RevealHide(true);
                cardList[i].MoveToPosition(transform.position + wastePositions[wastePos < 3 ? wastePos : 2] -
                    (wastePos >= 3 ? Vector3.up * wastePos * spaceBetweenCards : Vector3.zero));
                wastePos++;
            }
        }
    }

    int GetNextLast(int direction) //direction = 1 for next -1 for last and returns -2 if went out of boundries
    {
        if (cardPointer == -2) return -2;
        int cardIndex = cardPointer + direction;
        while (cardIndex >= 0 && cardIndex < cardList.Count && !cardList[cardIndex].inStock) cardIndex += direction;
        if (cardIndex == cardList.Count) return -2;
        else if (cardIndex < 0) return -1;
        return cardIndex;
    }

    void ResetStock()
    {
        cardPointer = -1;
        RefreshStockPositions();
    }

    public bool CanCardMove(Card card)
    {
        if (cardList.IndexOf(card) == cardPointer) return true;
        return false;
    }

    public void GoBack()
    {
        cardPointer = GetNextLast(-1);
        if (cardPointer != -1) RefreshStockPositions();
    }

    public void Undo(int cardsShown)
    {
        if (cardsShown < 0)
        {
            cardPointer = cardList.Count;
            cardPointer = GetNextLast(-1);
            RefreshStockPositions();
        }
        else
            for (int i = 0; i < cardsShown; i++)
            {
                cardPointer = GetNextLast(-1);
                RefreshStockPositions();
            }
    }

    public void RefreshAfterUndoFromPile()
    {
        cardPointer = GetNextLast(1);
        RefreshStockPositions();
    }
}
