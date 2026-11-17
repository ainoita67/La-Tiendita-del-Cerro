using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorFila : MonoBehaviour
{
    [Header("NPCs")] // Organizacion en el menú de Unity
    public npc prefabNpc;                              // el prefab NPC
    public AnimatorOverrideController[] personajes;    // los .overrideController de los NPCs
    public int cantidadInicial = 4;
    public float esperaEntreApariciones = 1f;          // para que no salgan todos a la vez

    [Header("Puntos de la escena")]
    public Transform entrada;      // borde por donde aparecen (detrás del último puesto)
    public Transform salida;       // borde por donde se van (delante del primer puesto)
    public Transform[] puestos;    // puestos[0] = el primero de la fila

    [Header("Turno")]
    public float duracionTurno = 60f;

    public float tiempoRestante { get; private set; }

    List<npc> fila = new List<npc>();
    List<AnimatorOverrideController> disponibles = new List<AnimatorOverrideController>();
    bool turnoEnCurso = false;

    void Start()
    {
        // Copia los personajes y los desordena para sacarlos al azar sin repetir
        disponibles.AddRange(personajes);
        Barajar(disponibles);

        StartCoroutine(CrearFilaInicial());
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

    // Crea un NPC en la entrada y lo manda al último puesto libre
    bool AnnadirNpc()
    {
        if (disponibles.Count == 0 || fila.Count >= puestos.Length) return false;

        AnimatorOverrideController personaje = disponibles[0];
        disponibles.RemoveAt(0);

        // X e Y de la entrada, pero la Z que tenga guardada el prefab
        Vector3 posicion = new Vector3(entrada.position.x, entrada.position.y, prefabNpc.transform.position.z);
        npc nuevo = Instantiate(prefabNpc, posicion, Quaternion.identity);
        nuevo.name = "NPC_" + personaje.name;
        nuevo.PonerPersonaje(personaje);

        fila.Add(nuevo);
        MandarASuPuesto(nuevo);
        return true;
    }

    void MandarASuPuesto(npc n)
    {
        int puesto = fila.IndexOf(n);
        n.IrA(puestos[puesto].position, () => AlLlegarAlPuesto(n));
    }

    // Se llama cada vez que un NPC llega a su puesto
    void AlLlegarAlPuesto(npc n)
    {
        // Solo empieza turno si es el primero de la fila y no hay otro turno en marcha
        if (fila.Count > 0 && fila[0] == n && !turnoEnCurso)
        {
            StartCoroutine(Turno(n));
        }
    }

    IEnumerator Turno(npc n)
    {
        turnoEnCurso = true;
        n.MostrarBocadillo(true);

        tiempoRestante = duracionTurno;
        while (tiempoRestante > 0f)
        {
            tiempoRestante -= Time.deltaTime;
            yield return null;
        }
        tiempoRestante = 0f;

        // Se acaba el turno: se va por la salida y desaparece al llegar
        n.MostrarBocadillo(false);
        fila.Remove(n);
        n.IrA(salida.position, () => Destroy(n.gameObject));

        turnoEnCurso = false;

        // El resto avanza un puesto
        foreach (npc otro in fila)
        {
            MandarASuPuesto(otro);
        }
    }

    void Barajar<T>(List<T> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (lista[i], lista[j]) = (lista[j], lista[i]);
        }
    }


}
