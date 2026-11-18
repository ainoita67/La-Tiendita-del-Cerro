using UnityEngine;
using TMPro;

public class ContadorMonedas : MonoBehaviour
{
    // Para poder usarlo desde cualquier script: ContadorMonedas.instancia.Sumar(5);
    public static ContadorMonedas instancia;

    public int monedas = 0;
    public TMP_Text texto;   // el texto que va encima del elemento Monedas

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        ActualizarTexto();
    }

    public void Sumar(int cantidad)
    {
        monedas += cantidad;
        ActualizarTexto();
    }

    public void Gastar(int cantidad)
    {
        monedas -= cantidad;
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (texto != null)
        {
            texto.text = monedas.ToString();
        }
    }

}
