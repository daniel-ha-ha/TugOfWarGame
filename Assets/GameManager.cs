using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> ropePosition = new NetworkVariable<float>();
    public GameObject flag; // Assign this in the Unity Editor
    public GameObject winnerScreen; // Assign this in the Unity Editor
    public Text winnerText; // Assign this in the Unity Editor
    public GameObject restartButton; // Assign this in the Unity Editor
    public AudioClip[] soundClips; // Assign this in the Unity Editor
    public AudioClip victorySoundPlayer1; // Assign this in the Unity Editor
    public AudioClip victorySoundPlayer2; // Assign this in the Unity Editor
    private AudioSource audioSource;

    public float winThreshold = 5.0f; // Distance from center to determine a win
    private float initialFlagXPosition;
    private bool gameOver = false;

    // Visibility of the winner screen (no longer need to track separately in UIManager)
    private NetworkVariable<bool> isWinnerScreenVisible = new NetworkVariable<bool>();

    // Singleton instance
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the game manager across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = gameObject.AddComponent<AudioSource>(); // Ensure there's an AudioSource component to play the sounds
    }

    private void Start()
    {
        if (flag != null)
        {
            initialFlagXPosition = flag.transform.position.x;
        }

        // Initially hide the winner screen and restart button
        if (winnerScreen != null)
        {
            winnerScreen.SetActive(false);
        }
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }
    }

    public void TugRope(float strength, ulong clientId)
    {
        if (IsServer && !gameOver)
        {
            ropePosition.Value += strength * (clientId == NetworkManager.Singleton.LocalClientId ? -1 : 1);

            // Start playing sounds when the rope is tugged
            if (!IsInvoking(nameof(PlayRandomSound)))
            {
                InvokeRepeating(nameof(PlayRandomSound), 0f, 1.5f);
            }

            if (Mathf.Abs(ropePosition.Value) >= winThreshold)
            {
                gameOver = true;
                CancelInvoke(nameof(PlayRandomSound)); // Stop playing sounds when the game is over
                string winnerText = ropePosition.Value > 0 ? "PLAYER 2 WINS!" : "PLAYER 1 WINS!";
                Debug.Log("Game Over. " + winnerText);
                DisplayWinner(winnerText);
            }
            else
            {
                UpdateFlagPosition();
            }
        }
    }

    private void DisplayWinner(string winnerText)
    {
        if (IsServer)
        {
            Debug.Log("Winner: " + winnerText);
            // Use an RPC to communicate the winner text to all clients, including the host
            ShowWinnerScreenServerRpc(winnerText);
            
            // Play the victory sound depending on the winner
            AudioClip victorySound = ropePosition.Value > 0 ? victorySoundPlayer2 : victorySoundPlayer1;
            audioSource.PlayOneShot(victorySound);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowWinnerScreenServerRpc(string winner)
    {
        isWinnerScreenVisible.Value = true;
        UpdateWinnerScreenVisibility();
        if (isWinnerScreenVisible.Value)
        {
            this.winnerText.text = winner; // Ensure this is updated on clients as well
        }
    }

    public void RestartGame()
    {
        if (IsServer)
        {
            gameOver = false;
            ropePosition.Value = 0;
            isWinnerScreenVisible.Value = false;
            UpdateWinnerScreenVisibility();
            CancelInvoke(nameof(PlayRandomSound)); // Ensure sounds are stopped when restarting
            // Optionally, reload the scene or reset game state as needed
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Update()
    {
        if (!gameOver)
        {
            UpdateFlagPosition();
        }
    }

    private void UpdateFlagPosition()
    {
        if (flag != null)
        {
            Vector3 flagPosition = flag.transform.position;
            flagPosition.x = initialFlagXPosition + ropePosition.Value;
            flag.transform.position = flagPosition;
        }
    }

    private void UpdateWinnerScreenVisibility()
    {
        // Update the visibility of the winner screen and restart button
        if (winnerScreen != null && restartButton != null)
        {
            bool isVisible = isWinnerScreenVisible.Value;
            winnerScreen.SetActive(isVisible);
            restartButton.SetActive(isVisible);
        }
    }

    private void PlayRandomSound()
    {
        if (soundClips.Length > 0)
        {
            int index = Random.Range(0, soundClips.Length);
            audioSource.PlayOneShot(soundClips[index]);
        }
    }
}
