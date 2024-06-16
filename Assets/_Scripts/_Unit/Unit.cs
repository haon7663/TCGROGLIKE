using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public enum StatType { MaxHealth = 100, Cost = 200, GetDamage = 300, TakeDamage = 400, TakeDefence = 500, TakeRecovery = 600, }

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
    public UnitSO unitSO;

    public Dictionary<StatType, UnitStat> Stats = new();
    [Header("스탯")]
    public int hp;
    public int defence;

    [Header("상태")]
    public bool canMove;
    public bool canAction;
    public List<StatusEffectSO> statuses;

    [Header("시스템")]
    public Unit targetUnit;
    public HexCoords targetCoords;

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
        if (_actionObject.transform.GetChild(0).TryGetComponent(out SpriteRenderer actionSpriteRenderer))
            _actionSpriteRenderer = actionSpriteRenderer;
        if (_actionObject.transform.GetChild(1).TryGetComponent(out TMP_Text actionTMP))
            _actionTMP = actionTMP;
    }

    public void Init(UnitSO unitSO, HexCoords coords)
    {
        this.unitSO = Instantiate(unitSO);

        _animator.runtimeAnimatorController = this.unitSO.animatorController;
    
        Stats = new Dictionary<StatType, UnitStat>
        {
            { StatType.MaxHealth, new UnitStat() },
            { StatType.Cost, new UnitStat() },
            { StatType.GetDamage, new UnitStat() },
            { StatType.TakeDamage, new UnitStat() },
            { StatType.TakeDefence, new UnitStat() },
            { StatType.TakeRecovery, new UnitStat() }
        };

        hp = Stats[StatType.MaxHealth].GetValue(this.unitSO.hp);
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
                
                CameraMovement.inst.VibrationForTime(0.15f);
                DOVirtual.Float(0.25f, 1, 0.5f, t =>
                {
                    Time.timeScale = t;
                    Time.fixedDeltaTime = Time.timeScale * 0.02f;
                }).SetEase(Ease.OutCubic);
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
            if (hp >= unitSO.hp)
                hp = unitSO.hp;
            
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
    public void Repeat(float x) => _spriteRenderer.flipX = coords.Pos.x < x;

    public void ShowAction(Sprite sprite)
    {
        _actionObject.SetActive(true);
        _actionSpriteRenderer.sprite = sprite;
        SetActionText();
    }

    public void ShowInfo()
    {
        UnitManager.inst.ShowUnitInfo(this);
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
