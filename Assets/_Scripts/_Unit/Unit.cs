using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Unit : MonoBehaviour
{
    [HideInInspector] public UnitMove move;
    [HideInInspector] public UnitCard card;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private GameObject _actionObject;
    private SpriteRenderer _actionSpriteRenderer;
    
    private TMP_Text _actionTMP;

    public HexCoords coords;
    public UnitData data;

    [Header("Stats")]
    public int hp;
    public int defence;
    private int _value;

    [Header("Systems")]
    public Unit targetUnit;
    public HexCoords targetCoords;
    public List<StatusInfo> statuses;

    private void Awake()
    {
        if (transform.TryGetComponent(out UnitMove unitMove))
            move = unitMove;
        if (transform.TryGetComponent(out UnitCard unitCard))
            card = unitCard;

        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            _spriteRenderer = spriteRenderer;
        if (transform.GetChild(0).TryGetComponent(out Animator animator))
            _animator = animator;

        _actionObject = transform.GetChild(3).gameObject;
        if (_actionObject.transform.GetChild(0).TryGetComponent(out SpriteRenderer renderer))
            _actionSpriteRenderer = renderer;
        if (_actionObject.transform.GetChild(1).TryGetComponent(out TMP_Text text))
            _actionTMP = text;
    }

    public void Init(UnitData unitData, HexCoords coords)
    {
        data = Instantiate(unitData);

        _animator.runtimeAnimatorController = this.data.animatorController;

        hp = data.hp;
        HealthManager.inst.GenerateHealthBar(this);

        this.coords = coords;
        transform.position = coords.Pos - Vector3.forward;
        GridManager.inst.SetTileUnit(coords, this);
    }

    #region OnReceive
    public bool OnDamage(int value)
    {
        if(true)
        {
            if (defence >= value)
                defence -= value;
            else
            {
                var overValue = defence - value;
                hp += overValue;
                defence = 0;
                StartCoroutine(HealthManager.inst.WhiteMaterial(this));
                Anim_SetTrigger("hit");
            }
            HealthManager.inst.UpdateHealthBar(this);
            UIManager.inst.ShowDamageTMP(this, value);

            if (hp <= 0)
            {
                UnitManager.inst.Death(this);
            }
            return true;
        }
    }
    public bool OnHealth(int value)
    {
        if (true)
        {
            print("Health");
            hp += value;
            if (hp >= data.hp)
                hp = data.hp;
            
            HealthManager.inst.UpdateHealthBar(this);
            UIManager.inst.ShowRecoveryTMP(this, value);
            return true;
        }
    }
    public bool OnDefence(int value)
    {
        if (true)
        {
            print("Defence");
            defence += value;
            HealthManager.inst.UpdateHealthBar(this);
            return true;
        }
    }
    #endregion

    private void OnMouseOver()
    {
        UnitManager.inst.UnitMouseOver(this);
    }
    private void OnMouseExit()
    {
        UnitManager.inst.UnitMouseExit(this);
    }
    private void OnMouseDown()
    {
        UnitManager.inst.UnitMouseDown(this);
    }
    private void OnMouseUp()
    {
        UnitManager.inst.UnitMouseUp(this);
    }

    #region Animations
    public void Anim_SetTrigger(string name) => _animator.SetTrigger(name);
    public void Anim_SetBool(string name, bool value) => _animator.SetBool(name, value);
    #endregion

    public void SetMaterial(Material material) => _spriteRenderer.material = material;
    public void SetFlipX(bool value) => _spriteRenderer.flipX = value;
    public void Repeat(float x) => SetFlipX(coords.Pos.x < x);

    public void ShowAction(Sprite sprite, int value)
    {
        this._value = value;
        _actionObject.SetActive(true);
        _actionSpriteRenderer.sprite = sprite;
        SetActionText();
    }
    public void SetActionText()
    {
        //if(actionObject.activeSelf)
            //actionText.text = StatusManager.Calculate(this, card.data, value).ToString();
    }
    public void HideAction()
    {
        _actionObject.SetActive(false);
    }
}
