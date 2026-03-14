using UnityEngine;

public class PlayerAfterimage : MonoBehaviour
{
    public float fadeSpeed = 6f;
    SpriteRenderer sr;
    Color c;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 1f);
    }

    void Update()
    {
        if (sr == null) return;

        c = sr.color;
        c.a -= fadeSpeed * Time.deltaTime;
        sr.color = c;

        if (c.a <= 0f) Destroy(gameObject);
    }

    public void Init(Sprite sprite, Vector3 scale, Vector3 pos, int order)
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        transform.localScale = scale;
        transform.position = pos;
        sr.sortingOrder = order;
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
    }
}
