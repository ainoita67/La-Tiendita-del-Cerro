using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public float velocidad = 2f;

    Vector3 movimiento;
    Animator anim;

    // Hacia dónde mira el personaje (empieza mirando hacia abajo)
    Vector2 ultimaDireccion = new Vector2(0, -1);

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        LeerMovimiento();
        ActualizarAnim();
    }

    void LeerMovimiento()
    {
        movimiento = Vector3.zero;

        if (Input.GetKey(KeyCode.D))
        {
            movimiento += Vector3.right;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movimiento += Vector3.left;
        }

        if (Input.GetKey(KeyCode.W))
        {
            movimiento += Vector3.up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movimiento += Vector3.down;
        }

        // Para que en diagonal no vaya más rápido
        movimiento.Normalize();

        // Guarda la última dirección para que al pararse siga mirando hacia ahí
        if (movimiento != Vector3.zero)
        {
            ultimaDireccion = movimiento;
        }
    }

    void ActualizarAnim()
    {
        anim.SetFloat("direccionX", ultimaDireccion.x);
        anim.SetFloat("direccionY", ultimaDireccion.y);
        anim.SetBool("moviendose", movimiento != Vector3.zero);
    }

    void FixedUpdate()
    {
        transform.position += movimiento * velocidad * Time.fixedDeltaTime;
    }
}
