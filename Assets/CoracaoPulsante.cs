using UnityEngine;

public class CoracaoPulsante : MonoBehaviour
{
    public float escalaMinima = 1f;
    public float escalaMaxima = 1.2f;
    public float velocidade = 1f;

    private Vector3 escalaInicial;

    void Start()
    {
        escalaInicial = transform.localScale;
    }

    void Update()
    {
        float tempo = (Time.unscaledTime * velocidade) % 1f;
        float batida = 0f;

        if (tempo < 0.15f)
        {
            batida = Mathf.Sin((tempo / 0.15f) * Mathf.PI);
        }
        else if (tempo > 0.25f && tempo < 0.40f)
        {
            float tempoSegundaBatida = (tempo - 0.25f) / 0.15f;
            batida = Mathf.Sin(tempoSegundaBatida * Mathf.PI) * 0.8f;
        }

        float escalaAtual = Mathf.Lerp(escalaMinima, escalaMaxima, batida);
        transform.localScale = escalaInicial * escalaAtual;
    }
}