using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Linq;

public class MagicText : MonoBehaviour
{
    #region Configuración Principal
    [Header("Referencias")]
    public TextMeshPro floatingText;
    public Transform playerTransform;
    public ParticleSystem particleSystem;
    
    [Header("Apariencia de Texto")]
    public float finalHeight = 2.0f;
    public float animationDuration = 2.0f;
    public float initialTextSize = 0.1f;
    public float finalTextSize = 3.0f;
    
    [Header("Efectos de Texto")]
    public float wobbleSpeed = 1.0f;
    public float wobbleAmount = 0.05f;
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 20.0f;
    
    [Header("Interfaz")]
    public TextMeshProUGUI nivelTexto;
    public TextMeshProUGUI nivelSimpleTexto;
    public GameObject panelSeleccionNivel;
    public List<Button> botonesNivel = new List<Button>(); // Lista para los botones de nivel
    
    [Header("Audio Feedback")]
    public AudioSource audioSource; // Referencia al componente de audio
    public AudioClip successSound; // Sonido para aciertos
    public AudioClip errorSound; // Sonido para errores
    #endregion
    
    #region Constantes
    private const int OUTLINE_LAYER = 6;
    private const int MAX_NIVELES = 7;
    #endregion
    
    #region Diccionarios y Datos
    // Categorías de palabras
    private string[] vegetales = { "Tomate", "Pimiento", "Pepino", "Zanahoria", "Repollo" };
    private string[] frutas = { "Sandía", "Fresa", "Naranja", "Banano", "Manzana" };
    private string[] pociones = { "Amor", "Moco", "Lágrima", "Vida", "Tierra", "Fuego", "Sombra" };
    
    // Diccionario de sílabas predefinidas
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
    
    // Colores para el nivel 7 (emociones)
    private Color[] coloresEmociones = new Color[] {
        new Color(1.0f, 0.0f, 0.0f),   // Rojo (rabia)
        new Color(0.0f, 0.8f, 0.0f),   // Verde (desagrado)
        new Color(0.0f, 0.0f, 1.0f),   // Azul (tristeza)
        new Color(1.0f, 1.0f, 0.0f),   // Amarillo (alegría)
        new Color(0.5f, 0.0f, 0.5f)    // Morado (miedo)
    };
    
    // Nombres de las emociones
    private string[] nombresEmociones = new string[] {
        "RABIA",
        "DESAGRADO",
        "TRISTEZA",
        "ALEGRÍA",
        "MIEDO"
    };
    
    // Contadores de aciertos y errores por nivel
    private int[] contadorAciertos;
    private int[] contadorErrores;
    #endregion
    
    #region Estado del Juego
    // Niveles de dificultad
    public List<NivelDificultad> nivelesDificultad = new List<NivelDificultad>();
    public int nivelActual = 0;
    
    // Lista para almacenar todos los objetos interactivos
    private List<ObjetoInteractivo> objetosInteractivos = new List<ObjetoInteractivo>();
    private ObjetoInteractivo ultimoObjetoIluminado = null;
    
    // Categoría actual seleccionada
    private int categoriaActual = 0;  // 0 = vegetales, 1 = frutas, 2 = pociones
    
    // Variables de animación
    private bool isAnimating = false;
    private float elapsedTime = 0f;
    private Vector3 initialPosition;
    private bool wobbleEffectInitialized = false;
    
    // Estado del texto
    private int contadorPulsaciones = 1;
    private string textoBase = "";
    private string fullText = "";
    private float currentTypewriterProgress = 0f;
    
    // Estado de la palabra actual
    private string palabraSeleccionada = "";
    private List<string> silabasDePalabra = new List<string>();
    
    // Variables para niveles específicos
    private List<float> tamañosTexto = new List<float>();
    private List<string> silabasMezcladas = new List<string>();
    private List<string> palabrasOriginalesMezcladas = new List<string>();
    private int indiceNivel5Actual = 0;
    private int indiceNivel6Actual = 0;
    private int indiceNivel6Resaltado = 0;
    private List<string> silabasNivel6Mostradas = new List<string>();
    
    // Usuario actual
    private string usuarioActualID = "";
    private bool usuarioLogueado = false;
    
    // Variables para almacenar los controladores
    private InputDevice rightController;
    private InputDevice leftController;
    
    // Variables para rastrear estados previos
    private bool prevXButtonState = false;
    private bool prevYButtonState = false;
    private bool prevAButtonState = false;
    private bool prevBButtonState = false;
    private bool prevTriggerLeftState = false;
    private bool prevTriggerRightState = false;
    private bool prevGripLeftState = false;
    private bool prevGripRightState = false;
    
    // Variables para el cooldown de botones
    private bool xButtonCooldown = false;
    private bool yButtonCooldown = false;
    private bool triggerLeftCooldown = false;
    private bool gripLeftCooldown = false;
    private float xButtonCooldownTime = 0f;
    private float yButtonCooldownTime = 0f;
    private float triggerLeftCooldownTime = 0f;
    private float gripLeftCooldownTime = 0f;
    private float cooldownDuration = 1f; // Duración del cooldown en segundos
    #endregion
    
    #region Clases y Estructuras
    [System.Serializable]
    public class NivelDificultad
    {
        public string nombre;
        public string descripcion;
        public float velocidadHabla = 1.0f;
        public List<string> ejemplos = new List<string>();
    }
    #endregion
    
    #region Inicialización
    void Start()
    {
        // Configuración inicial del texto
        floatingText.gameObject.SetActive(false);
        initialPosition = transform.position;
        
        // Buscar objetos interactivos
        EncontrarObjetosInteractivos();
        
        // Inicializar niveles de dificultad
        if (nivelesDificultad.Count == 0)
        {
            InicializarNivelesDificultad();
        }
        
        // Inicializar contadores de aciertos y errores
        contadorAciertos = new int[MAX_NIVELES];
        contadorErrores = new int[MAX_NIVELES];
        
        // Actualizar UI
        ActualizarTextoNivel();
        
        // Inicializar sistema de partículas
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
        
        // Asignar eventos a los botones de nivel
        ConfigurarBotonesNivel();
        
        // En lugar de cargar automáticamente el último usuario, solo inicializamos las variables
        usuarioActualID = "";
        usuarioLogueado = false;
        Debug.Log("Iniciando juego sin usuario activo. Regístrese o inicie sesión para guardar estadísticas.");
        
        // En Start() o Awake(), busca los controladores
        BuscarControladores();
    }
    
