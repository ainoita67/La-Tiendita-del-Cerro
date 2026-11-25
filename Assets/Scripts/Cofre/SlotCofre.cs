using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Una casilla del inventario del cofre.
public class SlotCofre : MonoBehaviour
{
    public Item item;              // qué item es (puede estar vacío si aún no existe)
    public int nivelMinimo = 1;    // nivel a partir del cual se puede elegir

    [Header("Partes de la casilla")]
    public Image icono;
    public TMP_Text textoNombre;
    public TMP_Text textoCantidad;
    public TMP_Text textoNivel;
    public GameObject marco;       // borde amarillo: dónde estás

    [Header("Colores")]
    public Color colorActivo = Color.white;
    public Color colorBloqueado = new Color(0.35f, 0.35f, 0.35f, 1f);   // gris

    public bool Bloqueado { get; private set; }
    public int Cantidad { get; private set; }

    // Solo se puede elegir si está desbloqueado y queda alguna unidad
    public bool PuedeElegirse
    {
        get { return !Bloqueado && Cantidad > 0; }
    }

    // Se llama cada vez que se abre el cofre para refrescar lo que se ve
    public void Refrescar(int nivelActual)
    {
        // Los items de las vacas no miran el nivel, solo si se han comprado
        bool necesitaVacas = item != null && item.necesitaVacas;
        bool desbloqueado = necesitaVacas ? item.Desbloqueado() : nivelActual >= nivelMinimo;
        Bloqueado = item == null || !desbloqueado;

        Cantidad = 0;
        if (item != null && Inventario.instancia != null)
        {
            Cantidad = Inventario.instancia.Cantidad(item);
        }

        Color color = Bloqueado ? colorBloqueado : colorActivo;

        if (textoNivel != null) textoNivel.text = necesitaVacas ? "Vacas" : "Nivel " + nivelMinimo;
        if (textoCantidad != null) textoCantidad.text = Bloqueado ? "" : "x" + Cantidad;

        if (textoNombre != null)
        {
            textoNombre.text = item != null ? item.nombre : "";
            textoNombre.color = color;
        }

        if (icono != null)
        {
            // Si el item aún no tiene icono, la casilla solo enseña el nombre
            icono.enabled = item != null && item.icono != null;
            if (icono.enabled) icono.sprite = item.icono;
            icono.color = color;
        }
    }

    public void Marcar(bool marcado)
    {
        if (marco != null) marco.SetActive(marcado);
    }
}
