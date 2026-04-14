using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    [Header("Configuración de Teclas")]
    [Tooltip("Ejemplo: <Keyboard>/w o <Gamepad>/leftStick/up")]
    public string upKey = "<Keyboard>/w";
    public string downKey = "<Keyboard>/s";
    public string leftKey = "<Keyboard>/a";
    public string rightKey = "<Keyboard>/d";

    [Header("Parámetros del Bondi")]
    public float acceleration = 40f;
    public float steering = 20f;
    [Range(0, 1)] public float driftFactor = 0.95f; // 1 = hielo, 0 = rieles
    [Range(0, 1)] public float engineBrake = 0.98f; // 1 = no frena, 0.9 = frena mucho

    private Rigidbody rb;
    private InputAction moveAction;

    void Awake()
    {
        // 1. Auto-asignación y configuración de física
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f; // Peso de un colectivo
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;

        // 2. Configuración de Input por Código
        moveAction = new InputAction("Move", binding: "");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", upKey)
            .With("Down", downKey)
            .With("Left", leftKey)
            .With("Right", rightKey);

        moveAction.Enable();
    }

    void FixedUpdate()
    {
        // Leer el movimiento (X: giro, Y: aceleración)
        Vector2 input = moveAction.ReadValue<Vector2>();

        // 1. Aceleración y Freno Motor
        if (Mathf.Abs(input.y) > 0.01f)
        {
            rb.AddForce(transform.forward * input.y * acceleration, ForceMode.Acceleration);
        }
        else
        {
            // Aplicar freno motor solo a la velocidad frontal
            Vector3 fwdVel = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
            rb.linearVelocity -= fwdVel * (1 - engineBrake);
        }

        // 2. Rotación (Giro del Bondi)
        if (rb.linearVelocity.magnitude > 0.1f) // Solo gira si se está moviendo
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            transform.Rotate(Vector3.up * input.x * steering * direction * Time.fixedDeltaTime * 5f);
        }

        // 3. Lógica de Drift (Fricción Lateral)
        // Separamos la velocidad frontal de la lateral
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        // La velocidad final es la frontal intacta + la lateral reducida por el drift
        rb.linearVelocity = forwardVelocity + (lateralVelocity * driftFactor);

        // Mantener la velocidad vertical (gravedad) intacta
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    private void OnDisable() => moveAction.Disable();
}