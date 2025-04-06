/*
 * ObjetoInteractivo.cs
 * 
 * Este script define el comportamiento de los objetos interactivos en el juego.
 * Maneja efectos visuales, categorización y estados de los objetos.
 * 
 * Características principales:
 * - Sistema de categorías (vegetales, frutas, pociones)
 * - Efectos visuales de outline
 * - Efectos de parpadeo
 * - Gestión de estados visuales
 */

using UnityEngine;
using System.Collections;

public class ObjetoInteractivo : MonoBehaviour
{
    // Categoría del objeto (0 = vegetal, 1 = fruta, 2 = poción)
    public int categoria;

    // Nombre específico del objeto (debe coincidir con los nombres en tus arrays)
    public string nombreObjeto;

    /// <summary>
    /// Aplica el efecto de outline al objeto o su modelo visual
    /// </summary>
    public void AplicarOutline()
    {
        // Si se especificó un modelo específico, usar ese
        GameObject objetoACambiar = gameObject;

        // Cambiar a layer Outline
        objetoACambiar.layer = 6; // Layer Outline
        Debug.Log("Objeto " + nombreObjeto + " cambiado a layer Outline");
    }

    /// <summary>
    /// Quita el efecto de outline del objeto
    /// </summary>
    public void QuitarOutline()
    {
        // Cambia el layer del objeto de vuelta a su layer original
        gameObject.layer = 0; // O el layer original que tenías
    }
}