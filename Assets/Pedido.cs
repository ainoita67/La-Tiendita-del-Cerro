// Lo que quiere un aldeano concreto: qué item y cuántos.
// No es un componente ni un archivo: es una clase normal que va dentro de cada npc.
// [System.Serializable] hace que se vea en el Inspector del npc (útil para comprobarlo).
[System.Serializable]
public class Pedido
{
    public Item item;      // apunta al archivo del item (ej: Trigo.asset)
    public int cantidad;   // cuántos quiere este aldeano

    public Pedido(Item item, int cantidad)
    {
        this.item = item;
        this.cantidad = cantidad;
    }

    // Lo que vale el pedido entero: precio por unidad x cantidad
    public int PrecioTotal()
    {
        return item.precio * cantidad;
    }

    public override string ToString()
    {
        return cantidad + " x " + item.nombre;
    }
}
