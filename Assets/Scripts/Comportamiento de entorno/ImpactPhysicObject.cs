using UnityEngine;

public class ImpactPhysicObject : MonoBehaviour
{
    private bool hasFallen = false; // Flag para evitar que caiga varias veces

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Verificamos el Tag 'Player' y que no haya caido
        if (collision.gameObject.CompareTag("Player") && !hasFallen)
        {
            // 2. Marcar como caido para no repetir la logica
            hasFallen = true;

            // 3. AGREGAR FÍSICA Y HACERLO TOP-HEAVY
            // Como el arbol ya es top-heavy (ver explicacion), 
            // el rigidbody solo lo 'activa' para caer.
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();

            // 4. CONFIGURAR RIGIDBODY PARA 'POSTE' (Masa y Estabilidad)
            rb.mass = 150f; // Darle masa para que el impacto se sienta
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Caida mas suave
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // Evita atravesar pisos

            // 5. CALCULAR IMPULSO DE ROTACIÓN (Copa Primero)
            // Para que la copa caiga primero, aplicamos torque perpendicular.
            // Obtenemos la direccion de impacto.
            Vector3 impactDir = collision.relativeVelocity.normalized;

            // Calculamos un eje de rotación perpendicular al impacto y hacia arriba.
            // Esto hara que el arbol gire alrededor de su base.
            Vector3 rotationAxis = Vector3.Cross(impactDir, Vector3.up);

            // 6. APLICAR TORQUE (Impulso Rotacional)
            // Usamos un torque alto con ForceMode.Impulse para una caida reactiva.
            float torqueMagnitude = 1800f; // Valor alto para un tip-over visible
            rb.AddTorque(rotationAxis * torqueMagnitude, ForceMode.Impulse);

            // 7. AUTODESTRUCCIÓN
            Destroy(gameObject, 3f);

            Debug.Log($"[Fisica] Arbol {gameObject.name} derribado por el bondi.");
        }
    }
}