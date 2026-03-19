using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Horizontal Gradient")]
public class GradienteHorizontalUI : BaseMeshEffect
{
    // Lado esquerdo continua totalmente transparente
    public Color corEsquerda = new Color(1, 1, 1, 0);

    // Lado direito (Base) agora com 0.5 de Alpha (50% transparente)
    public Color corDireita = new Color(1, 1, 1, 0.5f);

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        UIVertex v = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);

            // 0 e 1 são os vértices da esquerda
            if (i == 0 || i == 1)
            {
                v.color = corEsquerda;
            }
            // 2 e 3 são os vértices da direita (a base do seu gradiente)
            else
            {
                v.color = corDireita;
            }

            vh.SetUIVertex(v, i);
        }
    }
}