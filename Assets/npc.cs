using System;
using UnityEngine;
using TMPro;

public class npc : MonoBehaviour
{
    public float velocidad = 2f;
    public GameObject bocadillo;   // hijo con el cuadro de diálogo (se oculta al empezar)

    // Lo que quiere este aldeano. Lo asigna el GestorFila al crearlo.
    // Se ve en el Inspector al darle a Play.
    public Pedido pedido;

    [Header("Pedido en el bocadillo")]
    public GameObject contenedorPedido;   // hijo del bocadillo que agrupa icono + texto
    public SpriteRenderer iconoPedido;    // icono del item
    public TMP_Text cantidadPedido;          // cantidad, ej: "x3"
    public TMP_Text precioPedido;          // cantidad, ej: "x3"

    [Header("Bocadillo al terminar el turno")]
    public Sprite spriteBocadilloFinal;          // dialog box (el pequeño, solo para el emote)
    public float posXBocadilloFinal = -1.8f;     // X local a la que se mueve
    // Cuánto se van hacia el fondo bocadillo y ánimo al irse, para quedar detrás de los del siguiente npc
    public float fondoZFinal = 1.5f;

    [Header("Estado de ánimo")]
    public SpriteRenderer animo;   // hijo "animo" que va sobre el diálogo
    public Sprite[] spritesAnimo;  // de más contento a menos; el ÚLTIMO es el de turno terminado

    Animator anim;

    Vector3 destino;
    bool caminando = false;
    Action alLlegar;   // lo que hay que hacer cuando llegue (lo usa la fila)

    void Awake()
    {
        anim = GetComponent<Animator>();
        MostrarBocadillo(false);
        MostrarAnimo(false);
    }

    // Al terminar el turno: el bocadillo se mueve y cambia al pequeño,
    // para que el emote final vaya dentro de él mientras el NPC se va
    public void BocadilloFinal()
    {
        if (bocadillo == null) return;

        // En el bocadillo pequeño solo va el emote, el pedido se quita
        if (contenedorPedido != null)
        {
            contenedorPedido.SetActive(false);
        }

        Vector3 pos = bocadillo.transform.localPosition;
        pos.x = posXBocadilloFinal;
        pos.z += fondoZFinal;
        bocadillo.transform.localPosition = pos;

        // El ánimo también va al fondo delante de su propio bocadillo pero detras del siguiente
        if (animo != null)
        {
            Vector3 posAnimo = animo.transform.localPosition;
            posAnimo.z += fondoZFinal;
            animo.transform.localPosition = posAnimo;
        }

        SpriteRenderer sr = bocadillo.GetComponent<SpriteRenderer>();
        if (sr != null && spriteBocadilloFinal != null)
        {
            sr.sprite = spriteBocadilloFinal;
        }
    }

    // Rellena el icono y la cantidad del pedido dentro del bocadillo
    public void MostrarPedido()
    {
        if (contenedorPedido == null) return;

        // Sin pedido no hay nada que enseñar
        if (pedido == null || pedido.item == null)
        {
            contenedorPedido.SetActive(false);
            return;
        }

        contenedorPedido.SetActive(true);

        if (iconoPedido != null)
        {
            iconoPedido.sprite = pedido.item.icono;
        }

        if (cantidadPedido != null)
        {
            cantidadPedido.text = "x" + pedido.cantidadRestante;   // lo que falta por entregar
        }

        if (precioPedido != null)
        {
            precioPedido.text = "x" + pedido.total;
        }
    }

    public void MostrarAnimo(bool mostrar)
    {
        if (animo != null)
        {
            animo.gameObject.SetActive(mostrar);
        }
    }

    // progreso: 0 = acaba de empezar el turno, 1 = se ha acabado el tiempo.
    // Reparte el turno en tramos iguales, uno por cada sprite (menos el último).
    public void ActualizarAnimo(float progreso)
    {
        if (animo == null || spritesAnimo.Length < 2) return;

        int tramos = spritesAnimo.Length - 1;               // 6 sprites -> 5 tramos
        int indice = Mathf.FloorToInt(progreso * tramos);
        indice = Mathf.Clamp(indice, 0, tramos - 1);        // evita que sea negativo o que llegue al ultimo antes de tiempo

        animo.sprite = spritesAnimo[indice]; //cambia el sprite
    }

    // Pone el último sprite (turno terminado)
    public void AnimoFinal()
    {
        if (animo == null || spritesAnimo.Length == 0) return;
        animo.sprite = spritesAnimo[spritesAnimo.Length - 1];
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
