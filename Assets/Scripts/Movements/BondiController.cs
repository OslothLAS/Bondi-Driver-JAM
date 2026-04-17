using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    private Animator anim;

    [Header("Configuración del Chiflido (Logarítmico)")]
    public float aumentoSemitonos = 2f;
    public float semitonesMaximos = 12f;
    public float ventanaTiempoToqueRapido = 0.5f;
    public float velocidadResetPitch = 5.0f;

    private float lastHornTime;
    private float currentSemitones = 0f;

    [Header("Configuración de Teclas")]
    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;
    public string hornKey;

    [Header("Física")]
    public float acceleration = 50f;
    public float steering = 25f;
    public float maxSpeed = 18f; // <--- Límite de velocidad máxima
    [Range(0, 1)] public float driftFactor = 0.95f;

    [Header("Audio")]
    public AudioSource crashSound;
    public AudioSource hornSound;
    public AudioSource driftSound;
    public float driftSpeedThreshold = 8f;

    [Header("Audio Motor")]
    public AudioSource engineSound;
    public float minPitch = 0.8f;
    public float maxPitch = 2.2f;
    public float maxSpeedForPitch = 20f;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction hornAction;
    private InputAction steerAction;
    private Vector2 currentInput;
    private float accelerateInput;
    private float brakeInput;
    private int gamepadIndex;

    void Awake()
    {
        // Detectamos si es J1 o J2 por el nombre del objeto
        int playerIndex = (gameObject.name == "Bondi_J1") ? 0 : 1;

        if (playerIndex == 0)
        {
            Debug.Log($"Gamepad found: {gamepad.displayName}");
        }

        gamepadIndex = (gameObject.name == "Bondi_J1") ? 0 : 1;

        steerAction = new InputAction("Steer");
        accelerateAction = new InputAction("Accelerate");
        brakeAction = new InputAction("Brake");
        hornAction = new InputAction("Horn");

        if (Gamepad.all.Count > gamepadIndex)
        {
            Gamepad gamepad = Gamepad.all[gamepadIndex];
            Debug.Log($"[{gameObject.name}] Using gamepad: {gamepad.displayName}");
            steerAction.AddBinding(gamepad.leftStick.x);
            accelerateAction.AddBinding(gamepad.rightTrigger);
            brakeAction.AddBinding(gamepad.leftTrigger);
            hornAction.AddBinding(gamepad.buttonEast);
        }
        else
        {
            Debug.Log($"[{gameObject.name}] No gamepad {gamepadIndex} found. Use keyboard.");
        }

        // Configuración de Input Action Map
        moveAction = new InputAction("Move");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", upKey).With("Down", downKey)
            .With("Left", leftKey).With("Right", rightKey);

        moveAction.AddBinding($"<Gamepad>{{{playerIndex}}}/leftStick");
        moveAction.Enable();

        hornAction = new InputAction("Horn");
        hornAction.AddBinding(hornKey);
        hornAction.AddBinding($"<Gamepad>{{{playerIndex}}}/buttonSouth");
        hornAction.Enable();

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        if (engineSound != null)
        {
            engineSound.loop = true;
            engineSound.Play();
        }
    }

    void Update()
    {
        float steer = steerAction.ReadValue<float>();
        accelerateInput = accelerateAction.ReadValue<float>();
        brakeInput = brakeAction.ReadValue<float>();

        Debug.Log($"[{gameObject.name}] GamepadIndex: {gamepadIndex}, Steer: {steer}, Accelerate: {accelerateInput}, Brake: {brakeInput}");

        currentInput = new Vector2(steer, accelerateInput - brakeInput);

        // LÓGICA DE BOCINA (Logarítmica)
        if (hornAction.WasPressedThisFrame() && hornSound != null)
        {
            float timeSinceLastPress = Time.time - lastHornTime;

            if (timeSinceLastPress < ventanaTiempoToqueRapido)
            {
                currentSemitones += aumentoSemitonos;
            }
            else
            {
                currentSemitones = 0f;
            }

            currentSemitones = Mathf.Min(currentSemitones, semitonesMaximos);

            // Fórmula musical: 1.05946 es la raíz 12 de 2
            hornSound.pitch = Mathf.Pow(1.05946f, currentSemitones);
            hornSound.PlayOneShot(hornSound.clip);
            lastHornTime = Time.time;
        }

        // Reset gradual de los semitones de la bocina
        if (currentSemitones > 0f)
        {
            currentSemitones -= Time.deltaTime * velocidadResetPitch;
            if (currentSemitones < 0f) currentSemitones = 0f;
        }

        // Animación de dirección y velocidad
        if (anim != null)
        {
            anim.SetFloat("Velocidad", currentInput.y, 0.1f, Time.deltaTime);
            anim.SetFloat("Giro", currentInput.x, 0.1f, Time.deltaTime);
        }

        if (engineSound != null)
        {
            float speed = rb.linearVelocity.magnitude;
            float speedRatio = Mathf.Clamp01(speed / maxSpeedForPitch);
            engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
            engineSound.volume = Mathf.Lerp(0.4f, 0.8f, speedRatio);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        // 1. LIMITADOR DE FUERZA: Solo aceleramos si no pasamos la maxSpeed
        if (currentInput.y != 0)
        {
            // Si el input es hacia adelante y estamos bajo el límite, aceleramos.
            // Si es hacia atrás (frenado), siempre permitimos la fuerza.
            if (currentInput.y > 0 && currentSpeed < maxSpeed)
            {
                rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
            }
            else if (currentInput.y < 0)
            {
                rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
            }
        }

        // 2. CLAMP DE SEGURIDAD: Cortamos la velocidad si un choque o gravedad nos acelera de más
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Lógica de Giro
        if (currentSpeed > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            float sensitivity = Mathf.Clamp(currentSpeed / 10f, 1f, 2.5f);
            float rotation = currentInput.x * steering * direction * sensitivity * Time.fixedDeltaTime * 10f;

            Quaternion deltaRotation = Quaternion.Euler(0, rotation, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // Simulación de fricción lateral (Drift)
        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVel * (1f - driftFactor), ForceMode.VelocityChange);

        // Sonido de Derrape
        if (driftSound != null && driftSound.clip != null)
        {
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            bool isDrifting = forwardSpeed > driftSpeedThreshold && Mathf.Abs(currentInput.x) > 0.1f;

            if (isDrifting && !driftSound.isPlaying)
            {
                driftSound.loop = true;
                driftSound.volume = 0.5f;
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
        // No chocar contra las paradas
        if (collision.gameObject.GetComponent<ParadaController>() != null) return;

        if (collision.relativeVelocity.magnitude > 7f)
        {
            if (anim != null) anim.SetTrigger("Choque");
            if (crashSound != null) crashSound.Play();
        }
    }

    private void OnDisable()
    {
        steerAction.Disable();
        accelerateAction.Disable();
        brakeAction.Disable();
        hornAction.Disable();
    }
}