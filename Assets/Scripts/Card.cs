using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum CardType { heart, club, diamond, spade}

public class Card : MonoBehaviour
{
    public int number;
    public CardType type;
    public TextMeshPro[] cardNumberTexts, cardSymbolTexts;
    public bool cardFacingUp = false;
    public Pile currentPile, newPile;
    public bool inStock = true;

    //vars for dragging the card
    public bool beingDragged = false;
    RaycastHit hit;
    //vars for moving the card
    public float cardMovingSpeed = 0.5f;
    bool cardMoving;
    Vector3 positionToTransfer;
    Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }

    public void SetCard(int n, CardType t)
    {
        number = n;
        type = t;
        string cardNum = (n != 1 && n < 11 ? n.ToString() : (n == 1 ? "A" : n == 11 ? "J" : n == 12 ? "Q" : "K"));
        Color cardColor = Color.yellow;
        string cardSymbol = "";
        switch (type)
        {
            case CardType.heart:
                cardColor = Color.red;
                cardSymbol = "♥";
                break;
            case CardType.club:
                cardColor = Color.black;
                cardSymbol = "♣";
                break;
            case CardType.diamond:
                cardColor = Color.red;
                cardSymbol = "♦";
                break;
            case CardType.spade:
                cardColor = Color.black;
                cardSymbol = "♠";
                break;
        }
        foreach (TextMeshPro num in cardNumberTexts)
        {
            num.color = cardColor;
            num.text = cardNum;
        }
        foreach (TextMeshPro sym in cardSymbolTexts)
        {
            sym.text = cardSymbol;
            sym.color = cardColor;
        }
    }

    void Update()
    {
        if (!GameManager.current.playing) return;
        DragCard();
        MoveCard();
    }

    public void StartDrag()
    {
        if (CanCardMove())
        {
            beingDragged = true;
            SetNewPile(false);
            transform.eulerAngles = Vector3.right * 90f;
            transform.position += Vector3.up * 0.5f;
        }
    }

    bool CanCardMove()
    {
        return ((inStock && GameManager.current.stock.CanCardMove(this)) ||
                (!inStock && currentPile.CanCardMove(this))) &&
                !cardMoving;
    }

    void DragCard()
    {
        //Drag the card
        if (beingDragged)
        {
            Vector3 newPos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                camera.WorldToScreenPoint(transform.position).z));
            newPos.y = transform.position.y;
            transform.position = newPos;
            if (!inStock) currentPile.MoveStack(this);
        }
        //The card is left
        if (Input.GetMouseButtonUp(0) && beingDragged)
        {
            if (!inStock && currentPile != newPile) //if moving to a new pile
            {
                currentPile.MoveCardToPile(this, newPile, true);
            }
            else if (inStock && newPile != null) //if moving from stock to new pile
            {
                inStock = false;
                newPile.AddCard(this);
                GameManager.current.stock.GoBack();
                GameManager.current.RegisterMove("S " + newPile.pileNumber, newPile.isFoundation ? 10 : 5);
            }
            else //if the pile not changed return back
            {
                if (!inStock)
                {
                    currentPile.ReturnCards();
                }
                else
                {
                    MoveToPosition(GameManager.current.stock.GetFirstWastePosition());
                }
            }
            beingDragged = false;
        }
    }

    public void MoveToPosition(Vector3 pos)
    {
        cardMoving = true;
        positionToTransfer = pos;
    }

    void MoveCard()
    {
        if (cardMoving)
        {
            transform.position = Vector3.Slerp(transform.position, positionToTransfer, cardMovingSpeed);
            if (Vector3.Distance(transform.position, positionToTransfer) < 0.001f)
            {
                cardMoving = false;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(-5, 5));
            }
        }
    }

    public void RevealHide(bool reveal)
    {
        transform.eulerAngles = new Vector3(reveal?90:-90, Random.Range(-5, 5), 0);
        cardFacingUp = reveal;
    }

    //if the card entered it's original pile then the cardwelcome function will return it anyways
    public void SetNewPile(bool cardWelcome, Pile pile = null)
    {
        if (cardWelcome) newPile = pile;
        else newPile = currentPile;
    }

    public void RemovePileReferences()
    {
        newPile = currentPile = null;
    }

    public void ResetCard()
    {
        RevealHide(false);
        currentPile = newPile = null;
        inStock = true;
    }


}
