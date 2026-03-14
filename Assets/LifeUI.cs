using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LifeUI : MonoBehaviour
{
    public Character Character;

    public GameObject vida1Object;
    public Sprite vida1Flash;
    private Image vida1Image;
    private Sprite vida1Original;

    public GameObject vida2Object;
    public Sprite vida2Flash;
    private Image vida2Image;
    private Sprite vida2Original;

    public GameObject vida3Object;
    public Sprite vida3Flash;
    private Image vida3Image;
    private Sprite vida3Original;

    public float flashDuration = 0.2f;

    public GameObject gameOverPanel;

    // Lista para armazenar objetos clonados durante o jogo
    public List<GameObject> clonedObjects = new List<GameObject>();

    private int lastHP;

    void Start()
    {
        vida1Image = vida1Object.GetComponent<Image>();
        vida2Image = vida2Object.GetComponent<Image>();
        vida3Image = vida3Object.GetComponent<Image>();

        vida1Original = vida1Image.sprite;
        vida2Original = vida2Image.sprite;
        vida3Original = vida3Image.sprite;

        gameOverPanel.SetActive(false);

        lastHP = Character.currentHP;
        UpdateLives();
    }

    void Update()
    {
        if (Character.currentHP < lastHP)
        {
            int lostLifeIndex = Character.currentHP;
            StartCoroutine(FlashAndDisable(lostLifeIndex));
        }

        if (Character.currentHP > lastHP)
            UpdateLives();

        if (Character.currentHP <= 0 && !gameOverPanel.activeSelf)
            ShowGameOver();

        lastHP = Character.currentHP;
    }

    public void UpdateLives()
    {
        vida1Image.enabled = Character.currentHP >= 1;
        vida2Image.enabled = Character.currentHP >= 2;
        vida3Image.enabled = Character.currentHP >= 3;

        if (Character.currentHP >= 1) vida1Image.sprite = vida1Original;
        if (Character.currentHP >= 2) vida2Image.sprite = vida2Original;
        if (Character.currentHP >= 3) vida3Image.sprite = vida3Original;
    }

    private IEnumerator FlashAndDisable(int index)
    {
        Image img = null;
        Sprite flash = null;

        if (index == 0) { img = vida1Image; flash = vida1Flash; }
        else if (index == 1) { img = vida2Image; flash = vida2Flash; }
        else if (index == 2) { img = vida3Image; flash = vida3Flash; }

        if (img == null) yield break;

        img.sprite = flash;
        yield return new WaitForSeconds(flashDuration);
        img.enabled = false;
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        if (Character != null)
            Character.isGameOver = true;
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Destroi todos os objetos clonados
        foreach (var obj in clonedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        clonedObjects.Clear();

        if (Character != null)
            Character.isGameOver = false;

        Time.timeScale = 1f;

        // Reinicia a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
