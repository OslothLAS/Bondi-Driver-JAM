using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    [Header("Configuración de Teclas")]
    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;

    [Header("Física (Ajustar en el Inspector)")]
    public float acceleration = 50f;
    public float steering = 25f;
    [Range(0, 1)] public float driftFactor = 0.95f;

    private Rigidbody rb;
    private InputAction moveAction;

    void Awake()
    {
        // 1. Asignación por nombre (Tu lógica)
        if (gameObject.name == "Bondi_J1")
        {
            upKey = "<Keyboard>/w"; downKey = "<Keyboard>/s";
            leftKey = "<Keyboard>/a"; rightKey = "<Keyboard>/d";
        }
        else
        {
            upKey = "<Keyboard>/upArrow"; downKey = "<Keyboard>/downArrow";
            leftKey = "<Keyboard>/leftArrow"; rightKey = "<Keyboard>/rightArrow";
        }

        rb = GetComponent<Rigidbody>();

        // CONFIGURACIÓN CRÍTICA DE RIGIDBODY
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Adiós tirones visuales
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.linearDamping = 0.5f; // Resistencia al aire
        rb.angularDamping = 0.5f;

        moveAction = new InputAction("Move", binding: "");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", upKey).With("Down", downKey)
            .With("Left", leftKey).With("Right", rightKey);
        moveAction.Enable();
    }

    void FixedUpdate()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float currentSpeed = rb.linearVelocity.magnitude; // Velocidad actual en m/s

        // 1. ACELERACIÓN
        if (input.y != 0)
        {
            rb.AddForce(transform.forward * input.y * acceleration, ForceMode.Acceleration);
        }

        // 2. GIRO DINÁMICO (Más fácil cuanto más rápido vas)
        if (currentSpeed > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;

            // --- LÓGICA DE SENSIBILIDAD ---
            // Definimos que a más velocidad, más "multiplicador" de giro.
            // Ejemplo: Si vas a 20m/s, el giro será el doble de sensible que a 10m/s.
            // El Clamp evita que a velocidades absurdas el bondi gire como un ventilador.
            float sensitivity = Mathf.Clamp(currentSpeed / 10f, 1f, 2.5f);

            float rotation = input.x * steering * direction * sensitivity * Time.fixedDeltaTime * 10f;

            Quaternion deltaRotation = Quaternion.Euler(0, rotation, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 3. DRIFT PROFESIONAL
        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVel * (1f - driftFactor), ForceMode.VelocityChange);
    }

    private void OnDisable() => moveAction.Disable();
}