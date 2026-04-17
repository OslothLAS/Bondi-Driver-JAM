using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    private Animator anim;

    [Header("Configuración del Chiflido")]
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
    public float maxSpeed = 18f;
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
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction hornAction;
    private InputAction steerAction;

    private Vector2 currentInput;
    private int gamepadIndex;

    void Awake()
    {
        gamepadIndex = (gameObject.name == "Bondi_J1") ? 0 : 1;

        // Definimos las teclas según el jugador
        if (gamepadIndex == 0)
        {
            upKey = "<Keyboard>/w"; downKey = "<Keyboard>/s";
            leftKey = "<Keyboard>/a"; rightKey = "<Keyboard>/d";
            hornKey = "<Keyboard>/e";
        }
        else
        {
            upKey = "<Keyboard>/upArrow"; downKey = "<Keyboard>/downArrow";
            leftKey = "<Keyboard>/leftArrow"; rightKey = "<Keyboard>/rightArrow";
            hornKey = "<Keyboard>/oem1";
        }

        // --- CONFIGURACIÓN DE ACCIONES ---
        steerAction = new InputAction("Steer");
        accelerateAction = new InputAction("Accelerate");
        brakeAction = new InputAction("Brake");
        hornAction = new InputAction("Horn");

        // 1. Vinculamos Teclado (WASD / Flechas)
        steerAction.AddCompositeBinding("Axis")
            .With("Negative", leftKey)
            .With("Positive", rightKey);
        accelerateAction.AddBinding(upKey);
        brakeAction.AddBinding(downKey);
        hornAction.AddBinding(hornKey);

        // 2. Vinculamos Gamepad (si existe)
        if (Gamepad.all.Count > gamepadIndex)
        {
            Gamepad gamepad = Gamepad.all[gamepadIndex];
            steerAction.AddBinding(gamepad.leftStick.x);
            accelerateAction.AddBinding(gamepad.rightTrigger);
            brakeAction.AddBinding(gamepad.leftTrigger);
            hornAction.AddBinding(gamepad.buttonSouth);
        }

        steerAction.AddBinding(leftKey);
        accelerateAction.AddBinding(upKey);
        brakeAction.AddBinding(downKey);

        hornAction.AddBinding(hornKey);
        hornAction.Enable();
        steerAction.Enable();
        accelerateAction.Enable();
        brakeAction.Enable();

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
        // Leemos los valores (ahora combinan teclado y gamepad)
        float steer = steerAction.ReadValue<float>();
        float accel = accelerateAction.ReadValue<float>();
        float brake = brakeAction.ReadValue<float>();

        // Consolidamos el input
        currentInput = new Vector2(steer, accel - brake);

        // LÓGICA DE BOCINA
        if (hornAction.WasPressedThisFrame() && hornSound != null)
        {
            float timeSinceLastPress = Time.time - lastHornTime;
            if (timeSinceLastPress < ventanaTiempoToqueRapido)
                currentSemitones += aumentoSemitonos;
            else
                currentSemitones = 0f;

            currentSemitones = Mathf.Min(currentSemitones, semitonesMaximos);
            hornSound.pitch = Mathf.Pow(1.05946f, currentSemitones);
            hornSound.PlayOneShot(hornSound.clip);
            lastHornTime = Time.time;
        }

        if (currentSemitones > 0f)
        {
            currentSemitones -= Time.deltaTime * velocidadResetPitch;
            if (currentSemitones < 0f) currentSemitones = 0f;
        }

        // Animaciones
        if (anim != null)
        {
            anim.SetFloat("Velocidad", currentInput.y, 0.1f, Time.deltaTime);
            anim.SetFloat("Giro", currentInput.x, 0.1f, Time.deltaTime);
        }

        // Audio Motor
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

        // Aplicamos fuerza con el limitador de velocidad
        if (currentInput.y != 0)
        {
            if (currentInput.y > 0 && currentSpeed < maxSpeed)
                rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
            else if (currentInput.y < 0)
                rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
        }

        // Clamp de seguridad
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        // Giro
        if (currentSpeed > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            float sensitivity = Mathf.Clamp(currentSpeed / 10f, 1f, 2.5f);
            float rotation = currentInput.x * steering * direction * sensitivity * Time.fixedDeltaTime * 10f;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotation, 0));
        }

        // Drift / Fricción lateral
        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVel * (1f - driftFactor), ForceMode.VelocityChange);

        // Audio de Drift
        if (driftSound != null)
        {
            bool isDrifting = currentSpeed > driftSpeedThreshold && Mathf.Abs(currentInput.x) > 0.1f;
            if (isDrifting && !driftSound.isPlaying) driftSound.Play();
            else if (!isDrifting && driftSound.isPlaying) driftSound.Stop();
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
        steerAction.Disable();
        accelerateAction.Disable();
        brakeAction.Disable();
        hornAction.Disable();
    }
}