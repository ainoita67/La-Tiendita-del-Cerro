using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Menú "Tiendita" del editor de Unity.
// 1) Crea de golpe todo el interior del cofre (menú, casillas, inventario, cofre del mapa, item en brazos).
// 2) Parchea los scripts de Ainoa (player, GestorFila, GestorDias) para conectarlos con el cofre.
// Este archivo tiene que estar dentro de una carpeta llamada "Editor".
public static class CrearCofre
{
    const string CarpetaItems = "Assets/Items";

    static readonly Color Amarillo = new Color(1f, 0.85f, 0.2f, 1f);
    static readonly Color Marron = new Color(0.55f, 0.38f, 0.22f, 1f);
    static readonly Color MarronOscuro = new Color(0.22f, 0.14f, 0.09f, 0.97f);

    // ------------------------------------------------------------------
    // 1) CREAR TODO
    // ------------------------------------------------------------------
    [MenuItem("Tiendita/1. Crear cofre, menu e inventario")]
    static void CrearTodo()
    {
        if (Object.FindObjectOfType<PanelCofre>(true) != null)
        {
            EditorUtility.DisplayDialog("Tiendita",
                "Ya existe un GestorCofre en la escena.\nBorra PanelCofre, GestorCofre, Inventario y Cofre si quieres crearlos de nuevo.",
                "Vale");
            return;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) canvas = CrearCanvas();

        // --- Items (usa los que ya existen o crea los que falten) ---
        Item trigo = BuscarOCrearItem("Trigo");
        Item leche = BuscarOCrearItem("Leche");
        Item lana = BuscarOCrearItem("Lana");
        Item[] items = { trigo, leche, lana, null, null, null, null, null };

        // --- Panel del cofre ---
        RectTransform panel = NuevoRect("PanelCofre", canvas.transform);
        Estirar(panel, 0);
        Image fondoPanel = panel.gameObject.AddComponent<Image>();
        fondoPanel.color = MarronOscuro;
        panel.SetAsLastSibling();   // se dibuja por encima del resto de la interfaz
        Undo.RegisterCreatedObjectUndo(panel.gameObject, "Crear PanelCofre");

        NuevoTexto("Titulo", panel, "Inventario del cofre", 64, TextAlignmentOptions.Center,
            new Vector2(0f, 0.86f), new Vector2(1f, 1f));

        // --- Cuadricula de 4 columnas ---
        RectTransform cuadricula = NuevoRect("Cuadricula", panel);
        cuadricula.anchorMin = new Vector2(0.5f, 0.55f);
        cuadricula.anchorMax = new Vector2(0.5f, 0.55f);
        cuadricula.sizeDelta = new Vector2(960, 450);
        GridLayoutGroup grid = cuadricula.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 200);
        grid.spacing = new Vector2(30, 30);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.childAlignment = TextAnchor.MiddleCenter;

        // --- 8 casillas ---
        SlotCofre[] slots = new SlotCofre[8];
        for (int i = 0; i < 8; i++)
        {
            RectTransform slotRt = NuevoRect("Slot" + (i + 1), cuadricula);
            SlotCofre slot = slotRt.gameObject.AddComponent<SlotCofre>();

            Image marco = NuevaImagen("Marco", slotRt, Amarillo);
            Estirar(marco.rectTransform, -8);
            NuevaImagen("Fondo", slotRt, Marron);

            Image icono = NuevaImagen("Icono", slotRt, Color.white);
            icono.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            icono.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icono.rectTransform.sizeDelta = new Vector2(100, 100);
            icono.rectTransform.anchoredPosition = new Vector2(0, -4);
            icono.preserveAspect = true;

            TMP_Text nombre = NuevoTexto("TextoNombre", slotRt, "", 30, TextAlignmentOptions.Center,
                new Vector2(0f, 0.78f), new Vector2(1f, 1f));
            TMP_Text cantidad = NuevoTexto("TextoCantidad", slotRt, "", 30, TextAlignmentOptions.MidlineRight,
                new Vector2(0.45f, 0f), new Vector2(1f, 0.22f));
            TMP_Text nivel = NuevoTexto("TextoNivel", slotRt, "Nivel " + (i + 1), 24, TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f), new Vector2(0.55f, 0.22f));

            slot.item = items[i];
            slot.nivelMinimo = i + 1;
            slot.icono = icono;
            slot.textoNombre = nombre;
            slot.textoCantidad = cantidad;
            slot.textoNivel = nivel;
            slot.marco = marco.gameObject;
            marco.gameObject.SetActive(false);

