using UnityEngine;

/// <summary>
/// Componente que se encarga de destruir objetos que tienen el layer "Outline" cuando colisionan con este objeto.
/// Este script debe ser adjuntado a un objeto con un Collider configurado como trigger.
/// </summary>
public class DestroyOutlineObjects : MonoBehaviour
{
    /// <summary>
    /// Layer objetivo que queremos detectar (por defecto: Outline - layer 6).
    /// Asegúrate de que los objetos que deseas destruir estén en este layer.
    /// </summary>
    public int targetLayer = 6;

    /// <summary>
    /// Prefab del efecto de partículas que se instanciará cuando se destruya un objeto.
    /// Este campo es opcional y puede dejarse vacío si no se desea ningún efecto visual.
    /// </summary>
    public GameObject destructionEffect;

    /// <summary>
    /// Clip de audio que se reproducirá cuando se destruya un objeto.
    /// Este campo es opcional y puede dejarse vacío si no se desea ningún efecto de sonido.
    /// </summary>
    public AudioClip destructionSound;

    /// <summary>
    /// Se llama automáticamente cuando otro collider entra en el trigger de este objeto.
    /// </summary>
    /// <param name="other">El collider que ha entrado en el trigger.</param>
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo entró en el trigger");
        
        // Verificar si el objeto que entró está en el layer objetivo
        if (other.gameObject.layer == targetLayer)
        {
            /* Opcional: Crear efecto de partículas en la posición del objeto
            if (destructionEffect != null)
            {
                Instantiate(destructionEffect, other.transform.position, Quaternion.identity);
            }
            */

            /* Opcional: Reproducir sonido de destrucción
            if (destructionSound != null)
            {
                AudioSource.PlayClipAtPoint(destructionSound, other.transform.position);
            }
            */

            // Registrar en el log qué objeto fue destruido
            Debug.Log($"Objeto '{other.gameObject.name}' en layer Outline fue destruido");

            // Destruir el objeto que entró en el trigger
            Destroy(other.gameObject);
        }
    }
}