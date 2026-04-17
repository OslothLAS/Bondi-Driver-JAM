using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ImpactPhysicObject : MonoBehaviour
{
    private bool hasFallen = false;

    [Header("Configuración de Ingeniería (Física)")]
    public float fuerzaImpacto = 10f; // Bajamos un poco si sale muy disparado
    public float masaObjeto = 200f; // Más pesado para que se sienta como un árbol real

    [Header("Frenos (Drag)")]
    [Tooltip("Resistencia al movimiento lineal")]
    public float dragLineal = 0.5f;
    [Tooltip("Resistencia a la rotación (Evita que gire como loco)")]
    public float dragAngular = 2.5f;

    [Header("Gestión de Memoria")]
    public float tiempoParaDesaparecer = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !hasFallen)
        {
            hasFallen = true;

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = masaObjeto;

            // --- LA SOLUCIÓN AL GIRO LOCO ---
            rb.linearDamping = dragLineal;           // Frenado de traslación
            rb.angularDamping = dragAngular;   // Frenado de rotación (CLAVE)
            rb.maxAngularVelocity = 7f;     // Capamos la velocidad máxima de giro

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            // Cálculo de altura y palanca
            float alturaTotal = 2f;
            Renderer rend = GetComponent<Renderer>();
            if (rend == null) rend = GetComponentInChildren<Renderer>();
            if (rend != null) alturaTotal = rend.bounds.size.y;

            // Aplicamos la fuerza un poco más abajo de la punta total para que no sea tan extremo
            Vector3 puntoPalanca = transform.position + (Vector3.up * (alturaTotal * 0.8f));

            Vector3 direccionEmpuje = collision.relativeVelocity.normalized;
            direccionEmpuje.y = 0.1f;

            // Usamos Impulse en lugar de VelocityChange para que la masa importe
            rb.AddForceAtPosition(direccionEmpuje * fuerzaImpacto, puntoPalanca, ForceMode.Impulse);

            Destroy(gameObject, tiempoParaDesaparecer);

            Debug.Log($"[Fisica] {gameObject.name} derribado con amortiguación angular aplicada.");
        }
    }
}