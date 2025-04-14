using UnityEngine;
using System.Collections.Generic;


/// Controla la generación automática de objetos interactivos en puntos específicos del espacio.
/// Este componente permite generar objetos de forma periódica hasta alcanzar una cantidad máxima definida.

public class GeneradorObjetos : MonoBehaviour
{
    
    /// Prefab base que será instanciado en los puntos de generación.
    /// Este prefab debe contener o ser compatible con el componente ObjetoInteractivo.
    
    public GameObject prefabBase;
    
    // Configuración del ObjetoInteractivo
    [Header("Configuración del Objeto Interactivo")]
    
    /// Categoría del objeto interactivo que se generará.
    
    public int categoria;

    
    /// Nombre identificativo del objeto que se generará.
    
    public string nombreObjeto;
    
    // Configuración del generador
    [Header("Configuración de Generación")]
    
    /// Número máximo de objetos que pueden existir simultáneamente.
    
    public int cantidadMaxima = 3;

    
    /// Array de transforms que definen las posiciones donde se pueden generar objetos.
    
    public Transform[] puntosDeGeneracion;

    
    /// Tiempo en segundos entre cada intento de generación de objetos.
    
    public float tiempoEntreGeneraciones = 5f;
    
    
    /// Lista que mantiene el registro de todos los objetos generados actualmente.
    
    private List<GameObject> objetosGenerados = new List<GameObject>();

    
    /// Contador interno para controlar el tiempo entre generaciones.
    
    private float temporizador;
    
    
    /// Inicializa el temporizador al comenzar.
    
    void Start()
    {
        temporizador = tiempoEntreGeneraciones;
    }
    
    
    /// Actualiza el estado del generador cada frame, controlando la generación automática de objetos.
    
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
    
    
    /// Genera un nuevo objeto si no se ha alcanzado el límite máximo.
    
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
            //Debug.Log("Se ha añadido el componente ObjetoInteractivo al objeto generado");
        }
        
        // Configurar el ObjetoInteractivo (ahora estamos seguros de que existe)
        interactivo.categoria = this.categoria;
        interactivo.nombreObjeto = this.nombreObjeto;
        
        // Añadir a la lista de objetos generados
        objetosGenerados.Add(nuevoObjeto);
        
        //Debug.Log($"Objeto {nombreObjeto} generado con éxito. Total actual: {objetosGenerados.Count}");
    }
    
    
    /// Genera objetos hasta alcanzar el límite máximo establecido.
    
    public void GenerarHastaLimite()
    {
        while (objetosGenerados.Count < cantidadMaxima)
        {
            GenerarObjeto();
        }
    }
    
    
    /// Destruye todos los objetos generados y limpia la lista.
    
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