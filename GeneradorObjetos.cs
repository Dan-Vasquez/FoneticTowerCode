/*
 * GeneradorObjetos.cs
 * 
 * Este script se encarga de la generación y gestión de objetos interactivos en el juego.
 * Permite crear instancias de objetos en puntos específicos con propiedades configurables.
 * 
 * Características principales:
 * - Generación automática de objetos en puntos predefinidos
 * - Control de cantidad máxima de objetos
 * - Gestión del ciclo de vida de los objetos
 * - Configuración de propiedades de objetos interactivos
 */

using UnityEngine;
using System.Collections.Generic;

public class GeneradorObjetos : MonoBehaviour
{
    // Prefab base que se va a instanciar
    public GameObject prefabBase;
    
    // Configuración del ObjetoInteractivo
    [Header("Configuración del Objeto Interactivo")]
    public int categoria;
    public string nombreObjeto;
    public GameObject modeloVisual;
    
    // Configuración del generador
    [Header("Configuración de Generación")]
    public int cantidadMaxima = 3;
    public Transform[] puntosDeGeneracion;
    public float tiempoEntreGeneraciones = 5f;
    
    // Control interno
    private List<GameObject> objetosGenerados = new List<GameObject>();
    private float temporizador;
    
    /// <summary>
    /// Inicializa el generador y configura el temporizador inicial
    /// </summary>
    void Start()
    {
        temporizador = tiempoEntreGeneraciones;
    }
    
    /// <summary>
    /// Actualiza el estado del generador, limpia objetos destruidos y genera nuevos según sea necesario
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
    /// Genera un nuevo objeto interactivo en uno de los puntos de generación disponibles
    /// Configura sus propiedades y componentes según los parámetros establecidos
    /// </summary>
    public void GenerarObjeto()
    {
        // Verificar si hemos alcanzado el límite
        if (objetosGenerados.Count >= cantidadMaxima)
            return;
        
        // Verificar si tenemos puntos de generación
        if (puntosDeGeneracion == null || puntosDeGeneracion.Length == 0)
        {
            Debug.LogWarning("No hay puntos de generación configurados para " + nombreObjeto);
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
        
        // Si se especificó un modelo visual, configurarlo
        if (this.modeloVisual != null)
        {
            // Intentar encontrar un objeto hijo con el mismo nombre
            Transform childModel = nuevoObjeto.transform.Find(this.modeloVisual.name);
            if (childModel != null)
            {
                interactivo.modeloVisual = childModel.gameObject;
            }
            else
            {
                // Si no encuentra un hijo con ese nombre, asignar el objeto completo
                interactivo.modeloVisual = nuevoObjeto;
                Debug.Log("No se encontró un modelo visual específico. Se asignó el objeto completo como modelo visual.");
            }
        }
        else
        {
            // Si no se especificó modelo, usar el objeto completo
            interactivo.modeloVisual = nuevoObjeto;
        }
        
        // Añadir a la lista de objetos generados
        objetosGenerados.Add(nuevoObjeto);
        
        Debug.Log("Objeto " + nombreObjeto + " generado con éxito. Total actual: " + objetosGenerados.Count);
    }
    
    /// <summary>
    /// Genera objetos hasta alcanzar el límite máximo establecido
    /// </summary>
    public void GenerarHastaLimite()
    {
        while (objetosGenerados.Count < cantidadMaxima)
        {
            GenerarObjeto();
        }
    }
    
    /// <summary>
    /// Elimina todos los objetos generados y limpia la lista de referencias
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