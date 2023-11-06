using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType unitType;

    SpriteRenderer _renderer;

    void Start()
    {
        if (transform.GetChild(0).TryGetComponent(out SpriteRenderer spriteRenderer))
            _renderer = spriteRenderer;
    }
    public void Init(Sprite sprite = null)
    {
        if (sprite)
            _renderer.sprite = sprite;
    }
}