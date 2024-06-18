using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Player
{
    public Image panel;
    public TextMeshProUGUI text;
}

[System.Serializable]
public class PlayerColor
{
    public Color panelColor;
    public Color textColor;
}

public class GameController : MonoBehaviour
{
    [Header("Turn Indicator Settings")]
    [SerializeField] private Player playerX;
    [SerializeField] private Player playerO;
    [SerializeField] private PlayerColor activePlayerColor;
    [SerializeField] private PlayerColor inactivePlayerColor;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Game Board")]
    [SerializeField] private GameObject gameBoard;
    [SerializeField] private GameObject turnIndicator;
    [SerializeField] private TextMeshProUGUI[] buttonList;

    [SerializeField] private GameObject startScreen;

    [SerializeField] private string playerSide;

    private int moveCount;

    private bool gameOver = false;

    private LineRenderer lineRenderer;

    private bool computerOpponent = false;

    void Start()
    {
        mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(0));

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    private void Awake()
    {
        gameBoard.SetActive(false);
        turnIndicator.SetActive(false);

        playerSide = "X";
        SetTurnIndicatorColors(playerX, playerO);
        restartButton.SetActive(false);
        moveCount = 0;
        gameOverPanel.SetActive(false);

        ResetWinLine();
    }

    void SetControllerOnButtons()
    {
        if (buttonList != null)
        {
            for (int i = 0; i < buttonList.Length; i++)
            {
                buttonList[i].GetComponentInParent<GridSpace>().SetController(this);
            }
        }
    }

    public void StartOnePlayerGame()
    {
        computerOpponent = true;
        ResetUI();
        playerSide = "O";

        StartCoroutine(AITurn());
    }

    public void StartTwoPlayerGame()
    {
        computerOpponent = false;
        ResetUI();
        playerSide = "X"; // per spec, X goes first in 2 Player Mode
    }

    private void ResetUI()
    {
        gameBoard.SetActive(true);
        turnIndicator.SetActive(true);
        moveCount = 0;
        gameOverPanel.SetActive(false);
        startScreen.SetActive(false);
        SetControllerOnButtons();
    }

    public string GetCurrentTurn()
    {
        return playerSide; 
    }

    public IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(0.25f); // brief pause so user notices Turn Indicator update

        moveCount++;

        if (CheckForWinner())
        {
            gameOver = true;
            GameOver(playerSide);
        }
        else if (moveCount >= 9)
        {
            GameOver("Draw");
        }
        else
        {
            SwitchTurns();
            if (computerOpponent && playerSide == "O")
            {
                StartCoroutine(AITurn());
            }
        }
    }

    private bool CheckForWinner()
    {
        return
        DoBoxesMatch(0, 1, 2) || DoBoxesMatch(3, 4, 5) || DoBoxesMatch(6, 7, 8) || //Rows
        DoBoxesMatch(0, 3, 6) || DoBoxesMatch(1, 4, 7) || DoBoxesMatch(2, 5, 8) || //Columns
        DoBoxesMatch(0, 4, 8) || DoBoxesMatch(2, 4, 6); //Diagonals
    }

    bool DoBoxesMatch(int i, int j, int k)
    {
        var pm = playerSide;
        bool matched = (buttonList[i].text == pm && buttonList[j].text == pm && buttonList[k].text == pm);

        if (matched)
            DrawWinLine(buttonList[i].gameObject.transform, buttonList[k].gameObject.transform);

        return matched;
    }

    void DrawWinLine(Transform i, Transform k)
    {
        // encircle winning row, column, or diagonal trio
        lineRenderer.SetPosition(0, i.gameObject.transform.position);
        lineRenderer.SetPosition(1, k.gameObject.transform.position);

        lineRenderer.enabled = true;
    }

    void GameOver(string winningPlayer)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = false;
        }

        if (winningPlayer == "Draw")
        {
            SetGameOverText("It's a draw!");
        }
        else
        {
            SetGameOverText(playerSide + " Wins!");
        }

        restartButton.SetActive(true);
    }

    void SwitchTurns()
    {
        playerSide = (playerSide == "X") ? "O" : "X"; 
        if (playerSide == "X")
        {
            SetTurnIndicatorColors(playerX, playerO);
        }
        else
        {
            SetTurnIndicatorColors(playerO, playerX);
        }
    }

    void SetGameOverText(string myText)
    {
        gameOverText.text = myText;
        StartCoroutine(ShowGameOverPanel());
    }

    IEnumerator ShowGameOverPanel()
    {
        yield return new WaitForSeconds(0.4f);
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        if (computerOpponent)
        {
            StartOnePlayerGame();
        }
        else
        {
            StartTwoPlayerGame();
        }
        moveCount = 0;
        gameOverPanel.SetActive(false);

        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].text = string.Empty;
        }

        SetTurnIndicatorColors(playerX, playerO);
        SetBoardInteractable(true);
        restartButton.SetActive(false);
        ResetWinLine();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResetWinLine()
    {
        if(lineRenderer != null)
        lineRenderer.enabled = false;
    }

    void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    void SetTurnIndicatorColors(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;

        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        newPlayer.text.color = inactivePlayerColor.textColor;
    }

    IEnumerator AITurn()
    {
        Debug.Log("AI's move");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float randomWaitTime = (moveCount < 1) ? 0.1f : Random.Range(0.25f, 1.25f); //prevent inadvertent user input on first move, and make CPU appear to be "thinking"
        
        yield return new WaitForSeconds(randomWaitTime);
        Debug.Log("randomWaitTime: " + randomWaitTime);

        bool foundEmptySpot = false;

        while (!foundEmptySpot)
        {
            int randomNum = Random.Range(0, 9);

            if (buttonList[randomNum].GetComponentInParent<Button>().IsInteractable())
            {
                // select random empty space
                buttonList[randomNum].GetComponentInParent<Button>().onClick.Invoke();
                foundEmptySpot = true;

                yield return new WaitForSeconds(0.25f);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}