using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necessário para usar Coroutines

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject gameUI;

    [Header("Configurações de Fade")]
    public GameObject fadeImageObject; // O objeto da Image que está lá no UIManager
    public Animator fadeAnimator;      // O componente Animator (pode arrastar a mesma Image aqui)
    public float tempoDeEspera = 1.0f; // Quanto tempo dura o fade

    void Start()
    {
        // Garante que o jogo comece pausado
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        creditsPanel.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(false);

        // DESATIVA a imagem do fade no começo para você conseguir clicar nos botões
        if (fadeImageObject != null)
            fadeImageObject.SetActive(false);
    }

    public void StartGame()
    {
        // Inicia a sequência de Fade
        StartCoroutine(SequenciaStart());
    }

    IEnumerator SequenciaStart()
    {
        // 1. ATIVA o objeto da Image (a "caixa" que você queria)
        if (fadeImageObject != null)
        {
            fadeImageObject.SetActive(true);
        }

        // 2. Toca a animação de Fade
        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("ComecarFade");
        }

        // 3. Espera o tempo da animação em tempo real
        yield return new WaitForSecondsRealtime(tempoDeEspera);

        // 4. Libera o jogo e troca os painéis
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1f;

        if (gameUI != null)
            gameUI.SetActive(true);

        // 5. Opcional: Desativa a imagem de novo se ela for bloquear o clique no jogo
        // fadeImageObject.SetActive(false);
    }

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}