using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorFila : MonoBehaviour
{
    [Header("NPCs")] // Organizacion en el menú de Unity
    public npc prefabNpc;                              
    public AnimatorOverrideController[] personajes;    // los .overrideController de los NPCs
    public int cantidadInicial = 4;
    public float esperaEntreApariciones = 1f;          // para que no salgan todos a la vez

    [Header("Puntos de la escena")]
    public Transform entrada;      
    public Transform salida;       
    public Transform[] puestos;    

    [Header("Turno")]
    public float duracionTurno = 60f;

    [Header("Pedidos")]
    public Item[] itemsPosibles;   // los items que pueden pedir los aldeanos

    public float tiempoRestante { get; private set; } //visible puublicamente, solo se puede aignar desde aqui, por eso no se ve en el inspector unity

    List<npc> fila = new List<npc>();
    List<AnimatorOverrideController> disponibles = new List<AnimatorOverrideController>();
    bool turnoEnCurso = false;

    void Start()
    {
        // Copia los personajes y los desordena para sacarlos al azar sin repetir
        disponibles.AddRange(personajes);
        Barajar(disponibles);

        StartCoroutine(CrearFilaInicial()); //ejecuta crarfilainicial que es una corrutina (funcion que se puede pausar y continuar)
    }

    IEnumerator CrearFilaInicial()
    {
        int cuantos = Mathf.Min(cantidadInicial, puestos.Length);
        for (int i = 0; i < cuantos; i++)
        {
            if (!AnnadirNpc()) break;
            yield return new WaitForSeconds(esperaEntreApariciones);
        }
    }

    // Crea un NPC en la entrada y lo manda al último puesto libre. Lo hace una vez porque esta dentro del bucle en CrearFilaInicial()
    bool AnnadirNpc()
    {
        // tienen que quedar disponibles en la lista de overrides. Tiene que haber menos npcs en la fila que el total de puestos disponibles.
        if (disponibles.Count == 0 || fila.Count >= puestos.Length) return false; 
        // Se selecciona el siguiente override y se quita de la lista
        AnimatorOverrideController personaje = disponibles[0];
        disponibles.RemoveAt(0);

        // X e Y de la entrada, pero la Z que tenga guardada el prefab
        Vector3 posicion = new Vector3(entrada.position.x, entrada.position.y, prefabNpc.transform.position.z);
        npc nuevo = Instantiate(prefabNpc, posicion, Quaternion.identity); //llamada al awake() del prefav
        nuevo.name = "NPC_" + personaje.name; //para el hierarchy
        nuevo.PonerPersonaje(personaje); //metodo de npc, cambiadr el override

        // Cada aldeano llega ya sabiendo lo que quiere
        nuevo.pedido = GenerarPedido();
        Debug.Log(nuevo.name + " quiere " + nuevo.pedido);

        fila.Add(nuevo);
        MandarASuPuesto(nuevo);
        return true;
    }

    void MandarASuPuesto(npc n)
    {
        int puesto = fila.IndexOf(n);
        n.IrA(puestos[puesto].position, () => AlLlegarAlPuesto(n));
        // irA tiene un parametro para indicar que metodo ejecutar cuando termina lo suyo
    }

    // Se llama cada vez que un NPC llega a su puesto
    void AlLlegarAlPuesto(npc n)
    {
        // empieza turno si es el primero de la fila y no hay otro turno en marcha
        // dialogo
        if (fila.Count > 0 && fila[0] == n && !turnoEnCurso)
        {
            StartCoroutine(Turno(n)); // ejecuta turno que es una corrutina
        }
    }

    // esto es solo para el primero de la fila (cuando habla)
    IEnumerator Turno(npc n)
    {
        turnoEnCurso = true;
        n.MostrarBocadillo(true);
        n.MostrarPedido();
        n.MostrarAnimo(true);

        tiempoRestante = duracionTurno;
        while (tiempoRestante > 0f)
        {
            tiempoRestante -= Time.deltaTime;

            // progreso es un porcentaje a partir del cual actualizar animo calcula el sprite necesario
            float progreso = 1f - tiempoRestante / duracionTurno;
            n.ActualizarAnimo(progreso);

            yield return null;
        }
        tiempoRestante = 0f;

        // Se acaba el turno: cara final dentro de su bocadillo, se va por la salida y desaparece al llegar
        n.AnimoFinal();
        n.BocadilloFinal();
        fila.Remove(n);
        n.IrA(salida.position, () => Destroy(n.gameObject));

        turnoEnCurso = false;

        // El resto avanza un puesto
        foreach (npc otro in fila)
        {
            MandarASuPuesto(otro);
        }
    }

    // Elige un item al azar de los posibles y una cantidad al azar dentro de su rango
    Pedido GenerarPedido()
    {
        if (itemsPosibles == null || itemsPosibles.Length == 0)
        {
            Debug.LogWarning("GestorFila: no hay items en 'Items Posibles', el NPC no tendrá pedido");
            return null;
        }

        Item item = itemsPosibles[Random.Range(0, itemsPosibles.Length)];
        // Con números enteros, Random.Range NO incluye el máximo, por eso el +1
        int cantidad = Random.Range(item.cantidadMinima, item.cantidadMaxima + 1);

        return new Pedido(item, cantidad);
    }

    // Algoritmo "Fisher-Yates". Recorre lista desce atras e intercambia 1 por 1 con otro al azar.
    void Barajar<T>(List<T> lista) //lista de cualquier tipo no solo personajes.
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }


}
