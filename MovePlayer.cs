/*
 * MovePlayer.cs
 * 
 * Este script controla el movimiento del jugador en primera persona.
 * Implementa movimiento básico, salto y aplicación de gravedad.
 * 
 * Características principales:
 * - Movimiento en todas direcciones
 * - Sistema de salto
 * - Aplicación de gravedad
 * - Detección de suelo
 */

using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Variables para el movimiento del jugador
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = 9.8f;

    // Variable para verificar si el jugador est en el suelo
    private bool isGrounded;
    private Vector3 velocity;
    private CharacterController controller;

    /// <summary>
    /// Inicializa el controlador del jugador
    /// </summary>
    void Start()
    {
        // Obtener el componente CharacterController
        controller = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Actualiza el movimiento del jugador basado en el input
    /// Maneja el salto y aplica la gravedad
    /// </summary>
    void Update()
    {
        // Verificar si el jugador est en el suelo
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