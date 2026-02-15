
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMeterFill : MonoBehaviour
{
    [Header("VARIABLES")]
    [SerializeField]
    FloatVariable health;
    [SerializeField]
    FloatVariable maxHealth;

    [Header("REFERENCES")]
    [SerializeField]
    Image meter;
    [SerializeField]
    TextMeshProUGUI text;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    public void Update()
    {
        if (health == null)
        {
            return;
        }

        UpdateMeterFill();
        UpdateMeterText();
    }

    void UpdateMeterFill()
    {
        meter.fillAmount = health.Value / maxHealth.Value;
    }

    void UpdateMeterText()
    {
        int roundedHealth = Mathf.FloorToInt(health.Value);
        int roundedMaxHealth = Mathf.FloorToInt(maxHealth.Value);

        string healthString = roundedHealth.ToString() + "/" + roundedMaxHealth.ToString();
        text.text = healthString;
    }
}
