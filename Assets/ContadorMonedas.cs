using System.Collections;
using UnityEngine;
using TMPro;

public class ContadorMonedas : MonoBehaviour
{
    // Para poder usarlo desde cualquier script: ContadorMonedas.instancia.Sumar(5);
    public static ContadorMonedas instancia;

    public int monedas = 0;
    public TMP_Text texto;   // el texto que va encima del elemento Monedas

    public TMP_Text textoCambio;   // el texto junto al contador
    float duracionCambio = 1.5f;       // segundos que se ve
    // new Color(rojo, verde, azul) con valores de 0 a 1
    // Oscuros para que contrasten con el fondo verde claro
    Color colorSumar = new Color(0.12f, 0.23f, 0.42f);    // azul marino apagado (#1F3B6B)
    Color colorRestar = new Color(0.40f, 0.09f, 0.14f);   // granato oscuro (#661724)

    Coroutine ocultarCambio;   // para poder cancelarla si llega otro cambio antes de que se oculte

    void Awake()
    {
        instancia = this;
        if (textoCambio != null) textoCambio.gameObject.SetActive(false);
    }

    void Start()
    {
        ActualizarTexto();
    }

    public void Sumar(int cantidad)
    {
        monedas += cantidad;
        ActualizarTexto();
        MostrarCambio("+" + cantidad, colorSumar);
    }

    public void Gastar(int cantidad)
    {
        monedas -= cantidad;
        ActualizarTexto();

        // Llegar a -1 es perder la partida
        if (monedas < 0 && GestorDias.instancia != null)
        {
            GestorDias.instancia.PerderPartida();
        }
        MostrarCambio("-" + cantidad, colorRestar);
    }

    void ActualizarTexto()
    {
        if (texto != null)
        {
            texto.text = monedas.ToString();
        }
    }

    // Enseña el "+5" o "-3" y lo esconde pasados unos segundos
    void MostrarCambio(string mensaje, Color color)
    {
        if (textoCambio == null) return;

        textoCambio.text = mensaje;
        textoCambio.color = color;
        textoCambio.gameObject.SetActive(true);

        // Si había uno anterior esperando para ocultarse, se cancela y empieza la cuenta otra vez
        if (ocultarCambio != null) StopCoroutine(ocultarCambio);
        ocultarCambio = StartCoroutine(OcultarCambio());
    }

    IEnumerator OcultarCambio()
    {
        yield return new WaitForSeconds(duracionCambio);
        textoCambio.gameObject.SetActive(false);
        ocultarCambio = null;
    }
}
