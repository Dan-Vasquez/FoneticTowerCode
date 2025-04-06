using UnityEngine;

/// <summary>
/// Controla el movimiento de la cámara en primera persona.
/// Permite la rotación de la cámara basada en el movimiento del ratón y
/// sigue al jugador, implementando un sistema de vista en primera persona.
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Referencia al Transform del jugador que la cámara debe seguir.
    /// La cámara rotará alrededor de este punto para la vista en primera persona.
    /// </summary>
    public Transform player;

    /// <summary>
    /// Factor de sensibilidad para el movimiento del ratón.
    /// Valores más altos resultan en rotaciones más rápidas de la cámara.
    /// </summary>
    public float mouseSensitivity = 100f;

    /// <summary>
    /// Almacena el ángulo actual de rotación vertical de la cámara.
    /// Se utiliza para limitar el rango de movimiento vertical y evitar giros completos.
    /// </summary>
    private float xRotation = 0f;

    /// <summary>
    /// Inicializa la configuración de la cámara.
    /// Oculta y bloquea el cursor en el centro de la pantalla para mejor control.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Se mantiene vacío pero se conserva para posibles implementaciones futuras
    /// de lógica que necesite ejecutarse en cada frame antes de LateUpdate.
    /// </summary>
    void Update()
    {
        
    }

    /// <summary>
    /// Actualiza la rotación de la cámara después de que todos los Updates han sido llamados.
    /// Procesa la entrada del ratón y aplica las rotaciones correspondientes a la cámara y al jugador.
    /// </summary>
    private void LateUpdate()
    {
        // Calcular la rotación basada en el movimiento del ratón
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Actualizar y limitar la rotación vertical
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplicar la rotación vertical a la cámara
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Aplicar la rotación horizontal al jugador
        player.Rotate(Vector3.up * mouseX);
    }
}