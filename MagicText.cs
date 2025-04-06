/*
 * MagicText.cs
 * 
 * Este script es el componente principal del sistema de juego educativo.
 * Maneja la lógica de los niveles, efectos de texto, y la interacción con objetos.
 * 
 * Características principales:
 * - Sistema de niveles progresivos (7 niveles)
 * - Efectos visuales de texto (tamaño, color, ondulación)
 * - Sistema de partículas y feedback visual
 * - Gestión de palabras y sílabas
 * - Sistema de emociones y colores
 * - Interacción con objetos del juego
 */

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MagicText : MonoBehaviour
{
    // Referencia al texto que aparecerá
    public TextMeshPro floatingText;

    // Referencia al jugador para que el texto lo mire
    public Transform playerTransform;

    // Altura final del texto en metros
    public float finalHeight = 7.0f;

    // Duración de la animación en segundos
    public float animationDuration = 1.0f;

    // Tamaño inicial y final del texto
    public float initialTextSize = 1.0f;
    public float finalTextSize = 4.0f;

    // Parámetros para el efecto de ondulación
    public float wobbleSpeed = 1.0f;         // Velocidad de la ondulación
    public float wobbleAmount = 0.003f;       // Amplitud de la ondulación (reducida para ser más sutil)

    // Animación de escritura
    public bool useTypewriterEffect = false;  // Activar/desactivar efecto de escritura
    public float typewriterSpeed = 20.0f;    // Caracteres por segundo
    private float currentTypewriterProgress = 0f;
    private string fullText = "";

    public int nivelActual = 0;

    // Categoría actual seleccionada
    private int categoriaActual = 0;  // 0 = vegetales, 1 = frutas, 2 = pociones

    // Variable para controlar si la animación está en curso
    private bool isAnimating = false;

    // Tiempo transcurrido de la animación
    private float elapsedTime = 0f;

    // Posición inicial del texto
    private Vector3 initialPosition;

    // Contador de pulsaciones necesarias
    private int contadorPulsaciones = 1;

    // Texto base (sin el contador)
    private string textoBase = "";

    // Variables para el efecto de ondulación
    private bool wobbleEffectInitialized = false;

    // Layer al que cambiaremos la esfera (Outline)
    private int outlineLayer = 6;

    // Lista para almacenar todos los objetos interactivos
    private List<ObjetoInteractivo> objetosInteractivos = new List<ObjetoInteractivo>();

    // Variable para rastrear el último objeto que se cambió a Outline
    private ObjetoInteractivo ultimoObjetoIluminado = null;

    // UI para seleccionar nivel
    public TextMeshProUGUI nivelTexto;
    public GameObject panelSeleccionNivel;

    public ParticleSystem particleSystem2;

    // Variable para almacenar la palabra seleccionada
    private string palabraSeleccionada = "";

    // Variable para almacenar las sílabas de la palabra
    private List<string> silabasDePalabra = new List<string>();

    // Variables para el nivel 5 y 6 (tamaños variables)
    private List<float> tamañosTexto = new List<float>();

    // Variable para almacenar sílabas mezcladas de diferentes palabras (nivel 4)
    private List<string> silabasMezcladas = new List<string>();

    // Lista para guardar las palabras completas de donde se extraen las sílabas (nivel 4)
    private List<string> palabrasOriginalesMezcladas = new List<string>();

    // Agregar una nueva variable para rastrear el índice actual de tamaño en el nivel 5
    private int indiceNivel5Actual = 0;

    // Añadir variables para el nivel 6
    private int indiceNivel6Actual = 0;          // Índice de la sílaba actualmente mostrada
    private int indiceNivel6Resaltado = 0;       // Índice de la sílaba resaltada (con tamaño mayor)
    private List<string> silabasNivel6Mostradas = new List<string>(); // Sílabas mostradas hasta el momento

    void Start()
    {
        // Configuración inicial del texto
        floatingText.gameObject.SetActive(false);

        // Guardamos la posición inicial
        initialPosition = transform.position;

        // Encontrar y guardar todos los objetos interactivos en la escena
        ObjetoInteractivo[] objetosEnEscena = FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None);
        objetosInteractivos.AddRange(objetosEnEscena);

        // Actualizar el texto de nivel en la UI (si existe)
        ActualizarTextoNivel();

        // Asegurarse de que el sistema de partículas esté pausado al inicio
        if (particleSystem2 != null)
        {
            particleSystem2.Stop();
        }
    }

    void Update()
    {
        // Cambiar nivel con las teclas numéricas 1-7
        if (Input.GetKeyDown(KeyCode.Alpha1)) CambiarNivel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CambiarNivel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CambiarNivel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CambiarNivel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) CambiarNivel(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) CambiarNivel(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) CambiarNivel(6);

        // Mostrar/ocultar panel de selección de nivel con la tecla Tab
        if (Input.GetKeyDown(KeyCode.Tab) && panelSeleccionNivel != null)
        {
            panelSeleccionNivel.SetActive(!panelSeleccionNivel.activeSelf);
        }

        // Activar la animación al presionar E
        if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
        {
            StartAnimation();
        }

        // Si la animación está en curso, actualizarla
        if (isAnimating)
        {
            UpdateAnimation();

            // Aplicar el efecto de escritura si está activado
            if (useTypewriterEffect && currentTypewriterProgress < fullText.Length)
            {
                // Ajustar la velocidad según el nivel actual
                float velocidadAjustada = typewriterSpeed * GameData.Instance.nivelesDificultad[nivelActual].velocidadHabla;

                // Avanzar en el progreso del efecto de escritura
                currentTypewriterProgress += velocidadAjustada * Time.deltaTime;
                int charactersToShow = Mathf.Min(Mathf.FloorToInt(currentTypewriterProgress), fullText.Length);
                floatingText.text = fullText.Substring(0, charactersToShow);

                // Después de actualizar el texto, forzamos la actualización de la malla
                floatingText.ForceMeshUpdate();
                wobbleEffectInitialized = false;
            }

            // Aplicar el efecto de ondulación cuando el texto es visible
            if (floatingText.gameObject.activeInHierarchy)
            {
                ApplyWobbleEffect();
            }

            // Hacer que el texto mire hacia el jugador si tenemos referencia al jugador
            if (playerTransform != null && floatingText.gameObject.activeInHierarchy)
            {
                // Calculamos la dirección desde el texto hacia el jugador
                Vector3 directionToPlayer = playerTransform.position - floatingText.transform.position;

                // Si queremos que el texto mire solo horizontalmente al jugador (ignorando el eje Y)
                directionToPlayer.y = 0;

                // Solo rotamos si la dirección no es cero (evita errores)
                if (directionToPlayer != Vector3.zero)
                {
                    // Creamos una rotación que mira en la dirección del jugador
                    Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer);

                    // Aplicamos la rotación al texto
                    floatingText.transform.rotation = lookRotation;
                }
            }

            // Procesar la reducción del contador al presionar Q
            if (Input.GetKeyDown(KeyCode.Q))
            {
                contadorPulsaciones--;
                Debug.Log("Contador: " + contadorPulsaciones);

                // Para el nivel 5, cambiamos el tamaño al presionar Q
                if (nivelActual == 4) // Nivel 5
                {
                    // Avanzar al siguiente tamaño
                    indiceNivel5Actual++;
                    
                    // Si llegamos al final de los tamaños, volver al primero
                    if (indiceNivel5Actual >= tamañosTexto.Count)
                    {
                        indiceNivel5Actual = 0;
                    }
                    
                    Debug.Log("Nivel 5: Cambiando al tamaño " + (indiceNivel5Actual + 1) + 
                             " de " + tamañosTexto.Count + " (" + tamañosTexto[indiceNivel5Actual] + ")");
                }
                else if (nivelActual == 5) // Nivel 6: Mostrar sílabas progresivamente
                {
                    // Si aún quedan sílabas por mostrar, mostramos la siguiente
                    if (indiceNivel6Actual < silabasDePalabra.Count - 1)
                    {
                        indiceNivel6Actual++;
                        // Añadir la nueva sílaba a la lista de mostradas
                        silabasNivel6Mostradas.Add(silabasDePalabra[indiceNivel6Actual]);
                        
                        // Cambiar qué sílaba está resaltada
                        indiceNivel6Resaltado = indiceNivel6Actual; // Por defecto resaltamos la nueva
                        
                        Debug.Log("Nivel 6: Mostrando sílaba " + (indiceNivel6Actual + 1) + 
                                 " de " + silabasDePalabra.Count + " (" + silabasDePalabra[indiceNivel6Actual] + ")");
                    }
                    else if (indiceNivel6Actual >= silabasDePalabra.Count - 1)
                    {
                        // Ya se mostraron todas las sílabas, ahora rotamos cuál está resaltada
                        indiceNivel6Resaltado = (indiceNivel6Resaltado + 1) % silabasDePalabra.Count;
                        Debug.Log("Nivel 6: Resaltando sílaba " + (indiceNivel6Resaltado + 1) + 
                                 " de " + silabasDePalabra.Count + " (" + silabasDePalabra[indiceNivel6Resaltado] + ")");
                    }
                }

                // Actualizar el texto con el nuevo contador
                ActualizarTextoContador();

                // Si el contador llega a cero, finalizar la animación
                if (contadorPulsaciones == 0)
                {
                    // Llamar a la nueva función para cambiar el layer de la esfera
                    if (ultimoObjetoIluminado == null)
                    {
                        CambiarLayerObjeto();
                    }
                }
                else if (contadorPulsaciones < 0)
                {
                    // Para niveles 1-4, permitir finalizar sin verificar objetos en Outline
                    if (!ExistenObjetosEnLayerOutline())
                    {
                        Invoke("EndAnimation", 0);
                        isAnimating = false;
                    }
                    else
                    {
                        Debug.Log("No se puede finalizar la animación porque aún existen objetos en el layer Outline");
                    }
                }
            }
        }

        // Eliminar objetos con F en cualquier nivel
        if (Input.GetKeyDown(KeyCode.F))
        {
            EliminarObjetosEnLayerOutline();
        }
    }

    void ActualizarTextoNivel()
    {
        if (nivelTexto != null && GameData.Instance.nivelesDificultad.Count > 0)
        {
            nivelTexto.text = GameData.Instance.nivelesDificultad[nivelActual].nombre + "\n" +
                              GameData.Instance.nivelesDificultad[nivelActual].descripcion;
        }
    }

    private List<string> ObtenerSilabas(string palabra)
    {
        // Verificar si la palabra está en el diccionario de particiones
        if (GameData.Instance.particionesPredefinidas.ContainsKey(palabra))
        {
            return new List<string>(GameData.Instance.particionesPredefinidas[palabra]);
        }
        
        // Si no está en el diccionario, devolver una partición por cada carácter (fallback)
        List<string> silabas = new List<string>();
        foreach (char c in palabra)
        {
            silabas.Add(c.ToString());
        }
        return silabas;
    }

    private void ActualizarColorEmocion()
    {
        if (nivelActual == 6 && contadorPulsaciones > 0 && contadorPulsaciones <= GameData.Instance.coloresEmociones.Length)
        {
            // El índice es contadorPulsaciones - 1 porque contadorPulsaciones va de 5 a 1
            int indiceColor = GameData.Instance.coloresEmociones.Length - contadorPulsaciones;
            floatingText.color = GameData.Instance.coloresEmociones[indiceColor];
            
            // Actualizar también el texto para incluir la emoción
            Debug.Log("Cambiando color a " + GameData.Instance.nombresEmociones[indiceColor]);
        }
    }

    void CambiarNivel(int nuevoNivel)
    {
        if (nuevoNivel >= 0 && nuevoNivel < GameData.Instance.nivelesDificultad.Count)
        {
            nivelActual = nuevoNivel;
            ActualizarTextoNivel();
            Debug.Log("Nivel cambiado a: " + GameData.Instance.nivelesDificultad[nivelActual].nombre);
        }
    }

    void ActualizarTextoContador()
    {
        // Actualizar el texto completo basado en el contador según el nivel
        switch (nivelActual)
        {
            case 0: // Nivel 1: Una sílaba
            case 2: // Nivel 3: Variaciones de vocales
                fullText = textoBase;
                break;
            
            case 1: // Nivel 2: Repetir sílaba
                if (contadorPulsaciones > 1)
                    fullText = textoBase + " x" + contadorPulsaciones;
                else
                    fullText = textoBase;
                break;
            
            case 3: // Nivel 4: Secuencia de sílabas mezcladas
                if (contadorPulsaciones > 1)
                    fullText = textoBase + " x" + contadorPulsaciones;
                else
                    fullText = textoBase;
                break;
            
            case 4: // Nivel 5: Una sílaba con tamaños variables
                // Solo mostramos la sílaba, sin indicadores de tamaño
                fullText = textoBase;
                break;
            
            case 5: // Nivel 6: Todas las sílabas con una resaltada
                // No actualizamos fullText aquí, se manejará en ResaltarSilabaNivel6
                break;
                
            case 6: // Nivel 7: Palabra completa con emoción
                if (contadorPulsaciones > 0 && contadorPulsaciones <= GameData.Instance.coloresEmociones.Length)
                {
                    // El índice es contadorPulsaciones - 1 porque contadorPulsaciones va de 5 a 1
                    int indiceEmocion = GameData.Instance.coloresEmociones.Length - contadorPulsaciones;
                    fullText = textoBase + " (" + GameData.Instance.nombresEmociones[indiceEmocion] + ")";
                }
                else
                {
                    fullText = textoBase;
                }
                break;
        }

        // Si estamos en nivel 6, actualizar directamente el texto con formato
        if (nivelActual == 5)
        {
            ResaltarSilabaNivel6();
        }
        else if (!useTypewriterEffect)
        {
            floatingText.text = fullText;
            // Forzamos la actualización de la malla
            floatingText.ForceMeshUpdate();
        }
        else
        {
            // Si el efecto de escritura ya terminó, reiniciamos para el nuevo texto
            currentTypewriterProgress = 0f;
        }

        wobbleEffectInitialized = false;
    }

    void EndAnimation()
    {
        // Ocultar el texto
        floatingText.text = "";
        floatingText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Aplica efectos de ondulación al texto mostrado
    /// </summary>
    void ApplyWobbleEffect()
    {
        // Si no hay texto, no hacer nada
        if (string.IsNullOrEmpty(floatingText.text)) return;

        // Forzar la actualización de la malla del texto si no se ha inicializado aún
        if (!wobbleEffectInitialized)
        {
            floatingText.ForceMeshUpdate();
            wobbleEffectInitialized = true;
        }

        // Obtener la información del texto
        TMP_TextInfo textInfo = floatingText.textInfo;

        // Si no hay caracteres visibles, salir
        if (textInfo.characterCount == 0) return;

        // Arrays para almacenar los vértices originales y modificados
        Vector3[] vertices = textInfo.meshInfo[0].vertices;

        // Iteramos por cada carácter visible del texto
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            // Obtener la información del carácter
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Si el carácter no es visible, saltar al siguiente
            if (!charInfo.isVisible) continue;

            // Calculamos un offset ondulante basado en el tiempo y la posición del carácter
            // Cada carácter tendrá un offset ligeramente diferente para crear un efecto de onda
            float timeOffset = Time.time * wobbleSpeed + i * 0.1f;
            Vector3 offset = new Vector3(
                Mathf.Sin(timeOffset * 2.0f) * wobbleAmount,
                Mathf.Cos(timeOffset * 1.5f) * wobbleAmount,
                0
            );

            // Índices de los vértices para este carácter
            int vertexIndex = charInfo.vertexIndex;

            // Aplicar el offset a los 4 vértices del carácter
            vertices[vertexIndex] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        // Aplicar los cambios a la malla
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            floatingText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    // Nueva función para resaltar una sílaba específica en el nivel 6
    void ResaltarSilabaNivel6()
    {
        // Verificar si tenemos sílabas para mostrar
        if (silabasNivel6Mostradas.Count == 0) return;
        
        // Construir el texto con etiquetas de tamaño para resaltar la sílaba específica
        string textoConFormato = "";
        
        for (int i = 0; i <= indiceNivel6Actual && i < silabasDePalabra.Count; i++)
        {
            if (i > 0) textoConFormato += " ";
            
            // Si esta es la sílaba que debe ser resaltada, usar tamaño mayor
            if (i == indiceNivel6Resaltado)
            {
                float tamañoResaltado = tamañosTexto[1]; // Tamaño resaltado
                textoConFormato += "<size=" + Mathf.RoundToInt(tamañoResaltado * 100) + "%>" + silabasDePalabra[i] + "</size>";
            }
            else
            {
                // Usar tamaño normal
                textoConFormato += silabasDePalabra[i];
            }
        }
        
        // Aplicar el texto con formato directamente
        floatingText.text = textoConFormato;
        
        // Forzar actualización de la malla
        floatingText.ForceMeshUpdate();
        wobbleEffectInitialized = false;
    }

    // Función para verificar si existen objetos en el layer Outline
    bool ExistenObjetosEnLayerOutline()
    {
        // Verificar si hay objetos en el layer Outline
        int layerMask = 1 << outlineLayer;
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);
        return objectsInLayer.Length > 0;
    }

    // Nueva función para cambiar el layer de la esfera
    // Modificación para que la lista de objetos se actualice dinámicamente
    void CambiarLayerObjeto()
    {
        // Buscar objetos que coincidan con la palabra o sílaba según el nivel
        List<ObjetoInteractivo> objetosInteractivos = new List<ObjetoInteractivo>(
            FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None)
        );

        List<ObjetoInteractivo> objetosCoincidentes = new List<ObjetoInteractivo>();

        Debug.Log("Palabra seleccionada: " + palabraSeleccionada);
        Debug.Log("Texto base: " + textoBase);
        
        // Nivel 4: Mostrar palabras originales
        if (nivelActual == 3)
        {
            Debug.Log("Nivel 4 - Palabras originales: " + string.Join(", ", palabrasOriginalesMezcladas));
        }
        
        // Primero, intentar encontrar una coincidencia exacta con el nombre del objeto
        ObjetoInteractivo coincidenciaExacta = null;
        foreach (ObjetoInteractivo obj in objetosInteractivos)
        {
            if (string.Equals(obj.nombreObjeto, palabraSeleccionada, System.StringComparison.OrdinalIgnoreCase))
            {
                coincidenciaExacta = obj;
                Debug.Log("¡Coincidencia exacta encontrada! " + obj.nombreObjeto);
                break;
            }
        }
        
        // Si hay coincidencia exacta, usar ese objeto directamente
        if (coincidenciaExacta != null && nivelActual != 3) // No usar coincidencia exacta en nivel 4
        {
            
            coincidenciaExacta.AplicarOutline();
            ultimoObjetoIluminado = coincidenciaExacta;
            Debug.Log("Objeto exacto '" + coincidenciaExacta.nombreObjeto + "' iluminado");
            return;
        }

        // Si no hay coincidencia exacta, buscar según el nivel
        switch (nivelActual)
        {
            case 0: // Nivel 1: Coincidencia con la sílaba mostrada
            case 1: // Nivel 2: Coincidencia con la sílaba mostrada (repetida)
                foreach (ObjetoInteractivo obj in objetosInteractivos)
                {
                    Debug.Log("Verificando objeto: " + obj.nombreObjeto);
                    List<string> silabasObjeto = ObtenerSilabas(obj.nombreObjeto);
                    Debug.Log("Sílabas del objeto: " + string.Join(", ", silabasObjeto));
                    
                    // Si la sílaba mostrada está en las sílabas del objeto
                    bool encontrada = false;
                    foreach (string silaba in silabasObjeto)
                    {
                        if (string.Equals(silaba, textoBase, System.StringComparison.OrdinalIgnoreCase))
                        {
                            encontrada = true;
                            Debug.Log("Sílaba '" + textoBase + "' encontrada en objeto '" + obj.nombreObjeto + "'");
                            break;
                        }
                    }
                    
                    // Si encontramos la sílaba, agregar a coincidentes
                    if (encontrada)
                    {
                        objetosCoincidentes.Add(obj);
                    }
                }
                break;
                
            case 2: // Nivel 3: Coincidencia con las variaciones de vocales
                // Extraer la consonante principal del ejercicio
                string consonantePrincipal = "";
                if (textoBase.Length > 0 && !EsVocal(textoBase[0]))
                {
                    consonantePrincipal = textoBase[0].ToString();
                }
                
                foreach (ObjetoInteractivo obj in objetosInteractivos)
                {
                    string nombreObjetoLower = obj.nombreObjeto.ToLower();
                    List<string> silabasObjeto = ObtenerSilabas(obj.nombreObjeto);
                    
                    // Buscar sílabas que empiecen con la misma consonante
                    bool coincide = false;
                    foreach (string silaba in silabasObjeto)
                    {
                        if (silaba.Length > 0 && silaba.StartsWith(consonantePrincipal))
                        {
                            coincide = true;
                            break;
                        }
                    }
                    
                    if (coincide)
                    {
                        objetosCoincidentes.Add(obj);
                    }
                }
                break;
                
            case 3: // Nivel 4: Buscar objetos que coincidan con las palabras originales completas
                Debug.Log("Nivel 4: Buscando objetos que coincidan con las palabras originales: " + string.Join(", ", palabrasOriginalesMezcladas));
                
                // Crear un diccionario para almacenar un objeto representante por cada palabra
                Dictionary<string, ObjetoInteractivo> objetosPorPalabra = new Dictionary<string, ObjetoInteractivo>();
                
                // Para cada palabra original, buscar un objeto que coincida
                foreach (string palabraOriginal in palabrasOriginalesMezcladas)
                {
                    string palabraClave = palabraOriginal.ToLower();
                    
                    // Si la palabra es una poción con formato "Poción de X", extraer solo el nombre X
                    string nombrePocion = palabraOriginal;
                    if (palabraOriginal.StartsWith("Poción de"))
                    {
                        nombrePocion = palabraOriginal.Substring(10).Trim();
                        palabraClave = nombrePocion.ToLower();
                    }
                    
                    // Si ya tenemos un objeto para esta palabra, saltar
                    if (objetosPorPalabra.ContainsKey(palabraClave))
                        continue;
                    
                    // Buscar un objeto que coincida con esta palabra
                    foreach (ObjetoInteractivo obj in objetosInteractivos)
                    {
                        bool coincide = false;
                        
                        // Verificar coincidencia exacta con el nombre del objeto
                        if (string.Equals(obj.nombreObjeto, palabraOriginal, System.StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(obj.nombreObjeto, nombrePocion, System.StringComparison.OrdinalIgnoreCase))
                        {
                            coincide = true;
                        }
                        
                        // Si encontramos coincidencia, guardar este objeto como representante de esta palabra
                        if (coincide)
                        {
                            objetosPorPalabra[palabraClave] = obj;
                            Debug.Log("Palabra '" + palabraOriginal + "' será representada por objeto '" + obj.nombreObjeto + "'");
                            break;
                        }
                    }
                }
                
                // Agregar los objetos representantes a la lista de coincidentes
                foreach (var kvp in objetosPorPalabra)
                {
                    objetosCoincidentes.Add(kvp.Value);
                }
                break;
                
            case 4: // Nivel 5: Una sílaba con tamaños variables
                if (silabasDePalabra.Count > 0)
                {
                    int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
                    textoBase = silabasDePalabra[indiceSilaba];
                    
                    // Generar tamaños variables con cambios más notables (entre 3 y 5)
                    GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
                    
                    // Establecer el contador inicial al número de tamaños generados
                    contadorPulsaciones = tamañosTexto.Count;
                    
                    // Iniciamos con el primer tamaño
                    indiceNivel5Actual = 0;
                }
                else
                {
                    textoBase = palabraSeleccionada.Substring(0, 1);
                    // Generar tamaños variables con cambios más notables (entre 3 y 5)
                    GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
                    
                    // Establecer el contador inicial al número de tamaños generados
                    contadorPulsaciones = tamañosTexto.Count;
                    
                    // Iniciamos con el primer tamaño
                    indiceNivel5Actual = 0;
                }
                break;
                
            case 5: // Nivel 6: Mostrar todas las sílabas con una resaltada
                if (silabasDePalabra.Count > 0)
                {
                    // Inicializar las variables para el nivel 6
                    indiceNivel6Actual = 0;
                    indiceNivel6Resaltado = 0;
                    silabasNivel6Mostradas.Clear();
                    
                    // Inicialmente solo mostramos la primera sílaba
                    silabasNivel6Mostradas.Add(silabasDePalabra[0]);
                    
                    // Generar un tamaño base y un tamaño mayor para la sílaba resaltada
                    tamañosTexto.Clear();
                    tamañosTexto.Add(3.0f);     // Tamaño base
                    tamañosTexto.Add(5.0f);     // Tamaño resaltado (notablemente mayor)
                    
                    // Establecemos un contador para que haya que presionar Q varias veces
                    contadorPulsaciones = silabasDePalabra.Count + 3; // Sílabas + algunas rotaciones adicionales
                    
                    // No necesitamos definir textoBase aquí, se construirá dinámicamente
                }
                else
                {
                    textoBase = palabraSeleccionada;
                    contadorPulsaciones = 1;
                }
                break;
                
            case 6: // Nivel 7: Palabra completa repetida con emociones
                textoBase = palabraSeleccionada;
                contadorPulsaciones = 5; // Fijamos en 5 para mostrar todas las emociones
                // Configurar el color inicial (primer color de emoción)
                ActualizarColorEmocion();
                break;
        }

        // Imprimir lista de objetos coincidentes para depuración
        Debug.Log("Objetos coincidentes encontrados: " + objetosCoincidentes.Count);
        foreach (ObjetoInteractivo obj in objetosCoincidentes)
        {
            Debug.Log("- " + obj.nombreObjeto);
        }

        // Si hay objetos coincidentes, iluminar según el nivel
        if (objetosCoincidentes.Count > 0)
        {
            // Para el nivel 4 (nivelActual = 3), iluminar todos los objetos coincidentes
            if (nivelActual == 3) // Nivel 4: Alternancia de sílabas
            {
                Debug.Log("Nivel 4: Iluminando todos los objetos coincidentes (" + objetosCoincidentes.Count + ")");
                
                // Crear un resumen de los objetos que se iluminarán
                string resumenObjetos = "OBJETOS SELECCIONADOS: ";
                List<string> nombresObjetos = new List<string>();
                
                foreach (ObjetoInteractivo objCoincidente in objetosCoincidentes)
                {
                    objCoincidente.AplicarOutline();
                    nombresObjetos.Add(objCoincidente.nombreObjeto);
                }
                
                resumenObjetos += string.Join(", ", nombresObjetos);
                Debug.Log(resumenObjetos);
                
                // Guardamos el primero que iluminamos como referencia
                if (objetosCoincidentes.Count > 0)
                {
                    ultimoObjetoIluminado = objetosCoincidentes[0];
                }
            }
            else // Para el resto de niveles, seguimos seleccionando uno aleatorio
            {
                int indiceAleatorio = Random.Range(0, objetosCoincidentes.Count);
                ObjetoInteractivo objetoSeleccionado = objetosCoincidentes[indiceAleatorio];

                // Para nivel 2, aplicamos parpadeo múltiple
                if (nivelActual == 1) // Nivel 2
                {
                    objetoSeleccionado.AplicarParpadeoMultiple(contadorPulsaciones, 0.5f);
                }
                else // Otros niveles
                {
                    objetoSeleccionado.AplicarOutline();
                }

                ultimoObjetoIluminado = objetoSeleccionado;
                Debug.Log("Objeto final seleccionado: '" + objetoSeleccionado.nombreObjeto + "' iluminado");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró ningún objeto que coincida con: " + textoBase);
            
            // Si no se encuentran objetos, finalizar la animación
            Invoke("EndAnimation", 0.5f);
            isAnimating = false;
            Debug.Log("Finalizando animación porque no se encontraron objetos coincidentes");
        }
    }

    // Función auxiliar para verificar si un carácter es vocal
    private bool EsVocal(char c)
    {
        string vocales = "aeiouáéíóúü";
        return vocales.Contains(c.ToString().ToLower());
    }

    // Reemplazar la función GenerarTamañosVariablesCrecientes con una nueva función para el nivel 5
    void GenerarTamañosProporcionalesNivel5(int cantidad)
    {
        tamañosTexto.Clear();
        
        // Tamaños base más diferenciados
        float tamañoInicial = 2.0f;
        float factorProporcional = 1.5f; // Cada tamaño será 1.5 veces más grande que el anterior
        
        // Crear la lista con tamaños proporcionalmente crecientes
        for (int i = 0; i < cantidad; i++)
        {
            float nuevoTamaño = tamañoInicial * Mathf.Pow(factorProporcional, i);
            tamañosTexto.Add(nuevoTamaño);
        }
        
        Debug.Log("Tamaños proporcionales generados para nivel 5: " + string.Join(", ", tamañosTexto));
    }

    void EliminarObjetosEnLayerOutline()
    {
        // Crear una máscara de layer para el layer Outline
        int layerMask = 1 << outlineLayer;

        // Buscar todos los objetos en la escena que tengan colliders y estén en el layer Outline
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);

        // Variable para contar cuántos objetos eliminamos
        int objetosEliminados = 0;

        // Recorrer todos los objetos encontrados y eliminarlos
        foreach (Collider collider in objectsInLayer)
        {
            // Obtener el GameObject al que pertenece el collider
            GameObject objToDestroy = collider.gameObject;

            // Guardar el nombre para el log
            string nombreObjeto = objToDestroy.name;

            // Eliminar el objeto
            Destroy(objToDestroy);
            objetosEliminados++;

            Debug.Log("Objeto eliminado: " + nombreObjeto);
        }

        // Log con información sobre cuántos objetos se eliminaron
        if (objetosEliminados > 0)
        {
            ultimoObjetoIluminado = null;
            Debug.Log("Se eliminaron " + objetosEliminados + " objetos del layer Outline");

            // Activar el sistema de partículas
            if (particleSystem2 != null)
            {
                particleSystem2.Play();
                Invoke("StopParticleSystem", 5f); // Detener las partículas después de 5 segundos
            }
        }
        else
        {
            Debug.Log("No se encontraron objetos en el layer Outline para eliminar");
        }
    }

    void StopParticleSystem()
    {
        if (particleSystem2 != null)
        {
            particleSystem2.Stop();
        }
    }
}