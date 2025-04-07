using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginPanel : MonoBehaviour
{
    [Header("Estructura de Paneles")]
    public GameObject panelInicial;   // Panel con opciones de Login/Registro
    public GameObject panelLogin;     // Panel de inicio de sesión (solo TI)
    public GameObject panelRegistro;  // Panel de registro completo
    
    [Header("Campos del Panel de Login")]
    public TMP_InputField loginTIField;
    public Button loginAceptarBtn;
    public Button loginCancelarBtn;
    
    [Header("Campos del Panel de Registro")]
    public TMP_InputField registroTIField;
    public TMP_InputField registroNombreField;
    public TMP_Dropdown registroSexoDropdown;
    public TMP_InputField registroEdadField;
    public Button registroAceptarBtn;
    public Button registroCancelarBtn;
    
    [Header("Botones del Panel Inicial")]
    public Button btnIniciarSesion;
    public Button btnRegistrarse;
    
    [Header("Mensajes")]
    public TextMeshProUGUI mensajeStatus;
    
    // Referencia al gestor de usuarios
    private UserManager userManager;
    
    private void Awake()
    {
        // Inicializar referencia al UserManager
        userManager = FindObjectOfType<UserManager>();
        
        if (userManager == null)
        {
            Debug.LogError("No se encontró UserManager en la escena");
        }
    }
    
    private void Start()
    {
        // Configurar listeners para los botones
        ConfigurarBotones();
        
        // Mostrar solo el panel inicial al comenzar
        MostrarSoloPanelInicial();
        
        // Configurar opciones del dropdown de sexo si existe
        if (registroSexoDropdown != null)
        {
            ConfigurarDropdownSexo();
        }
    }
    
    // Configurar las opciones del dropdown de sexo
    private void ConfigurarDropdownSexo()
    {
        registroSexoDropdown.ClearOptions();
        registroSexoDropdown.AddOptions(new System.Collections.Generic.List<string> { "Masculino", "Femenino" });
    }
    
    // Configurar eventos para los botones
    private void ConfigurarBotones()
    {
        // Botones del panel inicial
        if (btnIniciarSesion != null)
            btnIniciarSesion.onClick.AddListener(MostrarPanelLogin);
            
        if (btnRegistrarse != null)
            btnRegistrarse.onClick.AddListener(MostrarPanelRegistro);
            
        // Botones del panel de login
        if (loginAceptarBtn != null)
            loginAceptarBtn.onClick.AddListener(ValidarLogin);
            
        if (loginCancelarBtn != null)
            loginCancelarBtn.onClick.AddListener(MostrarSoloPanelInicial);
            
        // Botones del panel de registro
        if (registroAceptarBtn != null)
            registroAceptarBtn.onClick.AddListener(ValidarRegistro);
            
        if (registroCancelarBtn != null)
            registroCancelarBtn.onClick.AddListener(MostrarSoloPanelInicial);
    }
    
    // Métodos para controlar la visibilidad de los paneles
    public void MostrarSoloPanelInicial()
    {
        if (panelInicial != null) panelInicial.SetActive(true);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegistro != null) panelRegistro.SetActive(false);
        
        LimpiarCampos();
        MostrarMensaje("");
    }
    
    public void MostrarPanelLogin()
    {
        if (panelInicial != null) panelInicial.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(true);
        if (panelRegistro != null) panelRegistro.SetActive(false);
        
        LimpiarCampos();
        MostrarMensaje("");
    }
    
    public void MostrarPanelRegistro()
    {
        if (panelInicial != null) panelInicial.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegistro != null) panelRegistro.SetActive(true);
        
        LimpiarCampos();
        MostrarMensaje("");
    }
    
    public void OcultarTodosPaneles()
    {
        if (panelInicial != null) panelInicial.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegistro != null) panelRegistro.SetActive(false);
    }
    
    // Métodos para validar las entradas
    private void ValidarLogin()
    {
        if (loginTIField == null || string.IsNullOrEmpty(loginTIField.text))
        {
            MostrarMensaje("Por favor, ingresa una TI válida");
            return;
        }
        
        // Asignar el valor al campo en UserManager
        if (userManager != null)
        {
            userManager.userIDInput = loginTIField;
            userManager.LoginUser();
        }
        else
        {
            MostrarMensaje("Error: No se encontró el gestor de usuarios");
        }
    }
    
    private void ValidarRegistro()
    {
        // Validar TI
        if (registroTIField == null || string.IsNullOrEmpty(registroTIField.text))
        {
            MostrarMensaje("Por favor, ingresa una TI válida");
            return;
        }
        
        // Validar nombre
        if (registroNombreField == null || string.IsNullOrEmpty(registroNombreField.text))
        {
            MostrarMensaje("Por favor, ingresa un nombre válido");
            return;
        }
        
        // Validar edad
        if (registroEdadField == null || string.IsNullOrEmpty(registroEdadField.text))
        {
            MostrarMensaje("Por favor, ingresa una edad válida");
            return;
        }
        
        int edad;
        if (!int.TryParse(registroEdadField.text, out edad) || edad <= 0 || edad > 120)
        {
            MostrarMensaje("La edad debe ser un número entre 1 y 120");
            return;
        }
        
        // Asignar los valores a los campos en UserManager
        if (userManager != null)
        {
            userManager.userIDInput = registroTIField;
            userManager.userNameInput = registroNombreField;
            
            // Obtener el sexo del dropdown
            string sexoSeleccionado = "Masculino";
            if (registroSexoDropdown != null)
            {
                sexoSeleccionado = registroSexoDropdown.options[registroSexoDropdown.value].text;
            }
            
            // Crear un InputField temporal para el sexo si es necesario
            if (userManager.sexoInput == null)
            {
                GameObject tempObject = new GameObject("TempSexoInput");
                tempObject.transform.SetParent(transform);
                TMP_InputField tempInput = tempObject.AddComponent<TMP_InputField>();
                tempInput.text = sexoSeleccionado;
                userManager.sexoInput = tempInput;
            }
            else
            {
                userManager.sexoInput.text = sexoSeleccionado;
            }
            
            // Asignar el campo de edad
            userManager.edadInput = registroEdadField;
            
            // Registrar el usuario
            userManager.RegisterUser();
            
            // Limpiar el InputField temporal si lo creamos
            if (tempObject != null)
            {
                Destroy(tempObject);
            }
        }
        else
        {
            MostrarMensaje("Error: No se encontró el gestor de usuarios");
        }
    }
    
    // Mostrar mensaje de estado
    private void MostrarMensaje(string mensaje)
    {
        if (mensajeStatus != null)
        {
            mensajeStatus.text = mensaje;
        }
    }
    
    // Limpiar todos los campos de entrada
    private void LimpiarCampos()
    {
        if (loginTIField != null)
            loginTIField.text = "";
            
        if (registroTIField != null)
            registroTIField.text = "";
            
        if (registroNombreField != null)
            registroNombreField.text = "";
            
        if (registroEdadField != null)
            registroEdadField.text = "";
            
        if (registroSexoDropdown != null)
            registroSexoDropdown.value = 0;
    }
} 