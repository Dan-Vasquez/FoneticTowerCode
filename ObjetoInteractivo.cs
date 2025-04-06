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
    /// Aplica un efecto de parpadeo múltiple al objeto
    /// </summary>
    /// <param name="repeticiones">Número de veces que parpadeará</param>
    /// <param name="intervalo">Tiempo entre parpadeos</param>
    public void AplicarParpadeoMultiple(int repeticiones, float intervalo)
    {
        StartCoroutine(ParpadeoContinuo(repeticiones, intervalo));
    }

    /// <summary>
    /// Corrutina que maneja el efecto de parpadeo continuo
    /// </summary>
    /// <param name="repeticiones">Número de repeticiones del parpadeo</param>
    /// <param name="intervalo">Intervalo entre estados de parpadeo</param>
    private IEnumerator ParpadeoContinuo(int repeticiones, float intervalo)
    {
        for (int i = 0; i < repeticiones; i++)
        {
            // Activar outline
            AplicarOutline();
            yield return new WaitForSeconds(intervalo / 2);

            // Desactivar outline (necesitarás implementar esta función)
            QuitarOutline();
            yield return new WaitForSeconds(intervalo / 2);
        }

        // Dejarlo activado al final
        AplicarOutline();
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