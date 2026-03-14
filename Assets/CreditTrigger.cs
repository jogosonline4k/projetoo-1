using UnityEngine;

public class CreditTrigger : MonoBehaviour
{
    public GameObject creditsPanel;

    private void Start()
    {
        creditsPanel.SetActive(false);
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
        creditsPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
