using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class ConnectionUIController : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button joinClientButton;

    private void Start()
    {
        // Проверка на наличие NetworkManager
        if (networkManager == null)
        {
            Debug.LogError("NetworkManager is not assigned!");
            return;
        }

        // Подписываемся на события кнопок
        startHostButton.onClick.AddListener(StartHost);
        joinClientButton.onClick.AddListener(JoinClient);
    }

    public void StartHost()
    {
        networkManager.StartHost();
    }

    public void JoinClient()
    {
        networkManager.networkAddress = ipInputField.text;
        networkManager.StartClient();
    }
}
