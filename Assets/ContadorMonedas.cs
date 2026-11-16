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

    // Devuelve true si había monedas suficientes y las gasta; false si no
    public bool Gastar(int cantidad)
    {
        if (monedas < cantidad)
        {
            return false;
        }

        monedas -= cantidad;
        ActualizarTexto();
        return true;
    }

    void ActualizarTexto()
    {
        if (texto != null)
        {
            texto.text = monedas.ToString();
        }
    }

    // Para que al cambiar el número en el Inspector se vea también en el texto
    void OnValidate()
    {
        ActualizarTexto();
    }
}
