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

    [HideInInspector] public List<CardInfo> _CardSO;

    [SerializeField] ECardState eCardState;
    [Space]
    [SerializeField] GameObject cardPrefab;
    [Space]
    [SerializeField] List<CardInfo> cardBuffer;
    [SerializeField] List<Card> cards;
    [SerializeField] List<CardInfo> trashCards;
    [SerializeField] List<CardInfo> exhaustCards;
    [Space(20)]
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardTrashPoint;
    [SerializeField] Transform cardBundle;
    [SerializeField] Transform cardDeck;
    [SerializeField] Transform cardLeftSetter;
    [SerializeField] Transform cardRightSetter;

    [SerializeField] int deckCount;

    Card hoveredCard;
    Card selectedCard;

    bool isCardDrag;
    bool onCardArea;
    bool onCardDeck;

    enum ECardState { Noting, CanMouseOver, CanMouseDrag }

    public CardInfo PopItem()
    {
        if (cardBuffer.Count == 0)
            SetupItemBuffer(UnitManager.Inst.Allies);

        CardInfo card = cardBuffer[0];
        cardBuffer.RemoveAt(0);
        return card;
    }

    void SetupItemBuffer(List<Unit> units)
    {
        cardBuffer = new List<CardInfo>();
        trashCards = new List<CardInfo>();

        for(int i = units.Count - 1; i >= 0; i--)
        {
            foreach (CardInfo cardInfo in units[i].data._CardInfo)
            {
                for (int j = 0; j < cardInfo.count; j++)
                {
                    cardInfo.unit = units[i];
                    cardBuffer.Add(new CardInfo(cardInfo));
                }
            }
        }
        for(int i = 0; i < cardBuffer.Count; i++)
        {
            int rand = Random.Range(i, cardBuffer.Count);
            (cardBuffer[rand], cardBuffer[i]) = (cardBuffer[i], cardBuffer[rand]);
        }
    }

    public void StartSet()
    {
        SetupItemBuffer(UnitManager.Inst.Allies);
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnTurnStarted += OnTurnStarted;

        //OpenDeck(cardBuffer);
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
        if (isCardDrag)
            CardDrag();

        SetCardLR(cards.Count);
        DetectCardArea();
        SetECardState();
    }

    void AddCard()
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.localPosition, Utils.QI);
        cardObject.transform.SetParent(cardBundle);
        var card = cardObject.GetComponent<Card>();
        card.SetUp(PopItem());
        cards.Add(card);

        SetOriginOrder();
        CardAlignment();
    }

    public void RemoveCards()
    {
        PRS trashCardPRS = new PRS(cardTrashPoint.localPosition, Quaternion.identity, Vector3.one);
        foreach (Card card in cards)
        {
            card.MoveTransform(trashCardPRS, true, 0.7f);
            trashCards.Add(card.cardInfo);
            Destroy(card.gameObject, 0.7f);
        }
        cards = new();
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
        if (UnitManager.sUnit_Card.UseCard(GridManager.Inst.selectedNode))
        {
            cards.Remove(selectedCard);
            trashCards.Add(selectedCard.cardInfo);
            selectedCard.transform.DOKill();
            DestroyImmediate(selectedCard.gameObject);
        }

        hoveredCard = null;
        selectedCard = null;
        CardAlignment();
    }

    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (onCardDeck)
        {
            EnlargeCard(true, card, true);
            return;
        }

        if (eCardState == ECardState.Noting || selectedCard)
            return;
        if (hoveredCard != card)
        {
            hoveredCard = card;
            EnlargeCard(true, card);
            var unit = card.GetUnit();
            UnitManager.Inst.SelectUnit(unit, true);
            unit.card.DrawArea(card.cardInfo.data);
            GridManager.Inst.ShowEntire();
        }
    }
    public void CardMouseExit(Card card)
    {
        if (selectedCard == card) return;
        EnlargeCard(false, card);
        UnitManager.Inst.DeSelectUnit(card.unit);

        if (!selectedCard) GridManager.Inst.RevertTiles();
        if (hoveredCard == card) hoveredCard = null;
    }
    public void CardMouseDown(Card card)
    {
        if (eCardState != ECardState.CanMouseDrag || onCardDeck)
            return;

        isCardDrag = true;
        UnitManager.Inst.SetOrder(false);
        LightManager.Inst.ChangeLight(true);
    }
    public void CardMouseUp(Card card)
    {
        isCardDrag = false;
        UnitManager.Inst.SetOrder(true);

        if (eCardState != ECardState.CanMouseDrag || onCardDeck)
            return;

        GridManager.Inst.RevertTiles();
        LightManager.Inst.ChangeLight(false);
        hoveredCard.ShowLiner(false);
        if (!onCardArea)
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
        if (onCardArea)
        {
            hoveredCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, hoveredCard.originPRS.scale), false, 0, false);
            hoveredCard.ShowLiner(false);
        }
        else
        {
            selectedCard.ShowLiner();
            if(selectedCard.cardInfo.data.rangeType == RangeType.Self)
            {
                var tile = GridManager.Inst.GetTile(selectedCard.unit);
                tile.OnDisplay(SelectOutline.Selected);
            }
        }
        GridManager.Inst.ShowEntire();
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("CardArea");
        onCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    void EnlargeCard(bool isEnlarge, Card card, bool isOriginPos = false)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -4f, -3);
            card.MoveTransform(new PRS(isOriginPos ? card.originPRS.pos : enlargePos, Utils.QI, Vector3.one * 1.5f), true, 0.1f);
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
            case Paze.Draw or Paze.End or Paze.Enemy:
                eCardState = ECardState.Noting;
                break;
            case Paze.Commander:
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
    Vector3 GetCardSetterPos(bool isLeft, int cardCount) => new Vector3((cardCount - 0.5f) * (isLeft ? -1 : 1), cardLeftSetter.localPosition.y, -2);
    Quaternion GetCardSetterRot(bool isLeft, int cardCount) => Quaternion.Euler(0, 0, (3 + cardCount * 2.5f) * (isLeft ? 1 : -1));

    #endregion

    #region CardDeck
    List<Card> openedCards = new List<Card>();
    void OpenDeck(List<CardInfo> cardsInfo)
    {
        onCardDeck = !onCardDeck;
        cardDeck.gameObject.SetActive(onCardDeck);
        if (onCardDeck)
        {
            cardsInfo = cardsInfo.OrderBy(x => x.data.name).ToList();
            for (int i = 0; i < cardsInfo.Count; i++)
            {
                var cardObject = Instantiate(cardPrefab);
                cardObject.transform.SetParent(cardDeck);

                var card = cardObject.GetComponent<Card>();
                card.originPRS = new PRS(new Vector3(((i % deckCount) - (deckCount - 1f) / 2) * 3.5f, i / deckCount * -4.5f + 7f, -5), Utils.QI, Vector2.one);
                card.MoveTransform(card.originPRS, false);
                card.SetUp(cardsInfo[i]);
                card.GetComponent<Order>().SetOriginOrder(99);
                openedCards.Add(card);
            }
        }
        else
        {
            foreach(Card card in openedCards)
                Destroy(card.gameObject);
            openedCards = new List<Card>();
        }
    }
    public void CallCardBuffer() => OpenDeck(cardBuffer);
    public void CallCardTrash() => OpenDeck(trashCards);
    public void CallCardExhaust() => OpenDeck(exhaustCards);
    #endregion
}
