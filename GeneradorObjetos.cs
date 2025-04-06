using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controla la generación automática de objetos interactivos en puntos específicos del espacio.
/// Este componente permite generar objetos de forma periódica hasta alcanzar una cantidad máxima definida.
/// </summary>
public class GeneradorObjetos : MonoBehaviour
{
    /// <summary>
    /// Prefab base que será instanciado en los puntos de generación.
    /// Este prefab debe contener o ser compatible con el componente ObjetoInteractivo.
    /// </summary>
    public GameObject prefabBase;
    
    // Configuración del ObjetoInteractivo
    [Header("Configuración del Objeto Interactivo")]
    /// <summary>
    /// Categoría del objeto interactivo que se generará.
    /// </summary>
    public int categoria;

    /// <summary>
    /// Nombre identificativo del objeto que se generará.
    /// </summary>
    public string nombreObjeto;
    
    // Configuración del generador
    [Header("Configuración de Generación")]
    /// <summary>
    /// Número máximo de objetos que pueden existir simultáneamente.
    /// </summary>
    public int cantidadMaxima = 3;

    /// <summary>
    /// Array de transforms que definen las posiciones donde se pueden generar objetos.
    /// </summary>
    public Transform[] puntosDeGeneracion;

    /// <summary>
    /// Tiempo en segundos entre cada intento de generación de objetos.
    /// </summary>
    public float tiempoEntreGeneraciones = 5f;
    
    /// <summary>
    /// Lista que mantiene el registro de todos los objetos generados actualmente.
    /// </summary>
    private List<GameObject> objetosGenerados = new List<GameObject>();

    /// <summary>
    /// Contador interno para controlar el tiempo entre generaciones.
    /// </summary>
    private float temporizador;
    
    /// <summary>
    /// Inicializa el temporizador al comenzar.
    /// </summary>
    void Start()
    {
        temporizador = tiempoEntreGeneraciones;
    }
    
    /// <summary>
    /// Actualiza el estado del generador cada frame, controlando la generación automática de objetos.
    /// </summary>
    void Update()
    {
        // Limpiar la lista de objetos que ya no existen
        objetosGenerados.RemoveAll(obj => obj == null);
        
        // Verificar si podemos generar más objetos
        if (objetosGenerados.Count < cantidadMaxima)
        {
            temporizador -= Time.deltaTime;
            
            if (temporizador <= 0)
            {
                GenerarObjeto();
                temporizador = tiempoEntreGeneraciones;
            }
        }
    }
    
    /// <summary>
    /// Genera un nuevo objeto si no se ha alcanzado el límite máximo.
    /// </summary>
    public void GenerarObjeto()
    {
        // Verificar si hemos alcanzado el límite
        if (objetosGenerados.Count >= cantidadMaxima)
            return;
        
        // Verificar si tenemos puntos de generación
        if (puntosDeGeneracion == null || puntosDeGeneracion.Length == 0)
        {
            Debug.LogWarning($"No hay puntos de generación configurados para {nombreObjeto}");
            return;
        }
        
        // Seleccionar un punto de generación aleatorio
        Transform puntoSeleccionado = puntosDeGeneracion[Random.Range(0, puntosDeGeneracion.Length)];
        
        // Instanciar el prefab
        GameObject nuevoObjeto = Instantiate(prefabBase, puntoSeleccionado.position, puntoSeleccionado.rotation);
        
        // Obtener o añadir el componente ObjetoInteractivo
        ObjetoInteractivo interactivo = nuevoObjeto.GetComponent<ObjetoInteractivo>();
        if (interactivo == null)
        {
            // Si no tiene el componente, añadirlo
            interactivo = nuevoObjeto.AddComponent<ObjetoInteractivo>();
            Debug.Log("Se ha añadido el componente ObjetoInteractivo al objeto generado");
        }
        
        // Configurar el ObjetoInteractivo (ahora estamos seguros de que existe)
        interactivo.categoria = this.categoria;
        interactivo.nombreObjeto = this.nombreObjeto;
        
        // Añadir a la lista de objetos generados
        objetosGenerados.Add(nuevoObjeto);
        
        Debug.Log($"Objeto {nombreObjeto} generado con éxito. Total actual: {objetosGenerados.Count}");
    }
    
    /// <summary>
    /// Genera objetos hasta alcanzar el límite máximo establecido.
    /// </summary>
    public void GenerarHastaLimite()
    {
        while (objetosGenerados.Count < cantidadMaxima)
        {
            GenerarObjeto();
        }
    }
    
    /// <summary>
    /// Destruye todos los objetos generados y limpia la lista.
    /// </summary>
    public void LimpiarObjetos()
    {
        foreach (GameObject obj in objetosGenerados)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        objetosGenerados.Clear();
    }
} 