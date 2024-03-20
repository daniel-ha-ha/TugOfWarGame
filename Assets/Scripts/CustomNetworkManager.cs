using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : MonoBehaviour
{
    public Transform spawnPoint1; // Locations are within the editor
    public Transform spawnPoint2;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // This is called on the server when a client connects
        // The host is already connected at this point
        if (NetworkManager.Singleton.IsServer)
        {
            // Determine the spawn point based on how many players are currently connected
            Vector3 spawnPosition = NetworkManager.Singleton.ConnectedClients.Count == 1 ? spawnPoint1.position : spawnPoint2.position;

            // Instantiate and spawn the player object
            var playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
            var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            var networkObject = playerInstance.GetComponent<NetworkObject>();

            // Make the player visible
            MakePlayerVisible(playerInstance);

            if (networkObject != null)
            {
                networkObject.SpawnAsPlayerObject(clientId);
            }
        }
    }

    // A method to make the player visible after being spawned
    private void MakePlayerVisible(GameObject player)
    {
        // Assuming the player has a SpriteRenderer component
        var spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // This sets the alpha to fully opaque. Adjust as necessary for your game's needs
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }


    }
}
