using UnityEngine;

// Ficha de datos de un item (trigo, huevo...).
// Se crea en Project con: clic derecho > Create > Tiendita > Item
// IMPORTANTE: solo se LEE durante el juego, nunca se modifica (los cambios se guardarían en el archivo).
[CreateAssetMenu(fileName = "NuevoItem", menuName = "Tiendita/Item")]
public class Item : ScriptableObject
{
    public string nombre; 
    public Sprite icono;

    public int cantidadMinima = 1;   // lo mínimo que puede pedir un aldeano
    public int cantidadMaxima = 5;   // lo máximo

    public int precio = 1;           // monedas por unidad (aún no se usa)

    public bool necesitaVacas = false;   // true = solo disponible tras comprar las vacas (ej: leche)

    // ¿Está disponible ahora mismo? Lo consultan los pedidos y el cofre
    public bool Desbloqueado()
    {
        return !necesitaVacas || PanelCofre.vacasDesbloqueadas;
    }
}
