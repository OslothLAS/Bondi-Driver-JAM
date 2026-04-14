using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Arrastrá acá tu Bondi
    public Vector3 offset;   // La distancia entre la cámara y el bus
    public float smoothSpeed = 0.125f; // Para que el seguimiento sea suave

    void Start()
    {
        // Si no configuraste el offset, calculamos el actual automáticamente
        if (offset == Vector3.zero)
            offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        // Calculamos la posición deseada (posición del bus + la distancia guardada)
        Vector3 desiredPosition = target.position + offset;

        // Movemos la cámara suavemente hacia esa posición
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // La rotación ni la tocamos, así que se queda quieta
    }
}