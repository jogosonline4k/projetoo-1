using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;

    // Referência à HUD do jogo (vida, score, etc)
    public GameObject gameUI;

    void Start()
    {
        Time.timeScale = 0f; // Jogo começa pausado
        mainMenuPanel.SetActive(true);
        creditsPanel.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(false); // Desliga a GUI enquanto o menu está ativo
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Libera o jogo

        if (gameUI != null)
            gameUI.SetActive(true); // Liga a GUI do jogo
    }

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        Time.timeScale = 0f;

        if (gameUI != null)
            gameUI.SetActive(false); // Desliga a GUI enquanto os créditos estão ativos
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        Time.timeScale = 0f;

        if (gameUI != null)
            gameUI.SetActive(false); // GUI continua desligada no menu principal
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
