using UnityEngine;

// Ficha de datos de un item (trigo, huevo...).
// Se crea en Project con: clic derecho > Create > Tiendita > Item
// IMPORTANTE: solo se LEE durante el juego, nunca se modifica (los cambios se guardarían en el archivo).
[CreateAssetMenu(fileName = "NuevoItem", menuName = "Tiendita/Item")]
public class Item : ScriptableObject
{
    public string nombre; 
    public Sprite icono;

    [Header("Pedidos")]
    public int cantidadMinima = 1;   // lo mínimo que puede pedir un aldeano
    public int cantidadMaxima = 5;   // lo máximo

    [Header("Tienda")]
    public int precio = 1;           // monedas por unidad (aún no se usa)
}
