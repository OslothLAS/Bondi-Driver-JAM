using UnityEngine;

public class ChildSmoothCamera : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Vector3 offset = new Vector3(0, 10, -15); // Distancia del hijo respecto al padre
    public float smoothSpeed = 0.125f;              // Qué tan "elástica" es la cámara

    [Header("Configuración de Rotación")]
    public bool bloquearRotacion = true;            // True = Estilo Isométrico, False = Gira con el Bondi
    public Vector3 rotacionFija = new Vector3(30, 0, 0);

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Al empezar, si queremos que esté en un lugar específico
        transform.localPosition = offset;
    }

    void LateUpdate()
    {
        // Al ser hija, el transform.parent es el Bondi
        if (transform.parent == null) return;

        // 1. CALCULAMOS LA POSICIÓN DESEADA
        // Si queremos suavizado siendo hijos, tenemos que manipular la posición local
        Vector3 desiredLocalPos = offset;

        // Usamos SmoothDamp para un movimiento de cámara profesional (sin tirones)
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, desiredLocalPos, ref velocity, smoothSpeed);

        // 2. CONTROL DE ROTACIÓN (Clave para que no sea un desastre visual)
        if (bloquearRotacion)
        {
            // Forzamos a que la cámara siempre mire hacia adelante en el mundo, 
            // aunque el bondi esté haciendo un trompo.
            transform.rotation = Quaternion.Euler(rotacionFija);
        }
    }
}