    private void EncontrarObjetosInteractivos()
    {
        ObjetoInteractivo[] objetosEnEscena = FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None);
        objetosInteractivos.AddRange(objetosEnEscena);
        Debug.Log($"Se encontraron {objetosInteractivos.Count} objetos interactivos en la escena");
    }
    
    private void InicializarNivelesDificultad()
    {
        // Nivel 1: Primera sílaba al azar
        NivelDificultad nivel1 = new NivelDificultad
        {
            nombre = "Nivel 1: Sílaba inicial",
            descripcion = "Pronunciar una sílaba de la palabra",
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel1);
        
        // Nivel 2: Repetición de una sílaba varias veces
        NivelDificultad nivel2 = new NivelDificultad
        {
            nombre = "Nivel 2: Repetición",
            descripcion = "Repetir la sílaba varias veces",
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel2);
        
        // Nivel 3: Variación de vocales en una sílaba
        NivelDificultad nivel3 = new NivelDificultad
        {
            nombre = "Nivel 3: Variación de vocales",
            descripcion = "Cambiar la vocal manteniendo la consonante",
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel3);
        
        // Nivel 4: Alternancia de sílabas con diferentes consonantes
        NivelDificultad nivel4 = new NivelDificultad
        {
            nombre = "Nivel 4: Diferentes consonantes",
            descripcion = "Alternancia de sílabas de diferentes palabras",
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel4);
        
        // Nivel 5: Repetición de una sílaba con tamaños variables
        NivelDificultad nivel5 = new NivelDificultad
        {
            nombre = "Nivel 5: Sílaba con tamaños variables",
            descripcion = "Repetir una sílaba con diferentes tamaños",
            velocidadHabla = 0.8f,
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel5);
        
        // Nivel 6: Mostrar todas las sílabas de una palabra con tamaños variables
        NivelDificultad nivel6 = new NivelDificultad
        {
            nombre = "Nivel 6: Palabra completa con sílabas",
            descripcion = "Mostrar todas las sílabas con diferentes tamaños",
            velocidadHabla = 0.9f,
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel6);
        
        // Nivel 7: Palabra completa repetida varias veces
        NivelDificultad nivel7 = new NivelDificultad
        {
            nombre = "Nivel 7: Palabra completa",
            descripcion = "Repetir la palabra completa",
            velocidadHabla = 1.0f,
            ejemplos = new List<string>()
        };
        nivelesDificultad.Add(nivel7);
    }
    
    private void ConfigurarBotonesNivel()
    {
        // Verificar que tenemos botones asignados
        if (botonesNivel.Count == 0)
        {
            Debug.LogWarning("No hay botones de nivel asignados en el inspector.");
            return;
        }
        
        // Asignar la función para cambiar nivel a cada botón
        for (int i = 0; i < botonesNivel.Count; i++)
        {
            if (botonesNivel[i] != null)
            {
                int nivelIndex = i; // Guardar el índice en una variable local para usar en la lambda
                botonesNivel[i].onClick.AddListener(() => CambiarNivel(nivelIndex));
                Debug.Log($"Botón de nivel {i+1} configurado correctamente");
            }
            else
            {
                Debug.LogWarning($"El botón de nivel en el índice {i} es nulo");
            }
        }
    }
    
    private void CargarUsuarioActual()
    {
        // Intentar obtener el ID de usuario del sistema de usuario
        string usuarioID = UserManager.Instance?.GetUsuarioActualID();
        
        if (!string.IsNullOrEmpty(usuarioID))
        {
            EstablecerUsuarioActual(usuarioID);
        }
        else
        {
            Debug.Log("No hay usuario logueado actualmente");
        }
    }
    
    public void EstablecerUsuarioActual(string id)
    {
        usuarioActualID = id;
        usuarioLogueado = true;
        Debug.Log($"Usuario establecido: {usuarioActualID}");
        
        // Cargar estadísticas previas del usuario si existen
        CargarEstadisticasUsuario();
    }
    
    private void CargarEstadisticasUsuario()
    {
        if (!usuarioLogueado || string.IsNullOrEmpty(usuarioActualID))
            return;
            
        // Aquí podríamos cargar las estadísticas desde UserManager
        UserStatistics stats = UserManager.Instance?.GetUserStatistics(usuarioActualID);
        
        if (stats != null)
        {
            // Cargar contadores desde las estadísticas
            contadorAciertos = stats.aciertos;
            contadorErrores = stats.errores;
            Debug.Log("Estadísticas de usuario cargadas correctamente");
        }
        else
        {
            // Inicializar nuevos contadores
            contadorAciertos = new int[MAX_NIVELES];
            contadorErrores = new int[MAX_NIVELES];
            Debug.Log("No se encontraron estadísticas previas, se inicializaron nuevas");
        }
    }
    
    private void GuardarEstadisticasUsuario()
    {
        if (!usuarioLogueado || string.IsNullOrEmpty(usuarioActualID))
            return;
        
        UserManager.Instance?.UpdateUserStatistics(usuarioActualID, contadorAciertos, contadorErrores);
        Debug.Log("Estadísticas de usuario guardadas correctamente");
    }
    
    private void BuscarControladores()
    {
        Debug.Log("Buscando controladores VR...");
        List<InputDevice> dispositivos = new List<InputDevice>();
        
        // Intento 1: Buscar por características específicas
        dispositivos.Clear();
        InputDeviceCharacteristics caracteristicasDerecho = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;
        InputDevices.GetDevicesWithCharacteristics(caracteristicasDerecho, dispositivos);
        if (dispositivos.Count > 0)
        {
            rightController = dispositivos[0];
            //Debug.Log($"Controlador derecho encontrado (Método 1): {rightController.name}");
        }
        
        dispositivos.Clear();
        InputDeviceCharacteristics caracteristicasIzquierdo = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;
        InputDevices.GetDevicesWithCharacteristics(caracteristicasIzquierdo, dispositivos);
        if (dispositivos.Count > 0)
        {
            leftController = dispositivos[0];
            //Debug.Log($"Controlador izquierdo encontrado (Método 1): {leftController.name}");
        }
        
        // Intento 2: Si no funciona, intentar buscar por nombre
        if (!rightController.isValid || !leftController.isValid)
        {
            dispositivos.Clear();
            InputDevices.GetDevices(dispositivos);
            
            foreach (var device in dispositivos)
            {
                string nombre = device.name.ToLower();
                
                // Buscar términos como "right", "derecho", "oculus touch", etc.
                if (!rightController.isValid && (nombre.Contains("right") || nombre.Contains("derecho")))
                {
                    rightController = device;
                    //Debug.Log($"Controlador derecho encontrado (por nombre): {rightController.name}");
                }
                else if (!leftController.isValid && (nombre.Contains("left") || nombre.Contains("izquierdo")))
                {
                    leftController = device;
                    //Debug.Log($"Controlador izquierdo encontrado (por nombre): {leftController.name}");
                }
            }
        }
        
        // Verificación final
        //Debug.Log($"Estado final - Controlador derecho: {(rightController.isValid ? "VÁLIDO" : "NO VÁLIDO")}");
        //Debug.Log($"Estado final - Controlador izquierdo: {(leftController.isValid ? "VÁLIDO" : "NO VÁLIDO")}");
    }
    #endregion
    
    #region Actualización
    void Update()
    {
        ProcesarTeclasNivel();
        
        // Mostrar/ocultar panel de selección de nivel con Tab
        if (Input.GetKeyDown(KeyCode.Tab) && panelSeleccionNivel != null)
        {
            panelSeleccionNivel.SetActive(!panelSeleccionNivel.activeSelf);
        }
        
        // Activar animación con E
        if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
        {
            StartAnimation();
        }
        
        // Actualizar animación en curso
        if (isAnimating)
        {
            UpdateAnimation();
            AplicarEfectoEscritura();
            AplicarEfectoOndulacion();
            OrientarTextoHaciaJugador();
            
            // Procesar feedback del usuario
            ProcesarFeedbackUsuario();
        }
        
        // Eliminar objetos con F
        if (Input.GetKeyDown(KeyCode.F))
        {
            EliminarObjetosEnLayerOutline();
        }
        
        // Actualizar cooldowns de botones
        ActualizarCooldowns();
        
        // Para el controlador izquierdo
        
        if (leftController.isValid)
        {
            // Probar todos los botones posibles
            bool primaryButtonPressed = false;
            bool secondaryButtonPressed = false;
            bool triggerButtonPressed = false;
            bool gripButtonPressed = false;
                        
            // Gatillo -> Eliminar objetos (igual que KeyCode.F)
            if (leftController.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed) && 
                triggerButtonPressed && !prevTriggerLeftState && !triggerLeftCooldown)
            {
                EliminarObjetosEnLayerOutline();
                Debug.Log("¡GATILLO IZQUIERDO PRESIONADO! Eliminando objetos en layer outline");
                triggerLeftCooldown = true;
                triggerLeftCooldownTime = cooldownDuration;
            }
            
            // Grip -> Iniciar animación (igual que KeyCode.E)
            if (leftController.TryGetFeatureValue(CommonUsages.gripButton, out gripButtonPressed) && 
                gripButtonPressed && !prevGripLeftState && !gripLeftCooldown)
            {
                if (!isAnimating) {
                    StartAnimation();
                    Debug.Log("¡GRIP IZQUIERDO PRESIONADO! Iniciando animación");
                    gripLeftCooldown = true;
                    gripLeftCooldownTime = cooldownDuration;
                }
            }
            
            // Actualizar estados previos
            prevXButtonState = primaryButtonPressed;
            prevYButtonState = secondaryButtonPressed;
            prevTriggerLeftState = triggerButtonPressed;
            prevGripLeftState = gripButtonPressed;
        }
        
    }
    
    private void ProcesarTeclasNivel()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) CambiarNivel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CambiarNivel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CambiarNivel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CambiarNivel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) CambiarNivel(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) CambiarNivel(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) CambiarNivel(6);
    }
    
    private void ProcesarFeedbackUsuario()
    {
        // Verificar si hay objetos en layer OUTLINE_LAYER (6)
        if (ExistenObjetosEnLayerOutline())
        {
            // Si hay objetos en outline, deshabilitamos el registro de aciertos/errores
            //Debug.Log("Objeto en outline detectado. Feedback deshabilitado temporalmente.");
            return;
        }
        
        // Verificar que existe un usuario activo
        if (!usuarioLogueado || string.IsNullOrEmpty(usuarioActualID))
        {
            // Si no hay usuario logueado, solo registraremos en los contadores locales
            // Tecla Z: Error 
            if (Input.GetKeyDown(KeyCode.Z))
            {
                RegistrarError();
                Debug.LogWarning("Se registró un error, pero no se guardará en la base de datos porque no hay usuario activo.");
            }

            // Tecla X: Acierto
            if (Input.GetKeyDown(KeyCode.X))
            {
                RegistrarAcierto();
                Debug.LogWarning("Se registró un acierto, pero no se guardará en la base de datos porque no hay usuario activo.");
            }     

            // Para el controlador izquierdo

            if (leftController.isValid)
            {
                // Probar todos los botones posibles
                bool primaryButtonPressed = false;
                bool secondaryButtonPressed = false;
                
                // Botón X (primario) -> Registrar acierto (igual que KeyCode.X)
                if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed) && 
                    primaryButtonPressed && !prevXButtonState && !xButtonCooldown)
                {
                    RegistrarAcierto();
                    Debug.Log("¡BOTÓN X PRESIONADO! Se registró acierto desde controlador izquierdo");
                    xButtonCooldown = true;
                    xButtonCooldownTime = cooldownDuration;
                }
                
                // Botón Y (secundario) -> Registrar error (igual que KeyCode.Z)
                if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed) && 
                    secondaryButtonPressed && !prevYButtonState && !yButtonCooldown)
                {
                    RegistrarError();
                    Debug.Log("¡BOTÓN Y PRESIONADO! Se registró error desde controlador izquierdo");
                    yButtonCooldown = true;
                    yButtonCooldownTime = cooldownDuration;
                }
                
                // Actualizar estados previos
                prevXButtonState = primaryButtonPressed;
                prevYButtonState = secondaryButtonPressed;
            }      

            return;
        }
        
        // Verificar que UserManager esté disponible
        if (UserManager.Instance == null)
        {
            Debug.LogError("No se puede acceder a UserManager para guardar estadísticas. Asegúrate de que existe en la escena.");
            
            // Registrar localmente incluso si no podemos guardar
            if (Input.GetKeyDown(KeyCode.Z))
            {
                RegistrarError();
            }
            
            if (Input.GetKeyDown(KeyCode.X))
            {
                RegistrarAcierto();
            }
            
            return;
        }
        
        // Tenemos usuario activo y UserManager, podemos proceder con seguridad
        
        // Tecla Z: Error - Registrar error y actualizar en la base de datos
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Verificar que el nivel actual es válido
            if (nivelActual >= 0 && nivelActual < MAX_NIVELES)
            {
                // Incrementar contador de errores para este nivel
                contadorErrores[nivelActual]++;
                Debug.Log($"Error registrado en nivel {nivelActual + 1}. Total errores: {contadorErrores[nivelActual]}");
                
                // Guardar inmediatamente en la base de datos
                UserManager.Instance.UpdateUserStatistics(usuarioActualID, contadorAciertos, contadorErrores);
                Debug.Log($"Estadísticas actualizadas en la base de datos para usuario {usuarioActualID}");
                
                // Llamar a la función original de registro de error para el resto de la lógica
                RegistrarError();
            }
            else
            {
                Debug.LogError($"Nivel actual {nivelActual} fuera de rango válido (0-{MAX_NIVELES-1})");
            }
        }
        
        // Tecla X: Acierto - Registrar acierto y actualizar en la base de datos
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Verificar que el nivel actual es válido
            if (nivelActual >= 0 && nivelActual < MAX_NIVELES)
            {
                // Incrementar contador de aciertos para este nivel
                contadorAciertos[nivelActual]++;
                Debug.Log($"Acierto registrado en nivel {nivelActual + 1}. Total aciertos: {contadorAciertos[nivelActual]}");
                
                // Guardar inmediatamente en la base de datos
                UserManager.Instance.UpdateUserStatistics(usuarioActualID, contadorAciertos, contadorErrores);
                Debug.Log($"Estadísticas actualizadas en la base de datos para usuario {usuarioActualID}");
                
                // Llamar a la función original de registro de acierto para el resto de la lógica
                RegistrarAcierto();
            }
            else
            {
                Debug.LogError($"Nivel actual {nivelActual} fuera de rango válido (0-{MAX_NIVELES-1})");
            }
        }
    }
    
    private void RegistrarError()
    {
        // Reproducir sonido de error
        if (audioSource != null && errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
        
        // Reducir contador de pulsaciones
        contadorPulsaciones--;
        Debug.Log("Contador: " + contadorPulsaciones);
        
        // Procesar acción por nivel
        ProcesarAccionPorNivel();
        ActualizarTextoContador();
        
        // Verificar si finalizar animación
        VerificarFinalizacionAnimacion();
    }
    
    private void RegistrarAcierto()
    {
        // Reproducir sonido de acierto
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }
        
        // Reducir contador de pulsaciones
        contadorPulsaciones--;
        Debug.Log("Contador: " + contadorPulsaciones);
        
        // Procesar acción por nivel
        ProcesarAccionPorNivel();
        ActualizarTextoContador();
        
        // Verificar si finalizar animación
        VerificarFinalizacionAnimacion();
    }
    
    private void VerificarFinalizacionAnimacion()
    {
        // Si el contador llega a cero, cambiar layer del objeto
        if (contadorPulsaciones == 0)
        {
            if (ultimoObjetoIluminado == null)
            {
                CambiarLayerObjeto();
            }
        }
        // Si el contador es negativo, finalizar animación si no hay objetos en Outline
        else if (contadorPulsaciones < 0)
        {
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
    
    private void AplicarEfectoEscritura()
    {
        if (useTypewriterEffect && currentTypewriterProgress < fullText.Length)
        {
            // Ajustar velocidad según nivel
            float velocidadAjustada = typewriterSpeed * nivelesDificultad[nivelActual].velocidadHabla;
            
            // Avanzar progreso
            currentTypewriterProgress += velocidadAjustada * Time.deltaTime;
            int charactersToShow = Mathf.Min(Mathf.FloorToInt(currentTypewriterProgress), fullText.Length);
            floatingText.text = fullText.Substring(0, charactersToShow);
            
            // Forzar actualización de malla
            floatingText.ForceMeshUpdate();
            wobbleEffectInitialized = false;
        }
    }
    
    private void AplicarEfectoOndulacion()
    {
        if (floatingText.gameObject.activeInHierarchy)
        {
            ApplyWobbleEffect();
        }
    }
    
    private void OrientarTextoHaciaJugador()
    {
        if (playerTransform != null && floatingText.gameObject.activeInHierarchy)
        {
            // Calcular dirección hacia el jugador (solo horizontalmente)
            Vector3 directionToPlayer = playerTransform.position - floatingText.transform.position;
            directionToPlayer.y = 0;
            
            // Rotar si la dirección no es cero
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer);
                floatingText.transform.rotation = lookRotation;
            }
        }
    }
    
    private void ProcesarAccionPorNivel()
    {
        if (nivelActual == 4) // Nivel 5: Tamaños variables
        {
            // Avanzar al siguiente tamaño
            indiceNivel5Actual++;
            
            // Si llegamos al final, volver al primero
            if (indiceNivel5Actual >= tamañosTexto.Count)
            {
                indiceNivel5Actual = 0;
            }
            
            Debug.Log($"Nivel 5: Cambiando al tamaño {indiceNivel5Actual + 1} " +
                      $"de {tamañosTexto.Count} ({tamañosTexto[indiceNivel5Actual]})");
        }
        else if (nivelActual == 5) // Nivel 6: Mostrar sílabas progresivamente
        {
            ProcesarNivel6();
        }
    }
    
    private void ProcesarNivel6()
    {
        // Si aún quedan sílabas por mostrar
        if (indiceNivel6Actual < silabasDePalabra.Count - 1)
        {
            indiceNivel6Actual++;
            // Añadir nueva sílaba a la lista
            silabasNivel6Mostradas.Add(silabasDePalabra[indiceNivel6Actual]);
            
            // Resaltar la nueva sílaba
            indiceNivel6Resaltado = indiceNivel6Actual;
            
            Debug.Log($"Nivel 6: Mostrando sílaba {indiceNivel6Actual + 1} " +
                      $"de {silabasDePalabra.Count} ({silabasDePalabra[indiceNivel6Actual]})");
        }
        else if (indiceNivel6Actual >= silabasDePalabra.Count - 1)
        {
            // Rotar cuál sílaba está resaltada
            indiceNivel6Resaltado = (indiceNivel6Resaltado + 1) % silabasDePalabra.Count;
            Debug.Log($"Nivel 6: Resaltando sílaba {indiceNivel6Resaltado + 1} " +
                      $"de {silabasDePalabra.Count} ({silabasDePalabra[indiceNivel6Resaltado]})");
        }
    }
    #endregion

    #region Manejo de Niveles y UI
    private void CambiarNivel(int nuevoNivel)
    {
        if (nuevoNivel >= 0 && nuevoNivel < nivelesDificultad.Count)
        {
            nivelActual = nuevoNivel;
            ActualizarTextoNivel();
            Debug.Log($"Nivel cambiado a: {nivelesDificultad[nivelActual].nombre}");
        }
    }

    private void ActualizarTextoNivel()
    {
        if (nivelTexto != null && nivelesDificultad.Count > 0)
        {
            nivelTexto.text = $"{nivelesDificultad[nivelActual].nombre}\n" +
                              $"{nivelesDificultad[nivelActual].descripcion}";
        }
        
        // Actualizar texto simplificado del nivel
        if (nivelSimpleTexto != null)
        {
            nivelSimpleTexto.text = $"Nivel actual: {nivelActual + 1}";
        }
    }
    
    // Método para mostrar estadísticas actuales
    public string ObtenerEstadisticasActuales()
    {
        string estadisticas = "Estadísticas del usuario:\n";
        
        for (int i = 0; i < MAX_NIVELES && i < nivelesDificultad.Count; i++)
        {
            estadisticas += $"Nivel {i+1}: Aciertos {contadorAciertos[i]}, Errores {contadorErrores[i]}\n";
        }
        
        return estadisticas;
    }
    #endregion

    #region Interacción con Objetos
    private bool ExistenObjetosEnLayerOutline()
    {
        int layerMask = 1 << OUTLINE_LAYER;
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);
        return objectsInLayer.Length > 0;
    }

    private void EliminarObjetosEnLayerOutline()
    {
        // Crear máscara para el layer Outline
        int layerMask = 1 << OUTLINE_LAYER;
        
        // Buscar todos los objetos con colliders en el layer Outline
        Collider[] objectsInLayer = Physics.OverlapSphere(Vector3.zero, Mathf.Infinity, layerMask);
        
        // Contador de objetos eliminados
        int objetosEliminados = 0;
        
        // Eliminar los objetos encontrados
        foreach (Collider collider in objectsInLayer)
        {
            GameObject objToDestroy = collider.gameObject;
            string nombreObjeto = objToDestroy.name;
            
            Destroy(objToDestroy);
            objetosEliminados++;

            EndAnimation();
            isAnimating = false;
            
            Debug.Log($"Objeto eliminado: {nombreObjeto}");
        }
        
        // Registrar resultado
        if (objetosEliminados > 0)
        {
            ultimoObjetoIluminado = null;
            Debug.Log($"Se eliminaron {objetosEliminados} objetos del layer Outline");
            
            // Activar sistema de partículas
            if (particleSystem != null)
            {
                particleSystem.Play();
                Invoke("StopParticleSystem", 5f);
            }
            
            // Hacer desaparecer el texto flotante
            EndAnimation();
            isAnimating = false;
        }
        else
        {
            Debug.Log("No se encontraron objetos en el layer Outline para eliminar");
        }
    }

    private void StopParticleSystem()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }
    
    private void CambiarLayerObjeto()
    {
        // Actualizar lista de objetos interactivos
        List<ObjetoInteractivo> objetosActuales = new List<ObjetoInteractivo>(
            FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None)
        );
        
        List<ObjetoInteractivo> objetosCoincidentes = new List<ObjetoInteractivo>();
        
        Debug.Log($"Palabra seleccionada: {palabraSeleccionada}");
        Debug.Log($"Texto base: {textoBase}");
        
        // Para nivel 4, mostrar palabras originales
        if (nivelActual == 3)
        {
            Debug.Log($"Nivel 4 - Palabras originales: {string.Join(", ", palabrasOriginalesMezcladas)}");
        }
        
        // Intentar coincidencia exacta primero
        ObjetoInteractivo coincidenciaExacta = BuscarCoincidenciaExacta(objetosActuales);
        
        // Si hay coincidencia exacta y no estamos en nivel 4, usarla directamente
        if (coincidenciaExacta != null && nivelActual != 3)
        {
            coincidenciaExacta.AplicarOutline();
            ultimoObjetoIluminado = coincidenciaExacta;
            Debug.Log($"Objeto exacto '{coincidenciaExacta.nombreObjeto}' iluminado");
            return;
        }
        
        // Buscar según nivel
        switch (nivelActual)
        {
            case 0: // Nivel 1: Coincidencia con la sílaba mostrada
            case 1: // Nivel 2: Coincidencia con sílaba repetida
                objetosCoincidentes = BuscarObjetosPorSilaba(objetosActuales, textoBase);
                break;
                
            case 2: // Nivel 3: Coincidencia con variaciones de vocales
                objetosCoincidentes = BuscarObjetosPorConsonante(objetosActuales, textoBase);
                break;
                
            case 3: // Nivel 4: Coincidencia con palabras originales
                objetosCoincidentes = BuscarObjetosPorPalabrasOriginales(objetosActuales);
                break;
                
            case 4: // Nivel 5: Sílaba con tamaños variables
                PreparaNivel5();
                return; // La selección se maneja en PreparaNivel5
                
            case 5: // Nivel 6: Sílabas resaltadas
                PrepararNivel6();
                return; // La selección se maneja en PrepararNivel6
                
            case 6: // Nivel 7: Palabra completa con emoción
                textoBase = palabraSeleccionada;
                contadorPulsaciones = 5; // 5 emociones
                ActualizarColorEmocion();
                break;
        }
        
        // Mostrar objetos encontrados
        MostrarObjetosCoincidentes(objetosCoincidentes);
    }
    
    private ObjetoInteractivo BuscarCoincidenciaExacta(List<ObjetoInteractivo> objetosActuales)
    {
        // Si estamos en nivel 7 (índice 6) y trabajamos con una poción, extraer solo el nombre
        string nombreBusqueda = palabraSeleccionada;
        if (nivelActual == 6 && palabraSeleccionada.StartsWith("Poción de "))
        {
            nombreBusqueda = palabraSeleccionada.Substring(10).Trim(); // Extraer el nombre después de "Poción de "
            Debug.Log($"Nivel 7: Buscando poción con nombre: {nombreBusqueda} en lugar de {palabraSeleccionada}");
        }
        
        // Buscar coincidencia exacta con el nombre de búsqueda
        foreach (ObjetoInteractivo obj in objetosActuales)
        {
            if (string.Equals(obj.nombreObjeto, nombreBusqueda, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"¡Coincidencia exacta encontrada! {obj.nombreObjeto}");
                return obj;
            }
        }
        return null;
    }
    
    private List<ObjetoInteractivo> BuscarObjetosPorSilaba(List<ObjetoInteractivo> objetosActuales, string silaba)
    {
        List<ObjetoInteractivo> resultado = new List<ObjetoInteractivo>();
        
        foreach (ObjetoInteractivo obj in objetosActuales)
        {
            Debug.Log($"Verificando objeto: {obj.nombreObjeto}");
            List<string> silabasObjeto = ObtenerSilabas(obj.nombreObjeto);
            Debug.Log($"Sílabas del objeto: {string.Join(", ", silabasObjeto)}");
            
            // Verificar si la sílaba mostrada está en el objeto
            bool encontrada = false;
            foreach (string silabaObj in silabasObjeto)
            {
                if (string.Equals(silabaObj, silaba, System.StringComparison.OrdinalIgnoreCase))
                {
                    encontrada = true;
                    Debug.Log($"Sílaba '{silaba}' encontrada en objeto '{obj.nombreObjeto}'");
                    break;
                }
            }
            
            if (encontrada)
            {
                resultado.Add(obj);
            }
        }
        
        return resultado;
    }
    
    private List<ObjetoInteractivo> BuscarObjetosPorConsonante(List<ObjetoInteractivo> objetosActuales, string textoBase)
    {
        List<ObjetoInteractivo> resultado = new List<ObjetoInteractivo>();
        
        // Extraer consonante principal
        string consonantePrincipal = "";
        if (textoBase.Length > 0 && !EsVocal(textoBase[0]))
        {
            consonantePrincipal = textoBase[0].ToString();
        }
        
        foreach (ObjetoInteractivo obj in objetosActuales)
        {
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
                resultado.Add(obj);
            }
        }
        
        return resultado;
    }
    
    private List<ObjetoInteractivo> BuscarObjetosPorPalabrasOriginales(List<ObjetoInteractivo> objetosActuales)
    {
        List<ObjetoInteractivo> resultado = new List<ObjetoInteractivo>();
        Debug.Log($"Nivel 4: Buscando objetos que coincidan con palabras originales: {string.Join(", ", palabrasOriginalesMezcladas)}");
        
        // Diccionario para un objeto por palabra
        Dictionary<string, ObjetoInteractivo> objetosPorPalabra = new Dictionary<string, ObjetoInteractivo>();
        
        // Para cada palabra original
        foreach (string palabraOriginal in palabrasOriginalesMezcladas)
        {
            string palabraClave = palabraOriginal.ToLower();
            
            // Si es poción, extraer nombre
            string nombrePocion = palabraOriginal;
            if (palabraOriginal.StartsWith("Poción de"))
            {
                nombrePocion = palabraOriginal.Substring(10).Trim();
                palabraClave = nombrePocion.ToLower();
            }
            
            // Evitar duplicados
            if (objetosPorPalabra.ContainsKey(palabraClave))
                continue;
            
            // Buscar coincidencia
            foreach (ObjetoInteractivo obj in objetosActuales)
            {
                if (string.Equals(obj.nombreObjeto, palabraOriginal, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(obj.nombreObjeto, nombrePocion, System.StringComparison.OrdinalIgnoreCase))
                {
                    objetosPorPalabra[palabraClave] = obj;
                    Debug.Log($"Palabra '{palabraOriginal}' representada por objeto '{obj.nombreObjeto}'");
                    break;
                }
            }
        }
        
        // Agregar objetos a la lista
        foreach (var kvp in objetosPorPalabra)
        {
            resultado.Add(kvp.Value);
        }
        
        return resultado;
    }
    
    private void PreparaNivel5()
    {
        if (silabasDePalabra.Count > 0)
        {
            int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
            textoBase = silabasDePalabra[indiceSilaba];
            
            // Generar tamaños variables (3-5)
            GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
            
            // Contador igual al número de tamaños
            contadorPulsaciones = tamañosTexto.Count;
            
            // Iniciar con primer tamaño
            indiceNivel5Actual = 0;
        }
        else
        {
            textoBase = palabraSeleccionada.Substring(0, 1);
            GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
            contadorPulsaciones = tamañosTexto.Count;
            indiceNivel5Actual = 0;
        }
        
        // Buscar objetos coincidentes
        List<ObjetoInteractivo> objetosActuales = new List<ObjetoInteractivo>(
            FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None)
        );
        List<ObjetoInteractivo> objetosCoincidentes = BuscarObjetosPorSilaba(objetosActuales, textoBase);
        MostrarObjetosCoincidentes(objetosCoincidentes);
    }
    
    private void PrepararNivel6()
    {
        if (silabasDePalabra.Count > 0)
        {
            // Inicializar variables nivel 6
            indiceNivel6Actual = 0;
            indiceNivel6Resaltado = 0;
            silabasNivel6Mostradas.Clear();
            
            // Inicialmente solo primera sílaba
            silabasNivel6Mostradas.Add(silabasDePalabra[0]);
            
            // Tamaños para normal y resaltado
            tamañosTexto.Clear();
            tamañosTexto.Add(3.0f);  // Base
            tamañosTexto.Add(5.0f);  // Resaltado
            
            // Contador para varias pulsaciones
            contadorPulsaciones = silabasDePalabra.Count + 3;
        }
        else
        {
            textoBase = palabraSeleccionada;
            contadorPulsaciones = 1;
        }
        
        // Buscar objetos coincidentes
        List<ObjetoInteractivo> objetosActuales = new List<ObjetoInteractivo>(
            FindObjectsByType<ObjetoInteractivo>(FindObjectsSortMode.None)
        );
        
        // Para nivel 6, buscar coincidencia con palabra completa
        ObjetoInteractivo coincidenciaExacta = BuscarCoincidenciaExacta(objetosActuales);
        if (coincidenciaExacta != null)
        {
            coincidenciaExacta.AplicarOutline();
            ultimoObjetoIluminado = coincidenciaExacta;
            Debug.Log($"Objeto exacto '{coincidenciaExacta.nombreObjeto}' iluminado para nivel 6");
        }
        else
        {
            // Si no hay coincidencia exacta, buscar por primera sílaba
            List<ObjetoInteractivo> objetosCoincidentes = 
                BuscarObjetosPorSilaba(objetosActuales, silabasDePalabra[0]);
            MostrarObjetosCoincidentes(objetosCoincidentes);
        }
    }
    
    private void MostrarObjetosCoincidentes(List<ObjetoInteractivo> objetosCoincidentes)
    {
        Debug.Log($"Objetos coincidentes encontrados: {objetosCoincidentes.Count}");
        foreach (ObjetoInteractivo obj in objetosCoincidentes)
        {
            Debug.Log($"- {obj.nombreObjeto}");
        }
        
        // Iluminar según nivel
        if (objetosCoincidentes.Count > 0)
        {
            if (nivelActual == 3) // Nivel 4: iluminar todos
            {
                Debug.Log($"Nivel 4: Iluminando {objetosCoincidentes.Count} objetos coincidentes");
                
                string resumenObjetos = "OBJETOS SELECCIONADOS: ";
                List<string> nombresObjetos = new List<string>();
                
                foreach (ObjetoInteractivo obj in objetosCoincidentes)
                {
                    obj.AplicarOutline();
                    nombresObjetos.Add(obj.nombreObjeto);
                }
                
                resumenObjetos += string.Join(", ", nombresObjetos);
                Debug.Log(resumenObjetos);
                
                // Guardar primero como referencia
                if (objetosCoincidentes.Count > 0)
                {
                    ultimoObjetoIluminado = objetosCoincidentes[0];
                }
            }
            else // Seleccionar uno aleatorio
            {
                int indiceAleatorio = Random.Range(0, objetosCoincidentes.Count);
                ObjetoInteractivo objetoSeleccionado = objetosCoincidentes[indiceAleatorio];
                objetoSeleccionado.AplicarOutline();
                ultimoObjetoIluminado = objetoSeleccionado;
                Debug.Log($"Objeto final seleccionado: '{objetoSeleccionado.nombreObjeto}' iluminado");
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró ningún objeto que coincida con: {textoBase}");
            
            // Finalizar animación
            Invoke("EndAnimation", 0.5f);
            isAnimating = false;
            Debug.Log("Finalizando animación porque no se encontraron objetos coincidentes");
        }
    }
    #endregion

    #region Utilidades para Sílabas y Vocales
    private bool EsVocal(char c)
    {
        string vocales = "aeiouáéíóúü";
        return vocales.Contains(c.ToString().ToLower());
    }
    
    private List<string> ObtenerSilabas(string palabra)
    {
        // Verificar si está en diccionario
        if (particionesPredefinidas.ContainsKey(palabra))
        {
            return new List<string>(particionesPredefinidas[palabra]);
        }
        
        // Fallback: una partición por carácter
        List<string> silabas = new List<string>();
        foreach (char c in palabra)
        {
            silabas.Add(c.ToString());
        }
        return silabas;
    }
    
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
    
    private void GenerarTamañosProporcionalesNivel5(int cantidad)
    {
        tamañosTexto.Clear();
        
        float tamañoInicial = 2.0f;
        float factorProporcional = 1.5f; // Cada tamaño 1.5 veces mayor
        
        for (int i = 0; i < cantidad; i++)
        {
            float nuevoTamaño = tamañoInicial * Mathf.Pow(factorProporcional, i);
            tamañosTexto.Add(nuevoTamaño);
        }
        
        Debug.Log($"Tamaños proporcionales generados para nivel 5: {string.Join(", ", tamañosTexto)}");
    }
    #endregion

    #region Animación y Efectos
    private void StartAnimation()
    {
        // Inicializar animación
        isAnimating = true;
        elapsedTime = 0f;
        wobbleEffectInitialized = false;
        
        // Seleccionar palabra aleatoria
        SeleccionarPalabraAleatoria();
        
        // Procesar según nivel
        ProcesarPalabraSegunNivel();
        
        // Configurar texto inicial
        if (useTypewriterEffect)
        {
            floatingText.text = "";
            currentTypewriterProgress = 0f;
        }
        else
        {
            floatingText.text = fullText;
        }
        
        // Configurar color según nivel
        if (nivelActual == 6) // Nivel 7: Emociones
        {
            ActualizarColorEmocion();
        }
        else
        {
            floatingText.color = Color.white;
        }
        
        // Restablecer posición y tamaño
        floatingText.transform.position = transform.position;
        floatingText.fontSize = initialTextSize;
        
        // Activar objeto de texto
        floatingText.gameObject.SetActive(true);
        
        // Forzar actualización de malla
        floatingText.ForceMeshUpdate();
    }
    
    private void SeleccionarPalabraAleatoria()
    {
        // Seleccionar categoría al azar (0=vegetales, 1=frutas, 2=pociones)
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
                // Formato según nivel
                if (nivelActual < 6) // Niveles 1-6: solo nombre
                    palabraSeleccionada = pociones[indiceAleatorio];
                else // Nivel 7: "Poción de X"
                    palabraSeleccionada = "Poción de " + pociones[indiceAleatorio];
                break;
        }
        
        // Obtener sílabas
        silabasDePalabra = ObtenerSilabas(palabraSeleccionada);
        Debug.Log($"Palabra seleccionada: {palabraSeleccionada}");
        Debug.Log($"Sílabas: {string.Join("-", silabasDePalabra)}");
    }
    
    private void ProcesarPalabraSegunNivel()
    {
        switch (nivelActual)
        {
            case 0: // Nivel 1: Una sílaba al azar
                ConfigurarNivel1();
                break;
                
            case 1: // Nivel 2: Repetir sílaba
                ConfigurarNivel2();
                break;
                
            case 2: // Nivel 3: Variaciones de vocales
                ConfigurarNivel3();
                break;
                
            case 3: // Nivel 4: Sílabas de diferentes palabras
                GenerarSecuenciaSilabasMezcladas();
                contadorPulsaciones = Random.Range(3, 6); // 3-5 veces
                break;
                
            case 4: // Nivel 5: Sílaba con tamaños variables
                ConfigurarNivel5();
                break;
                
            case 5: // Nivel 6: Sílabas con resaltado
                ConfigurarNivel6();
                break;
                
            case 6: // Nivel 7: Palabra con emociones
                textoBase = palabraSeleccionada;
                contadorPulsaciones = 5; // 5 emociones
                ActualizarColorEmocion();
                break;
        }
        
        // Preparar texto
        PrepararTextoCompleto();
    }
    
    private void ConfigurarNivel1()
    {
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
    }
    
    private void ConfigurarNivel2()
    {
        contadorPulsaciones = Random.Range(2, 6); // 2-5 veces
        if (silabasDePalabra.Count > 0)
        {
            int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
            textoBase = silabasDePalabra[indiceSilaba];
        }
        else
        {
            textoBase = palabraSeleccionada.Substring(0, 1);
        }
    }
    
    private void ConfigurarNivel3()
    {
        if (silabasDePalabra.Count > 0)
        {
            int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
            string silabaBase = silabasDePalabra[indiceSilaba];
            
            // Generar variaciones vocales
            List<string> variaciones = GenerarVariacionesVocales(silabaBase);
            
            // Mezclar variaciones
            for (int i = 0; i < variaciones.Count; i++)
            {
                int indiceAleatorio = Random.Range(i, variaciones.Count);
                string temp = variaciones[i];
                variaciones[i] = variaciones[indiceAleatorio];
                variaciones[indiceAleatorio] = temp;
            }
            
            // Limitar a 3-5 variaciones
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
    }
    
    private void GenerarSecuenciaSilabasMezcladas()
    {
        silabasMezcladas.Clear();
        palabrasOriginalesMezcladas.Clear();
        
        // Crear listas temporales que contengan todos los elementos disponibles
        List<string> todasPalabras = new List<string>();
        todasPalabras.AddRange(vegetales);
        todasPalabras.AddRange(frutas);
        todasPalabras.AddRange(pociones);
        
        // Mezclar todas las palabras
        for (int i = 0; i < todasPalabras.Count; i++)
        {
            int j = Random.Range(i, todasPalabras.Count);
            string temp = todasPalabras[i];
            todasPalabras[i] = todasPalabras[j];
            todasPalabras[j] = temp;
        }
        
        // Seleccionar las primeras 3 palabras de la lista mezclada
        List<string> palabrasSeleccionadas = todasPalabras.Take(3).ToList();
        
        // Guardar palabras originales
        palabrasOriginalesMezcladas = new List<string>(palabrasSeleccionadas);
        Debug.Log($"Palabras originales para nivel 4: {string.Join(", ", palabrasOriginalesMezcladas)}");
        
        // Obtener una sílaba de cada palabra
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
                // Si no hay sílabas, usar primera letra
                silabasSeleccionadas.Add(palabra.Substring(0, 1));
            }
        }
        
        // Mezclar orden de las sílabas
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
    
    private void ConfigurarNivel5()
    {
        if (silabasDePalabra.Count > 0)
        {
            int indiceSilaba = Random.Range(0, silabasDePalabra.Count);
            textoBase = silabasDePalabra[indiceSilaba];
            
            // Tamaños variables (3-5)
            GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
            
            // Contador al número de tamaños
            contadorPulsaciones = tamañosTexto.Count;
            
            // Empezar con primer tamaño
            indiceNivel5Actual = 0;
        }
        else
        {
            textoBase = palabraSeleccionada.Substring(0, 1);
            GenerarTamañosProporcionalesNivel5(Random.Range(3, 6));
            contadorPulsaciones = tamañosTexto.Count;
            indiceNivel5Actual = 0;
        }
    }
    
    private void ConfigurarNivel6()
    {
        if (silabasDePalabra.Count > 0)
        {
            // Inicializar variables
            indiceNivel6Actual = 0;
            indiceNivel6Resaltado = 0;
            silabasNivel6Mostradas.Clear();
            
            // Inicialmente solo primera sílaba
            silabasNivel6Mostradas.Add(silabasDePalabra[0]);
            
            // Tamaños para normal/resaltado
            tamañosTexto.Clear();
            tamañosTexto.Add(3.0f);  // Base
            tamañosTexto.Add(5.0f);  // Resaltado
            
            // Contador para varias pulsaciones
            contadorPulsaciones = silabasDePalabra.Count + 3;
        }
        else
        {
            textoBase = palabraSeleccionada;
            contadorPulsaciones = 1;
        }
    }
    
    private void PrepararTextoCompleto()
    {
        switch (nivelActual)
        {
            case 0: // Nivel 1: Una sílaba
            case 2: // Nivel 3: Variaciones
                fullText = textoBase;
                break;
                
            case 1: // Nivel 2: Repetir sílaba
            case 3: // Nivel 4: Secuencia mezclada
                if (contadorPulsaciones > 1)
                    fullText = textoBase + " x" + contadorPulsaciones;
                else
                    fullText = textoBase;
                break;
                
            case 4: // Nivel 5: Sílaba con tamaños variables
                fullText = textoBase;
                break;
                
            case 5: // Nivel 6: Sílabas progresivas
                // Construir texto desde sílabas mostradas
                if (silabasNivel6Mostradas.Count > 0)
                {
                    fullText = string.Join(" ", silabasNivel6Mostradas);
                }
                else
                {
                    fullText = "";
                }
                break;
                
            case 6: // Nivel 7: Emoción
                if (contadorPulsaciones > 0 && contadorPulsaciones <= coloresEmociones.Length)
                {
                    // Índice va de 5 a 1, por lo que restamos
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
    
    private void UpdateAnimation()
    {
        // Actualizar tiempo
        elapsedTime += Time.deltaTime;
        
        // Calcular progreso (0-1)
        float progress = Mathf.Clamp01(elapsedTime / animationDuration);
        
        // Nueva posición ascendente
        Vector3 newPosition = transform.position + Vector3.up * (finalHeight * progress);
        
        // Nuevo tamaño según nivel
        float newSize = CalcularTamañoTextoSegunNivel(progress);
        
        // Aplicar cambios
        floatingText.transform.position = newPosition;
        floatingText.fontSize = newSize;
        
        // Para nivel 6, aplicar enfoque especial
        if (nivelActual == 5 && floatingText.gameObject.activeInHierarchy)
        {
            ResaltarSilabaNivel6();
        }
    }
    
    private float CalcularTamañoTextoSegunNivel(float progress)
    {
        if (nivelActual == 4) // Nivel 5: Tamaños variables
        {
            // Usar índice seleccionado manualmente
            if (tamañosTexto.Count > 0 && indiceNivel5Actual < tamañosTexto.Count)
            {
                return tamañosTexto[indiceNivel5Actual];
            }
            return finalTextSize;
        }
        else if (nivelActual == 5) // Nivel 6: Sílabas con tamaños
        {
            // Tamaño base
            return tamañosTexto[0];
        }
        else
        {
            // Para otros niveles, interpolación normal
            return Mathf.Lerp(initialTextSize, finalTextSize, progress);
        }
    }
    
    private void EndAnimation()
    {
        // Ocultar texto
        floatingText.text = "";
        floatingText.gameObject.SetActive(false);
    }
    
    private void ApplyWobbleEffect()
    {
        // Si no hay texto, salir
        if (string.IsNullOrEmpty(floatingText.text)) return;
        
        // Forzar actualización de malla si necesario
        if (!wobbleEffectInitialized)
        {
            floatingText.ForceMeshUpdate();
            wobbleEffectInitialized = true;
        }
        
        // Obtener información del texto
        TMP_TextInfo textInfo = floatingText.textInfo;
        
        // Si no hay caracteres visibles, salir
        if (textInfo.characterCount == 0) return;
        
        // Obtener vértices
        Vector3[] vertices = textInfo.meshInfo[0].vertices;
        
        // Procesar cada carácter visible
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            // Si no es visible, saltar
            if (!charInfo.isVisible) continue;
            
            // Calcular offset ondulante
            float timeOffset = Time.time * wobbleSpeed + i * 0.1f;
            Vector3 offset = new Vector3(
                Mathf.Sin(timeOffset * 2.0f) * wobbleAmount,
                Mathf.Cos(timeOffset * 1.5f) * wobbleAmount,
                0
            );
            
            // Obtener índice de vértices
            int vertexIndex = charInfo.vertexIndex;
            
            // Aplicar offset a los 4 vértices del carácter
            vertices[vertexIndex] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }
        
        // Aplicar cambios a la malla
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            floatingText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    
    private void ActualizarTextoContador()
    {
        // Actualizar texto según nivel
        switch (nivelActual)
        {
            case 0: // Nivel 1: Una sílaba
            case 2: // Nivel 3: Variaciones
                fullText = textoBase;
                break;
                
            case 1: // Nivel 2: Repetir sílaba
            case 3: // Nivel 4: Secuencia mezclada
                if (contadorPulsaciones > 1)
                    fullText = textoBase + " x" + contadorPulsaciones;
                else
                    fullText = textoBase;
                break;
                
            case 4: // Nivel 5: Tamaños variables
                fullText = textoBase;
                break;
                
            case 5: // Nivel 6: No se actualiza aquí
                break;
                
            case 6: // Nivel 7: Emoción
                if (contadorPulsaciones > 0 && contadorPulsaciones <= coloresEmociones.Length)
                {
                    int indiceEmocion = coloresEmociones.Length - contadorPulsaciones;
                    fullText = textoBase + " (" + nombresEmociones[indiceEmocion] + ")";
                    ActualizarColorEmocion();
                }
                else
                {
                    fullText = textoBase;
                }
                break;
        }
        
        // Aplicar texto según nivel y modo
        if (nivelActual == 5)
        {
            ResaltarSilabaNivel6();
        }
        else if (!useTypewriterEffect)
        {
            floatingText.text = fullText;
            floatingText.ForceMeshUpdate();
        }
        else
        {
            // Reiniciar efecto escritura
            currentTypewriterProgress = 0f;
        }
        
        wobbleEffectInitialized = false;
    }
    
    private void ResaltarSilabaNivel6()
    {
        // Verificar que haya sílabas
        if (silabasNivel6Mostradas.Count == 0) return;
        
        // Construir texto con etiquetas de tamaño
        string textoConFormato = "";
        
        for (int i = 0; i <= indiceNivel6Actual && i < silabasDePalabra.Count; i++)
        {
            if (i > 0) textoConFormato += " ";
            
            // Si es la resaltada, usar tamaño mayor
            if (i == indiceNivel6Resaltado)
            {
                float tamañoResaltado = tamañosTexto[1];
                textoConFormato += "<size=" + Mathf.RoundToInt(tamañoResaltado * 100) + "%>" + silabasDePalabra[i] + "</size>";
            }
            else
            {
                // Tamaño normal
                textoConFormato += silabasDePalabra[i];
            }
        }
        
        // Aplicar texto con formato
        floatingText.text = textoConFormato;
        
        // Forzar actualización
        floatingText.ForceMeshUpdate();
        wobbleEffectInitialized = false;
    }
    
    private void ActualizarColorEmocion()
    {
        if (nivelActual == 6 && contadorPulsaciones > 0 && contadorPulsaciones <= coloresEmociones.Length)
        {
            int indiceColor = coloresEmociones.Length - contadorPulsaciones;
            floatingText.color = coloresEmociones[indiceColor];
            Debug.Log($"Cambiando color a {nombresEmociones[indiceColor]}");
        }
    }
    #endregion

    #region Añadir detección de controladores
    void OnEnable()
    {
        InputDevices.deviceConnected += DeviceConnected;
        InputDevices.deviceDisconnected += DeviceDisconnected;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= DeviceConnected;
        InputDevices.deviceDisconnected -= DeviceDisconnected;
    }

    private void DeviceConnected(InputDevice device)
    {
        Debug.Log($"Dispositivo conectado: {device.name}");
        BuscarControladores(); // Actualizar referencias
    }

    private void DeviceDisconnected(InputDevice device)
    {
        Debug.Log($"Dispositivo desconectado: {device.name}");
        BuscarControladores(); // Actualizar referencias
    }
    #endregion

    private void ActualizarCooldowns()
    {
        // Actualizar cooldowns de botones
        if (xButtonCooldown)
        {
            xButtonCooldownTime -= Time.deltaTime;
            if (xButtonCooldownTime <= 0f)
            {
                xButtonCooldown = false;
                Debug.Log("Botón X disponible nuevamente");
            }
        }
        
        if (yButtonCooldown)
        {
            yButtonCooldownTime -= Time.deltaTime;
            if (yButtonCooldownTime <= 0f)
            {
                yButtonCooldown = false;
                Debug.Log("Botón Y disponible nuevamente");
            }
        }
        
        if (triggerLeftCooldown)
        {
            triggerLeftCooldownTime -= Time.deltaTime;
            if (triggerLeftCooldownTime <= 0f)
            {
                triggerLeftCooldown = false;
                Debug.Log("Gatillo izquierdo disponible nuevamente");
            }
        }
        
        if (gripLeftCooldown)
        {
            gripLeftCooldownTime -= Time.deltaTime;
            if (gripLeftCooldownTime <= 0f)
            {
                gripLeftCooldown = false;
                Debug.Log("Grip izquierdo disponible nuevamente");
            }
        }
    }
}