using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stock : MonoBehaviour
{
    [HideInInspector]
    public List<Card> cardList;
    public float spaceBetweenCards = 0.01f;
    int cardPointer;
    public Vector3[] wastePositions; //waste cards position is relative to the stock object
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
            if (cardPointer == cardList.Count)
            {
                ResetStock();
                return;
            }
            while (!cardList[cardPointer].inStock) cardPointer++;
            cardList[cardPointer].RotateCard();
            cardList[cardPointer].MoveToPosition(transform.position + wastePositions[0]);
            for (int i = cardPointer - 1; i >= 0; i--)
            {
                if (cardList[i].inStock)
                    cardList[i].MoveToPosition(transform.position + wastePositions[i == cardPointer - 1 ? 1 : 2]);
            }
            cardPointer++;
        }
    }

    void ResetStock()
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            if (!cardList[i].inStock) continue;
            cardList[i].MoveToPosition(transform.position + Vector3.up * (cardList.Count - 1 - i) * spaceBetweenCards);
            cardList[i].RotateCard();
        }
        cardPointer = 0;
    }

    public bool CanCardMove(Card card)
    {
        if (cardList.IndexOf(card) == cardPointer - 1) return true;
        return false;
    }
}
