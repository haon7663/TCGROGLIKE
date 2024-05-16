using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.Serialization;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    private void Awake() => Inst = this;
    
    private enum ECardState { Noting, CanMouseOver, CanMouseDrag }

    [Header("카드 상태")]
    [SerializeField] private ECardState eCardState;
    
    [Header("프리팹")]
    [SerializeField] private GameObject cardPrefab;
    
    [Header("덱")]
    [SerializeField] private List<CardInfo> cardBuffer;
    [SerializeField] private List<Card> cards;
    [SerializeField] private List<CardInfo> trashCards;
    [SerializeField] private List<CardInfo> exhaustCards;
    
    [Header("트랜스폼")]
    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform cardTrashPoint;
    [SerializeField] private Transform cardBundle;
    [SerializeField] private Transform cardDeck;
    [SerializeField] private Transform cardLeftSetter;
    [SerializeField] private Transform cardRightSetter;
    
    [SerializeField] private int deckCount;
    
    [HideInInspector] public List<CardInfo> cardInfos;
    [HideInInspector] public Card hoveredCard;

    private List<Card> _usingCards;
    private Card _selectedCard;

    private bool _isCardDrag;
    private bool _onCardArea;
    private bool _onCardDeck;

    public CardInfo PopItem()
    {
        if (cardBuffer.Count == 0)
            SetupItemBuffer(UnitManager.inst.allies);

        CardInfo card = cardBuffer[0];
        cardBuffer.RemoveAt(0);
        return card;
    }

    private void SetupItemBuffer(List<Unit> units)
    {
        cardBuffer = new List<CardInfo>();
        trashCards = new List<CardInfo>();

        for(var i = units.Count - 1; i >= 0; i--)
        {
            foreach (var cardInfo in units[i].data.cardInfo)
            {
                for (var j = 0; j < cardInfo.count; j++)
                {
                    cardInfo.unit = units[i];
                    cardBuffer.Add(new CardInfo(cardInfo));
                }
            }
        }
        for(var i = 0; i < cardBuffer.Count; i++)
        {
            var rand = Random.Range(i, cardBuffer.Count);
            (cardBuffer[rand], cardBuffer[i]) = (cardBuffer[i], cardBuffer[rand]);
        }
    }

    public void StartSet()
    {
        SetupItemBuffer(UnitManager.inst.allies);
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

    private void Update()
    {
        if (_isCardDrag)
            CardDrag();

        SetCardLR(cards.Count);
        DetectCardArea();
        SetECardState();
    }

    private void AddCard()
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
    private void CardAlignment()
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

    private List<PRS> RoundAlignment(int objCount, float height, Vector3 scale)
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

    public void PutCard(Card card)
    {
        StartCoroutine(card.unit.card.UseCard(GridManager.inst.selectedNode));
        
        cards.Remove(card);
        trashCards.Add(card.cardInfo);
        card.transform.DOKill();
        DestroyImmediate(card.gameObject);

        hoveredCard = null;
        _selectedCard = null;
        CardAlignment();
    }

    #region MyCard

    public void CardMouseOver(Card card)
    {
        if (_onCardDeck)
        {
            EnlargeCard(true, card, true);
            return;
        }

        if (eCardState == ECardState.Noting || _selectedCard)
            return;
        if (hoveredCard != card)
        {
            LightManager.inst.ChangeLight(true);
            hoveredCard = card;
            EnlargeCard(true, card);
            var unit = card.GetUnit();
            UnitManager.inst.SelectUnit(unit, true);
            unit.card.DrawRange(card.cardInfo.data, eCardState == ECardState.CanMouseDrag);
        }
    }
    public void CardMouseExit(Card card)
    {
        if (_selectedCard == card)
            return;

        EnlargeCard(false, card);
        UnitManager.inst.DeSelectUnit(card.unit);
        LightManager.inst.ChangeLight(false);

        if (!_selectedCard)
        {
            GridManager.inst.RevertTiles(card.unit);
        }
        if (hoveredCard == card) hoveredCard = null;
    }
    public void CardMouseDown(Card card)
    {
        if (eCardState != ECardState.CanMouseDrag || _onCardDeck)
            return;

        _isCardDrag = true;
        UnitManager.inst.SetOrderUnits(false);
        LightManager.inst.ChangeLight(true);
    }
    public void CardMouseUp(Card card)
    {
        _isCardDrag = false;
        UnitManager.inst.SetOrderUnits(true);

        if (eCardState != ECardState.CanMouseDrag || _onCardDeck)
            return;

        LightManager.inst.ChangeLight(false);
        hoveredCard.ShowLiner(false);
        
        _selectedCard = null;
        hoveredCard = null;
        EnlargeCard(false, card);
        
        if (_onCardArea)
            return;

        PutCard(card);
        //StartCoroutine(UnitManager.sUnit.card.UseCard(GridManager.inst.selectedNode, card.cardInfo.data));
        
        print("ASE");
    }

    private void CardDrag()
    {
        if (eCardState != ECardState.CanMouseDrag)
            return;

        SelectCard(true, hoveredCard);
        if (_onCardArea)
        {
            hoveredCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, hoveredCard.originPRS.scale), false, 0, false);
            hoveredCard.ShowLiner(false);
        }
        else
        {
            _selectedCard.ShowLiner();
            if(_selectedCard.cardInfo.data.rangeType == RangeType.Self)
            {
                var tile = GridManager.inst.GetTile(_selectedCard.unit);
                //tile.OnDisplay(AreaType.Select);
            }
        }
        //GridManager.Inst.ShowEntire();
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("CardArea");
        _onCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
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
            _selectedCard = card;
            Vector3 enlargePos = new Vector3(0, -4.06f, -9);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one), true, 0.1f);
        }
        else
        {
            _selectedCard.ShowLiner(false);
            _selectedCard = null;
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
        _onCardDeck = !_onCardDeck;
        cardDeck.gameObject.SetActive(_onCardDeck);
        if (_onCardDeck)
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
