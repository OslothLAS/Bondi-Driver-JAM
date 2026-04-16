using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    private Animator anim;

    [Header("Configuración de Teclas")]
    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;

    [Header("Física")]
    public float acceleration = 50f;
    public float steering = 25f;
    [Range(0, 1)] public float driftFactor = 0.95f;

    private Rigidbody rb;
    private InputAction moveAction;
    private Vector2 currentInput; // Guardamos el input para usarlo en Update y FixedUpdate

    void Awake()
    {
        // 1. Identificación y asignación de teclas (Tu lógica hardcodeada)
        if (gameObject.name == "Bondi_J1")
        {
            upKey = "<Keyboard>/w"; downKey = "<Keyboard>/s";
            leftKey = "<Keyboard>/a"; rightKey = "<Keyboard>/d";
        }
        else if (gameObject.name == "Bondi_J2")
        {
            upKey = "<Keyboard>/upArrow"; downKey = "<Keyboard>/downArrow";
            leftKey = "<Keyboard>/leftArrow"; rightKey = "<Keyboard>/rightArrow";
        }
        else if (gameObject.name == "Bondi_J3")
        {
            upKey = "<Keyboard>/i"; downKey = "<Keyboard>/j";
            leftKey = "<Keyboard>/k"; rightKey = "<Keyboard>/l";
        }

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;

        // 2. Configuración del New Input System para este bondi específico
        moveAction = new InputAction("Move", binding: "");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", upKey).With("Down", downKey)
            .With("Left", leftKey).With("Right", rightKey);
        moveAction.Enable();
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // LEER EL INPUT ESPECÍFICO DE ESTE JUGADOR
        currentInput = moveAction.ReadValue<Vector2>();

        if (anim != null)
        {
            // --- SINCRONIZACIÓN CON EL BLEND TREE ---
            // Usamos DampTime (0.1f) para que la aguja del Animator no salte 
            // y la transición de las animaciones sea suave como un bondi con suspensión nueva.
            anim.SetFloat("Velocidad", currentInput.y, 0.1f, Time.deltaTime);
            anim.SetFloat("Giro", currentInput.x, 0.1f, Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        // 1. ACELERACIÓN FÍSICA
        if (currentInput.y != 0)
        {
            rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
        }

        // 2. GIRO DINÁMICO
        if (currentSpeed > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            float sensitivity = Mathf.Clamp(currentSpeed / 10f, 1f, 2.5f);
            float rotation = currentInput.x * steering * direction * sensitivity * Time.fixedDeltaTime * 10f;

            Quaternion deltaRotation = Quaternion.Euler(0, rotation, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 3. DRIFT
        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVel * (1f - driftFactor), ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Gatillo para la animación de Damage (si el choque es fuerte)
        if (collision.relativeVelocity.magnitude > 7f)
        {
            if (anim != null) anim.SetTrigger("Choque");
        }
    }

    private void OnDisable() => moveAction.Disable();
}