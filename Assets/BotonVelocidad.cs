using System.Collections;
using UnityEngine;
using TMPro;

// Botón del suelo: cuando el jugador lo pisa, cambia de sprite y desbloquea las vacas.
// Necesita un SpriteRenderer y un Collider2D con Is Trigger marcado.
public class BotonVelocidad : MonoBehaviour
{
    public Sprite spriteNormal;    // sin pisar
    public Sprite spritePulsado;   // pisado

    public int precio = 20;            // lo que cuesta desbloquear las vacas
    public TMP_Text textoAviso;         // hijo del botón

    SpriteRenderer sr;
    Coroutine ocultarAviso;
    bool pulsado = false;   // para que solo se desbloquee una vez

    public player jugador;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = spriteNormal;
        if (textoAviso != null) textoAviso.gameObject.SetActive(false);
    }

    // Unity lo llama cuando algo entra en el collider (que tiene que ser trigger)
    void OnTriggerEnter2D(Collider2D otro)
    {
        if (pulsado) return;                              // ya se había pisado
        if (otro.GetComponent<player>() == null) return;  // solo cuenta el jugador

        StartCoroutine(PulsarBoton());   // las corrutinas se arrancan así
    }

    IEnumerator PulsarBoton()
    {
        // No llega el dinero: se avisa y se sale sin pulsar el botón
        if (ContadorMonedas.instancia.monedas < precio)
        {
            MostrarAviso();
            yield break;
        }

        ContadorMonedas.instancia.Gastar(precio);
        pulsado = true;
        sr.sprite = spritePulsado;

        // Si el aviso de un intento anterior seguía en pantalla, se quita
        if (textoAviso != null) textoAviso.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        SubirVelocidad();
        this.gameObject.SetActive(false);   // lo último: desactivarlo para sus corrutinas
    }

    void MostrarAviso()
    {
        if (textoAviso == null) return;

        textoAviso.gameObject.SetActive(true);

        // Si ya se estaba mostrando, la cuenta empieza otra vez
        if (ocultarAviso != null) StopCoroutine(ocultarAviso);
        ocultarAviso = StartCoroutine(OcultarAviso());
    }

    IEnumerator OcultarAviso()
    {
        yield return new WaitForSeconds(2f);
        textoAviso.gameObject.SetActive(false);
        ocultarAviso = null;
    }

    void SubirVelocidad()
    {
        jugador.velocidad = 6f;
    }
}
