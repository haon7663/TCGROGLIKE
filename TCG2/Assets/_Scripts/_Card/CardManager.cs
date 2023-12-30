using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }

    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] List<Card> myCards;
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

        SetCardLR(myCards.Count);
        DetectCardArea();
        SetECardState();
    }

    void AddCard()
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        cardObject.transform.SetParent(cardBundle);
        var card = cardObject.GetComponent<Card>();
        card.SetUp(PopItem());
        myCards.Add(card);

        SetOriginOrder();
        CardAlignment();
    }

    void SetOriginOrder()
    {
        int count = myCards.Count;
        for(int i = 0; i < count; i++)
        {
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCards.Count, 0.5f, Vector3.one);

        var targetCards = myCards; //isMine ? myCards : null;
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

    public bool TryPutCard()
    {
        Card card = hoveredCard;
        var spawnPos = Utils.MousePos;
        var targetCards = myCards;

        if (true)
        {
            targetCards.Remove(card);
            card.transform.DOKill();
            DestroyImmediate(card.gameObject);
            hoveredCard = null;
            selectedCard = null;
            CardAlignment();
            return true;
        }
        else
        {
            targetCards.ForEach(x => x.GetComponent<Order>().SetMostFrontOrder(false));
            CardAlignment();
            return false;
        }
    }


    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (eCardState == ECardState.Noting || selectedCard)
            return;
        if (hoveredCard != card)
        {
            hoveredCard = card;
            UnitManager.sUnit_Attack.OnDrawArea(true, card.item);
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

        if (onMyCardArea)
        {
            hoveredCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, hoveredCard.originPRS.scale), false);
        }
        else
        {
            SelectCard(true, hoveredCard);
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
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -3.75f, -9);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 1.2f), true, 0.1f);
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
            selectedCard = null;
            card.MoveTransform(card.originPRS, true, 0.3f);
        }

        card.GetComponent<Order>().SetMostFrontOrder(isSelect);
    }

    void SetECardState()
    {
        if (TurnManager.Inst.isLoading)
            eCardState = ECardState.Noting;
        else if (!TurnManager.Inst.myTurn)
            eCardState = ECardState.CanMouseOver;
        else if (TurnManager.Inst.myTurn)
            eCardState = ECardState.CanMouseDrag;
    }

    void SetCardLR(int cardCount)
    {
        cardLeftSetter.position = Vector3.Lerp(cardLeftSetter.position, GetCardSetterPos(true, cardCount), Time.deltaTime * 5);
        cardRightSetter.position = Vector3.Lerp(cardRightSetter.position, GetCardSetterPos(false, cardCount), Time.deltaTime * 5);
        cardLeftSetter.rotation = Quaternion.Lerp(cardLeftSetter.rotation, GetCardSetterRot(true, cardCount), Time.deltaTime * 5);
        cardRightSetter.rotation = Quaternion.Lerp(cardRightSetter.rotation, GetCardSetterRot(false, cardCount), Time.deltaTime * 5);
    }
    Vector3 GetCardSetterPos(bool isLeft, int cardCount)
    {
        return new Vector3((cardCount - 0.5f) * (isLeft ? -1 : 1), cardLeftSetter.position.y, -8);
    }
    Quaternion GetCardSetterRot(bool isLeft, int cardCount)
    {
        return Quaternion.Euler(0, 0, (3 + cardCount * 2.5f) * (isLeft ? 1 : -1));
    }

    #endregion
}
