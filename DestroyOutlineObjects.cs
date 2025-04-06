/*
 * DestroyOutlineObjects.cs
 * 
 * Este script maneja la destrucción de objetos que tienen el componente Outline.
 * Se utiliza para eliminar objetos específicos cuando entran en contacto con un trigger.
 * 
 * Características principales:
 * - Detección de colisiones con objetos en layer específico
 * - Efectos visuales opcionales al destruir objetos
 * - Efectos de sonido opcionales al destruir objetos
 * - Sistema de logging para seguimiento de objetos destruidos
 */

using UnityEngine;

public class DestroyOutlineObjects : MonoBehaviour
{
    // Layer que queremos detectar (Outline - layer 6)
    public int targetLayer = 6;

    // Opcional: Efecto de partículas para cuando se destruye un objeto
    public GameObject destructionEffect;

    // Opcional: Sonido para cuando se destruye un objeto
    public AudioClip destructionSound;

    /// <summary>
    /// Se ejecuta cuando un objeto entra en el trigger
    /// Verifica si el objeto está en el layer correcto y lo destruye si es necesario
    /// </summary>
    /// <param name="other">Collider del objeto que entró en el trigger</param>
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo entro");
        // Verificar si el objeto que entró está en el layer Outline
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
            Debug.Log("Objeto en layer Outline destruido: " + other.gameObject.name);

            // Destruir el objeto
            Destroy(other.gameObject);

        }
    }
}