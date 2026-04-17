using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Configuraci�n")]
    public Vector3 offset = new Vector3(0, 12, -10);
    public float smoothSpeed = 0.125f;
    public Vector3 fixedRotation = new Vector3(45, 0, 0);

    public void SetTarget(Transform newTarget) => target = newTarget;

    void LateUpdate()
    {
        if (target == null) return;

        // Seguimiento de posici�n con Lerp
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Bloqueo de rotaci�n para que no gire con el bondi
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}