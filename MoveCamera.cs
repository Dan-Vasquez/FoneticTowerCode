/*
 * MoveCamera.cs
 * 
 * Este script controla el movimiento de la cámara en primera persona.
 * Permite la rotación de la cámara basada en el movimiento del ratón y sigue al jugador.
 * 
 * Características principales:
 * - Rotación suave de la cámara
 * - Seguimiento del jugador
 * - Control de sensibilidad del ratón
 * - Limitación de ángulos de rotación vertical
 */

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Objetivo a seguir (el jugador)
    public Transform player;

    // Sensibilidad del ratón
    public float mouseSensitivity = 100f;

    // Ángulo de rotación vertical
    private float xRotation = 0f;

    /// <summary>
    /// Inicializa la cámara y configura el cursor
    /// </summary>
    void Start()
    {
        // Ocultar y bloquear el cursor en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Actualización principal del comportamiento de la cámara
    /// </summary>
    void Update()
    {
        
    }

    /// <summary>
    /// Actualiza la rotación de la cámara basada en el input del ratón
    /// Se ejecuta después de Update para asegurar movimientos suaves
    /// </summary>
    private void LateUpdate()
    {
        // Obtener entrada del ratón
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotar la cámara verticalmente (limitando para que no gire completamente)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplicar rotación a la cámara
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotar el jugador horizontalmente
        player.Rotate(Vector3.up * mouseX);
    }
}