using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class UIIntegerText : MonoBehaviour
{
    public string prefix;

    [SerializeField] IntegerVariable value;
    [SerializeField] TextMeshProUGUI text;

    int displayedValue;
    string displayedPrefix;
    bool hasDisplayedValue;

    void Update()
    {
        if (value == null || text == null ||
            (hasDisplayedValue && value.Value == displayedValue && prefix == displayedPrefix))
        {
            return;
        }

        displayedValue = value.Value;
        displayedPrefix = prefix;
        hasDisplayedValue = true;
        text.text = prefix + displayedValue;
    }
}
