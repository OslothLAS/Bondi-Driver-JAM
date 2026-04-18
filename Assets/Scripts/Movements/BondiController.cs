using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BondiController : MonoBehaviour
{
    private Animator anim;

    [Header("Configuración de Teclas (Manual)")]
    public string upKey;
    public string downKey;
    public string leftKey;
    public string rightKey;
    public string hornKey;

    [Header("Configuración del Chiflido")]
    public float aumentoSemitonos = 2f;
    public float semitonesMaximos = 12f;
    public float ventanaTiempoToqueRapido = 0.5f;
    public float velocidadResetPitch = 5.0f;

    private float lastHornTime;
    private float currentSemitones = 0f;

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
    private int playerIndex; // 0 a 3

    void Awake()
    {
        // Determinamos el índice por el nombre del objeto
        if (gameObject.name.Contains("J1")) playerIndex = 0;
        else if (gameObject.name.Contains("J2")) playerIndex = 1;
        else if (gameObject.name.Contains("J3")) playerIndex = 2;
        else if (gameObject.name.Contains("J4")) playerIndex = 3;

        ConfigurarInputs();

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void ConfigurarInputs()
    {
        steerAction = new InputAction("Steer");
        accelerateAction = new InputAction("Accelerate");
        brakeAction = new InputAction("Brake");
        hornAction = new InputAction("Horn");

        string up = "", down = "", left = "", right = "", horn = "";

        // --- MAPEO DE TECLADO ---
        switch (playerIndex)
        {
            case 0: // J1: WASD + E
                up = "<Keyboard>/w"; down = "<Keyboard>/s"; left = "<Keyboard>/a"; right = "<Keyboard>/d"; horn = "<Keyboard>/e";
                break;
            case 1: // J2: IJKL + O
                up = "<Keyboard>/i"; down = "<Keyboard>/k"; left = "<Keyboard>/j"; right = "<Keyboard>/l"; horn = "<Keyboard>/o";
                break;
            case 2: // J3: FLECHAS + CTRL
                up = "<Keyboard>/upArrow"; down = "<Keyboard>/downArrow"; left = "<Keyboard>/leftArrow"; right = "<Keyboard>/rightArrow"; horn = "<Keyboard>/rightCtrl";
                break;
            case 3: // J4: NUMPAD 8462 + *
                up = "<Keyboard>/numpad8"; down = "<Keyboard>/numpad2"; left = "<Keyboard>/numpad4"; right = "<Keyboard>/numpad6"; horn = "<Keyboard>/numpadMultiply";
                break;
        }

        // Bindings de Teclado
        steerAction.AddCompositeBinding("Axis").With("Negative", left).With("Positive", right);
        accelerateAction.AddBinding(up);
        brakeAction.AddBinding(down);
        hornAction.AddBinding(horn);

        // --- BINDINGS DE GAMEPAD (Automático por índice) ---
        if (Gamepad.all.Count > playerIndex)
        {
            Gamepad gamepad = Gamepad.all[playerIndex];
            steerAction.AddBinding(gamepad.leftStick.x);
            accelerateAction.AddBinding(gamepad.rightTrigger);
            brakeAction.AddBinding(gamepad.leftTrigger);
            hornAction.AddBinding(gamepad.buttonSouth); // Botón A / X
        }

        // Habilitar todo
        steerAction.Enable(); accelerateAction.Enable(); brakeAction.Enable(); hornAction.Enable();
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
        float accel = accelerateAction.ReadValue<float>();
        float brake = brakeAction.ReadValue<float>();

        currentInput = new Vector2(steer, accel - brake);

        // BOCINA
        if (hornAction.WasPressedThisFrame() && hornSound != null)
        {
            float timeSinceLastPress = Time.time - lastHornTime;
            if (timeSinceLastPress < ventanaTiempoToqueRapido) currentSemitones += aumentoSemitonos;
            else currentSemitones = 0f;

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

        // Animaciones y Audio Motor
        if (anim != null)
        {
            anim.SetFloat("Velocidad", currentInput.y, 0.1f, Time.deltaTime);
            anim.SetFloat("Giro", currentInput.x, 0.1f, Time.deltaTime);
        }

        if (engineSound != null)
        {
            float speedRatio = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeedForPitch);
            engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
            engineSound.volume = Mathf.Lerp(0.4f, 0.8f, speedRatio);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.linearVelocity.magnitude;

        if (currentInput.y != 0)
        {
            if (currentInput.y > 0 && currentSpeed < maxSpeed)
                rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
            else if (currentInput.y < 0)
                rb.AddForce(transform.forward * currentInput.y * acceleration, ForceMode.Acceleration);
        }

        if (currentSpeed > maxSpeed) rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        if (currentSpeed > 0.1f)
        {
            float direction = Vector3.Dot(rb.linearVelocity, transform.forward) > 0 ? 1 : -1;
            float sensitivity = Mathf.Clamp(currentSpeed / 10f, 1f, 2.5f);
            float rotation = currentInput.x * steering * direction * sensitivity * Time.fixedDeltaTime * 10f;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotation, 0));
        }

        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVel * (1f - driftFactor), ForceMode.VelocityChange);

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
        steerAction.Disable(); accelerateAction.Disable(); brakeAction.Disable(); hornAction.Disable();
    }
}