using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;

// Estructura para almacenar las estadísticas de un usuario
[System.Serializable]
public class UserStatistics
{
    public string userID; // Identificador del usuario (TI)
    public string userName; // Nombre del usuario
    public string sexo; // Sexo del usuario (Masculino/Femenino)
    public int edad; // Edad del usuario
    public int[] aciertos; // Contadores de aciertos por nivel
    public int[] errores; // Contadores de errores por nivel
    public DateTime lastSession; // Fecha de la última sesión

    public UserStatistics(string id, string name, string gender, int age, int nivelCount)
    {
        userID = id;
        userName = name;
        sexo = gender;
        edad = age;
        aciertos = new int[nivelCount];
        errores = new int[nivelCount];
        lastSession = DateTime.Now;
    }
}

// Clase para almacenar la base de datos de usuarios
[System.Serializable]
public class UserDatabase
{
    public List<UserStatistics> users = new List<UserStatistics>();
}

// Singleton que gestiona los usuarios y sus estadísticas
public class UserManager : MonoBehaviour
{
    // Singleton
    public static UserManager Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject loginPanel; // Panel de inicio de sesión (para compatibilidad)
    public LoginPanel loginPanelController; // Referencia al controlador de paneles de login
    public TMP_InputField userIDInput; // Campo para TI
    public TMP_InputField userNameInput; // Campo para nombre
    public TMP_InputField sexoInput; // Campo para sexo
    public TMP_InputField edadInput; // Campo para edad
    public Button loginButton; // Botón de inicio de sesión
    public Button registerButton; // Botón de registro
    public TextMeshProUGUI statusText; // Texto de estado/errores
    public TextMeshProUGUI userInfoText; // Texto para mostrar información del usuario activo

    [Header("Configuración")]
    public int nivelCount = 7; // Número de niveles para estadísticas
    public string databaseFileName = "user_database.json"; // Nombre del archivo de base de datos

    // Datos
    private UserDatabase userDatabase;
    private string databasePath;
    private string currentUserID = "";
    private UserStatistics currentUserStats;

    private void Awake()
    {
        // Configurar singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicializar ruta de la base de datos
        databasePath = Path.Combine(Application.persistentDataPath, databaseFileName);
        Debug.Log($"Base de datos ubicada en: {databasePath}");

        // Cargar la base de datos
        LoadDatabase();

        // Buscar controlador de paneles si no está asignado
        if (loginPanelController == null)
        {
            loginPanelController = FindObjectOfType<LoginPanel>();
        }
    }

