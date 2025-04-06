using UnityEngine;

/// <summary>
/// Controla el comportamiento de objetos interactivos en el juego.
/// Este componente permite gestionar objetos que pueden ser resaltados
/// y categorizados para su uso en ejercicios de pronunciación.
/// </summary>
public class ObjetoInteractivo : MonoBehaviour
{
    /// <summary>
    /// Categoría del objeto:
    /// 0 = vegetal
    /// 1 = fruta
    /// 2 = poción
    /// </summary>
    public int categoria;

    /// <summary>
    /// Nombre específico del objeto que debe coincidir con los nombres
    /// definidos en los arrays de palabras del sistema.
    /// </summary>
    public string nombreObjeto;

    /// <summary>
    /// Referencia opcional al modelo 3D o componente visual que debe
    /// tener el efecto de outline. Si no se especifica, se aplicará al GameObject actual.
    /// </summary>
    public GameObject modeloVisual;

    /// <summary>
    /// Aplica el efecto de outline al objeto cambiando su layer al layer Outline (6).
    /// Si hay un modelo visual específico definido, se aplicará a ese modelo,
    /// de lo contrario se aplicará al GameObject actual.
    /// </summary>
    public void AplicarOutline()
    {
        GameObject objetoACambiar = modeloVisual != null ? modeloVisual : gameObject;
        objetoACambiar.layer = 6; // Layer Outline
        Debug.Log($"Objeto {nombreObjeto} cambiado a layer Outline");
    }

    /// <summary>
    /// Quita el efecto de outline del objeto restaurando su layer original.
    /// </summary>
    public void QuitarOutline()
    {
        GameObject objetoACambiar = modeloVisual != null ? modeloVisual : gameObject;
        objetoACambiar.layer = 0; // Layer por defecto
    }
}