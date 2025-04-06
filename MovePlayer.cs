using UnityEngine;

/// <summary>
/// Controla el movimiento del jugador utilizando el sistema de CharacterController de Unity.
/// Permite el movimiento en todas las direcciones, salto y aplica gravedad.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    /// <summary>
    /// Velocidad de movimiento del jugador en unidades por segundo.
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// Fuerza inicial aplicada al salto del jugador.
    /// Un valor más alto resulta en saltos más altos.
    /// </summary>
    public float jumpForce = 5f;

    /// <summary>
    /// Fuerza de gravedad aplicada al jugador.
    /// Por defecto usa el valor aproximado de la gravedad terrestre.
    /// </summary>
    public float gravity = 9.8f;

    /// <summary>
    /// Indica si el jugador está tocando el suelo.
    /// Se actualiza cada frame usando el CharacterController.
    /// </summary>
    private bool isGrounded;

    /// <summary>
    /// Vector de velocidad actual del jugador.
    /// Se usa principalmente para el cálculo de la gravedad y el salto.
    /// </summary>
    private Vector3 velocity;

    /// <summary>
    /// Referencia al componente CharacterController que maneja las colisiones
    /// y el movimiento físico del jugador.
    /// </summary>
    private CharacterController controller;

    /// <summary>
    /// Inicializa las referencias necesarias al comenzar.
    /// </summary>
    void Start()
    {
        // Obtener el componente CharacterController
        controller = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Actualiza el movimiento del jugador cada frame.
    /// Procesa las entradas del usuario y aplica física básica.
    /// </summary>
    void Update()
    {
        // Verificar si el jugador está en el suelo
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Pequeño valor negativo para mantener el personaje pegado al suelo
        }

        // Obtener entradas de teclado para movimiento
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Crear vector de movimiento
        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;

        // Aplicar movimiento
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * 2f * gravity);
        }

        // Aplicar gravedad
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}