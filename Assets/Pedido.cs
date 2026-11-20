// Lo que quiere un aldeano concreto: qué item y cuántos.
// No es un componente ni un archivo: es una clase normal que va dentro de cada npc.
// [System.Serializable] hace que se vea en el Inspector del npc (útil para comprobarlo).
[System.Serializable]
public class Pedido
{
    public Item item;      // apunta al archivo del item (ej: Trigo.asset)
    public int cantidad;   // cuántos quiere este aldeano
    public int cantidadRestante; // total - entregado por el jugador
    public int total;

    public Pedido(Item item, int cantidad)
    {
        this.item = item;
        this.cantidad = cantidad;
        this.cantidadRestante = cantidad;
        this.total = (item.precio * cantidad ) + 2;
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

    // El jugador entrega 1 unidad. Devuelve true si se ha podido restar
    // (false si ya no quedaba nada por entregar).
    public bool RestarItem()
    {
        if (cantidadRestante <= 0) return false;
        cantidadRestante--;
        return true;
    }

    // true cuando ya se ha entregado todo
    public bool Completado()
    {
        return cantidadRestante <= 0;
    }
}
