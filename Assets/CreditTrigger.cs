using UnityEngine;

public class CreditTrigger : MonoBehaviour
{
    public GameObject creditsPanel;
    public GameObject gameUI;

    private void Start()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Character ch = other.GetComponent<Character>();

        if (ch != null)
        {
            OpenCredits();
        }
    }

    void OpenCredits()
    {
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        
        Time.timeScale = 0f;
    }
}