    private void Start()
    {
        // Si hay usuario almacenado, cargar directamente
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("LastUserID")))
        {
            string lastUserID = PlayerPrefs.GetString("LastUserID");
            UserStatistics userStats = FindUser(lastUserID);
            
            if (userStats != null)
            {
                SetCurrentUser(userStats);
                
                // Ocultar paneles de login
                if (loginPanelController != null)
                {
                    loginPanelController.OcultarTodosPaneles();
                }
                else if (loginPanel != null)
                {
                    loginPanel.SetActive(false);
                }
                
                return;
            }
        }
        
        // Si no hay usuario almacenado o no se encontró, mostrar panel de login
        // El panel ya debería estar configurado correctamente en LoginPanel.Start()
    }

    // Cargar la base de datos desde el archivo JSON
    private void LoadDatabase()
    {
        if (File.Exists(databasePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(databasePath);
                userDatabase = JsonUtility.FromJson<UserDatabase>(jsonContent);
                Debug.Log($"Base de datos cargada: {userDatabase.users.Count} usuarios encontrados");

                // Crear un usuario de prueba si no hay usuarios en la base de datos
                if (userDatabase.users.Count == 0)
                {
                    CreateTestUser();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al cargar la base de datos: {e.Message}");
                userDatabase = new UserDatabase();
                CreateTestUser();
            }
        }
        else
        {
            Debug.Log("No se encontró base de datos existente. Creando nueva.");
            userDatabase = new UserDatabase();
            CreateTestUser();
            SaveDatabase();
        }
    }

    // Guardar la base de datos en el archivo JSON
    private void SaveDatabase()
    {
        try
        {
            string jsonContent = JsonUtility.ToJson(userDatabase, true);
            File.WriteAllText(databasePath, jsonContent);
            Debug.Log("Base de datos guardada correctamente");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar la base de datos: {e.Message}");
        }
    }

    // Crear un usuario de prueba para la base de datos
    private void CreateTestUser()
    {
        UserStatistics testUser = new UserStatistics("123456789", "Usuario de Prueba", "Masculino", 8, nivelCount);
        
        // Añadir algunos datos de ejemplo
        testUser.aciertos[0] = 5;
        testUser.errores[0] = 2;
        testUser.aciertos[1] = 3;
        testUser.errores[1] = 1;
        
        userDatabase.users.Add(testUser);
        Debug.Log("Usuario de prueba creado: ID 123456789, Nombre: Usuario de Prueba");
    }

    // Iniciar sesión de usuario
    public void LoginUser()
    {
        if (userIDInput == null || string.IsNullOrEmpty(userIDInput.text))
        {
            UpdateStatusText("Por favor, ingresa una TI válida.");
            return;
        }

        string userID = userIDInput.text.Trim();
        UserStatistics userStats = FindUser(userID);

        if (userStats != null)
        {
            // Usuario encontrado, iniciar sesión
            SetCurrentUser(userStats);
            UpdateStatusText("¡Bienvenido de nuevo, " + userStats.userName + "!");
            
            // Guardar el ID del usuario para futuras sesiones
            PlayerPrefs.SetString("LastUserID", userID);
            PlayerPrefs.Save();
            
            // Ocultar paneles según la estructura
            if (loginPanelController != null)
            {
                loginPanelController.OcultarTodosPaneles();
            }
            else if (loginPanel != null)
            {
                loginPanel.SetActive(false);
            }
            
            // Actualizar texto de información del usuario si existe
            if (userInfoText != null)
            {
                userInfoText.text = GetCurrentUserInfo();
            }
        }
        else
        {
            UpdateStatusText("Usuario no encontrado. Por favor, regístrate primero.");
        }
    }

    // Registrar nuevo usuario
    public void RegisterUser()
    {
        if (userIDInput == null || string.IsNullOrEmpty(userIDInput.text))
        {
            UpdateStatusText("Por favor, ingresa una TI válida.");
            return;
        }

        if (userNameInput == null || string.IsNullOrEmpty(userNameInput.text))
        {
            UpdateStatusText("Por favor, ingresa un nombre válido.");
            return;
        }
        
        if (sexoInput == null || string.IsNullOrEmpty(sexoInput.text))
        {
            UpdateStatusText("Por favor, ingresa el sexo (Masculino/Femenino).");
            return;
        }
        
        if (edadInput == null || string.IsNullOrEmpty(edadInput.text))
        {
            UpdateStatusText("Por favor, ingresa la edad.");
            return;
        }

        string userID = userIDInput.text.Trim();
        string userName = userNameInput.text.Trim();
        string sexo = sexoInput.text.Trim();
        
        // Validar y convertir edad
        int edad;
        if (!int.TryParse(edadInput.text.Trim(), out edad) || edad <= 0 || edad > 120)
        {
            UpdateStatusText("Por favor, ingresa una edad válida (1-120).");
            return;
        }

        // Verificar si el usuario ya existe
        if (FindUser(userID) != null)
        {
            UpdateStatusText("Esta TI ya está registrada. Inicia sesión.");
            return;
        }

        // Crear nuevo usuario
        UserStatistics newUser = new UserStatistics(userID, userName, sexo, edad, nivelCount);
        userDatabase.users.Add(newUser);
        SaveDatabase();

        // Establecer como usuario actual
        SetCurrentUser(newUser);
        UpdateStatusText("¡Registro exitoso! Bienvenido, " + userName);
        
        // Guardar el ID del usuario para futuras sesiones
        PlayerPrefs.SetString("LastUserID", userID);
        PlayerPrefs.Save();

        // Ocultar paneles según la estructura
        if (loginPanelController != null)
        {
            loginPanelController.OcultarTodosPaneles();
        }
        else if (loginPanel != null)
        {
            loginPanel.SetActive(false);
        }
        
        // Actualizar texto de información del usuario si existe
        if (userInfoText != null)
        {
            userInfoText.text = GetCurrentUserInfo();
        }
    }

    // Actualizar texto de estado en la UI
    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            Debug.Log("Estado: " + message);
        }
    }

    // Buscar usuario por ID
    private UserStatistics FindUser(string userID)
    {
        foreach (UserStatistics user in userDatabase.users)
        {
            if (user.userID == userID)
                return user;
        }
        return null;
    }

    // Establecer usuario actual
    private void SetCurrentUser(UserStatistics userStats)
    {
        currentUserID = userStats.userID;
        currentUserStats = userStats;
        currentUserStats.lastSession = DateTime.Now;
        SaveDatabase();

        // Notificar a MagicText que hay un usuario activo
        MagicText magicText = FindObjectOfType<MagicText>();
        if (magicText != null)
        {
            magicText.EstablecerUsuarioActual(currentUserID);
        }
        
        Debug.Log($"Usuario activo establecido: {userStats.userName} (ID: {userStats.userID})");
    }

    // Cerrar sesión
    public void LogoutUser()
    {
        currentUserID = "";
        currentUserStats = null;
        
        // Eliminar ID guardado
        PlayerPrefs.DeleteKey("LastUserID");
        PlayerPrefs.Save();
        
        // Mostrar panel inicial
        if (loginPanelController != null)
        {
            loginPanelController.MostrarSoloPanelInicial();
        }
        else if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }
        
        // Limpiar información del usuario
        if (userInfoText != null)
        {
            userInfoText.text = "No hay usuario activo";
        }
        
        UpdateStatusText("Sesión cerrada correctamente");
    }

    // Obtener ID del usuario actual
    public string GetUsuarioActualID()
    {
        return currentUserID;
    }

    // Obtener estadísticas del usuario
    public UserStatistics GetUserStatistics(string userID)
    {
        return FindUser(userID);
    }

    // Actualizar estadísticas del usuario
    public void UpdateUserStatistics(string userID, int[] aciertos, int[] errores)
    {
        UserStatistics userStats = FindUser(userID);
        
        if (userStats != null)
        {
            // Combinar estadísticas existentes con las nuevas
            for (int i = 0; i < Mathf.Min(aciertos.Length, userStats.aciertos.Length); i++)
            {
                userStats.aciertos[i] = aciertos[i];
            }
            
            for (int i = 0; i < Mathf.Min(errores.Length, userStats.errores.Length); i++)
            {
                userStats.errores[i] = errores[i];
            }
            
            userStats.lastSession = DateTime.Now;
            SaveDatabase();
            Debug.Log($"Estadísticas actualizadas para usuario {userID}");
        }
        else
        {
            Debug.LogWarning($"No se pudo actualizar estadísticas: usuario {userID} no encontrado");
        }
    }

    // Mostrar panel de login
    public void ShowLoginPanel()
    {
        if (loginPanelController != null)
        {
            loginPanelController.MostrarSoloPanelInicial();
        }
        else if (loginPanel != null)
        {
            loginPanel.SetActive(true);
        }
    }

    // Obtener información del usuario actual (para mostrar en UI)
    public string GetCurrentUserInfo()
    {
        if (currentUserStats != null)
        {
            return $"Usuario: {currentUserStats.userName} (TI: {currentUserStats.userID})\n" +
                   $"Edad: {currentUserStats.edad} - Sexo: {currentUserStats.sexo}";
        }
        return "No hay usuario activo";
    }
} 