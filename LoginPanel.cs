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
    public Button loginBotonAdicional;  // Botón adicional que aparece cuando se encuentra un usuario

    [Header("Campos del Panel de Registro")]
    public TMP_InputField registroTIField;
    public TMP_InputField registroNombreField;
    public TMP_Dropdown registroSexoDropdown;
    public TMP_InputField registroEdadField;
    public Button registroAceptarBtn;
    public Button registroCancelarBtn;
    public Button registroBotonAdicional;  // Botón adicional que aparece cuando el registro es exitoso

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
        userManager = FindFirstObjectByType<UserManager>();

        if (userManager == null)
        {
            Debug.LogError("No se encontró UserManager en la escena");
        }
    }

    private void Start()
    {
        // Configurar listeners para los botones
        ConfigurarBotones();

        // Configurar opciones del dropdown de sexo si existe
        if (registroSexoDropdown != null)
        {
            ConfigurarDropdownSexo();
        }
        
        // Ocultar los botones adicionales inicialmente
        if (loginBotonAdicional != null)
        {
            loginBotonAdicional.gameObject.SetActive(false);
        }
        
        if (registroBotonAdicional != null)
        {
            registroBotonAdicional.gameObject.SetActive(false);
        }
    }

    // Configurar las opciones del dropdown de sexo
    private void ConfigurarDropdownSexo()
    {
        registroSexoDropdown.ClearOptions();
        registroSexoDropdown.AddOptions(new System.Collections.Generic.List<string> { "Masculino", "Femenino" });
        Debug.Log("Dropdown de sexo configurado con opciones: Masculino, Femenino");
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

        // Ocultar botones adicionales
        OcultarBotonesAdicionales();
        
        LimpiarCampos();
        MostrarMensaje("");
    }

    public void MostrarPanelLogin()
    {
        if (panelInicial != null) panelInicial.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(true);
        if (panelRegistro != null) panelRegistro.SetActive(false);

        // Ocultar botones adicionales
        OcultarBotonesAdicionales();
        
        LimpiarCampos();
        MostrarMensaje("");
    }

    public void MostrarPanelRegistro()
    {
        if (panelInicial != null) panelInicial.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegistro != null) panelRegistro.SetActive(true);

        // Ocultar botones adicionales
        OcultarBotonesAdicionales();
        
        LimpiarCampos();
        MostrarMensaje("");
    }

    public void OcultarTodosPaneles()
    {
        if (panelInicial != null) panelInicial.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegistro != null) panelRegistro.SetActive(false);
    }
    
    // Ocultar los botones adicionales
    private void OcultarBotonesAdicionales()
    {
        if (loginBotonAdicional != null)
        {
            loginBotonAdicional.gameObject.SetActive(false);
        }
        
        if (registroBotonAdicional != null)
        {
            registroBotonAdicional.gameObject.SetActive(false);
        }
        
        // Asegurarse de que los botones de aceptar estén visibles
        if (loginAceptarBtn != null)
        {
            loginAceptarBtn.gameObject.SetActive(true);
        }
        
        if (registroAceptarBtn != null)
        {
            registroAceptarBtn.gameObject.SetActive(true);
        }
    }

    // Métodos para validar las entradas
    private void ValidarLogin()
    {
        if (loginTIField == null || string.IsNullOrEmpty(loginTIField.text))
        {
            MostrarMensaje("Por favor, ingresa una TI válida");
            Debug.LogWarning("Campo TI vacío");
            return;
        }

        string ti = loginTIField.text.Trim();
        
        // Verificar que solo contenga números
        foreach (char c in ti)
        {
            if (!char.IsDigit(c))
            {
                MostrarMensaje("La TI debe contener solo números");
                Debug.LogWarning("La TI debe contener solo números");
                return;
            }
        }

        // Asignar el valor al campo en UserManager
        if (userManager != null)
        {
            userManager.userIDInput = loginTIField;
            bool usuarioEncontrado = userManager.VerificarUsuarioExiste(ti);
            
            if (usuarioEncontrado)
            {
                Debug.Log($"Usuario encontrado con TI: {ti}");
                
                // Ocultar botón aceptar y mostrar botón adicional antes de llamar a LoginUser
                if (loginAceptarBtn != null)
                {
                    loginAceptarBtn.gameObject.SetActive(false);
                }
                
                if (loginBotonAdicional != null)
                {
                    loginBotonAdicional.gameObject.SetActive(true);
                }
                
                // Iniciar sesión
                userManager.LoginUser();
            }
            else
            {
                MostrarMensaje("Usuario no encontrado");
                Debug.LogWarning($"No se encontró usuario con TI: {ti}");
                
                // Asegurar que el botón adicional esté oculto
                if (loginBotonAdicional != null)
                {
                    loginBotonAdicional.gameObject.SetActive(false);
                }
                
                // Asegurar que el botón aceptar esté visible
                if (loginAceptarBtn != null)
                {
                    loginAceptarBtn.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            MostrarMensaje("Error: No se encontró el gestor de usuarios");
            Debug.LogError("No se encontró el gestor de usuarios");
        }
    }

    private void ValidarRegistro()
    {
        // Validar TI (máximo 12 números)
        if (registroTIField == null || string.IsNullOrEmpty(registroTIField.text))
        {
            MostrarMensaje("Por favor, ingresa una TI válida");
            Debug.LogWarning("Campo TI vacío");
            return;
        }

        string ti = registroTIField.text.Trim();
        
        // Verificar longitud máxima
        if (ti.Length > 12)
        {
            MostrarMensaje("La TI debe tener máximo 12 números");
            Debug.LogWarning("TI excede el máximo de 12 dígitos");
            return;
        }
        
        // Verificar que solo contenga números
        foreach (char c in ti)
        {
            if (!char.IsDigit(c))
            {
                MostrarMensaje("La TI debe contener solo números");
                Debug.LogWarning("La TI debe contener solo números");
                return;
            }
        }

        // Validar nombre (solo texto)
        if (registroNombreField == null || string.IsNullOrEmpty(registroNombreField.text))
        {
            MostrarMensaje("Por favor, ingresa un nombre válido");
            Debug.LogWarning("Campo nombre vacío");
            return;
        }

        string nombre = registroNombreField.text.Trim();
        
        // Verificar que el nombre solo contenga letras y espacios
        foreach (char c in nombre)
        {
            if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
            {
                MostrarMensaje("El nombre debe contener solo letras");
                Debug.LogWarning("El nombre contiene caracteres no válidos");
                return;
            }
        }

        // Validar edad (entre 6 y 14)
        if (registroEdadField == null || string.IsNullOrEmpty(registroEdadField.text))
        {
            MostrarMensaje("Por favor, ingresa una edad válida");
            Debug.LogWarning("Campo edad vacío");
            return;
        }

        int edad;
        if (!int.TryParse(registroEdadField.text, out edad))
        {
            MostrarMensaje("La edad debe ser un número");
            Debug.LogWarning("La edad no es un número válido");
            return;
        }
        
        if (edad < 6 || edad > 14)
        {
            MostrarMensaje("La edad debe estar entre 6 y 14 años");
            Debug.LogWarning($"Edad fuera de rango permitido: {edad}");
            return;
        }

        // Asignar los valores a los campos en UserManager
        if (userManager != null)
        {
            // Verificar si el usuario ya existe
            if (userManager.VerificarUsuarioExiste(ti))
            {
                MostrarMensaje("Esta TI ya está registrada. Usa otra TI o inicia sesión.");
                Debug.LogWarning($"Intento de registro con TI existente: {ti}");
                return;
            }
            
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

            // Ocultar botón aceptar y mostrar botón adicional antes de registrar
            if (registroAceptarBtn != null)
            {
                registroAceptarBtn.gameObject.SetActive(false);
            }
            
            if (registroBotonAdicional != null)
            {
                registroBotonAdicional.gameObject.SetActive(true);
            }

            // Registrar el usuario
            Debug.Log($"Registrando nuevo usuario - TI: {ti}, Nombre: {nombre}, Sexo: {sexoSeleccionado}, Edad: {edad}");
            userManager.RegisterUser();
        }
        else
        {
            MostrarMensaje("Error: No se encontró el gestor de usuarios");
            Debug.LogError("No se encontró el gestor de usuarios");
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
        
        // Restablecer los botones al limpiar los campos
        RestablecerBotones();
    }

    // Restablecer los botones a su estado original
    private void RestablecerBotones()
    {
        // Mostrar botones de aceptar
        if (loginAceptarBtn != null)
        {
            loginAceptarBtn.gameObject.SetActive(true);
        }
        
        if (registroAceptarBtn != null)
        {
            registroAceptarBtn.gameObject.SetActive(true);
        }
        
        // Ocultar botones adicionales
        if (loginBotonAdicional != null)
        {
            loginBotonAdicional.gameObject.SetActive(false);
        }
        
        if (registroBotonAdicional != null)
        {
            registroBotonAdicional.gameObject.SetActive(false);
        }
    }
}