using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/GradienteSimples")]
public class GradienteSimples : BaseMeshEffect
{
    public Color corEsquerda = new Color(1, 1, 1, 0); // Transparente
    public Color corDireita = new Color(1, 1, 1, 1);  // Sólido

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        UIVertex v = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);
            // Verifica a posição horizontal (x) para decidir a cor
            v.color = (i == 0 || i == 1) ? corEsquerda : corDireita;
            vh.SetUIVertex(v, i);
        }
    }
}