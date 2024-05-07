using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectButton : MonoBehaviour
{
    enum ButtonType { Sprite, Outline }

    SpriteRenderer spriteRenderer;
    Sprite originSprite;

    [SerializeField] ButtonType buttonType;
    [DrawIf("buttonType", ButtonType.Sprite)] [SerializeField] Sprite pressedSprite;
    [Space(20)]
    [SerializeField] UnityEvent onEnter;
    [SerializeField] UnityEvent onOver;
    [SerializeField] UnityEvent onClick;
    [SerializeField] UnityEvent onExit;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (buttonType == ButtonType.Sprite)
        {
            originSprite = spriteRenderer.sprite;
        }
    }

    void OnMouseEnter()
    {
        onEnter.Invoke();
    }
    void OnMouseOver()
    {
        if(buttonType == ButtonType.Sprite)
            spriteRenderer.sprite = pressedSprite;

        onOver.Invoke();
    }
    void OnMouseExit()
    {
        if (buttonType == ButtonType.Sprite)
            spriteRenderer.sprite = originSprite;

        onExit.Invoke();
    }
    void OnMouseDown()
    {
        onClick.Invoke();
    }
}
