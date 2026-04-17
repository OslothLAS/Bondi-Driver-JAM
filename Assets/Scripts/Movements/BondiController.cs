using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    private Animator anim;

    [Header("Configuraci�n de Teclas")]
    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;
    public string hornKey;

    [Header("F�sica")]
    public float acceleration = 50f;
    public float steering = 25f;
    [Range(0, 1)] public float driftFactor = 0.95f;

    [Header("Audio")]
    public AudioSource crashSound;
    public AudioSource hornSound;
    public AudioSource driftSound;
    public float driftSpeedThreshold = 8f;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction hornAction;
    private Vector2 currentInput;

    void Awake()
    {
        // 1. Identificaci�n y asignaci�n de teclas (Tu l�gica hardcodeada)
        if (gameObject.name == "Bondi_J1")
        {
            upKey = "<Keyboard>/w"; downKey = "<Keyboard>/s";
            leftKey = "<Keyboard>/a"; rightKey = "<Keyboard>/d";
            hornKey = "<Keyboard>/e";
        }
        else if (gameObject.name == "Bondi_J2")
        {
            upKey = "<Keyboard>/upArrow"; downKey = "<Keyboard>/downArrow";
            leftKey = "<Keyboard>/leftArrow"; rightKey = "<Keyboard>/rightArrow";
            hornKey = "<Keyboard>/oem1";
        }
        else if (gameObject.name == "Bondi_J3")
        {
            upKey = "<Keyboard>/i"; downKey = "<Keyboard>/j";
            leftKey = "<Keyboard>/k"; rightKey = "<Keyboard>/l";
            hornKey = "<Keyboard>/p";
        }

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;

        // 2. Configuraci�n del New Input System para este bondi espec�fico
        moveAction = new InputAction("Move", binding: "");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", upKey).With("Down", downKey)
            .With("Left", leftKey).With("Right", rightKey);
        moveAction.Enable();

        hornAction = new InputAction("Horn", binding: hornKey);
        hornAction.Enable();
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        currentInput = moveAction.ReadValue<Vector2>();

        if (hornAction.WasPressedThisFrame() && hornSound != null)
        {
            hornSound.Play();
        }

        if (anim != null)
        {
            anim.SetFloat("Velocidad", currentInput.y, 0.1f, Time.deltaTime);
            anim.SetFloat("Giro", currentInput.x, 0.1f, Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        // 1. ACELERACI�N F�SICA
        if (currentInput.y != 0)
        {
            rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
        }

        // 2. GIRO DIN�MICO
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

        // 4. DRIFT SOUND
        if (driftSound != null && driftSound.clip != null)
        {
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            bool isDrifting = forwardSpeed > driftSpeedThreshold && Mathf.Abs(currentInput.x) > 0.1f;
            if (isDrifting && !driftSound.isPlaying)
            {
                driftSound.loop = true;
                driftSound.Play();
            }
            else if (!isDrifting && driftSound.isPlaying)
            {
                driftSound.Stop();
            }
        }
    }

private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ParadaController>() != null) return;

        if (collision.relativeVelocity.magnitude > 7f)
        {
            if (anim != null) anim.SetTrigger("Choque");
            if (crashSound != null) crashSound.Play();
        }
    }

    private void OnDisable()
    {
        moveAction.Disable();
        hornAction.Disable();
    }
}