using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RaceLauncher : MonoBehaviour
{
    public TMP_InputField playerNameInput;

    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName") && playerNameInput != null)
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void SetName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
    }

    // WAŻNE: numer w LoadScene powinien być taki sam, jak indeks sceny gry w ustawieniach Build Settings
    public void StartTrial()
    {
        SceneManager.LoadScene(1); // Upewnij się, że scena "Game" ma indeks 1. Jeśli ma 0, zmień na 0.
    }
}