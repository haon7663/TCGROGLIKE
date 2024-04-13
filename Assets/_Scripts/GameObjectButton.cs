using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectButton : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Sprite originSprite;
    [SerializeField] Sprite pressedSprite;
    [Space(20)]
    [SerializeField] UnityEvent onClick;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originSprite = spriteRenderer.sprite;
    }

    void OnMouseOver()
    {
        spriteRenderer.sprite = pressedSprite;
    }
    void OnMouseExit()
    {
        spriteRenderer.sprite = originSprite;
    }
    void OnMouseDown()
    {
        onClick.Invoke();
    }
}
