using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;   // o personagem a seguir
    public float smoothSpeed = 0.125f;
    public Vector3 offset;     // ajuste de posição da câmera

    public float minY = -2f;   // limite inferior
    public float maxY = 2f;    // limite superior

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // trava o Y dentro do limite
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        // suaviza o movimento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}
