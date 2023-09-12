using UnityEngine;
using Mirror;

public class CameraController : NetworkBehaviour
{
    public Camera playerCamera;

    private void Start()
    {
        if (isLocalPlayer)
        {
            playerCamera.enabled = true;
        }
        else
        {
            playerCamera.enabled = false;
        }
    }
}
