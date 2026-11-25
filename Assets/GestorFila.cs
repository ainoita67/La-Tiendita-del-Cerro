using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorFila : MonoBehaviour
{
    // Para poder usarlo desde cualquier script: GestorFila.instancia.EmpezarDia();
    public static GestorFila instancia;

    void Awake()
    {
        instancia = this;
    }

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

    // Para saber cuándo se acaba el día
    int npcsEnEscena = 0;          // creados y todavía no destruidos (incluye los que se están yendo)
    bool creandoFila = false;      // true mientras siguen apareciendo NPCs

    // Lo llama el GestorDias al empezar cada día.
    public void EmpezarDia()
    {
        // Cada día se vuelven a barajar todos los personajes
        disponibles.Clear();
        disponibles.AddRange(personajes);
        Barajar(disponibles);

        StartCoroutine(CrearFilaInicial()); //ejecuta crarfilainicial que es una corrutina (funcion que se puede pausar y continuar)
    }

    IEnumerator CrearFilaInicial()
    {
        creandoFila = true;
        int cuantos = Mathf.Min(cantidadInicial, puestos.Length);
        for (int i = 0; i < cuantos; i++)
        {
            if (!AnnadirNpc()) break;
            yield return new WaitForSeconds(esperaEntreApariciones);
        }
        creandoFila = false;
    }

    // Se llama cuando un NPC llega a la salida
    void AlSalirNpc(npc n)
    {
        Destroy(n.gameObject);
        npcsEnEscena--;

        // Era el último del día: no queda nadie y no van a salir más
        if (npcsEnEscena == 0 && !creandoFila)
        {
            GestorDias.instancia.TerminarDia();
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

        fila.Add(nuevo);
        npcsEnEscena++;
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
        // Se espera hasta que se acabe el tiempo O el jugador le haya dado todo
        while (tiempoRestante > 0f && !PedidoCompletado(n))
        {
            tiempoRestante -= Time.deltaTime;

            // progreso es un porcentaje a partir del cual actualizar animo calcula el sprite necesario
            float progreso = 1f - tiempoRestante / duracionTurno;
            n.ActualizarAnimo(progreso);
            if (progreso > 0.6f && n.pedido.propina)
            {
                n.pedido.propina = false;
                n.pedido.total -= 2;
                n.MostrarPedido();
            }

            yield return null;
        }
        tiempoRestante = 0f;

        if (!PedidoCompletado(n))
        {
            // No le han atendido a tiempo: se pierde la mitad de lo que valía el pedido
            // y se va con la cara final
            CobrarPenalizacion(n);
            n.AnimoFinal();
        }
        else
        {
            // Lo ha recibido todo: paga y se va con la cara que tenía en ese momento (no se toca el ánimo)
            Cobrar(n);
        }

        // Se acaba el turno: bocadillo pequeño con el emote, se va por la salida y desaparece al llegar
        n.BocadilloFinal();
        fila.Remove(n);
        n.IrA(salida.position, () => AlSalirNpc(n));

        turnoEnCurso = false;

        // El resto avanza un puesto
        foreach (npc otro in fila)
        {
            MandarASuPuesto(otro);
        }
    }

    // El NPC que está hablando ahora mismo (null si no hay turno en marcha)
    public npc NpcConTurno()
    {
        if (!turnoEnCurso || fila.Count == 0) return null;
        return fila[0];
    }

    // true si el NPC ya ha recibido todo lo que pedía
    bool PedidoCompletado(npc n)
    {
        return n.pedido != null && n.pedido.Completado();
    }

    // La llama la ZonaEntrega: el jugador da 1 item al NPC que tiene el turno
    public void EntregarItem()
    {
        // Con la partida perdida ya no se puede entregar nada
        // Por si aparece tarde la pantalla para que no te deje corregir rapido y no perder
        if (GestorDias.instancia != null && GestorDias.instancia.partidaPerdida) return;

        npc n = NpcConTurno();
        if (n == null || n.pedido == null) return;

        // Solo se puede entregar lo que llevas en brazos, y tiene que ser lo que pide el aldeano
        if (Inventario.instancia == null || Inventario.instancia.llevando != n.pedido.item) return;

        if (n.pedido.RestarItem())
        {
            Inventario.instancia.SoltarLlevado();   // se entrega 1 y los brazos quedan libres
            n.MostrarPedido();                      // refresca el bocadillo
        }
    }

    // El NPC paga el precio total de su pedido
    void Cobrar(npc n)
    {
        if (n.pedido == null || ContadorMonedas.instancia == null) return;

        ContadorMonedas.instancia.Sumar(n.pedido.total);
    }

    // Resta de las monedas la mitad del precio total del pedido (redondeando hacia abajo)
    void CobrarPenalizacion(npc n)
    {
        if (n.pedido == null || n.pedido.item == null || ContadorMonedas.instancia == null) return;

        int penalizacion = Mathf.FloorToInt(n.pedido.PrecioTotal() / 2f);
        ContadorMonedas.instancia.Gastar(penalizacion);
    }

    // Elige un item al azar de los posibles y una cantidad al azar dentro de su rango
    Pedido GenerarPedido()
    {
        if (itemsPosibles == null || itemsPosibles.Length == 0)
        {
            return null;
        }

        // Solo se puede pedir lo que ya esté desbloqueado
        List<Item> desbloqueados = new List<Item>();
        foreach (Item i in itemsPosibles)
        {
            if (i != null && i.Desbloqueado()) desbloqueados.Add(i);
        }
        if (desbloqueados.Count == 0) return null;

        Item item = desbloqueados[Random.Range(0, desbloqueados.Count)];
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
