using System.Collections;
using UnityEngine;
using TMPro;

// Lleva la cuenta de los días: empieza cada día en el GestorFila,
// enseña la pantalla de noche cuando acaba y bloquea el juego tras el último día.
public class GestorDias : MonoBehaviour
{
    public int diasTotales = 5;
    public float duracionNoche = 4f;   // segundos que se ve la pantalla de noche
    public int costeCena = 20;         // lo que cuesta dar de cenar a los gatitos cada noche

    [Header("Pantalla de noche")]
    public GameObject tutorial;
    public GameObject pantallaNoche;   // panel oscuro de la UI
    public TMP_Text textoDia;          // "Fin del día 1"
    public TMP_Text textoMonedas;      // "Monedas: 20"

    [Header("HUD (lo que se ve durante el día)")]
    public TMP_Text textoDiaHud;          // el texto "dia" junto a las monedas

    [Header("Fin de la partida")]
    public GameObject pantallaFin;     // (opcional) se enseña tras el último día
    public GameObject pantallaSinMonedas;   // "Fin del juego, te has quedado sin monedas"

    public bool partidaPerdida { get; private set; }

    public int diaActual { get; private set; }

    // Para poder usarlo desde cualquier script: GestorDias.instancia.TerminarDia();
    public static GestorDias instancia;

    void Awake()
    {
        instancia = this;
    }

    IEnumerator Start()
    {
        Time.timeScale = 1f;   // por si venía bloqueado de antes
        if (pantallaNoche != null) pantallaNoche.SetActive(false);
        if (pantallaFin != null) pantallaFin.SetActive(false);
        if (pantallaSinMonedas != null) pantallaSinMonedas.SetActive(false);

        partidaPerdida = false;
        diaActual = 1;
        yield return StartCoroutine(MostrarTutorial());
        EmpezarDia();
    }

    void EmpezarDia()
    {
        if (textoDiaHud != null) textoDiaHud.text = "dia " + diaActual;

        // Cada mañana el cofre se rellena
        if (Inventario.instancia != null) Inventario.instancia.NuevoDia();
        GestorFila.instancia.EmpezarDia();
    }

    // La llama el GestorFila cuando el último NPC del día sale de la pantalla
    public void TerminarDia()
    {
        StartCoroutine(Noche());
    }

    IEnumerator Noche()
    {
        // Se paga la cena de los gatitos antes de enseñar las monedas, para que salgan ya descontadas
        if (ContadorMonedas.instancia != null)
        {
            ContadorMonedas.instancia.Gastar(costeCena);
        }

        // Si con la cena se ha quedado en negativo, ya se ha perdido: no sale la noche
        if (partidaPerdida) yield break;

        // Pantalla de noche con el resumen del día
        if (textoDia != null) textoDia.text = "Termina el dia " + diaActual;
        if (textoMonedas != null && ContadorMonedas.instancia != null)
        {
            textoMonedas.text = "x" + ContadorMonedas.instancia.monedas;
        }
        if (pantallaNoche != null) pantallaNoche.SetActive(true);

        yield return new WaitForSeconds(duracionNoche);

        // ¿Era el último día? Se acaba la partida
        // if (diaActual >= diasTotales)
        // {
        //     TerminarPartida();
        //     yield break;   // corta la corrutina aquí
        // }

        // Si no, empieza el día siguiente
        if (pantallaNoche != null) pantallaNoche.SetActive(false);
        diaActual++;
        EmpezarDia();
    }

    // La llama ContadorMonedas cuando las monedas bajan de 0
    public void PerderPartida()
    {
        if (partidaPerdida) return;   // por si se llama dos veces
        partidaPerdida = true;

        if (pantallaNoche != null) pantallaNoche.SetActive(false);
        if (pantallaSinMonedas != null) pantallaSinMonedas.SetActive(true);

        // Congela el juego
        Time.timeScale = 0f;
    }

    // void TerminarPartida()
    // {
    //     // Si hay pantalla de fin se enseña; si no, se queda la de noche
    //     if (pantallaFin != null)
    //     {
    //         if (pantallaNoche != null) pantallaNoche.SetActive(false);
    //         if (pantallaFin != null) pantallaFin.SetActive(true);
    //     }

    //     // Congela el juego: todo lo que usa el tiempo (movimiento, animaciones...) se para
    //     Time.timeScale = 0f;
    // }

    IEnumerator MostrarTutorial(){
        if (tutorial != null) tutorial.SetActive(true);
        yield return new WaitForSeconds(10f);
        if (tutorial != null) tutorial.SetActive(false);

    }

}
