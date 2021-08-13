using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    
    RaycastHit hit;
    bool aCardIsBeingDragged = false;
    float timeSinceLastClick;

    void Update()
    {
        if (!GameManager.current.playing) return;
        timeSinceLastClick += Time.deltaTime;
        CheckForAClick();
    }

    void CheckForAClick()
    {
        if (!aCardIsBeingDragged &&
            Input.GetMouseButtonDown(0) &&
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
        {
            if (hit.transform.GetComponent<Stock>() != null)
            {
                GameManager.current.stock.ClickStock();
                return;
            }
            Card clickedCard = hit.transform.GetComponent<Card>();
            if (clickedCard != null)
            {
                clickedCard.StartDrag();
                aCardIsBeingDragged = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            timeSinceLastClick = 0;
            aCardIsBeingDragged = false;
        }
    }
}
