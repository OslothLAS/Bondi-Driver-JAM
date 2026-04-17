using UnityEngine;

public class ManejadorExplosiones : MonoBehaviour
{
    // Arrastra tu prefab 'Explosion_J1' a este espacio en el Inspector de Unity
    [Header("Referencia del Prefab")]
    public GameObject prefabExplosion;

    // Tiempo que dura la partícula antes de ser destruida para ahorrar memoria
    [Header("Configuración")]
    public float duracionParticula = 2.0f;

    // Esta función la puedes llamar desde cualquier otro script (ej: al chocar)
    // Recibe la posición Vector3 exacta donde quieres que aparezca
    public void InstanciarExplosion(Vector3 posicion)
    {
        if (prefabExplosion != null)
        {
            // 1. Instanciar (crear) el objeto en la escena
            // Quaternion.identity significa que no tiene rotación (rotación cero)
            GameObject instanciaExplosion = Instantiate(prefabExplosion, posicion, Quaternion.identity);

            // 2. ¡IMPORTANTE! Limpieza de memoria.
            // Unity seguirá creando objetos hasta que el juego se ralentice si no los destruyes.
            // Esto destruye el objeto creado después de 'duracionParticula' segundos.
            Destroy(instanciaExplosion, duracionParticula);
        }
        else
        {
            Debug.LogError("No se ha asignado el prefab 'Explosion_J1' en el script ManejadorExplosiones.");
        }
    }

    // --- Ejemplo para testear ---
    // Si presionas la tecla 'E', creará una explosión en la posición de este script.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InstanciarExplosion(transform.position);
        }
    }
}