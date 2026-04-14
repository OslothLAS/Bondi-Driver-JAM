using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    [Header("Configuracion de Teclas")]
    [Tooltip("Ejemplo: <Keyboard>/w o <Gamepad>/leftStick/up")]
    public string upKey = "<Keyboard>/w";
    public string downKey = "<Keyboard>/s";
    public string leftKey = "<Keyboard>/a";
    public string rightKey = "<Keyboard>/d";

    [Header("Parametros del Bondi")]
    public float acceleration = 40f;
    public float steering = 20f;
    [Range(0, 1)] public float driftFactor = 0.95f; // 1 = hielo, 0 = rieles
    [Range(0, 1)] public float engineBrake = 0.98f; // 1 = no frena, 0.9 = frena mucho

    private Rigidbody rb;
    private InputAction moveAction;

    void Awake()
    {
        // 1. Auto-asignacion y configuracion de fisica
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f; // Peso de un colectivo
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;

        // 2. Configuracion de Input por Codigo
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
        // Leer el movimiento (X: giro, Y: aceleracion)
        Vector2 input = moveAction.ReadValue<Vector2>();

        // 1. Aceleracion y Freno Motor
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

        // 2. Rotacion (Giro del Bondi)
        if (rb.linearVelocity.magnitude > 0.1f) // Solo gira si se esta moviendo
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            transform.Rotate(Vector3.up * input.x * steering * direction * Time.fixedDeltaTime * 5f);
        }

        // 3. Logica de Drift (Friccion Lateral)
        // Guardamos la velocidad vertical (gravedad) antes de manipular la horizontal
        float verticalVelocity = rb.linearVelocity.y;

        // Separamos la velocidad frontal de la lateral
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
        Vector3 lateralVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);

        // Calculamos la nueva velocidad horizontal: frontal intacta + lateral reducida por el drift
        Vector3 combinedVelocity = forwardVelocity + (lateralVelocity * driftFactor);

        // Reasignamos la velocidad manteniendo la vertical original para no anular la gravedad
        rb.linearVelocity = new Vector3(combinedVelocity.x, verticalVelocity, combinedVelocity.z);
    }

    private void OnDisable() => moveAction.Disable();
}