            slots[i] = slot;
        }

        // --- Botón Salir ---
        RectTransform salir = NuevoRect("Salir", panel);
        salir.anchorMin = new Vector2(0.5f, 0.09f);
        salir.anchorMax = new Vector2(0.5f, 0.09f);
        salir.sizeDelta = new Vector2(360, 80);
        Image salirMarco = NuevaImagen("Marco", salir, Amarillo);
        Estirar(salirMarco.rectTransform, -8);
        NuevaImagen("Fondo", salir, Marron);
        NuevoTexto("Texto", salir, "Salir", 44, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        salirMarco.gameObject.SetActive(false);

        // --- GestorCofre (el script que controla el menú) ---
        GameObject gestor = new GameObject("GestorCofre");
        PanelCofre pc = gestor.AddComponent<PanelCofre>();
        pc.panel = panel.gameObject;
        pc.slots = slots;
        pc.marcoSalir = salirMarco.gameObject;
        pc.columnas = 4;
        Undo.RegisterCreatedObjectUndo(gestor, "Crear GestorCofre");

        // --- Inventario (lo que aparece en el cofre cada mañana) ---
        GameObject invGo = new GameObject("Inventario");
        Inventario inv = invGo.AddComponent<Inventario>();
        inv.diaria = new Inventario.Reposicion[]
        {
            new Inventario.Reposicion { item = trigo, cantidad = 28 },
            new Inventario.Reposicion { item = leche, cantidad = 28 },
            new Inventario.Reposicion { item = lana, cantidad = 28 }
        };
        Undo.RegisterCreatedObjectUndo(invGo, "Crear Inventario");

        // --- Cofre del mapa ---
        GameObject cofre = new GameObject("Cofre");
        SpriteRenderer sr = cofre.AddComponent<SpriteRenderer>();
        sr.sprite = PrimerSprite("Assets/inventario/cofre.png");
        sr.sortingOrder = 3;
        BoxCollider2D col = cofre.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = sr.sprite != null ? (Vector2)sr.sprite.bounds.size + Vector2.one * 0.8f : Vector2.one * 2f;
        cofre.AddComponent<Cofre>();
        SceneView vista = SceneView.lastActiveSceneView;
        Vector3 pos = vista != null ? vista.pivot : Vector3.zero;
        pos.z = 0;
        cofre.transform.position = pos;
        Undo.RegisterCreatedObjectUndo(cofre, "Crear Cofre");

        // --- Item en brazos del jugador ---
        player jugador = Object.FindObjectOfType<player>();
        string avisoJugador = "";
        if (jugador != null)
        {
            GameObject brazos = new GameObject("ItemEnBrazos", typeof(SpriteRenderer), typeof(ItemEnBrazos));
            brazos.transform.SetParent(jugador.transform, false);
            brazos.transform.localPosition = new Vector3(0, 0.45f, 0);
            brazos.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            SpriteRenderer psr = jugador.GetComponent<SpriteRenderer>();
            brazos.GetComponent<SpriteRenderer>().sortingOrder = (psr != null ? psr.sortingOrder : 0) + 1;
            Undo.RegisterCreatedObjectUndo(brazos, "Crear ItemEnBrazos");
        }
        else
        {
            avisoJugador = "\n\nAVISO: no he encontrado el jugador en la escena, así que no he creado ItemEnBrazos.";
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = cofre;

        EditorUtility.DisplayDialog("Tiendita",
            "Listo. Ahora:\n" +
            "1) Arrastra el objeto 'Cofre' dentro de la casita.\n" +
            "2) Ajusta el Box Collider 2D del cofre para que no llegue a la puerta ni a las paredes.\n" +
            "3) Guarda la escena (Ctrl+S).\n" +
            "4) Prueba con Play (con E se abre el menú sin cofre)." + avisoJugador,
            "Vale");
    }

    // ------------------------------------------------------------------
    // 2) PARCHEAR LOS SCRIPTS DE AINOA
    // ------------------------------------------------------------------
    [MenuItem("Tiendita/2. Conectar scripts de Ainoa (player, GestorFila, GestorDias)")]
    static void ParchearScripts()
    {
        string r1 = Parchear("player", "CofreOcupado", new string[][]
        {
            // Método nuevo justo antes de LeerPickup
            new string[]
            {
                "    void LeerPickup()\n",
                "    // true mientras el cofre se abre o está abierto: el personaje se queda quieto\n" +
                "    bool CofreOcupado()\n" +
                "    {\n" +
                "        return PanelCofre.instancia != null && PanelCofre.instancia.Bloqueado;\n" +
                "    }\n\n" +
                "    void LeerPickup()\n"
            },
            // Con el cofre abierto, Espacio sirve para elegir, no para coger
            new string[]
            {
                "if (Input.GetKeyDown(KeyCode.Space))",
                "if (Input.GetKeyDown(KeyCode.Space) && !CofreOcupado())"
            },
            // Con el cofre abierto no se mueve
            new string[]
            {
                "        movimiento = Vector3.zero;\n",
                "        movimiento = Vector3.zero;\n\n" +
                "        // Con el cofre abierto no se mueve\n" +
                "        if (CofreOcupado()) return;\n"
            }
        });

        string r2 = Parchear("GestorFila", "Inventario.instancia", new string[][]
        {
            new string[]
            {
                "        if (n.pedido.RestarItem())\n" +
                "        {\n" +
                "            n.MostrarPedido();   // refresca el bocadillo\n" +
                "        }\n",

                "        // Solo se puede entregar lo que llevas en brazos, y tiene que ser lo que pide el aldeano\n" +
                "        if (Inventario.instancia == null || Inventario.instancia.llevando != n.pedido.item) return;\n\n" +
                "        if (n.pedido.RestarItem())\n" +
                "        {\n" +
                "            Inventario.instancia.SoltarLlevado();   // se entrega 1 y los brazos quedan libres\n" +
                "            n.MostrarPedido();                      // refresca el bocadillo\n" +
                "        }\n"
            }
        });

        string r3 = Parchear("GestorDias", "Inventario.instancia", new string[][]
        {
            new string[]
            {
                "        if (textoDiaHud != null) textoDiaHud.text = \"dia \" + diaActual;\n",
                "        if (textoDiaHud != null) textoDiaHud.text = \"dia \" + diaActual;\n\n" +
                "        // Cada mañana el cofre se rellena\n" +
                "        if (Inventario.instancia != null) Inventario.instancia.NuevoDia();\n"
            }
        });

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Tiendita", r1 + "\n\n" + r2 + "\n\n" + r3, "Vale");
    }

    // ------------------------------------------------------------------
    // AYUDAS
    // ------------------------------------------------------------------
    static string Parchear(string nombreScript, string yaParcheado, string[][] cambios)
    {
        string ruta = RutaScript(nombreScript);
        if (ruta == null) return nombreScript + ".cs: no lo encuentro en el proyecto.";

        string texto = File.ReadAllText(ruta);
        if (texto.Contains(yaParcheado)) return nombreScript + ".cs: ya estaba conectado, no toco nada.";

        bool crlf = texto.Contains("\r\n");
        string t = texto.Replace("\r\n", "\n");

        foreach (string[] c in cambios)
        {
            if (!t.Contains(c[0]))
            {
                return nombreScript + ".cs: NO he encontrado este trozo (¿lo ha cambiado Ainoa?), hay que hacerlo a mano:\n" + c[0];
            }
            t = t.Replace(c[0], c[1]);
        }

        if (crlf) t = t.Replace("\n", "\r\n");
        File.WriteAllText(ruta, t);
        return nombreScript + ".cs: conectado correctamente.";
    }

    static string RutaScript(string nombre)
    {
        foreach (string guid in AssetDatabase.FindAssets(nombre + " t:MonoScript"))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(ruta) == nombre) return ruta;
        }
        return null;
    }

    static Item BuscarOCrearItem(string clave)
    {
        string c = clave.ToLower();

        foreach (string guid in AssetDatabase.FindAssets("t:Item"))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            Item it = AssetDatabase.LoadAssetAtPath<Item>(ruta);
            if (it == null) continue;

            string nombreArchivo = Path.GetFileNameWithoutExtension(ruta).ToLower();
            string nombreItem = string.IsNullOrEmpty(it.nombre) ? "" : it.nombre.ToLower();

            if (nombreArchivo.Contains(c) || nombreItem.Contains(c))
            {
                if (string.IsNullOrEmpty(it.nombre))
                {
                    it.nombre = clave;
                    EditorUtility.SetDirty(it);
                }
                return it;
            }
        }

        if (!AssetDatabase.IsValidFolder(CarpetaItems)) AssetDatabase.CreateFolder("Assets", "Items");

        Item nuevo = ScriptableObject.CreateInstance<Item>();
        nuevo.nombre = clave;
        nuevo.precio = 2;
        AssetDatabase.CreateAsset(nuevo, CarpetaItems + "/Item" + clave + ".asset");
        return nuevo;
    }

    static Sprite PrimerSprite(string ruta)
    {
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(ruta))
        {
            if (o is Sprite s) return s;
        }
        return null;
    }

    static Canvas CrearCanvas()
    {
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas c = go.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = go.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        Undo.RegisterCreatedObjectUndo(go, "Crear Canvas");
        return c;
    }

    static RectTransform NuevoRect(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    // Estira el elemento para que llene a su padre, con un margen (negativo = sobresale)
    static void Estirar(RectTransform r, float margen)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(margen, margen);
        r.offsetMax = new Vector2(-margen, -margen);
    }

    static Image NuevaImagen(string nombre, Transform padre, Color color)
    {
        RectTransform r = NuevoRect(nombre, padre);
        Estirar(r, 0);
        Image img = r.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TMP_Text NuevoTexto(string nombre, Transform padre, string texto, float tamano,
        TextAlignmentOptions alineacion, Vector2 anclaMin, Vector2 anclaMax)
    {
        RectTransform r = NuevoRect(nombre, padre);
        r.anchorMin = anclaMin;
        r.anchorMax = anclaMax;
        r.offsetMin = new Vector2(8, 4);
        r.offsetMax = new Vector2(-8, -4);

        TextMeshProUGUI t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = texto;
        t.fontSize = tamano;
        t.alignment = alineacion;
        t.color = Color.white;
        t.raycastTarget = false;
        return t;
    }
}
