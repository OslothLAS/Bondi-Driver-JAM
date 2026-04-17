using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    private Animator anim;
    [Header("Configuración del Chiflido (Logarítmico)")]
    public float aumentoSemitonos = 2f; // Cuántos semitones sube por toque
    public float semitonesMaximos = 12f; // Límite (una octava arriba)
    public float ventanaTiempoToqueRapido = 0.5f;
    public float velocidadResetPitch = 5.0f; // Qué tan rápido bajan los semitones

    private float lastHornTime;
    private float currentSemitones = 0f; // Ahora trackeamos semitones, no el pitch directo

    [Header("Configuración de Teclas")]
    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;
    public string hornKey;

    [Header("Física")]
    public float acceleration = 50f;
    public float steering = 25f;
    [Range(0, 1)] public float driftFactor = 0.95f;

    [Header("Audio")]
    public AudioSource crashSound;
    public AudioSource hornSound;
    public AudioSource driftSound;
    public float driftSpeedThreshold = 8f;

    [Header("Audio Motor (NUEVO)")]
    public AudioSource engineSound;
    public float minPitch = 0.8f;      // Tono en ralentí (quieto)
    public float maxPitch = 2.2f;      // Tono a máxima velocidad
    public float maxSpeedForPitch = 20f; // Velocidad donde el tono deja de subir

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction hornAction;
    private Vector2 currentInput;

    void Awake()
    {
        int playerIndex = (gameObject.name == "Bondi_J1") ? 0 : 1;

        if (playerIndex == 0)
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

        // Configuración inicial del motor
        if (engineSound != null)
        {
            engineSound.loop = true;
            engineSound.Play();
        }
    }

    void Update()
    {

        currentInput = moveAction.ReadValue<Vector2>();

        if (hornAction.WasPressedThisFrame() && hornSound != null)
        {
            float timeSinceLastPress = Time.time - lastHornTime;

            if (timeSinceLastPress < ventanaTiempoToqueRapido)
            {
                // Sumamos semitones de forma lineal (que al oído es logarítmico)
                currentSemitones += aumentoSemitonos;
            }
            else
            {
                currentSemitones = 0f; // Primer toque = Tono original
            }

            currentSemitones = Mathf.Min(currentSemitones, semitonesMaximos);

            // CONVERSIÓN DE SEMITONES A MULTIPLICADOR DE PITCH (Fórmula musical)
            // 1 semitono arriba es aproximadamente 1.0594 (raíz 12 de 2)
            hornSound.pitch = Mathf.Pow(1.05946f, currentSemitones);

            hornSound.PlayOneShot(hornSound.clip);
            lastHornTime = Time.time;
        }

        // El reset también tiene que ser sobre los semitones
        if (currentSemitones > 0f)
        {
            currentSemitones -= Time.deltaTime * velocidadResetPitch;
            if (currentSemitones < 0f) currentSemitones = 0f;
        }

        if (hornAction.WasPressedThisFrame() && hornSound != null)
        {
            hornSound.Play();
        }

        if (anim != null)
        {
            anim.SetFloat("Velocidad", currentInput.y, 0.1f, Time.deltaTime);
            anim.SetFloat("Giro", currentInput.x, 0.1f, Time.deltaTime);
        }

        // --- LÓGICA DE PITCH DEL MOTOR ---
        if (engineSound != null)
        {
            float speed = rb.linearVelocity.magnitude;
            // Normalizamos la velocidad entre 0 y 1
            float speedRatio = Mathf.Clamp01(speed / maxSpeedForPitch);

            // Cambiamos el tono según la velocidad
            engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

            // Tip extra: subir un poco el volumen cuando acelera
            engineSound.volume = Mathf.Lerp(0.4f, 0.8f, speedRatio);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentInput.y != 0)
        {
            rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
        }

        if (currentSpeed > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            float sensitivity = Mathf.Clamp(currentSpeed / 10f, 1f, 2.5f);
            float rotation = currentInput.x * steering * direction * sensitivity * Time.fixedDeltaTime * 10f;

            Quaternion deltaRotation = Quaternion.Euler(0, rotation, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVel * (1f - driftFactor), ForceMode.VelocityChange);

        // DRIFT SOUND
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