using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar
{
    public Transform healthBar;
    public Image healthFilled;
    public TMP_Text healthText;
    public TMP_Text defenceText;
    public float hitTimer;

    public HealthBar(Transform healthBar, float hitTime = 0)
    {
        this.healthBar = healthBar;
        healthFilled = healthBar.GetChild(0).GetComponent<Image>();
        healthText = healthBar.GetChild(1).GetComponent<TMP_Text>();
        defenceText = healthBar.GetChild(2).GetComponentInChildren<TMP_Text>();
        hitTimer = hitTime;
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

            /*if(healthBar.Value.hitTimer > 0)
            {
                healthBar.Key.SetMaterial(whiteMaterial);
                healthBar.Value.hitTimer -= Time.deltaTime;
            }
            else
                healthBar.Key.SetMaterial(defaultMaterial);*/
        }
    }

    public void GenerateHealthBar(Unit unit)
    {
        healthBars.Add(unit, new HealthBar(Instantiate(healthBar, canvas)));
        SetHealthBar(unit);
    }

    public void SetHealthBar(Unit unit)
    {
        healthBars[unit].healthFilled.fillAmount = (float)unit.hp / unit.unitData.hp;
        healthBars[unit].healthText.text = unit.hp + "/" + unit.unitData.hp;
        healthBars[unit].defenceText.text = unit.defence.ToString();
    }

    public IEnumerator WhiteMaterial(Unit unit)
    {
        Debug.Log("SetMaterial");
        for(float i = 0; i < 0.1f; i += Time.deltaTime)
        {
            unit.SetMaterial(whiteMaterial);
            yield return null;
        }
        unit.SetMaterial(defaultMaterial);
    }
}
