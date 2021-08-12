using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Help : MonoBehaviour
{
    public Transform helpIndicator;

    private void Start()
    {
        helpIndicator.localScale = Vector3.zero;
    }

    public bool FindMove(out Card suggestedCard, out Pile suggestedPile)
    {
        suggestedCard = null;
        suggestedPile = null;
        foreach (Card card in GameManager.current.cardReferences)
        {
            Pile p = CanCardGo(card);
            if (p != null)
            {
                suggestedCard = card;
                suggestedPile = p;
                return true;
            }
        }
        return false;
    }

    public Pile CanCardGo(Card card)
    {
        if (card.cardFacingUp)
        {
            if (card.inStock && !GameManager.current.stock.CanCardMove(card)) return null;
            else if (!card.inStock && card.currentPile.isFoundation) return null;
            foreach (Pile pile in GameManager.current.piles)
            {
                if (pile == card.currentPile) continue;
                if (card.number == 1 && !pile.isFoundation) continue;
                if (card.number == 13 && !pile.isFoundation && !card.inStock && card.currentPile.GetCardIndex(card) == 0) continue;
                if (!card.inStock && card.currentPile.OnTopOfASimilarCard(card) && !pile.isFoundation) continue;
                if (pile.IsCardWelcome(card)) return pile;
            }
        }
        return null;
    }

    public void ShowHelp()
    {
        Pile pile;
        Card card;
        helpIndicator.DOComplete();
        if (FindMove(out card, out pile)) //Move found
        {
            helpIndicator.localScale = Vector3.one;
            helpIndicator.position = card.transform.position;
            helpIndicator.DOMove(pile.GetNewCardPosition(), 0.5f).SetLoops(2, LoopType.Restart).OnComplete(() =>
             {
                 helpIndicator.localScale = Vector3.zero;
             }).SetEase(Ease.Linear);
        }
        else //try the stock
        {
            helpIndicator.localScale = Vector3.one;
            helpIndicator.position = GameManager.current.stock.transform.position + Vector3.up * 0.3f;
            helpIndicator.DOMove(GameManager.current.stock.transform.position + Vector3.left + Vector3.up * 0.3f, 0.5f).SetLoops(2, LoopType.Restart).OnComplete(() =>
            {
                helpIndicator.localScale = Vector3.zero;
            }).SetEase(Ease.OutBack);
        }
    }
}
