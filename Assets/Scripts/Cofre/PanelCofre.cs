using UnityEngine;

// Controla el interior del cofre: abrir, moverse con las flechas,
// elegir con Espacio y salir.
public class PanelCofre : MonoBehaviour
{
    // Para usarlo desde cualquier script: PanelCofre.instancia.Bloqueado
    public static PanelCofre instancia;

    public GameObject panel;           // el objeto PanelCofre de la UI
    public SlotCofre[] slots;          // las 8 casillas, en orden (Nivel 1 ... Nivel 8)
    public GameObject marcoSalir;      // borde amarillo del botón Salir
    public int columnas = 4;           // casillas por fila

    [Header("Nivel")]
    // Mientras no se decida la regla de subir de nivel: el nivel es el número de día
    // (día 1 = solo trigo, día 2 = también leche...). Desmárcalo para usar "Nivel Actual" a mano.
    public bool usarDiaComoNivel = true;
    public int nivelActual = 1;
    public static bool vacasDesbloqueadas;

    [Header("Pruebas")]
    public bool teclaDePrueba = true;  // con E se abre el menú sin necesidad del cofre del mapa

    // true desde que el cofre empieza a abrirse hasta que se cierra el menú:
    // durante ese tiempo el personaje se queda quieto
    public bool Bloqueado { get; set; }

    public bool Abierto { get; private set; }

    // 0..slots.Length-1 = casillas, slots.Length = botón Salir
    int seleccion;
    Cofre cofreActual;   // el cofre del mapa que ha abierto el menú (null si es con la tecla de prueba)

    void Awake()
    {
        instancia = this;
        vacasDesbloqueadas = false;
    }

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (!Abierto)
        {
            if (teclaDePrueba && !Bloqueado && Input.GetKeyDown(KeyCode.E)) Abrir(null);
            return;
        }

        Mover();

        if (Input.GetKeyDown(KeyCode.Space)) Elegir();
    }

    int NivelAhora()
    {
        int nivel = nivelActual;
        if (usarDiaComoNivel && GestorDias.instancia != null) nivel = GestorDias.instancia.diaActual;
        if (vacasDesbloqueadas && nivel < 2) nivel = 2;   // con las vacas, la leche ya se puede coger
        return nivel;
    }

    // La llama el Cofre del mapa cuando termina su animación de abrirse
    public void Abrir(Cofre cofre)
    {
        cofreActual = cofre;
        Abierto = true;
        Bloqueado = true;
        panel.SetActive(true);

        int nivel = NivelAhora();
        foreach (SlotCofre s in slots)
        {
            s.Refrescar(nivel);
        }

        seleccion = 0;
        MarcarSeleccion();
    }

    void Cerrar()
    {
        Abierto = false;
        Bloqueado = false;
        panel.SetActive(false);

        // El cofre del mapa hace su animación de cerrarse
        if (cofreActual != null)
        {
            cofreActual.AlCerrarPanel();
            cofreActual = null;
        }
    }

    void Mover()
    {
        int salir = slots.Length;
        int anterior = seleccion;

        if (seleccion == salir)
        {
            // Desde Salir solo se puede subir a la fila de abajo
            if (Input.GetKeyDown(KeyCode.UpArrow)) seleccion = slots.Length - columnas;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && seleccion % columnas < columnas - 1 && seleccion + 1 < slots.Length) seleccion++;
            if (Input.GetKeyDown(KeyCode.LeftArrow) && seleccion % columnas > 0) seleccion--;
            if (Input.GetKeyDown(KeyCode.UpArrow) && seleccion - columnas >= 0) seleccion -= columnas;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (seleccion + columnas < slots.Length) seleccion += columnas;
                else seleccion = salir;   // si no hay más filas, baja al botón Salir
            }
        }

        if (seleccion != anterior) MarcarSeleccion();
    }

    void MarcarSeleccion()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Marcar(i == seleccion);
        }
        marcoSalir.SetActive(seleccion == slots.Length);
    }

    void Elegir()
    {
        // Salir: cierra sin cambiar lo que llevas en brazos
        if (seleccion == slots.Length)
        {
            Cerrar();
            return;
        }

        SlotCofre slot = slots[seleccion];
        if (!slot.PuedeElegirse) return;   // en gris o sin existencias: no hace nada

        // Si se ha podido coger (devolviendo antes lo que llevabas), el menú se cierra
        if (Inventario.instancia.TomarUno(slot.item))
        {
            Cerrar();
        }
    }
}
