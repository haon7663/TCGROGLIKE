using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }

    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> cards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardBundle;
    [SerializeField] Transform cardLeftSetter;
    [SerializeField] Transform cardRightSetter;
    [SerializeField] ECardState eCardState;

    List<Item> itemBuffer;
    public Card hoveredCard;
    public Card selectedCard;
    bool isMyCardDrag;
    bool onMyCardArea;
    enum ECardState { Noting, CanMouseOver, CanMouseDrag }

    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
            SetupItmeBuffer();

        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }

    void SetupItmeBuffer()
    {
        itemBuffer = new List<Item>();
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.cardCount; j++)
                itemBuffer.Add(item);
        }

        for(int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            (itemBuffer[rand], itemBuffer[i]) = (itemBuffer[i], itemBuffer[rand]);
        }

    }

    void Start()
    {
        SetupItmeBuffer();
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;

        itemSO.items.Append(itemSO.items[0]);
        foreach(Item item in itemSO.items)
        {
            print(item.name);
        }    
    }

    void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    void OnTurnStarted(bool myTurn)
    {
    }

    void Update()
    {
        if (isMyCardDrag)
            CardDrag();

        SetCardLR(cards.Count);
        DetectCardArea();
        SetECardState();
    }

    void AddCard()
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        cardObject.transform.SetParent(cardBundle);
        var card = cardObject.GetComponent<Card>();
        card.SetUp(PopItem());
        cards.Add(card);

        SetOriginOrder();
        CardAlignment();
    }

    void SetOriginOrder()
    {
        int count = cards.Count;
        for(int i = 0; i < count; i++)
        {
            var targetCard = cards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(cards.Count, 0.5f, Vector3.one);

        var targetCards = cards; //isMine ? myCards : null;
        for (int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }

    List<PRS> RoundAlignment(int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        if(objCount == 1)
            objLerps[0] = 0.5f;
        else
        {
            float interval = 1f / (objCount - 1);
            for (int i = 0; i < objCount; i++)
                objLerps[i] = interval * i;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(GetCardSetterPos(true, objCount), GetCardSetterPos(false, objCount), objLerps[i]);
            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            curve = height >= 0 ? curve : -curve;
            targetPos.y += curve;

            var targetRot = Quaternion.Slerp(GetCardSetterRot(true, objCount), GetCardSetterRot(false, objCount), objLerps[i]);
            results.Add(new PRS(targetPos, targetRot, scale));
        }
        return results;
    }

    public void TryPutCard()
    {
        if (!UnitManager.sUnit_Attack.OnAttack())
            return;
        cards.Remove(selectedCard);
        selectedCard.transform.DOKill();
        DestroyImmediate(selectedCard.gameObject);

        hoveredCard = null;
        selectedCard = null;
        CardAlignment();
    }


    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Noting || selectedCard)
            return;
        if (hoveredCard != card)
        {
            hoveredCard = card;
            UnitManager.sUnit_Attack.DrawArea(card.item);
            EnlargeCard(true, card);
        }
    }
    public void CardMouseExit(Card card)
    {
        if (selectedCard == card) return;
        EnlargeCard(false, card);

        if (!selectedCard) GridManager.Inst.RevertTiles();
        if (hoveredCard == card) hoveredCard = null;
    }
    public void CardMouseDown(Card card)
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        isMyCardDrag = true;
    }
    public void CardMouseUp(Card card)
    {
        isMyCardDrag = false;

        if (eCardState != ECardState.CanMouseDrag)
            return;

        GridManager.Inst.RevertTiles();
        if (!onMyCardArea)
            TryPutCard();
        else
        {
            selectedCard = null;
            hoveredCard = null;
            EnlargeCard(false, card);
        }
    }

    void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        SelectCard(true, hoveredCard);
        if (onMyCardArea)
        {
            hoveredCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, hoveredCard.originPRS.scale), false);
            hoveredCard.ShowLiner(false);
        }
        else
        {
            selectedCard.ShowLiner();
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("CardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4f, -9);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 1.5f), true, 0.1f);
        }
        else
            card.MoveTransform(card.originPRS, true, 0.3f);

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }
    void SelectCard(bool isSelect, Card card)
    {
        if (isSelect)
        {
            selectedCard = card;
            Vector3 enlargePos = new Vector3(0, -4.06f, -9);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one), true, 0.1f);
        }
        else
        {
            selectedCard.ShowLiner(false);
            selectedCard = null;
            card.MoveTransform(card.originPRS, true, 0.3f);
        }

        card.GetComponent<Order>().SetMostFrontOrder(isSelect);
    }

    void SetECardState()
    {
        switch (TurnManager.Inst.paze)
        {
            case Paze.Draw | Paze.End | Paze.Enemy:
                eCardState = ECardState.Noting;
                break;
            case Paze.Move:
                eCardState = ECardState.CanMouseOver;
                break;
            case Paze.Card:
                eCardState = ECardState.CanMouseDrag;
                break;
        }
    }

    void SetCardLR(int cardCount)
    {
        cardLeftSetter.localPosition = Vector3.Lerp(cardLeftSetter.localPosition, GetCardSetterPos(true, cardCount), Time.deltaTime * 5);
        cardRightSetter.localPosition = Vector3.Lerp(cardRightSetter.localPosition, GetCardSetterPos(false, cardCount), Time.deltaTime * 5);
        cardLeftSetter.rotation = Quaternion.Lerp(cardLeftSetter.rotation, GetCardSetterRot(true, cardCount), Time.deltaTime * 5);
        cardRightSetter.rotation = Quaternion.Lerp(cardRightSetter.rotation, GetCardSetterRot(false, cardCount), Time.deltaTime * 5);
    }
    Vector3 GetCardSetterPos(bool isLeft, int cardCount)
    {
        return new Vector3((cardCount - 0.5f) * (isLeft ? -1 : 1), cardLeftSetter.localPosition.y, -8);
    }
    Quaternion GetCardSetterRot(bool isLeft, int cardCount)
    {
        return Quaternion.Euler(0, 0, (3 + cardCount * 2.5f) * (isLeft ? 1 : -1));
    }

    #endregion
}
