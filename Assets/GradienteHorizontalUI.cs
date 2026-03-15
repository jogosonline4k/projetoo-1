using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Horizontal Gradient")]
public class GradienteHorizontalUI : BaseMeshEffect
{
    public Color corEsquerda = new Color(1, 1, 1, 0); // Ponta (Transparente)
    public Color corDireita = new Color(1, 1, 1, 1);  // Base (Sólida)

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        UIVertex v = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref v, i);

            // O Unity organiza os vértices da UI geralmente nesta ordem:
            // 0: Inferior Esquerdo, 1: Superior Esquerdo, 2: Superior Direito, 3: Inferior Direito
            if (i == 0 || i == 1)
            {
                v.color = corEsquerda;
            }
            else
            {
                v.color = corDireita;
            }

            vh.SetUIVertex(v, i);
        }
    }
}