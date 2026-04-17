using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Referencias")]
    public Transform target; // Arrastrá el bondi acá

    [Header("Configuración de Posición")]
    public Vector3 offset = new Vector3(0, 5, -10); // Distancia y altura
    public float smoothSpeed = 0.125f; // Qué tan suave sigue (0.01 es muy lento, 1 es rígido)

    [Header("Configuración de Rotación")]
    public float rotationSmoothSpeed = 5f;

    void Update()
    {
        if (target == null) return;

        // 1. CALCULAR POSICIÓN DESEADA
        // Multiplicamos el offset por la rotación del bondi para que la cámara siempre esté atrás
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // 2. INTERPOLACIÓN (El suavizado)
        // Usamos Lerp para que la cámara no "salte" a la posición, sino que viaje fluidamente
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 3. ROTACIÓN (Mirar al bondi)
        // Queremos que la cámara siempre apunte al colectivo, pero suavemente
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}