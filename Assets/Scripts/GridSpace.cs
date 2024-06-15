using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridSpace : MonoBehaviour
{
    private GameController myController; 

    public Button myButton;
    public TextMeshProUGUI buttonText;
    public string playerSide;

    private void Start()
    {
        myButton = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetSpace()
    {
        buttonText.text = myController.GetCurrentTurn();
        myButton.interactable = false;
        StartCoroutine(myController.EndTurn());
    }

    public void SetController(GameController controller)
    {
        myController = controller;
    }
}