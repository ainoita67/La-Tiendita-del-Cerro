using System.Collections;
using UnityEngine;

// Cofre del mapa: se abre al acercarte y muestra el menú del interior.
// Necesita un Box Collider 2D con "Is Trigger" y, para la animación, un Animator
// con los triggers "abrir" y "cerrar".
public class Cofre : MonoBehaviour
{
    public float duracionApertura = 0.6f;   // lo que dura la animación de abrir (se cambia en el Inspector)

    Animator anim;
    bool puedeAbrir = true;   // evita que se abra varias veces seguidas

    void Awake()
    {
        anim = GetComponent<Animator>();   // si no tiene Animator, se abre sin animación
    }

    // Unity lo llama cuando algo entra en la zona del trigger
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.GetComponent<player>() == null) return;   // solo nos interesa el jugador
        if (!puedeAbrir) return;
        if (PanelCofre.instancia == null) return;

        puedeAbrir = false;
        StartCoroutine(Abrir());
    }

    // Al alejarte, vuelve a poder abrirse (para cambiar de item)
    void OnTriggerExit2D(Collider2D otro)
    {
        if (otro.GetComponent<player>() != null) puedeAbrir = true;
    }

    // Corrutina: una función que se puede pausar con yield
    IEnumerator Abrir()
    {
        PanelCofre.instancia.Bloqueado = true;   // el personaje se queda quieto

        if (anim != null)
        {
            anim.SetTrigger("abrir");                          // lanza la animación
            yield return new WaitForSeconds(duracionApertura); // espera a que termine
        }

        PanelCofre.instancia.Abrir(this);   // enseña el menú
    }

    // La llama el PanelCofre al cerrar el menú
    public void AlCerrarPanel()
    {
        if (anim != null) anim.SetTrigger("cerrar");
    }
}