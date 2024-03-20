using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance; // Singleton instance

    public GameObject winnerScreen; // Assign this in the Unity Editor
    public Text winnerText;
    public GameObject restartButton; // Assign this in the Unity Editor

    private bool isWinnerScreenVisible = false; // Track winner screen visibility

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of UIManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Hide the winner screen and restart button initially
        if (winnerScreen != null)
        {
            winnerScreen.SetActive(false);
        }
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }
    }

    public void ShowWinnerScreen(string winner)
    {
        // Show the winner screen and update the winner text on the server
        if (IsServer)
        {
            if (winnerScreen != null && winnerText != null)
            {
                winnerText.text = winner;
                isWinnerScreenVisible = true;
                UpdateWinnerScreenVisibility(); // Update visibility on the server
                ShowWinnerScreenServerRpc(winner, isWinnerScreenVisible); // Call RPC to update on clients
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowWinnerScreenServerRpc(string winner, bool isVisible)
    {
        // Show the winner screen and update the winner text on all clients
        isWinnerScreenVisible = isVisible;
        UpdateWinnerScreenVisibility();
    }

    public void RestartGame()
    {
        // Restart the game by calling GameManager's RestartGame method on the server
        if (IsServer)
        {
            GameManager.Instance.RestartGame(); // Assuming GameManager is also a singleton
            isWinnerScreenVisible = false;
            UpdateWinnerScreenVisibility(); // Update visibility on the server
            ShowWinnerScreenServerRpc("", isWinnerScreenVisible); // Call RPC to update on clients
        }
    }

    private void UpdateWinnerScreenVisibility()
    {
        // Update the visibility of the winner screen and restart button
        if (winnerScreen != null && restartButton != null)
        {
            winnerScreen.SetActive(isWinnerScreenVisible);
            restartButton.SetActive(isWinnerScreenVisible);
        }
    }
}
