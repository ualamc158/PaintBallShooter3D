using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;

// MÁQUINA DE ESTADOS (Requisito RA4.1)
public enum EstadoEnemigo
{
    Patrullando,
    Persiguiendo,
    Atacando
}

public class ControlEnemigoMejorado : MonoBehaviour
{
    [Header("Estadística")]
    public int vidasActual;
    public int vidasMax;
    public int puntuacionEnemigo;

    [Header("Máquina de Estados e IA (RA4.1 y RA4.3)")]
    public EstadoEnemigo estadoActual = EstadoEnemigo.Patrullando;
    public float rangoDeteccion = 15f; // Distancia a la que te ve y deja de patrullar
    public float rangoAtaque = 10f; // Distancia a la que empieza a disparar
    public float distanciaParada = 2f; // Distancia para no chocarse contigo

    [Header("Patrullaje (Waypoints)")]
    public Transform[] waypoints; // Arrastra aquí los puntos vacíos de la escena
    private int waypointActual = 0; // Por cuál punto va ahora mismo
    public float distanciaCambioWaypoint = 1.5f; // A qué distancia da el punto por alcanzado

    [Header("Movimiento")]
    public float velocidadPatrulla = 2f; // Camina más despacio al patrullar
    public float velocidadPersecucion = 5f; // Corre al perseguirte

    private List<Vector3> listaCaminos;
    private ControlArma arma;
    private GameObject objetivo;

    private Animator animator;
    private bool estaMuerto = false;

    void Start()
    {
        arma = GetComponent<ControlArma>();
        objetivo = GameObject.FindGameObjectWithTag("Jugador");
        animator = GetComponentInChildren<Animator>();

        // Iniciamos la rutina de calcular caminos en el NavMesh
        InvokeRepeating("ActualizarCaminos", 0.0f, 0.2f);
    }

    private void Update()
    {
        if (estaMuerto) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, objetivo.transform.position);

        // 1. EVALUAR TRANSICIONES DE ESTADO (Cerebro de la IA)
        if (distanciaAlJugador <= rangoAtaque)
        {
            estadoActual = EstadoEnemigo.Atacando;
        }
        else if (distanciaAlJugador <= rangoDeteccion)
        {
            estadoActual = EstadoEnemigo.Persiguiendo; // RA4.3 Abandona patrulla
        }
        else
        {
            estadoActual = EstadoEnemigo.Patrullando; // Vuelve a patrullar si te alejas mucho
        }

        // 2. EJECUTAR COMPORTAMIENTO SEGÚN EL ESTADO
        switch (estadoActual)
        {
            case EstadoEnemigo.Patrullando:
                Patrullar();
                break;
            case EstadoEnemigo.Persiguiendo:
                Perseguir();
                break;
            case EstadoEnemigo.Atacando:
                Atacar();
                break;
        }
    }

    private void Patrullar()
    {
        // Si no hay waypoints asignados, se queda vigilando en el sitio
        if (waypoints == null || waypoints.Length == 0)
        {
            animator.SetFloat("Velocidad", 0f);
            return;
        }

        // Moverse hacia el waypoint actual a velocidad de patrulla
        MoverPorCamino(velocidadPatrulla);
        animator.SetFloat("Velocidad", velocidadPatrulla);

        // Comprobar si hemos llegado al waypoint para ir al siguiente
        float distanciaAlWaypoint = Vector3.Distance(transform.position, waypoints[waypointActual].position);
        if (distanciaAlWaypoint <= distanciaCambioWaypoint)
        {
            waypointActual++;
            // RA4.1: Movimiento Cíclico (Si llega al último, vuelve al primero)
            if (waypointActual >= waypoints.Length)
            {
                waypointActual = 0;
            }
        }
    }

    private void Perseguir()
    {
        // Moverse hacia el jugador corriendo
        MoverPorCamino(velocidadPersecucion);
        animator.SetFloat("Velocidad", velocidadPersecucion);

        // Mirar fijamente al jugador
        MirarAlObjetivo(objetivo.transform.position);
    }

    private void Atacar()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, objetivo.transform.position);

        // Se sigue acercando un poco hasta la distancia de parada para no disparar de lejísimos
        if (distanciaAlJugador > distanciaParada)
        {
            MoverPorCamino(velocidadPersecucion);
            animator.SetFloat("Velocidad", velocidadPersecucion);
        }
        else
        {
            animator.SetFloat("Velocidad", 0f); // Se para si está pegado a ti
        }

        // Disparar
        if (arma.PuedeDisparar())
        {
            arma.Disparar();
            animator.SetTrigger("Disparar");
        }

        // Mirar al jugador SIEMPRE al atacar
        MirarAlObjetivo(objetivo.transform.position);
    }

    // --- FUNCIONES DE MOVIMIENTO Y RUTAS ---

    private void MirarAlObjetivo(Vector3 posicionObjetivo)
    {
        Vector3 direccion = (posicionObjetivo - transform.position).normalized;
        if (direccion != Vector3.zero)
        {
            float angulo = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg;
            transform.eulerAngles = Vector3.up * angulo;
        }
    }

    private void MoverPorCamino(float velocidadActual)
    {
        if (listaCaminos == null || listaCaminos.Count == 0) return;

        Vector3 destino = new Vector3(listaCaminos[0].x, transform.position.y, listaCaminos[0].z);
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidadActual * Time.deltaTime);

        // Si llegó al punto de la esquina del NavMesh, lo borra para ir al siguiente
        if (Vector3.Distance(transform.position, destino) < 0.1f)
        {
            listaCaminos.RemoveAt(0);
        }

        // Si está patrullando, mira hacia donde camina. Si te persigue, el Update ya lo hace mirarte a ti.
        if (estadoActual == EstadoEnemigo.Patrullando)
        {
            MirarAlObjetivo(destino);
        }
    }

    void ActualizarCaminos()
    {
        if (estaMuerto) return;

        Vector3 destinoFinal = transform.position;

        // La IA decide hacia dónde calcular la ruta del NavMesh
        if (estadoActual == EstadoEnemigo.Patrullando && waypoints != null && waypoints.Length > 0)
        {
            destinoFinal = waypoints[waypointActual].position;
        }
        else if (estadoActual == EstadoEnemigo.Persiguiendo || estadoActual == EstadoEnemigo.Atacando)
        {
            destinoFinal = objetivo.transform.position;
        }

        NavMeshPath caminoCalulado = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, destinoFinal, NavMesh.AllAreas, caminoCalulado);
        listaCaminos = caminoCalulado.corners.ToList();
    }

    public void QuitarVidasEnemigo(int cantidad)
    {
        if (estaMuerto) return;

        vidasActual -= cantidad;
        animator.SetTrigger("Danio");
        estadoActual = EstadoEnemigo.Persiguiendo;

        if (vidasActual <= 0)
        {
            estaMuerto = true;

            // TRUCO CLAVE: Le quitamos la etiqueta para que no cuente como enemigo vivo
            gameObject.tag = "Untagged";

            animator.SetTrigger("Morir");
            ControlJuego.instancia.PonerPuntuacion(puntuacionEnemigo);
            animator.SetLayerWeight(1, 0f);
            GetComponent<Collider>().enabled = false;

            // Avisamos al juego de que compruebe si hemos ganado
            ControlJuego.instancia.ComprobarVictoria();

            Destroy(gameObject, 3f);
        }
    }
}