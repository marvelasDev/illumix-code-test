using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Dropdown))]
public class SaveDropdownValue : MonoBehaviour
{
    const string PrefName = "optionvalue";

    private Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<Dropdown>();

        dropdown.onValueChanged.AddListener(new UnityAction<int>(index =>
        {
            PlayerPrefs.SetInt(PrefName, dropdown.value);
            PlayerPrefs.Save();
        }));
    }

    void Start()
    {
        dropdown.value = PlayerPrefs.GetInt(PrefName, 0);   
    }  
}