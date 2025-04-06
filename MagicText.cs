using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MagicText : MonoBehaviour
{
    // Referencia al texto que aparecerб
    public TextMeshPro floatingText;

    // Referencia al jugador para que el texto lo mire
    public Transform playerTransform;

    // Altura final del texto en metros
    public float finalHeight = 2.0f;

    // Duraciуn de la animaciуn en segundos
    public float animationDuration = 2.0f;

    // Tamaсo inicial y final del texto
    public float initialTextSize = 0.1f;
    public float finalTextSize = 3.0f;

    // Parбmetros para el efecto de ondulaciуn
    public float wobbleSpeed = 1.0f;         // Velocidad de la ondulaciуn
    public float wobbleAmount = 0.05f;       // Amplitud de la ondulaciуn (reducida para ser mбs sutil)

    // Animaciуn de escritura
    public bool useTypewriterEffect = true;  // Activar/desactivar efecto de escritura
    public float typewriterSpeed = 20.0f;    // Caracteres por segundo
    private float currentTypewriterProgress = 0f;
    private string fullText = "";

    // Arrays para las categorнas de textos
    private string[] vegetales = { "Tomate", "Pimiento", "Pepino", "Zanahoria", "Repollo" };
    private string[] frutas = { "Sandía", "Fresa", "Naranja", "Banano", "Manzana" };
    private string[] pociones = { "Amor", "Moco", "Lágrima", "Vida", "Tierra", "Fuego", "Sombra" };

    // Diccionario con las particiones predefinidas para cada palabra
    private Dictionary<string, List<string>> particionesPredefinidas = new Dictionary<string, List<string>>()
    {
        // Vegetales
        { "Tomate", new List<string> { "To", "ma", "te" } },
        { "Pimiento", new List<string> { "Pi", "mi", "en", "to" } },
        { "Pepino", new List<string> { "Pe", "pi", "no" } },
        { "Zanahoria", new List<string> { "Za", "na", "ho", "ri" } },
        { "Repollo", new List<string> { "Re", "po", "llo" } },
        
        // Frutas
        { "Sandía", new List<string> { "Sa", "dí" } },
        { "Fresa", new List<string> { "Fre", "sa" } },
        { "Naranja", new List<string> { "Na", "ra", "ja" } },
        { "Banano", new List<string> { "Ba", "na", "no" } },
        { "Manzana", new List<string> { "Ma", "za", "na" } },
        
        // Pociones
        { "Amor", new List<string> { "mo" } },
        { "Moco", new List<string> { "Mo", "co" } },
        { "Lágrima", new List<string> { "Lá", "gri", "ma" } },
        { "Vida", new List<string> { "Vi", "da" } },
        { "Tierra", new List<string> { "Ti", "rra" } },
        { "Fuego", new List<string> { "Fu", "go" } },
        { "Sombra", new List<string> { "So", "bra"} },
    };

    // Niveles de dificultad y ejemplos para cada nivel
    [System.Serializable]
    public class NivelDificultad
    {
        public string nombre;
        public string descripcion;
        public float velocidadHabla = 1.0f; // Multiplicador de velocidad
        public List<string> ejemplos = new List<string>();
    }

    // Función para obtener la primera sílaba de una palabra
    private string ObtenerPrimeraSilaba(string palabra)
    {
        // Algoritmo simple: busca consonante+vocal o solo vocal al inicio
        string palabraLimpia = palabra.Trim().ToLower();
        if (palabraLimpia.Length == 0) return "";

        // Si comienza con vocal, esa es la primera sílaba
        char primeraLetra = palabraLimpia[0];
        if ("aeiouáéíóú".Contains(primeraLetra.ToString()))
        {
            return primeraLetra.ToString();
        }

        // Si comienza con consonante, busca hasta la primera vocal
        for (int i = 1; i < palabraLimpia.Length; i++)
        {
            if ("aeiouáéíóú".Contains(palabraLimpia[i].ToString()))
            {
                // Retornamos consonante + vocal
                return palabraLimpia.Substring(0, i + 1);
            }
        }

        // Si no encontró vocales, devolver la primera letra
        return palabraLimpia[0].ToString();
    }


    public List<NivelDificultad> nivelesDificultad = new List<NivelDificultad>();
    public int nivelActual = 0;

    // Categorнa actual seleccionada
    private int categoriaActual = 0;  // 0 = vegetales, 1 = frutas, 2 = pociones

    // Variable para controlar si la animaciуn estб en curso
    private bool isAnimating = false;

    // Tiempo transcurrido de la animaciуn
    private float elapsedTime = 0f;

    // Posiciуn inicial del texto
    private Vector3 initialPosition;

    // Contador de pulsaciones necesarias
    private int contadorPulsaciones = 1;

    // Texto base (sin el contador)
    private string textoBase = "";

    // Variables para el efecto de ondulaciуn
    private bool wobbleEffectInitialized = false;

    // Layer al que cambiaremos la esfera (Outline)
    private int outlineLayer = 6;

    // Lista para almacenar todos los objetos interactivos
    private List<ObjetoInteractivo> objetosInteractivos = new List<ObjetoInteractivo>();

    // Variable para rastrear el ъltimo objeto que se cambiу a Outline
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

    // Colores para el nivel 7 (emociones)
    private Color[] coloresEmociones = new Color[] {
        new Color(1.0f, 0.0f, 0.0f),   // Rojo (rabia)
        new Color(0.0f, 0.8f, 0.0f),   // Verde (desagrado)
        new Color(0.0f, 0.0f, 1.0f),   // Azul (tristeza)
        new Color(1.0f, 1.0f, 0.0f),   // Amarillo (alegría)
        new Color(0.5f, 0.0f, 0.5f)    // Morado (miedo)
    };
    
    // Nombres de las emociones para mostrar junto con la palabra
    private string[] nombresEmociones = new string[] {
        "RABIA",
        "DESAGRADO",
        "TRISTEZA",
        "ALEGRÍA",
        "MIEDO"
    };

    void Start()
    {
        // Configuraciуn inicial del texto
        floatingText.gameObject.SetActive(false);

        // Guardamos la posiciуn inicial
        initialPosition = transform.position;

        // Encontrar y guardar todos los objetos interactivos en la escena
        ObjetoInteractivo[] objetosEnEscena = FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None);
        objetosInteractivos.AddRange(objetosEnEscena);

        Debug.Log("Se encontraron " + objetosInteractivos.Count + " objetos interactivos en la escena");

        // Inicializar los niveles de dificultad si estбn vacнos
        if (nivelesDificultad.Count == 0)
        {
            InicializarNivelesDificultad();
        }

        // Actualizar el texto de nivel en la UI (si existe)
        ActualizarTextoNivel();

        // Asegurarse de que el sistema de partículas esté pausado al inicio
        if (particleSystem2 != null)
        {
            particleSystem2.Stop();
        }
    }

    void InicializarNivelesDificultad()
    {
        // Nivel 1: Primera sílaba al azar
        NivelDificultad nivel1 = new NivelDificultad();
        nivel1.nombre = "Nivel 1: Sílaba inicial";
        nivel1.descripcion = "Pronunciar una sílaba de la palabra";
        nivel1.ejemplos.Clear();
        nivelesDificultad.Add(nivel1);

        // Nivel 2: Repetición de una sílaba varias veces
        NivelDificultad nivel2 = new NivelDificultad();
        nivel2.nombre = "Nivel 2: Repetición";
        nivel2.descripcion = "Repetir la sílaba varias veces";
        nivel2.ejemplos.Clear();
        nivelesDificultad.Add(nivel2);

        // Nivel 3: Variación de vocales en una sílaba
        NivelDificultad nivel3 = new NivelDificultad();
        nivel3.nombre = "Nivel 3: Variación de vocales";
        nivel3.descripcion = "Cambiar la vocal manteniendo la consonante";
        nivel3.ejemplos.Clear();
        nivelesDificultad.Add(nivel3);

        // Nivel 4: Alternancia de sílabas con diferentes consonantes
        NivelDificultad nivel4 = new NivelDificultad();
        nivel4.nombre = "Nivel 4: Diferentes consonantes";
        nivel4.descripcion = "Alternancia de sílabas de diferentes palabras";
        nivel4.ejemplos.Clear();
        nivelesDificultad.Add(nivel4);

        // Nivel 5: Repetición de una sílaba con tamaños variables
        NivelDificultad nivel5 = new NivelDificultad();
        nivel5.nombre = "Nivel 5: Sílaba con tamaños variables";
        nivel5.descripcion = "Repetir una sílaba con diferentes tamaños";
        nivel5.velocidadHabla = 0.8f;
        nivel5.ejemplos.Clear();
        nivelesDificultad.Add(nivel5);

        // Nivel 6: Mostrar todas las sílabas de una palabra con tamaños variables
        NivelDificultad nivel6 = new NivelDificultad();
        nivel6.nombre = "Nivel 6: Palabra completa con sílabas";
        nivel6.descripcion = "Mostrar todas las sílabas con diferentes tamaños";
        nivel6.velocidadHabla = 0.9f;
        nivel6.ejemplos.Clear();
        nivelesDificultad.Add(nivel6);

        // Nivel 7: Palabra completa repetida varias veces
        NivelDificultad nivel7 = new NivelDificultad();
        nivel7.nombre = "Nivel 7: Palabra completa";
        nivel7.descripcion = "Repetir la palabra completa";
        nivel7.velocidadHabla = 1.0f;
        nivel7.ejemplos.Clear();
        nivelesDificultad.Add(nivel7);
    }

    void Update()
    {
        // Cambiar nivel con las teclas numйricas 1-7
        if (Input.GetKeyDown(KeyCode.Alpha1)) CambiarNivel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CambiarNivel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CambiarNivel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CambiarNivel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) CambiarNivel(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) CambiarNivel(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) CambiarNivel(6);

        // Mostrar/ocultar panel de selecciуn de nivel con la tecla Tab
        if (Input.GetKeyDown(KeyCode.Tab) && panelSeleccionNivel != null)
        {
            panelSeleccionNivel.SetActive(!panelSeleccionNivel.activeSelf);
        }

        // Activar la animacion al presionar E
        if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
        {
            StartAnimation();
        }

        // Si la animaciуn estб en curso, actualizarla
        if (isAnimating)
        {
            UpdateAnimation();

            // Aplicar el efecto de escritura si estб activado
            if (useTypewriterEffect && currentTypewriterProgress < fullText.Length)
            {
                // Ajustar la velocidad segъn el nivel actual
                float velocidadAjustada = typewriterSpeed * nivelesDificultad[nivelActual].velocidadHabla;

                // Avanzar en el progreso del efecto de escritura
                currentTypewriterProgress += velocidadAjustada * Time.deltaTime;
                int charactersToShow = Mathf.Min(Mathf.FloorToInt(currentTypewriterProgress), fullText.Length);
                floatingText.text = fullText.Substring(0, charactersToShow);

                // Despuйs de actualizar el texto, forzamos la actualizaciуn de la malla
                floatingText.ForceMeshUpdate();
                wobbleEffectInitialized = false;
            }

            // Aplicar el efecto de ondulacion cuando el texto es visible
            if (floatingText.gameObject.activeInHierarchy)
            {
                ApplyWobbleEffect();
            }

            // Hacer que el texto mire hacia el jugador si tenemos referencia al jugador
            if (playerTransform != null && floatingText.gameObject.activeInHierarchy)
            {
                // Calculamos la direcciуn desde el texto hacia el jugador
                Vector3 directionToPlayer = playerTransform.position - floatingText.transform.position;

                // Si queremos que el texto mire solo horizontalmente al jugador (ignorando el eje Y)
                directionToPlayer.y = 0;

                // Solo rotamos si la direcciуn no es cero (evita errores)
                if (directionToPlayer != Vector3.zero)
                {
                    // Creamos una rotaciуn que mira en la direcciуn del jugador
                    Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer);

                    // Aplicamos la rotaciуn al texto
                    floatingText.transform.rotation = lookRotation;
                }
            }

            // Procesar la reducciуn del contador al presionar Q
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

    void CambiarNivel(int nuevoNivel)
    {
        if (nuevoNivel >= 0 && nuevoNivel < nivelesDificultad.Count)
        {
            nivelActual = nuevoNivel;
            ActualizarTextoNivel();
            Debug.Log("Nivel cambiado a: " + nivelesDificultad[nivelActual].nombre);
        }
    }

    void ActualizarTextoNivel()
    {
        if (nivelTexto != null && nivelesDificultad.Count > 0)
        {
            nivelTexto.text = nivelesDificultad[nivelActual].nombre + "\n" +
                              nivelesDificultad[nivelActual].descripcion;
        }
    }

    // Nueva funciуn para eliminar objetos en el layer Outline
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


    // Función modificada para verificar si existen objetos en el layer Outline
    bool ExistenObjetosEnLayerOutline()
    {
        // Verificar si hay objetos en el layer Outline
        int layerMask = 1 << outlineLayer;
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);
        return objectsInLayer.Length > 0;
    }

    // Nueva funciуn para cambiar el layer de la esfera
    // Modificaciуn para que la lista de objetos se actualice dinбmicamente
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
                objetoSeleccionado.AplicarOutline();
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

    void StartAnimation()
    {
        // Iniciar la animación
        isAnimating = true;
        elapsedTime = 0f;
        wobbleEffectInitialized = false;

        // Seleccionar aleatoriamente una palabra según la categoría
        SeleccionarPalabraAleatoria();
        
        // Procesar la palabra según el nivel actual
        ProcesarPalabraSegunNivel();
        
        // Si usamos efecto de escritura, comenzamos con el texto vacío
        if (useTypewriterEffect)
        {
            floatingText.text = "";
            currentTypewriterProgress = 0f;
        }
        else
        {
            // Si no, establecemos el texto completo inmediatamente
            floatingText.text = fullText;
        }

        // Para nivel 7, establecer el color inicial
        if (nivelActual == 6)
        {
            ActualizarColorEmocion();
        }
        else
        {
            // Para otros niveles, usar color blanco por defecto
            floatingText.color = Color.white;
        }

        // Restablecer la posición del texto
        floatingText.transform.position = transform.position;
        floatingText.fontSize = initialTextSize;

        // Activar el objeto de texto
        floatingText.gameObject.SetActive(true);

        // Forzamos la actualización de la malla después de activar el objeto
        floatingText.ForceMeshUpdate();
    }

    void SeleccionarPalabraAleatoria()
    {
        // Seleccionar aleatoriamente una categoría (0 = vegetales, 1 = frutas, 2 = pociones)
            categoriaActual = Random.Range(0, 3);
            int indiceAleatorio = 0;

            switch (categoriaActual)
            {
                case 0: // Vegetales
                    indiceAleatorio = Random.Range(0, vegetales.Length);
                palabraSeleccionada = vegetales[indiceAleatorio];
                    break;

                case 1: // Frutas
                    indiceAleatorio = Random.Range(0, frutas.Length);
                palabraSeleccionada = frutas[indiceAleatorio];
                    break;

                case 2: // Pociones
                    indiceAleatorio = Random.Range(0, pociones.Length);
                if (nivelActual < 6) // Para niveles 1-6 usamos solo el nombre de la poción
                    palabraSeleccionada = pociones[indiceAleatorio];
                else // Para nivel 7 usamos "Poción de X"
                    palabraSeleccionada = "Poción de " + pociones[indiceAleatorio];
                    break;
            }
        
        // Dividir la palabra en sílabas usando el diccionario de particiones
        silabasDePalabra = ObtenerSilabas(palabraSeleccionada);
        Debug.Log("Palabra seleccionada: " + palabraSeleccionada);
        Debug.Log("Sílabas: " + string.Join("-", silabasDePalabra));
    }

    void ProcesarPalabraSegunNivel()
    {
        switch (nivelActual)
        {
            case 0: // Nivel 1: Mostrar una sílaba al azar
                contadorPulsaciones = 1;
                if (silabasDePalabra.Count > 0)
                {
                    int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
                    textoBase = silabasDePalabra[indiceSilaba];
        }
        else
        {
                    textoBase = palabraSeleccionada.Substring(0, 1);
        }
                break;

            case 1: // Nivel 2: Repetir una sílaba varias veces
                contadorPulsaciones = Random.Range(2, 6); // Entre 2 y 5 veces
                if (silabasDePalabra.Count > 0)
        {
                    int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
                    textoBase = silabasDePalabra[indiceSilaba];
        }
        else
        {
                    textoBase = palabraSeleccionada.Substring(0, 1);
                }
                break;
                
            case 2: // Nivel 3: Variaciones de vocales en una sílaba
                if (silabasDePalabra.Count > 0)
                {
                    int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
                    string silabaBase = silabasDePalabra[indiceSilaba];
                    
                    // Generar variaciones cambiando la vocal
                    List<string> variaciones = GenerarVariacionesVocales(silabaBase);
                    
                    // Mezclar las variaciones
                    for (int i = 0; i < variaciones.Count; i++)
                    {
                        int indiceAleatorio = Random.Range(i, variaciones.Count);
                        string temp = variaciones[i];
                        variaciones[i] = variaciones[indiceAleatorio];
                        variaciones[indiceAleatorio] = temp;
                    }
                    
                    // Tomar solo 3-5 variaciones
                    int cantidadVariaciones = Random.Range(3, 6);
                    if (cantidadVariaciones > variaciones.Count)
                        cantidadVariaciones = variaciones.Count;
                        
                    variaciones = variaciones.GetRange(0, cantidadVariaciones);
                    
                    // Unir con guiones
                    textoBase = string.Join("-", variaciones);
                    contadorPulsaciones = 1;
                }
                else
                {
                    textoBase = palabraSeleccionada;
                    contadorPulsaciones = 1;
                }
                break;
                
            case 3: // Nivel 4: Sílabas de diferentes palabras
                GenerarSecuenciaSilabasMezcladas();
                contadorPulsaciones = Random.Range(3, 6); // Entre 3 y 5 veces
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

        // Preparar el texto completo
        PrepararTextoCompleto();
    }

    void GenerarSecuenciaSilabasMezcladas()
    {
        silabasMezcladas.Clear();
        palabrasOriginalesMezcladas.Clear();
        
        // Necesitamos seleccionar 3 palabras diferentes para mezclar sus sílabas
        List<string> palabrasSeleccionadas = new List<string>();
        palabrasSeleccionadas.Add(palabraSeleccionada); // Ya tenemos una palabra
        
        // Seleccionar otras dos palabras aleatorias de categorías aleatorias
        for (int i = 0; i < 2; i++)
        {
            int categoriaTmp = Random.Range(0, 3);
            string palabraTmp = "";
            
            switch (categoriaTmp)
            {
                case 0: // Vegetales
                    palabraTmp = vegetales[Random.Range(0, vegetales.Length)];
                    break;
                case 1: // Frutas
                    palabraTmp = frutas[Random.Range(0, frutas.Length)];
                    break;
                case 2: // Pociones
                    // Para pociones, aplicar el formato "Poción de X" para mejor coincidencia
                    string nombrePocion = pociones[Random.Range(0, pociones.Length)];
                    palabraTmp = nombrePocion;
                    break;
            }
            
            // Evitar palabras repetidas
            if (!palabrasSeleccionadas.Contains(palabraTmp))
                palabrasSeleccionadas.Add(palabraTmp);
            else
                i--; // Repetir la iteración
        }
        
        // Guardar las palabras originales completas
        palabrasOriginalesMezcladas = new List<string>(palabrasSeleccionadas);
        Debug.Log("Palabras originales para el nivel 4: " + string.Join(", ", palabrasOriginalesMezcladas));
        
        // Para cada palabra, obtener sus sílabas y seleccionar una al azar
        List<string> silabasSeleccionadas = new List<string>();
        
        foreach (string palabra in palabrasSeleccionadas)
        {
            List<string> silabas = ObtenerSilabas(palabra);
            if (silabas.Count > 0)
            {
                int indiceSilaba = Random.Range(0, silabas.Count);
                silabasSeleccionadas.Add(silabas[indiceSilaba]);
            }
            else
            {
                // Si no se pudo dividir en sílabas, usar la primera letra
                silabasSeleccionadas.Add(palabra.Substring(0, 1));
            }
        }
        
        // Mezclar el orden de las sílabas seleccionadas
        for (int i = 0; i < silabasSeleccionadas.Count; i++)
        {
            int indiceAleatorio = Random.Range(i, silabasSeleccionadas.Count);
            string temp = silabasSeleccionadas[i];
            silabasSeleccionadas[i] = silabasSeleccionadas[indiceAleatorio];
            silabasSeleccionadas[indiceAleatorio] = temp;
        }
        
        silabasMezcladas = silabasSeleccionadas;
        textoBase = string.Join("-", silabasMezcladas);
    }

    void GenerarTamañosVariables(int cantidad)
    {
        tamañosTexto.Clear();
        
        // Base de tamaños
        float[] basesTamaño = { 2.0f, 3.0f, 4.0f, 5.0f };
        
        // Modo de ordenación (0: creciente, 1: decreciente, 2: aleatorio)
        int modoOrden = Random.Range(0, 3);
        
        // Crear lista inicial
        List<float> tamaños = new List<float>();
        for (int i = 0; i < cantidad; i++)
        {
            if (i < basesTamaño.Length)
                tamaños.Add(basesTamaño[i]);
            else
                tamaños.Add(basesTamaño[Random.Range(0, basesTamaño.Length)]);
        }
        
        // Ordenar según el modo
        if (modoOrden == 0) // Creciente
        {
            tamaños.Sort();
        }
        else if (modoOrden == 1) // Decreciente
        {
            tamaños.Sort();
            tamaños.Reverse();
        }
        else // Aleatorio (ya está mezclado)
        {
            // Mezclar más
            for (int i = 0; i < tamaños.Count; i++)
            {
                int j = Random.Range(0, tamaños.Count);
                float temp = tamaños[i];
                tamaños[i] = tamaños[j];
                tamaños[j] = temp;
            }
        }
        
        tamañosTexto = tamaños;
    }

    void PrepararTextoCompleto()
    {
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
            
            case 5: // Nivel 6: Mostrar sílabas progresivamente con una resaltada
                // Construir el texto dinámicamente a partir de las sílabas mostradas
                if (silabasNivel6Mostradas.Count > 0)
                {
                    fullText = string.Join(" ", silabasNivel6Mostradas);
                }
                else
                {
                    fullText = "";
                }
                break;
                
            case 6: // Nivel 7: Palabra completa con emoción
                if (contadorPulsaciones > 0 && contadorPulsaciones <= coloresEmociones.Length)
                {
                    // El índice es contadorPulsaciones - 1 porque contadorPulsaciones va de 5 a 1
                    int indiceEmocion = coloresEmociones.Length - contadorPulsaciones;
                    fullText = textoBase + " (" + nombresEmociones[indiceEmocion] + ")";
                }
                else
                {
                    fullText = textoBase;
                }
                break;
        }
    }

    void UpdateAnimation()
    {
        // Actualizar el tiempo transcurrido
        elapsedTime += Time.deltaTime;

        // Calcular el progreso de la animación (0 a 1)
        float progress = Mathf.Clamp01(elapsedTime / animationDuration);

        // Calcular la nueva posición
        Vector3 newPosition = transform.position + Vector3.up * (finalHeight * progress);

        // Calcular el nuevo tamaño del texto según el nivel
        float newSize = initialTextSize;
        
        if (nivelActual == 4) // Nivel 5 con tamaños variables controlados por presionar Q
        {
            // Para el nivel 5, usamos el índice actual seleccionado manualmente
            if (tamañosTexto.Count > 0 && indiceNivel5Actual < tamañosTexto.Count)
            {
                newSize = tamañosTexto[indiceNivel5Actual];
            }
            else
            {
                newSize = finalTextSize;
            }
        }
        else if (nivelActual == 5) // Nivel 6 con sílabas progresivas y una resaltada
        {
            // El tamaño base es para todas las sílabas excepto la resaltada
            newSize = tamañosTexto[0]; // Tamaño base

            // Verificar si tenemos información de la sílaba resaltada
            if (indiceNivel6Resaltado >= 0 && indiceNivel6Resaltado <= indiceNivel6Actual && 
                indiceNivel6Actual < silabasDePalabra.Count)
            {
                // Necesitamos resaltar una sílaba específica
                // Esto no se puede hacer aquí, ya que TMPro no permite tamaños diferentes por carácter
                // En su lugar, implementaremos una solución más adelante
            }
        }
        else
        {
            // Para los demás niveles, interpolación normal
            newSize = Mathf.Lerp(initialTextSize, finalTextSize, progress);
        }

        // Aplicar los cambios
        floatingText.transform.position = newPosition;
        floatingText.fontSize = newSize;
        
        // Para nivel 6, necesitamos un enfoque especial para las sílabas de distintos tamaños
        if (nivelActual == 5 && floatingText.gameObject.activeInHierarchy)
        {
            ResaltarSilabaNivel6();
        }
    }

    void EndAnimation()
    {
        // Ocultar el texto
        floatingText.text = "";
        floatingText.gameObject.SetActive(false);
    }

    // Aplica el efecto de ondulaciуn al texto
    void ApplyWobbleEffect()
    {
        // Si no hay texto, no hacer nada
        if (string.IsNullOrEmpty(floatingText.text)) return;

        // Forzar la actualizaciуn de la malla del texto si no se ha inicializado aъn
        if (!wobbleEffectInitialized)
        {
            floatingText.ForceMeshUpdate();
            wobbleEffectInitialized = true;
        }

        // Obtener la informaciуn del texto
        TMP_TextInfo textInfo = floatingText.textInfo;

        // Si no hay caracteres visibles, salir
        if (textInfo.characterCount == 0) return;

        // Arrays para almacenar los vйrtices originales y modificados
        Vector3[] vertices = textInfo.meshInfo[0].vertices;

        // Iteramos por cada carбcter visible del texto
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            // Obtener la informaciуn del carбcter
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // Si el carбcter no es visible, saltar al siguiente
            if (!charInfo.isVisible) continue;

            // Calculamos un offset ondulante basado en el tiempo y la posiciуn del carбcter
            // Cada carбcter tendrб un offset ligeramente diferente para crear un efecto de onda
            float timeOffset = Time.time * wobbleSpeed + i * 0.1f;
            Vector3 offset = new Vector3(
                Mathf.Sin(timeOffset * 2.0f) * wobbleAmount,
                Mathf.Cos(timeOffset * 1.5f) * wobbleAmount,
                0
            );

            // Нndices de los vйrtices para este carбcter
            int vertexIndex = charInfo.vertexIndex;

            // Aplicar el offset a los 4 vйrtices del carбcter
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

    // Función para separar una palabra en sílabas según reglas básicas del español
    private List<string> SepararEnSilabas(string palabra)
    {
        List<string> silabas = new List<string>();
        string palabraLimpia = palabra.Trim().ToLower();
        
        if (string.IsNullOrEmpty(palabraLimpia)) return silabas;

        int indiceActual = 0;
        
        while (indiceActual < palabraLimpia.Length)
        {
            // Obtener la siguiente sílaba
            string silaba = ObtenerSiguienteSilaba(palabraLimpia, indiceActual);
            silabas.Add(silaba);
            indiceActual += silaba.Length;
        }
        
        return silabas;
    }

    // Función auxiliar para obtener la siguiente sílaba de una palabra
    private string ObtenerSiguienteSilaba(string palabra, int indiceInicio)
    {
        if (indiceInicio >= palabra.Length) return "";
        
        string vocales = "aeiouáéíóúü";
        
        // Si comienza con vocal
        if (vocales.Contains(palabra[indiceInicio].ToString()))
        {
            // V (una vocal sola forma sílaba)
            if (indiceInicio + 1 >= palabra.Length)
                return palabra[indiceInicio].ToString();
            
            // V-C
            if (indiceInicio + 1 < palabra.Length && !vocales.Contains(palabra[indiceInicio + 1].ToString()))
            {
                // V-C+
                if (indiceInicio + 2 < palabra.Length && !vocales.Contains(palabra[indiceInicio + 2].ToString()))
                    return palabra.Substring(indiceInicio, 1);
                else
                    return palabra.Substring(indiceInicio, 1);
            }
            
            // V-V (diptongo)
            if (indiceInicio + 1 < palabra.Length && vocales.Contains(palabra[indiceInicio + 1].ToString()))
            {
                // Comprobar si forman diptongo
                bool esDiptongo = EsDiptongo(palabra[indiceInicio], palabra[indiceInicio + 1]);
                if (esDiptongo)
                {
                    if (indiceInicio + 2 >= palabra.Length)
                        return palabra.Substring(indiceInicio, 2);
                    
                    if (!vocales.Contains(palabra[indiceInicio + 2].ToString()))
                        return palabra.Substring(indiceInicio, 2);
                    else
                        return palabra.Substring(indiceInicio, 2);
                }
                else
                {
                    return palabra.Substring(indiceInicio, 1);
                }
            }
            
            return palabra.Substring(indiceInicio, 1);
        }
        else // Comienza con consonante
        {
            // Buscar la primera vocal
            int indiceVocal = indiceInicio;
            while (indiceVocal < palabra.Length && !vocales.Contains(palabra[indiceVocal].ToString()))
            {
                indiceVocal++;
            }
            
            if (indiceVocal >= palabra.Length)
                return palabra.Substring(indiceInicio);
            
            // C-V
            if (indiceVocal == indiceInicio + 1)
            {
                // C-V+
                if (indiceVocal + 1 < palabra.Length)
                {
                    // C-V-C
                    if (!vocales.Contains(palabra[indiceVocal + 1].ToString()))
                    {
                        return palabra.Substring(indiceInicio, 2);
                    }
                    else // C-V-V
                    {
                        return palabra.Substring(indiceInicio, 2);
                    }
                }
                else
                {
                    return palabra.Substring(indiceInicio, 2);
                }
            }
            else // CC-V
            {
                // Grupo consonántico que no se separa (bl, br, cl, cr, dr, fl, fr, gl, gr, pl, pr, tr, tl)
                string grupoConsonantico = palabra.Substring(indiceVocal - 2, 2).ToLower();
                string gruposValidos = "blbrclcrdrflfrglgrplprtrst";
                
                if (indiceVocal >= indiceInicio + 2 && gruposValidos.Contains(grupoConsonantico))
                {
                    // Incluir el grupo consonántico entero con la vocal
                    return palabra.Substring(indiceVocal - 2, 3);
                }
                else
                {
                    // Separar la primera consonante
                    return palabra.Substring(indiceInicio, 1);
                }
            }
        }
    }

    // Función para determinar si dos vocales forman un diptongo
    private bool EsDiptongo(char vocal1, char vocal2)
    {
        string vocalesDebiles = "iuü";
        string vocalesFuertes = "aeoáéóú";
        
        // Dos vocales débiles forman diptongo
        if (vocalesDebiles.Contains(vocal1) && vocalesDebiles.Contains(vocal2))
            return true;
        
        // Una vocal débil átona + una vocal fuerte forman diptongo
        if ((vocalesDebiles.Contains(vocal1) && vocalesFuertes.Contains(vocal2)) ||
            (vocalesFuertes.Contains(vocal1) && vocalesDebiles.Contains(vocal2)))
            return true;
        
        return false;
    }

    // Función para cambiar la vocal de una sílaba manteniendo la consonante
    private string CambiarVocal(string silaba, char nuevaVocal)
    {
        string vocales = "aeiouáéíóú";
        string resultado = "";
        
        foreach (char c in silaba)
        {
            if (vocales.Contains(c.ToString().ToLower()))
                resultado += nuevaVocal;
            else
                resultado += c;
        }
        
        return resultado;
    }

    // Función para generar variaciones de una sílaba cambiando la vocal
    private List<string> GenerarVariacionesVocales(string silaba)
    {
        List<string> variaciones = new List<string>();
        string vocales = "aeiou";
        
        foreach (char vocal in vocales)
        {
            variaciones.Add(CambiarVocal(silaba, vocal));
        }
        
        return variaciones;
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
                if (contadorPulsaciones > 0 && contadorPulsaciones <= coloresEmociones.Length)
                {
                    // El índice es contadorPulsaciones - 1 porque contadorPulsaciones va de 5 a 1
                    int indiceEmocion = coloresEmociones.Length - contadorPulsaciones;
                    fullText = textoBase + " (" + nombresEmociones[indiceEmocion] + ")";
                    // Actualizar el color según la emoción
                    ActualizarColorEmocion();
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

    // Función para obtener las sílabas de una palabra usando el diccionario de particiones
    private List<string> ObtenerSilabas(string palabra)
    {
        // Verificar si la palabra está en el diccionario de particiones
        if (particionesPredefinidas.ContainsKey(palabra))
        {
            return new List<string>(particionesPredefinidas[palabra]);
        }
        
        // Si no está en el diccionario, devolver una partición por cada carácter (fallback)
        List<string> silabas = new List<string>();
        foreach (char c in palabra)
        {
            silabas.Add(c.ToString());
        }
        return silabas;
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

    // Función para actualizar el color según la emoción actual
    private void ActualizarColorEmocion()
    {
        if (nivelActual == 6 && contadorPulsaciones > 0 && contadorPulsaciones <= coloresEmociones.Length)
        {
            // El índice es contadorPulsaciones - 1 porque contadorPulsaciones va de 5 a 1
            int indiceColor = coloresEmociones.Length - contadorPulsaciones;
            floatingText.color = coloresEmociones[indiceColor];
            
            // Actualizar también el texto para incluir la emoción
            Debug.Log("Cambiando color a " + nombresEmociones[indiceColor]);
        }
    }
}