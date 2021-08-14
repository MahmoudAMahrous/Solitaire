using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float clickMaxTime = 0.1f;
    public Help help;
    RaycastHit hit;
    Card cardBeingDragged;
    float clickTime = 0;

    void Update()
    {
        if (!GameManager.current.playing) return;
        clickTime += Time.deltaTime;
        CheckForAClick();
    }

    void CheckForAClick()
    {
        if (cardBeingDragged == null &&
            Input.GetMouseButtonDown(0) &&
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (hit.transform.GetComponent<Stock>() != null)
            {
                GameManager.current.stock.ClickStock(true);
                return;
            }
            cardBeingDragged = hit.transform.GetComponent<Card>();
            if (cardBeingDragged != null && !cardBeingDragged.StartDragging()) cardBeingDragged = null;
            clickTime = 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (cardBeingDragged == null) return;
            if (clickTime <= clickMaxTime)
            {
                Pile newPile = help.CanCardGo(cardBeingDragged);
                if (newPile != null) cardBeingDragged.SetNewPile(true, newPile);
            }
            cardBeingDragged.StopDragging();
            cardBeingDragged = null;
        }
    }
}
