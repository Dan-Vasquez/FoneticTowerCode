using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controla un área que elimina objetos interactivos cuando entran en su collider.
/// Este script debe ser asignado a un GameObject con un Collider marcado como Trigger.
/// </summary>
public class AreaEliminadoraObjetos : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Si está activo, eliminará automáticamente los objetos que entren en la zona")]
    public bool eliminacionAutomatica = true;

    [Tooltip("Tiempo de espera antes de eliminar el objeto (en segundos)")]
    public float tiempoEspera = 0.5f;

    [Tooltip("Efecto de partículas que se mostrará al eliminar un objeto")]
    public ParticleSystem efectoEliminacion;

    [Tooltip("Sonido que se reproducirá al eliminar un objeto")]
    public AudioClip sonidoEliminacion;

    [Header("Filtros")]
    [Tooltip("Si está activo, solo eliminará objetos de las categorías especificadas")]
    public bool filtrarPorCategoria = false;

    [Tooltip("Categorías de objetos que serán eliminados (0=vegetal, 1=fruta, 2=poción)")]
    public List<int> categoriasPermitidas = new List<int> { 0, 1, 2 };

    // Lista de objetos actualmente dentro del área
    private List<ObjetoInteractivo> objetosDentroDelArea = new List<ObjetoInteractivo>();
    private AudioSource audioSource;

    /// <summary>
    /// Inicialización del componente.
    /// </summary>
    private void Start()
    {
        // Verificar que el objeto tiene un collider configurado como trigger
        Collider collider = GetComponent<Collider>();
        if (collider == null || !collider.isTrigger)
        {
            Debug.LogError("AreaEliminadoraObjetos: El objeto debe tener un Collider configurado como Trigger");
        }

        // Inicializar componente de audio si no existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoEliminacion != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // Audio 3D
        }

        // Inicializar el sistema de partículas si es necesario
        if (efectoEliminacion != null)
        {
            efectoEliminacion.Stop();
        }
    }

    /// <summary>
    /// Se llama cuando otro collider entra en el trigger.
    /// </summary>
    /// <param name="other">El collider que ha entrado en contacto.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto tiene el componente ObjetoInteractivo
        ObjetoInteractivo objetoInteractivo = other.GetComponent<ObjetoInteractivo>();
        if (objetoInteractivo != null)
        {
            // Verificar si el objeto cumple con los filtros de categoría
            if (filtrarPorCategoria && !categoriasPermitidas.Contains(objetoInteractivo.categoria))
            {
                // El objeto no está en una categoría permitida
                return;
            }

            // Añadir a la lista de objetos dentro del área
            if (!objetosDentroDelArea.Contains(objetoInteractivo))
            {
                objetosDentroDelArea.Add(objetoInteractivo);
                Debug.Log($"Objeto '{objetoInteractivo.nombreObjeto}' entró en el área de eliminación");

                // Si la eliminación automática está activada, programar eliminación
                if (eliminacionAutomatica)
                {
                    Invoke("EliminarObjetosEnArea", tiempoEspera);
                }
            }
        }
    }

    /// <summary>
    /// Se llama cuando otro collider sale del trigger.
    /// </summary>
    /// <param name="other">El collider que ha salido.</param>
    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto tiene el componente ObjetoInteractivo
        ObjetoInteractivo objetoInteractivo = other.GetComponent<ObjetoInteractivo>();
        if (objetoInteractivo != null && objetosDentroDelArea.Contains(objetoInteractivo))
        {
            // Quitar de la lista de objetos dentro del área
            objetosDentroDelArea.Remove(objetoInteractivo);
            Debug.Log($"Objeto '{objetoInteractivo.nombreObjeto}' salió del área de eliminación");
        }
    }

    /// <summary>
    /// Elimina todos los objetos interactivos que están actualmente dentro del área.
    /// </summary>
    public void EliminarObjetosEnArea()
    {
        if (objetosDentroDelArea.Count == 0)
        {
            return;
        }

        int objetosEliminados = 0;
        
        // Crear una copia de la lista para evitar problemas al modificarla durante la iteración
        List<ObjetoInteractivo> objetosParaEliminar = new List<ObjetoInteractivo>(objetosDentroDelArea);
        
        foreach (ObjetoInteractivo objeto in objetosParaEliminar)
        {
            if (objeto != null)
            {
                // Reproducir efectos antes de eliminar
                ReproducirEfectos(objeto.transform.position);
                
                // Eliminar el objeto
                objetosDentroDelArea.Remove(objeto);
                Destroy(objeto.gameObject);
                objetosEliminados++;
                
                Debug.Log($"Objeto '{objeto.nombreObjeto}' eliminado del área");
            }
        }
        
        // Limpiar referencias nulas después de eliminar
        objetosDentroDelArea.RemoveAll(item => item == null);
        
        if (objetosEliminados > 0)
        {
            Debug.Log($"Se eliminaron {objetosEliminados} objetos del área");
        }
    }

    /// <summary>
    /// Reproduce los efectos visuales y de sonido en la posición especificada.
    /// </summary>
    /// <param name="posicion">Posición donde reproducir los efectos.</param>
    private void ReproducirEfectos(Vector3 posicion)
    {
        // Reproducir efecto de partículas
        if (efectoEliminacion != null)
        {
            efectoEliminacion.transform.position = posicion;
            efectoEliminacion.Play();
        }
        
        // Reproducir sonido
        if (audioSource != null && sonidoEliminacion != null)
        {
            audioSource.PlayOneShot(sonidoEliminacion);
        }
    }

    /// <summary>
    /// Método público para eliminar objetos de forma manual.
    /// Útil para llamar desde botones UI o eventos.
    /// </summary>
    public void EliminarObjetosManuales()
    {
        EliminarObjetosEnArea();
    }
} 