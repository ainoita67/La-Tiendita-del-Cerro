using UnityEngine;

// Va en un hijo del jugador que tiene un SpriteRenderer.
// Enseña el icono del item que el jugador lleva en brazos.
public class ItemEnBrazos : MonoBehaviour
{
    SpriteRenderer render;

    void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        render.enabled = false;
    }

    void Update()
    {
        Item item = Inventario.instancia != null ? Inventario.instancia.llevando : null;

        // Si el item no tiene icono todavía, no se ve nada (pero sigue contando como llevado)
        render.enabled = item != null && item.icono != null;
        if (render.enabled) render.sprite = item.icono;
    }
}
