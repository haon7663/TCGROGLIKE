using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar
{
    public Transform healthBar;
    public Image healthFilled;

    public HealthBar(Transform healthBar)
    {
        this.healthBar = healthBar;
        healthFilled = healthBar.GetChild(0).GetComponent<Image>();
    }
}

public class HealthManager : MonoBehaviour
{
    public static HealthManager Inst;
    void Awake() => Inst = this;

    Dictionary<Unit, HealthBar> healthBars = new Dictionary<Unit, HealthBar>();

    [SerializeField] Transform canvas;
    [SerializeField] Transform healthBar;
    [Header("Material")]
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material whiteMaterial;

    Vector2 addPos = new Vector2(0, -0.1875f);

    void Update()
    {
        foreach (KeyValuePair<Unit, HealthBar> healthBar in healthBars)
        {
            healthBar.Value.healthBar.transform.position = healthBar.Key.coords.Pos + addPos;
        }
    }

    public void GenerateHealthBar(Unit unit)
    {
        healthBars.Add(unit, new HealthBar(Instantiate(healthBar, canvas)));
    }

    public void SetFilled(Unit unit)
    {
        healthBars[unit].healthFilled.fillAmount = (float)unit.curHp / unit.maxHp;
    }


}
