using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class UserInfoDisplay : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_InputField userIDInputField;
    public Button buscarButton;
    public TextMeshProUGUI resultadoText;
    public GameObject panelResultados;
    
    [Header("Navegación entre Usuarios")]
    public Button botonAnteriorUsuario;    // Botón para navegar al usuario anterior
    public Button botonSiguienteUsuario;   // Botón para navegar al usuario siguiente
    public Button botonEliminarUsuario;    // Botón para eliminar el usuario actual
    
    [Header("Edición de Usuario")]
    public Button botonEditarUsuario;      // Botón para iniciar edición
    public Button botonAceptarEdicion;     // Botón para guardar cambios
    public Button botonCancelarEdicion;    // Botón para cancelar edición
    public GameObject panelEdicion;        // Panel que contiene los campos de edición

    // Campos de visualización (TextMeshPro)
    [Header("Detalles de Usuario")]
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI sexoText;
    public TextMeshProUGUI edadText;
    public TextMeshProUGUI ultimaFechaText;
    public TextMeshProUGUI aciertosText;
    public TextMeshProUGUI erroresText;
    
    // Campos para edición
    [Header("Campos de Edición")]
    public TMP_InputField editUserIDField;    // Campo para editar TI (diferente al de búsqueda)
    public TMP_InputField editNombreField;    // Campo para editar nombre
    public TMP_InputField editEdadField;      // Campo para editar edad
    public TMP_Dropdown editSexoDropdown;     // Dropdown para editar sexo
    
    [Header("Mensajes Temporales")]
    public TextMeshProUGUI mensajeTemporalText; // Campo para mostrar mensajes temporales de error
    
    private UserManager userManager;
    private UserStatistics usuarioActual;  // Usuario actualmente mostrado
    private int indiceUsuarioActual = -1;  // Índice del usuario actual en la lista
    private List<UserStatistics> listaUsuarios = new List<UserStatistics>();
    private bool modoEdicion = false;      // Indica si estamos en modo edición
    private Coroutine mensajeTemporalCoroutine; // Para gestionar la duración del mensaje temporal
    
    private void Awake()
    {
        // Obtener referencia al UserManager
        userManager = UserManager.Instance;
        if (userManager == null)
        {
            userManager = FindFirstObjectByType<UserManager>();
        }
        
        if (userManager == null)
        {
            Debug.LogError("No se encontró UserManager en la escena. El componente UserInfoDisplay no funcionará correctamente.");
        }
        
        // Inicialmente ocultar el panel de resultados y el panel de edición
        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }
        
        if (panelEdicion != null)
        {
            panelEdicion.SetActive(false);
        }
        
        // Configurar opciones del dropdown de sexo
        if (editSexoDropdown != null)
        {
            editSexoDropdown.ClearOptions();
            editSexoDropdown.AddOptions(new List<string> { "Masculino", "Femenino" });
        }
    }
    
    private void Start()
    {
        // Asignar eventos a los botones
        if (buscarButton != null)
        {
            buscarButton.onClick.AddListener(BuscarUsuario);
        }
        
        if (botonAnteriorUsuario != null)
        {
            botonAnteriorUsuario.onClick.AddListener(MostrarUsuarioAnterior);
        }
        
        if (botonSiguienteUsuario != null)
        {
            botonSiguienteUsuario.onClick.AddListener(MostrarUsuarioSiguiente);
        }
        
        if (botonEliminarUsuario != null)
        {
            botonEliminarUsuario.onClick.AddListener(EliminarUsuarioActual);
        }
        
        // Configurar botones de edición
        if (botonEditarUsuario != null)
        {
            botonEditarUsuario.onClick.AddListener(IniciarEdicionUsuario);
        }
        
        if (botonAceptarEdicion != null)
        {
            botonAceptarEdicion.onClick.AddListener(GuardarCambiosUsuario);
        }
        
        if (botonCancelarEdicion != null)
        {
            botonCancelarEdicion.onClick.AddListener(CancelarEdicionUsuario);
        }
        
        // Inicialmente cargar la lista de usuarios
        ActualizarListaUsuarios();
    }
    
    // Iniciar modo de edición del usuario actual
    public void IniciarEdicionUsuario()
    {
        if (usuarioActual == null)
        {
            MostrarMensajeTemporal("No hay usuario seleccionado para editar.");
            return;
        }
        
        modoEdicion = true;
        
        // Rellenar campos de edición con datos actuales
        if (editUserIDField != null)
        {
            editUserIDField.text = usuarioActual.userID;
        }
        
        if (editNombreField != null)
        {
            editNombreField.text = usuarioActual.userName;
        }
        
        if (editEdadField != null)
        {
            editEdadField.text = usuarioActual.edad.ToString();
        }
        
        if (editSexoDropdown != null)
        {
            // Seleccionar opción según género actual
            editSexoDropdown.value = usuarioActual.sexo == "Femenino" ? 1 : 0;
        }
        
        // Ocultar elementos
        // Ocultar todos los TextMesh de Detalles de Usuario
        if (nombreText != null) nombreText.gameObject.SetActive(false);
        if (sexoText != null) sexoText.gameObject.SetActive(false);
        if (edadText != null) edadText.gameObject.SetActive(false);
        if (ultimaFechaText != null) ultimaFechaText.gameObject.SetActive(false);
        if (aciertosText != null) aciertosText.gameObject.SetActive(false);
        if (erroresText != null) erroresText.gameObject.SetActive(false);
        
        // Ocultar botones de navegación y eliminación
        if (botonAnteriorUsuario != null) botonAnteriorUsuario.gameObject.SetActive(false);
        if (botonSiguienteUsuario != null) botonSiguienteUsuario.gameObject.SetActive(false);
        if (botonEliminarUsuario != null) botonEliminarUsuario.gameObject.SetActive(false);
        
        // Ocultar botón de editar y campo de búsqueda
        if (botonEditarUsuario != null) botonEditarUsuario.gameObject.SetActive(false);
        if (userIDInputField != null) userIDInputField.gameObject.SetActive(false);
        if (buscarButton != null) buscarButton.gameObject.SetActive(false);
        
        // Mostrar elementos
        // Mostrar campos de edición
        if (editUserIDField != null) editUserIDField.gameObject.SetActive(true);
        if (editNombreField != null) editNombreField.gameObject.SetActive(true);
        if (editEdadField != null) editEdadField.gameObject.SetActive(true);
        if (editSexoDropdown != null) editSexoDropdown.gameObject.SetActive(true);
        
        // Mostrar botones de aceptar y cancelar edición
        if (botonAceptarEdicion != null) botonAceptarEdicion.gameObject.SetActive(true);
        if (botonCancelarEdicion != null) botonCancelarEdicion.gameObject.SetActive(true);
        
        // Mostrar panel de edición
        if (panelEdicion != null) panelEdicion.SetActive(true);
        
        MostrarMensajeTemporal("Modo edición activado. Modifique los datos y pulse Aceptar para guardar.");
    }
    
    // Método para guardar los cambios del usuario
    public void GuardarCambiosUsuario()
    {
        // Validar campos
        if (string.IsNullOrEmpty(editUserIDField.text))
        {
            MostrarMensajeTemporal("Error: El ID de usuario no puede estar vacío");
            return;
        }

        if (string.IsNullOrEmpty(editNombreField.text))
        {
            MostrarMensajeTemporal("Error: El nombre no puede estar vacío");
            return;
        }

        int edad;
        if (editEdadField == null || string.IsNullOrEmpty(editEdadField.text) || 
            !int.TryParse(editEdadField.text, out edad) || edad < 6 || edad > 14)
        {
            MostrarMensajeTemporal("Error: La edad debe ser un número entre 6 y 14");
            return;
        }

        // Crear un nuevo objeto con los datos editados
        UserStatistics usuarioEditado = new UserStatistics(
            editUserIDField.text,
            editNombreField.text,
            editSexoDropdown != null ? editSexoDropdown.options[editSexoDropdown.value].text : "Masculino",
            edad,
            usuarioActual.aciertos.Length
        );
        
        // Copiar las estadísticas existentes
        for (int i = 0; i < usuarioActual.aciertos.Length; i++)
        {
            usuarioEditado.aciertos[i] = usuarioActual.aciertos[i];
            usuarioEditado.errores[i] = usuarioActual.errores[i];
        }
        
        // Preservar fecha de la última sesión del usuario original
        usuarioEditado.lastSession = usuarioActual.lastSession;

        // Guardar los cambios utilizando el UserManager
        string userIDAnterior = usuarioActual.userID;
        bool exito = userManager.ActualizarUsuario(userIDAnterior, usuarioEditado);

        if (exito)
        {
            // Actualizar el usuario actual
            UserStatistics usuarioActualizado = userManager.GetUserStatistics(usuarioEditado.userID);
            if (usuarioActualizado != null)
            {
                usuarioActual = usuarioActualizado;
            }
            
            // Ocultar elementos
            // Ocultar campos de edición
            if (editUserIDField != null) editUserIDField.gameObject.SetActive(false);
            if (editNombreField != null) editNombreField.gameObject.SetActive(false);
            if (editEdadField != null) editEdadField.gameObject.SetActive(false);
            if (editSexoDropdown != null) editSexoDropdown.gameObject.SetActive(false);
            
            // Ocultar botones de aceptar y cancelar edición
            if (botonAceptarEdicion != null) botonAceptarEdicion.gameObject.SetActive(false);
            if (botonCancelarEdicion != null) botonCancelarEdicion.gameObject.SetActive(false);
            
            // Ocultar panel de edición
            if (panelEdicion != null) panelEdicion.SetActive(false);
            
            // Mostrar elementos
            // Mostrar todos los TextMesh de Detalles de Usuario
            if (nombreText != null) nombreText.gameObject.SetActive(true);
            if (sexoText != null) sexoText.gameObject.SetActive(true);
            if (edadText != null) edadText.gameObject.SetActive(true);
            if (ultimaFechaText != null) ultimaFechaText.gameObject.SetActive(true);
            if (aciertosText != null) aciertosText.gameObject.SetActive(true);
            if (erroresText != null) erroresText.gameObject.SetActive(true);
            
            // Mostrar botones de navegación y eliminación
            if (botonAnteriorUsuario != null) botonAnteriorUsuario.gameObject.SetActive(true);
            if (botonSiguienteUsuario != null) botonSiguienteUsuario.gameObject.SetActive(true);
            if (botonEliminarUsuario != null) botonEliminarUsuario.gameObject.SetActive(true);
            
            // Mostrar botón de editar y campo de búsqueda
            if (botonEditarUsuario != null) botonEditarUsuario.gameObject.SetActive(true);
            if (userIDInputField != null) userIDInputField.gameObject.SetActive(true);
            if (buscarButton != null) buscarButton.gameObject.SetActive(true);
            
            modoEdicion = false;
            
            // Actualizar lista de usuarios
            ActualizarListaUsuarios();
            
            // Si cambió el ID, actualizar índice
            if (editUserIDField.text != userIDAnterior)
            {
                indiceUsuarioActual = EncontrarIndiceUsuario(editUserIDField.text);
            }
            
            // Actualizar visualización
            MostrarInformacionUsuario(usuarioActual);
            
            // Actualizar campo de búsqueda
            if (userIDInputField != null)
            {
                userIDInputField.text = editUserIDField.text;
            }
            
            MostrarMensajeTemporal("Usuario actualizado correctamente");
        }
        else
        {
            MostrarMensajeTemporal("Error al actualizar el usuario. ID posiblemente duplicado.");
        }
    }
    
    // Cancelar la edición del usuario
    public void CancelarEdicionUsuario()
    {
        modoEdicion = false;
        
        // Ocultar panel de edición
        if (panelEdicion != null)
        {
            panelEdicion.SetActive(false);
        }
        
        // Mostrar el campo de búsqueda
        if (userIDInputField != null)
        {
            userIDInputField.gameObject.SetActive(true);
        }
        
        if (buscarButton != null)
        {
            buscarButton.gameObject.SetActive(true);
        }
        
        // Mostrar textos de visualización
        MostrarTextosVisualizacion();
        
        // Mostrar botones de navegación y eliminar
        if (botonAnteriorUsuario != null) botonAnteriorUsuario.gameObject.SetActive(true);
        if (botonSiguienteUsuario != null) botonSiguienteUsuario.gameObject.SetActive(true);
        if (botonEliminarUsuario != null) botonEliminarUsuario.gameObject.SetActive(true);
        if (botonEditarUsuario != null) botonEditarUsuario.gameObject.SetActive(true);
        
        // Ocultar botones de aceptar y cancelar
        if (botonAceptarEdicion != null) botonAceptarEdicion.gameObject.SetActive(false);
        if (botonCancelarEdicion != null) botonCancelarEdicion.gameObject.SetActive(false);
        
        MostrarMensaje("Edición cancelada");
    }
    
    // Ocultar textos de visualización
    private void OcultarTextosVisualizacion()
    {
        if (nombreText != null) nombreText.gameObject.SetActive(false);
        if (sexoText != null) sexoText.gameObject.SetActive(false);
        if (edadText != null) edadText.gameObject.SetActive(false);
    }
    
    // Mostrar textos de visualización
    private void MostrarTextosVisualizacion()
    {
        if (nombreText != null) nombreText.gameObject.SetActive(true);
        if (sexoText != null) sexoText.gameObject.SetActive(true);
        if (edadText != null) edadText.gameObject.SetActive(true);
    }
    
    // Actualizar la lista local de usuarios desde el UserManager
    private void ActualizarListaUsuarios()
    {
        if (userManager != null)
        {
            listaUsuarios = userManager.ObtenerListaUsuarios();
            Debug.Log($"Lista de usuarios actualizada: {listaUsuarios.Count} usuarios encontrados");
        }
    }
    
    // Método para buscar un usuario por ID
    public void BuscarUsuario()
    {
        if (userManager == null)
        {
            MostrarMensajeTemporal("Error: No se ha encontrado el gestor de usuarios.");
            Debug.LogError("No se puede buscar usuario: UserManager no encontrado");
            return;
        }
        
        if (userIDInputField == null || string.IsNullOrEmpty(userIDInputField.text))
        {
            MostrarMensajeTemporal("Por favor, ingresa una TI válida para buscar.");
            return;
        }
        
        string userID = userIDInputField.text.Trim();
        
        // Verificar que solo contenga números
        foreach (char c in userID)
        {
            if (!char.IsDigit(c))
            {
                MostrarMensajeTemporal("La TI debe contener solo números.");
                return;
            }
        }
        
        // Obtener estadísticas del usuario
        UserStatistics userStats = userManager.GetUserStatistics(userID);
        
        if (userStats != null)
        {
            // Usuario encontrado, mostrar información
            MostrarInformacionUsuario(userStats);
            
            // Actualizar el índice del usuario actual
            ActualizarListaUsuarios();
            indiceUsuarioActual = EncontrarIndiceUsuario(userStats.userID);
            
            MostrarMensajeTemporal("Usuario encontrado. Mostrando información.");
        }
        else
        {
            // Usuario no encontrado
            MostrarMensajeTemporal("No se encontró ningún usuario con la TI proporcionada.");
            
            // Ocultar panel de resultados si está visible
            if (panelResultados != null)
            {
                panelResultados.SetActive(false);
            }
            
            usuarioActual = null;
            indiceUsuarioActual = -1;
        }
    }
    
    // Mostrar información detallada del usuario
    private void MostrarInformacionUsuario(UserStatistics userStats)
    {
        usuarioActual = userStats;
        
        // Mostrar el panel de resultados
        if (panelResultados != null)
        {
            panelResultados.SetActive(true);
        }
        
        // Actualizar cada campo con la información del usuario
        if (nombreText != null)
        {
            nombreText.text = "Nombre: " + userStats.userName;
        }
        
        if (sexoText != null)
        {
            sexoText.text = "Sexo: " + userStats.sexo;
        }
        
        if (edadText != null)
        {
            edadText.text = "Edad: " + userStats.edad.ToString();
        }
        
        if (ultimaFechaText != null)
        {
            // Recuperar la fecha a través de la propiedad lastSession
            DateTime ultimaFecha = userStats.lastSession;
            // Mostrar en formato legible
            ultimaFechaText.text = "Última sesión: " + ultimaFecha.ToString("dd/MM/yyyy HH:mm");
            // Para depuración, también mostrar el string original
            Debug.Log($"Valor original lastSessionString: {userStats.lastSessionString}");
        }
        
        // Mostrar resumen de aciertos y errores
        if (aciertosText != null)
        {
            // Construir texto con aciertos por nivel
            string textoAciertos = "Aciertos:\n";
            for (int i = 0; i < userStats.aciertos.Length; i++)
            {
                textoAciertos += $"LV-{i+1}: {userStats.aciertos[i]}\n";
            }
            aciertosText.text = textoAciertos;
        }
        
        if (erroresText != null)
        {
            // Construir texto con errores por nivel
            string textoErrores = "Errores:\n";
            for (int i = 0; i < userStats.errores.Length; i++)
            {
                textoErrores += $"LV-{i+1}: {userStats.errores[i]}\n";
            }
            erroresText.text = textoErrores;
        }
        
        // Mostrar mensaje de éxito
        MostrarMensaje("Usuario encontrado. Mostrando información.");
        Debug.Log($"Información mostrada para usuario: {userStats.userName} (ID: {userStats.userID})");
        
        // Actualizar campo de búsqueda con el ID actual
        if (userIDInputField != null)
        {
            userIDInputField.text = userStats.userID;
        }
    }
    
    // Navegar al usuario anterior
    public void MostrarUsuarioAnterior()
    {
        if (listaUsuarios.Count == 0)
        {
            MostrarMensaje("No hay usuarios en la base de datos.");
            return;
        }
        
        // Si hay un usuario en el campo de búsqueda, intentar buscar ese primero
        if (userIDInputField != null && !string.IsNullOrEmpty(userIDInputField.text))
        {
            string userID = userIDInputField.text.Trim();
            if (usuarioActual == null || usuarioActual.userID != userID)
            {
                // Buscar el usuario primero
                BuscarUsuario();
            }
        }
        
        // Si después de la búsqueda no hay usuario actual, tomar el último
        if (indiceUsuarioActual == -1)
        {
            indiceUsuarioActual = listaUsuarios.Count - 1;
        }
        else
        {
            // Moverse al anterior (o al último si estamos en el primero)
            indiceUsuarioActual--;
            if (indiceUsuarioActual < 0)
            {
                indiceUsuarioActual = listaUsuarios.Count - 1;
            }
        }
        
        // Mostrar el usuario
        if (indiceUsuarioActual >= 0 && indiceUsuarioActual < listaUsuarios.Count)
        {
            MostrarInformacionUsuario(listaUsuarios[indiceUsuarioActual]);
        }
    }
    
    // Navegar al usuario siguiente
    public void MostrarUsuarioSiguiente()
    {
        if (listaUsuarios.Count == 0)
        {
            MostrarMensaje("No hay usuarios en la base de datos.");
            return;
        }
        
        // Si hay un usuario en el campo de búsqueda, intentar buscar ese primero
        if (userIDInputField != null && !string.IsNullOrEmpty(userIDInputField.text))
        {
            string userID = userIDInputField.text.Trim();
            if (usuarioActual == null || usuarioActual.userID != userID)
            {
                // Buscar el usuario primero
                BuscarUsuario();
            }
        }
        
        // Si después de la búsqueda no hay usuario actual, tomar el primero
        if (indiceUsuarioActual == -1)
        {
            indiceUsuarioActual = 0;
        }
        else
        {
            // Moverse al siguiente (o al primero si estamos en el último)
            indiceUsuarioActual++;
            if (indiceUsuarioActual >= listaUsuarios.Count)
            {
                indiceUsuarioActual = 0;
            }
        }
        
        // Mostrar el usuario
        if (indiceUsuarioActual >= 0 && indiceUsuarioActual < listaUsuarios.Count)
        {
            MostrarInformacionUsuario(listaUsuarios[indiceUsuarioActual]);
        }
    }
    
    // Eliminar el usuario actual
    public void EliminarUsuarioActual()
    {
        if (usuarioActual == null)
        {
            MostrarMensaje("No hay usuario seleccionado para eliminar.");
            return;
        }
        
        // Confirmar con el usuario antes de eliminar
        if (userManager != null)
        {
            string userID = usuarioActual.userID;
            string userName = usuarioActual.userName;
            
            // Eliminar usuario
            bool eliminado = userManager.EliminarUsuario(userID);
            
            if (eliminado)
            {
                MostrarMensaje($"El usuario {userName} (ID: {userID}) ha sido eliminado.");
                
                // Actualizar lista de usuarios
                ActualizarListaUsuarios();
                
                // Si no hay más usuarios, ocultar panel
                if (listaUsuarios.Count == 0)
                {
                    if (panelResultados != null)
                    {
                        panelResultados.SetActive(false);
                    }
                    usuarioActual = null;
                    indiceUsuarioActual = -1;
                }
                else
                {
                    // Mostrar el siguiente usuario si existe
                    if (indiceUsuarioActual >= listaUsuarios.Count)
                    {
                        indiceUsuarioActual = listaUsuarios.Count - 1;
                    }
                    
                    // Si todavía hay un índice válido, mostrar ese usuario
                    if (indiceUsuarioActual >= 0 && indiceUsuarioActual < listaUsuarios.Count)
                    {
                        MostrarInformacionUsuario(listaUsuarios[indiceUsuarioActual]);
                    }
                    else
                    {
                        // Si no hay índice válido, ocultar panel
                        if (panelResultados != null)
                        {
                            panelResultados.SetActive(false);
                        }
                        usuarioActual = null;
                        indiceUsuarioActual = -1;
                    }
                }
            }
            else
            {
                MostrarMensaje("No se pudo eliminar el usuario. Inténtalo de nuevo.");
            }
        }
        else
        {
            MostrarMensaje("Error: No se ha encontrado el gestor de usuarios.");
        }
    }
    
    // Encontrar el índice de un usuario en la lista por ID
    private int EncontrarIndiceUsuario(string userID)
    {
        for (int i = 0; i < listaUsuarios.Count; i++)
        {
            if (listaUsuarios[i].userID == userID)
            {
                return i;
            }
        }
        return -1;
    }
    
    // Mostrar mensaje temporal que desaparece después de 4 segundos
    public void MostrarMensajeTemporal(string mensaje)
    {
        if (mensajeTemporalText != null)
        {
            mensajeTemporalText.text = mensaje;
            mensajeTemporalText.gameObject.SetActive(true);
            
            // Cancelar coroutine anterior si existe
            if (mensajeTemporalCoroutine != null)
            {
                StopCoroutine(mensajeTemporalCoroutine);
            }
            
            // Iniciar nueva coroutine para ocultar el mensaje después de 4 segundos
            mensajeTemporalCoroutine = StartCoroutine(OcultarMensajeTemporal(4f));
        }
        
        // También mostramos en el log para depuración
        Debug.Log(mensaje);
    }
    
    // Coroutine para ocultar el mensaje temporal después de un tiempo específico
    private IEnumerator OcultarMensajeTemporal(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        
        if (mensajeTemporalText != null)
        {
            mensajeTemporalText.gameObject.SetActive(false);
        }
        
        mensajeTemporalCoroutine = null;
    }
    
    // Mostrar mensaje en el texto de resultados (versión no temporal)
    private void MostrarMensaje(string mensaje)
    {
        if (resultadoText != null)
        {
            resultadoText.text = mensaje;
        }
        Debug.Log(mensaje);
    }
    
    // Método opcional para limpiar la búsqueda
    public void LimpiarBusqueda()
    {
        if (userIDInputField != null)
        {
            userIDInputField.text = "";
        }
        
        if (resultadoText != null)
        {
            resultadoText.text = "";
        }
        
        if (panelResultados != null)
        {
            panelResultados.SetActive(false);
        }
        
        usuarioActual = null;
        indiceUsuarioActual = -1;
    }
    
    // Método para mostrar información detallada por niveles (opcional)
    public void MostrarDetallesPorNivel(UserStatistics userStats)
    {
        string detalles = "Detalles por nivel:\n\n";
        
        for (int i = 0; i < userStats.aciertos.Length; i++)
        {
            detalles += $"Nivel {i+1}: Aciertos = {userStats.aciertos[i]}, Errores = {userStats.errores[i]}\n";
        }
        
        Debug.Log(detalles);
        
        // Si hay un campo de texto adicional para mostrar detalles, se puede usar aquí
        // if (detallesText != null)
        // {
        //     detallesText.text = detalles;
        // }
    }
} 