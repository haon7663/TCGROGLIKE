using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar
{
    public GameObject prefab;
    public Image healthFilled;
    public TMP_Text healthText;
    public TMP_Text defenceText;
    public float hitTimer;

    public HealthBar(GameObject prefab, float hitTime = 0)
    {
        this.prefab = prefab;
        healthFilled = prefab.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        healthText = prefab.transform.GetChild(1).GetComponent<TMP_Text>();
        defenceText = prefab.transform.GetChild(2).GetComponentInChildren<TMP_Text>();
        hitTimer = hitTime;
    }
}

public class HealthManager : MonoBehaviour
{
    public static HealthManager inst;
    void Awake() => inst = this;

    Dictionary<Unit, HealthBar> healthBars = new Dictionary<Unit, HealthBar>();

    [SerializeField] Transform canvas;
    [SerializeField] GameObject healthBarPrefab;
    [Header("Material")]
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material whiteMaterial;

    Vector3 addPos = new Vector2(0, 0);

    private void Update()
    {
        foreach (KeyValuePair<Unit, HealthBar> healthBar in healthBars)
        {
            healthBar.Value.prefab.transform.position = healthBar.Key.coords.Pos + addPos;

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
        healthBars.Add(unit, new HealthBar(Instantiate(healthBarPrefab, canvas)));
        UpdateHealthBar(unit);
    }

    public void UpdateHealthBar(Unit unit)
    {
        healthBars[unit].healthFilled.fillAmount = (float)unit.hp / unit.unitSO.hp;
        healthBars[unit].healthText.text = unit.hp.ToString();
        healthBars[unit].defenceText.text = unit.defence.ToString();
    }

    public void DestroyHealthBar(Unit unit)
    {
        if (healthBars.ContainsKey(unit))
            Destroy(healthBars[unit].prefab);
        healthBars.Remove(unit);
    }

    public IEnumerator WhiteMaterial(Unit unit)
    {
        unit.SetMaterial(whiteMaterial);
        yield return YieldInstructionCache.WaitForSeconds(0.1f);
        unit.SetMaterial(defaultMaterial);
    }
}
