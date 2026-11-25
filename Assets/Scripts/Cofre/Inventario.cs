using System.Collections.Generic;
using UnityEngine;

// Guarda cuántas unidades hay de cada item en el cofre
// y qué item lleva el jugador en los brazos.
public class Inventario : MonoBehaviour
{
    // Para usarlo desde cualquier script: Inventario.instancia.Cantidad(item);
    public static Inventario instancia;

    // Lo que aparece en el cofre cada mañana: qué item y cuántas unidades
    [System.Serializable]
    public class Reposicion
    {
        public Item item;
        public int cantidad = 28;
    }

    [Header("Cada mañana")]
    public Reposicion[] diaria;

    // Item que lleva el jugador en brazos (null = no lleva nada)
    public Item llevando { get; private set; }

    // Diccionario = lista de parejas "item -> cantidad"
    Dictionary<Item, int> existencias = new Dictionary<Item, int>();

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        NuevoDia();
    }

    // Rellena el cofre como cada mañana y deja los brazos libres
    public void NuevoDia()
    {
        existencias.Clear();
        llevando = null;

        if (diaria == null) return;
        foreach (Reposicion r in diaria)
        {
            if (r.item != null) existencias[r.item] = r.cantidad;
        }
    }

    public void Anadir(Item item, int cantidad)
    {
        if (item == null) return;
        if (!existencias.ContainsKey(item)) existencias[item] = 0;
        existencias[item] += cantidad;
    }

    public int Cantidad(Item item)
    {
        if (item != null && existencias.TryGetValue(item, out int c)) return c;
        return 0;
    }

    // El jugador saca 1 unidad del cofre y se la lleva en brazos.
    // Si ya llevaba otro item, ese vuelve al cofre (se cambia uno por otro).
    // Devuelve false si no queda de ese item.
    public bool TomarUno(Item item)
    {
        if (Cantidad(item) <= 0) return false;

        // Lo que llevaba vuelve al cofre
        DevolverLlevado();

        existencias[item]--;
        llevando = item;
        Debug.Log("Llevas: " + item.nombre + ". Quedan " + existencias[item]);
        return true;
    }

    // Lo que lleva en brazos vuelve al cofre
    public void DevolverLlevado()
    {
        if (llevando == null) return;
        Anadir(llevando, 1);
        llevando = null;
    }

    // Se llama al entregar al aldeano: el jugador se queda con los brazos libres
    public void SoltarLlevado()
    {
        llevando = null;
    }
}
