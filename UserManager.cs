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
    
    // Fecha de la última sesión como string (para serialización JSON)
    public string lastSessionString;
    
    // Campo no serializado para uso interno
    [System.NonSerialized]
    private DateTime _lastSession;
    
    // Propiedad para acceder a la fecha como DateTime
    public DateTime lastSession
    {
        get
        {
            // Si el string está vacío o es null, devolver la fecha actual
            if (string.IsNullOrEmpty(lastSessionString))
            {
                return DateTime.Now;
            }
            
            // Intentar convertir el string a DateTime
            DateTime result;
            if (DateTime.TryParse(lastSessionString, out result))
            {
                return result;
            }
            else
            {
                // Si hay error en el formato, devolver la fecha actual
                return DateTime.Now;
            }
        }
        set
        {
            // Guardar la fecha como DateTime y también como string
            _lastSession = value;
            lastSessionString = value.ToString("o"); // Formato ISO 8601 para mejor compatibilidad
        }
    }

    public UserStatistics(string id, string name, string gender, int age, int nivelCount)
    {
        userID = id;
        userName = name;
        sexo = gender;
        edad = age;
        aciertos = new int[nivelCount];
        errores = new int[nivelCount];
        lastSession = DateTime.Now; // Esto actualizará también lastSessionString
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

    public TextAsset userDatabaseAsset; // Archivo JSON de la base de datos de usuarios
    private string userDatabasePath; // Ruta donde se guardará la base de datos

    // Datos
    private UserDatabase userDatabase;
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
        userDatabasePath = Path.Combine(Application.persistentDataPath, "user_database.json");
        Debug.Log($"Base de datos ubicada en: {userDatabasePath}");
        Debug.Log($"RUTA COMPLETA DEL ARCHIVO JSON: {Path.GetFullPath(userDatabasePath)}");
        
        // Para ayudar a localizar el archivo, mostrar información sobre cómo acceder a él
        Debug.Log("INFORMACIÓN IMPORTANTE: Para acceder a este archivo, navega a la ruta de Application.persistentDataPath");
        #if UNITY_EDITOR
        Debug.Log($"En Unity Editor, Application.persistentDataPath es: {Application.persistentDataPath}");
        #elif UNITY_ANDROID
        Debug.Log("En Android, puedes acceder al archivo usando ADB o conectando el dispositivo a la computadora");
        #elif UNITY_IOS
        Debug.Log("En iOS, puedes acceder al archivo mediante iTunes o la aplicación Archivos");
        #elif UNITY_STANDALONE_WIN
        Debug.Log($"En Windows, Application.persistentDataPath suele estar en: %userprofile%\\AppData\\LocalLow\\[CompanyName]\\[ProductName]");
        #elif UNITY_STANDALONE_OSX
        Debug.Log($"En macOS, Application.persistentDataPath suele estar en: ~/Library/Application Support/[CompanyName]/[ProductName]");
        #endif

        // Cargar la base de datos
        LoadDatabase();

        // Buscar controlador de paneles si no está asignado
        if (loginPanelController == null)
        {
            loginPanelController = FindFirstObjectByType<LoginPanel>();
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
                return;
            }
        }

        // Si no hay usuario almacenado o no se encontró, mostrar panel de login
        // El panel ya debería estar configurado correctamente en LoginPanel.Start()
    }

    // Cargar la base de datos desde el archivo JSON
    private void LoadDatabase()
    {
        try
        {
            // Primero intentamos cargar desde el archivo en persistentDataPath
            if (File.Exists(userDatabasePath))
            {
                string jsonContent = File.ReadAllText(userDatabasePath);
                userDatabase = JsonUtility.FromJson<UserDatabase>(jsonContent);
                Debug.Log($"Base de datos cargada desde archivo: {userDatabasePath}");
                Debug.Log($"Usuarios encontrados: {userDatabase.users.Count}");
            }
            // Si no existe, intentamos cargar desde el asset
            else if (userDatabaseAsset != null)
            {
                string jsonContent = userDatabaseAsset.text;
                userDatabase = JsonUtility.FromJson<UserDatabase>(jsonContent);
                Debug.Log($"Base de datos cargada desde asset con {userDatabase.users.Count} usuarios");
                
                // Guardar en persistentDataPath para futuros cambios
                SaveDatabase();
            }
            else
            {
                Debug.Log("No se encontró base de datos existente. Creando nueva.");
                userDatabase = new UserDatabase();
                CreateTestUser();
                SaveDatabase();
            }
            
            // Crear un usuario de prueba si no hay usuarios en la base de datos
            if (userDatabase.users.Count == 0)
            {
                CreateTestUser();
                SaveDatabase();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al cargar la base de datos: {e.Message}");
            userDatabase = new UserDatabase();
            CreateTestUser();
            SaveDatabase();
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
        
        // Asegurarse de que la fecha quede bien establecida
        testUser.lastSession = DateTime.Now;
        Debug.Log($"Usuario de prueba creado con fecha: {testUser.lastSession}, String: {testUser.lastSessionString}");

        userDatabase.users.Add(testUser);
        Debug.Log("Usuario de prueba creado: ID 123456789, Nombre: Usuario de Prueba");
    }
    
    // Método para ayudar a depurar problemas de serialización de fechas
    private void VerificarFechasUsuarios()
    {
        Debug.Log("=== VERIFICACIÓN DE FECHAS DE USUARIOS ===");
        foreach (UserStatistics user in userDatabase.users)
        {
            Debug.Log($"Usuario: {user.userName} (ID: {user.userID})");
            Debug.Log($"  - lastSessionString: {user.lastSessionString}");
            Debug.Log($"  - lastSession (DateTime): {user.lastSession}");
        }
        Debug.Log("======================================");
    }
    
    // Guardar la base de datos en el archivo JSON
    private void SaveDatabase()
    {
        try
        {
            // Verificar fechas antes de guardar (solo para depuración)
            VerificarFechasUsuarios();
            
            string jsonContent = JsonUtility.ToJson(userDatabase, true);
            File.WriteAllText(userDatabasePath, jsonContent);
            Debug.Log($"Base de datos guardada correctamente en: {userDatabasePath}");
            
            // Mostrar la ruta completa para fácil localización del archivo
            Debug.Log($"RUTA COMPLETA DEL ARCHIVO JSON: {Path.GetFullPath(userDatabasePath)}");
            
            // Imprimir el contenido JSON para verificación
            Debug.Log("Contenido JSON guardado:");
            Debug.Log(jsonContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar la base de datos: {e.Message}");
        }
    }

    // Iniciar sesión de usuario
    public void LoginUser()
    {
        if (userIDInput == null || string.IsNullOrEmpty(userIDInput.text))
        {
            UpdateStatusText("Por favor, ingresa una TI válida.");
            Debug.LogError("Error de inicio de sesión: TI vacía");
            return;
        }

        string userID = userIDInput.text.Trim();
        
        // Verificar que la TI solo contenga números
        foreach (char c in userID)
        {
            if (!char.IsDigit(c))
            {
                UpdateStatusText("La TI debe contener solo números.");
                Debug.LogError("Error de inicio de sesión: TI contiene caracteres no numéricos");
                return;
            }
        }
        
        UserStatistics userStats = FindUser(userID);

        if (userStats != null)
        {
            // Usuario encontrado, iniciar sesión
            SetCurrentUser(userStats);
            UpdateStatusText("¡Bienvenido de nuevo, " + userStats.userName + "!");
            Debug.Log($"Inicio de sesión exitoso - Usuario: {userStats.userName} (ID: {userID})");
            Debug.Log($"Fecha de última sesión actualizada: {userStats.lastSession}, String: {userStats.lastSessionString}");

            // Guardar el ID del usuario para futuras sesiones
            PlayerPrefs.SetString("LastUserID", userID);
            PlayerPrefs.Save();

            // La visibilidad de botones ahora se maneja en LoginPanel.cs
            // para evitar duplicación de código

            // Actualizar texto de información del usuario si existe
            if (userInfoText != null)
            {
                userInfoText.text = GetCurrentUserInfo();
            }
        }
        else
        {
            UpdateStatusText("Usuario no encontrado. Por favor, regístrate primero.");
            Debug.LogWarning($"Intento de inicio de sesión fallido: No se encontró usuario con TI: {userID}");
            
            // La visibilidad de botones ahora se maneja en LoginPanel.cs
        }
    }

    // Registrar nuevo usuario
    public void RegisterUser()
    {
        if (userIDInput == null || string.IsNullOrEmpty(userIDInput.text))
        {
            UpdateStatusText("Por favor, ingresa una TI válida.");
            Debug.LogError("Error de registro: TI vacía");
            return;
        }

        if (userNameInput == null || string.IsNullOrEmpty(userNameInput.text))
        {
            UpdateStatusText("Por favor, ingresa un nombre válido.");
            Debug.LogError("Error de registro: Nombre vacío");
            return;
        }

        if (sexoInput == null || string.IsNullOrEmpty(sexoInput.text))
        {
            UpdateStatusText("Por favor, ingresa el sexo (Masculino/Femenino).");
            Debug.LogError("Error de registro: Sexo no seleccionado");
            return;
        }

        if (edadInput == null || string.IsNullOrEmpty(edadInput.text))
        {
            UpdateStatusText("Por favor, ingresa la edad.");
            Debug.LogError("Error de registro: Edad vacía");
            return;
        }

        string userID = userIDInput.text.Trim();
        string userName = userNameInput.text.Trim();
        string sexo = sexoInput.text.Trim();

        // Validar TI (máximo 12 números)
        if (userID.Length > 12)
        {
            UpdateStatusText("La TI debe tener máximo 12 números.");
            Debug.LogError("Error de registro: TI excede el máximo de 12 dígitos");
            return;
        }

        // Verificar que la TI solo contenga números
        foreach (char c in userID)
        {
            if (!char.IsDigit(c))
            {
                UpdateStatusText("La TI debe contener solo números.");
                Debug.LogError("Error de registro: TI contiene caracteres no numéricos");
                return;
            }
        }

        // Validar y convertir edad
        int edad;
        if (!int.TryParse(edadInput.text.Trim(), out edad))
        {
            UpdateStatusText("Por favor, ingresa una edad válida (número).");
            Debug.LogError("Error de registro: Edad no es un número válido");
            return;
        }

        if (edad < 6 || edad > 14)
        {
            UpdateStatusText("Por favor, ingresa una edad entre 6 y 14 años.");
            Debug.LogError($"Error de registro: Edad fuera de rango (6-14): {edad}");
            return;
        }

        // Verificar si el usuario ya existe
        if (FindUser(userID) != null)
        {
            UpdateStatusText("Esta TI ya está registrada. Inicia sesión.");
            Debug.LogWarning($"Intento de registro con TI existente: {userID}");
            return;
        }

        // Crear nuevo usuario con la fecha actual
        UserStatistics newUser = new UserStatistics(userID, userName, sexo, edad, nivelCount);
        newUser.lastSession = DateTime.Now;
        Debug.Log($"Nuevo usuario creado con fecha: {newUser.lastSession}, String: {newUser.lastSessionString}");
        userDatabase.users.Add(newUser);
        SaveDatabase();

        // Establecer como usuario actual
        SetCurrentUser(newUser);
        UpdateStatusText("¡Registro exitoso! Bienvenido, " + userName);
        Debug.Log($"Usuario registrado correctamente: ID={userID}, Nombre={userName}, Sexo={sexo}, Edad={edad}");

        // Guardar el ID del usuario para futuras sesiones
        PlayerPrefs.SetString("LastUserID", userID);
        PlayerPrefs.Save();

        // La visibilidad de botones ahora se maneja en LoginPanel.cs
        // para evitar duplicación de código

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

    // Verificar si existe un usuario con la TI proporcionada
    public bool VerificarUsuarioExiste(string userID)
    {
        UserStatistics usuario = FindUser(userID);
        return usuario != null;
    }

    // Establecer usuario actual
    private void SetCurrentUser(UserStatistics userStats)
    {
        currentUserID = userStats.userID;
        currentUserStats = userStats;
        
        // Actualizar fecha de última sesión
        currentUserStats.lastSession = DateTime.Now;
        Debug.Log($"Actualizando fecha de última sesión para usuario {userStats.userName} a: {currentUserStats.lastSession}");
        Debug.Log($"lastSessionString actualizada a: {currentUserStats.lastSessionString}");
        
        SaveDatabase();

        // Notificar a MagicText que hay un usuario activo
        MagicText magicText = FindFirstObjectByType<MagicText>();
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

    // Obtener la lista completa de usuarios
    public List<UserStatistics> ObtenerListaUsuarios()
    {
        if (userDatabase != null && userDatabase.users != null)
        {
            // Devolver una copia de la lista para evitar modificaciones no deseadas
            return new List<UserStatistics>(userDatabase.users);
        }
        return new List<UserStatistics>();
    }

    // Eliminar un usuario de la base de datos
    public bool EliminarUsuario(string userID)
    {
        UserStatistics usuario = FindUser(userID);
        
        if (usuario != null)
        {
            // Si el usuario a eliminar es el usuario actual, cerrar sesión primero
            if (currentUserID == userID)
            {
                LogoutUser();
            }
            
            // Eliminar de la base de datos
            userDatabase.users.Remove(usuario);
            SaveDatabase();
            
            Debug.Log($"Usuario eliminado correctamente: ID={userID}, Nombre={usuario.userName}");
            return true;
        }
        
        Debug.LogWarning($"No se pudo eliminar el usuario con ID: {userID} porque no existe");
        return false;
    }

    // Actualizar datos de un usuario existente
    public bool ActualizarUsuario(string userIDAnterior, UserStatistics usuarioActualizado)
    {
        if (string.IsNullOrEmpty(userIDAnterior) || usuarioActualizado == null)
        {
            Debug.LogError("Error al actualizar usuario: ID anterior o datos actualizados nulos");
            return false;
        }
        
        try
        {
            // Buscar el usuario por su ID anterior
            UserStatistics usuarioExistente = FindUser(userIDAnterior);
            
            if (usuarioExistente == null)
            {
                Debug.LogWarning($"No se encontró usuario con ID {userIDAnterior} para actualizar");
                return false;
            }
            
            // Verificar si se cambió el ID y si ya existe otro usuario con ese ID
            if (userIDAnterior != usuarioActualizado.userID && FindUser(usuarioActualizado.userID) != null)
            {
                Debug.LogWarning($"Ya existe un usuario con ID {usuarioActualizado.userID}");
                return false;
            }
            
            // Actualizar los datos del usuario existente
            if (userIDAnterior != usuarioActualizado.userID)
            {
                // Si el ID cambió y este es el usuario actual, actualizamos también el ID actual
                if (currentUserID == userIDAnterior)
                {
                    currentUserID = usuarioActualizado.userID;
                    PlayerPrefs.SetString("LastUserID", usuarioActualizado.userID);
                    PlayerPrefs.Save();
                }
            }
            
            // Si es el mismo objeto, simplemente actualizamos la base de datos
            if (usuarioExistente == usuarioActualizado)
            {
                SaveDatabase();
                Debug.Log($"Usuario actualizado: ID={usuarioActualizado.userID}, Nombre={usuarioActualizado.userName}");
                return true;
            }
            
            // Si son objetos diferentes, actualizamos los campos uno por uno
            usuarioExistente.userID = usuarioActualizado.userID;
            usuarioExistente.userName = usuarioActualizado.userName;
            usuarioExistente.sexo = usuarioActualizado.sexo;
            usuarioExistente.edad = usuarioActualizado.edad;
            
            // Copiar los arrays de aciertos y errores
            for (int i = 0; i < Math.Min(usuarioExistente.aciertos.Length, usuarioActualizado.aciertos.Length); i++)
            {
                usuarioExistente.aciertos[i] = usuarioActualizado.aciertos[i];
                usuarioExistente.errores[i] = usuarioActualizado.errores[i];
            }
            
            // Actualizar la fecha de última sesión
            usuarioExistente.lastSession = usuarioActualizado.lastSession;
            
            // Guardamos la base de datos actualizada
            SaveDatabase();
            
            Debug.Log($"Usuario actualizado: ID={usuarioActualizado.userID}, Nombre={usuarioActualizado.userName}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al actualizar usuario: {e.Message}");
            return false;
        }
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