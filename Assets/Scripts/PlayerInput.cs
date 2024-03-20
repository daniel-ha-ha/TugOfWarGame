using UnityEngine;
using Unity.Netcode;

public class PlayerInput : NetworkBehaviour
{
    private float tugStrength = 0.1f; // How much each button press affects the rope position
    
    // Update is called once per frame
    void Update()
    {
        // Check if this is the owner of the object
        if (IsOwner)
        {
            // For the host player (also a server)
            if (IsServer && Input.GetKeyDown(KeyCode.Q))
            {
                TugRopeServerRpc();
            }
            // For the client player
            else if (!IsServer && Input.GetKeyDown(KeyCode.P)) 
            {
                TugRopeServerRpc();
            }
        }
    }

    [ServerRpc]
    void TugRopeServerRpc(ServerRpcParams rpcParams = default)
    {
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.TugRope(tugStrength, OwnerClientId);
        }
    }
}
