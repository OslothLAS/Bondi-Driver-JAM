using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Configuración")]
    public Vector3 offset = new Vector3(0, 12, -10);
    public float smoothSpeed = 0.125f;
    public Vector3 fixedRotation = new Vector3(45, 0, 0);

    public void SetTarget(Transform newTarget) => target = newTarget;

    void LateUpdate()
    {
        if (target == null) return;

        // Seguimiento de posición con Lerp
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Bloqueo de rotación para que no gire con el bondi
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}