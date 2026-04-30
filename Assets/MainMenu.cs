using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject creditsPanel;
    public GameObject gameUI;

    [Header("Configurações de Fade")]
    public GameObject fadeImageObject; 
    public Animator fadeAnimator;      
    public float tempoDeEspera = 1.0f; 

    void Awake()
    {
        if (PlayerPrefs.GetInt("DeveIniciarDireto", 0) == 1)
        {
            PlayerPrefs.SetInt("DeveIniciarDireto", 0);
            PlayerPrefs.Save();
            EntrarNoJogo();
        }
        else
        {
            FicarNoMenu();
        }
    }

    void Start()
    {
        if (fadeImageObject != null)
            fadeImageObject.SetActive(false);
    }

    void FicarNoMenu()
    {
        Time.timeScale = 0f;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    void EntrarNoJogo()
    {
        Time.timeScale = 1f;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(SequenciaStart());
    }

    IEnumerator SequenciaStart()
    {
        if (fadeImageObject != null)
        {
            fadeImageObject.SetActive(true);
            if (fadeAnimator != null)
            {
                fadeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                fadeAnimator.SetTrigger("ComecarFade");
            }
        }

        yield return new WaitForSecondsRealtime(tempoDeEspera);

        PlayerPrefs.SetInt("DeveIniciarDireto", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
    }
}