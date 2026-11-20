using UnityEngine;

// Zona del mapa (el mostrador) donde el jugador entrega items.
// Si el jugador está dentro y pulsa Espacio, se resta 1 al pedido del NPC que tiene el turno.
// Necesita un Collider2D con Is Trigger marcado, y el player necesita Collider2D + Rigidbody2D.
public class ZonaEntrega : MonoBehaviour
{
    bool jugadorDentro = false;

    // Unity lo llama solo cuando un collider entra en el trigger
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponent<player>() != null)   // solo nos interesa el jugador
        {
            jugadorDentro = true;
        }
    }

    // Y este cuando sale
    void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.GetComponent<player>() != null)
        {
            jugadorDentro = false;
        }
    }

    void Update()
    {
        if (jugadorDentro && Input.GetKeyDown(KeyCode.Space))
        {
            GestorFila.instancia.EntregarItem();
        }
    }

    // Dibuja la zona en la vista Scene: azul si está vacía, rojo si el jugador está dentro
    void OnDrawGizmos()
    {
        BoxCollider2D caja = GetComponent<BoxCollider2D>();
        if (caja == null) return;

        Gizmos.color = jugadorDentro ? Color.red : Color.blue;

        // Centro y tamaño reales del collider (teniendo en cuenta offset y escala)
        Gizmos.DrawWireCube(caja.bounds.center, caja.bounds.size);
    }
}
