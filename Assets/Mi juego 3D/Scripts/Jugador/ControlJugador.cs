using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ControlJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad;
    public float fuerzaSalto;

    [Header("Camara")]
    public float sensibilidadRaton;
    public float maxVistaX;
    public float minVistaX;
    private float rotacionX;

    private Camera camara;
    private Rigidbody fisica;
    private ControlArma arma; // Se asinga cuando el Inventario llama a EquiparArma()

    [Header("Vidas")]
    public int vidasActual;
    public int vidasMax;

    // --- NUEVA VARIABLE PARA LA ANIMACIÓN ---
    private Animator animator;

    public void Start()
    {
        Time.timeScale = 1.0f;

        // Solo actualizamos la vida y puntuación, las balas las actualiza EquiparArma()
        ControlHUD.instancia.actualizaBarraVida(vidasActual, vidasMax);
        ControlHUD.instancia.actualizarPuntuacion(0);
    }

    private void Awake()
    {
        camara = Camera.main;
        fisica = GetComponent<Rigidbody>();

        // Obtenemos el Animator (Asumiendo que el modelo 3D es hijo de este objeto)
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (ControlJuego.instancia.juegoPausado)
        {
            return;
        }

        // Si el jugador ha muerto, ya no puede moverse ni disparar
        if (vidasActual <= 0) return;

        Movimiento();
        VistaCamara();

        if (Input.GetButtonDown("Jump"))
        {
            Salto();
        }

        // Disparo (Comprobamos que el arma exista antes de usarla)
        if (Input.GetButton("Fire1") && arma != null)
        {
            if (arma.PuedeDisparar())
            {
                arma.Disparar();

                // --- LANZAMOS ANIMACIÓN DE DISPARO ---
                if (animator != null) animator.SetTrigger("Disparar");
            }
        }
    }

    // Esta función la llama el script InventarioArmas.cs al cambiar la ruleta del ratón
    public void EquiparArma(ControlArma nuevaArma)
    {
        arma = nuevaArma;
        // Actualizamos el HUD para mostrar las balas del arma nueva
        ControlHUD.instancia.actualizarBalasTexto(arma.municionActual, arma.municionMax);
    }

    private void Salto()
    {
        Ray rayo = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(rayo, 1.1f))
        {
            fisica.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private void VistaCamara()
    {
        float y = Input.GetAxis("Mouse X") * sensibilidadRaton;
        rotacionX += Input.GetAxis("Mouse Y") * sensibilidadRaton;

        rotacionX = Mathf.Clamp(rotacionX, minVistaX, maxVistaX);

        camara.transform.localRotation = Quaternion.Euler(-rotacionX, 0, 0);
        transform.eulerAngles += Vector3.up * y;
    }

    private void Movimiento()
    {
        float movX = Input.GetAxis("Horizontal");
        float movZ = Input.GetAxis("Vertical");

        float x = movX * velocidad;
        float z = movZ * velocidad;

        Vector3 direccion = transform.right * x + transform.forward * z;

        // FIX: Conservamos la velocidad 'Y' para no romper la gravedad ni el salto
        fisica.linearVelocity = new Vector3(direccion.x, fisica.linearVelocity.y, direccion.z);

        // --- ANIMACIÓN DE CORRER ---
        // Calculamos la magnitud del input para saber si estamos quietos o caminando (0 a 1)
        float magnitudMovimiento = new Vector2(movX, movZ).magnitude;
        if (animator != null) animator.SetFloat("Velocidad", magnitudMovimiento * velocidad);
    }

    internal void QuitarVidasJugador(int cantidadVida)
    {
        if (vidasActual <= 0) return;

        vidasActual -= cantidadVida;
        ControlHUD.instancia.actualizaBarraVida(vidasActual, vidasMax);

        // --- ANIMACIÓN DE RECIBIR DAÑO ---
        if (animator != null) animator.SetTrigger("Danio");

        if (vidasActual <= 0)
        {
            TerminaJugador();
        }
    }

    private void TerminaJugador()
    {
        Debug.Log("Game OVER!!!");

        // --- ANIMACIÓN DE MORIR ---
        if (animator != null) animator.SetTrigger("Morir");

        ControlHUD.instancia.establecerVentanaFinJuego(false);
    }

    internal void IncrementaVida(int cantidad)
    {
        vidasActual = Mathf.Clamp(vidasActual + cantidad, 0, vidasMax);
        ControlHUD.instancia.actualizaBarraVida(vidasActual, vidasMax);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto contra el que hemos chocado tiene la etiqueta "Agua"
        if (other.CompareTag("Agua"))
        {
            // Nos quitamos toda la vida máxima de golpe
            QuitarVidasJugador(vidasMax);
        }
    }
}