using System;
using UnityEngine;

public class npc : MonoBehaviour
{
    public float velocidad = 2f;
    public GameObject bocadillo;   // hijo con el cuadro de diálogo (se oculta al empezar)

    Animator anim;

    Vector3 destino;
    bool caminando = false;
    Action alLlegar;   // lo que hay que hacer cuando llegue (lo usa la fila)

    void Awake()
    {
        anim = GetComponent<Animator>();
        MostrarBocadillo(false);
    }

    public void MostrarBocadillo(bool mostrar)
    {
        if (bocadillo != null)
        {
            bocadillo.SetActive(mostrar);
        }
    }

    // Cambia qué personaje es (mismo prefab, distintas animaciones)
    public void PonerPersonaje(AnimatorOverrideController personaje)
    {
        anim.runtimeAnimatorController = personaje;
    }

    // Asigna el destino y el estaddo de las animaciones. 
    // Se llama en el gestor de filas para indicar los cambios de posicion entre npcs
    // Esas variables se revisan en el Update

    // "alLlegar" es opcional: una función que se ejecuta cuando llegue 
    // Sirve para cuando llegan al principio de la fila sacar el dialogo y cuando llegan a la salida eliminarlos.
    public void IrA(Vector3 posicion, Action alLlegar = null)
    {
        destino = new Vector3(posicion.x, posicion.y, transform.position.z);
        this.alLlegar = alLlegar;
        caminando = true;

        // Mira hacia el lado al que va: -1 izquierda, 1 derecha
        float lado = Mathf.Sign(destino.x - transform.position.x);
        anim.SetFloat("direccionX", lado);
        anim.SetBool("moviendose", true);
    }

    void Update()
    {
        if (!caminando) return;

        transform.position = Vector3.MoveTowards(
            transform.position, destino, velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino) < 0.01f)
        {
            transform.position = destino;
            caminando = false;
            anim.SetBool("moviendose", false);   // vuelve al idle mirando abajo

            // Avisa de que ha llegado (y lo borra para que no se llame dos veces)
            Action aviso = alLlegar;
            alLlegar = null;
            aviso?.Invoke();
        }
    }
}
