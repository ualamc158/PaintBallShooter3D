using UnityEngine;
using UnityEngine.AI;

public class HumanSoldierController : MonoBehaviour
{
    [Header("Estadística")]
    public int vidasActual;
    public int vidasMax;
    public int puntuacionEnemigo;

    [Header("Movimiento y Ataque")]
    public float rangoVision = 15f; // NUEVO: Si estás más lejos de esto, se queda quieto
    public float rangoAtaque = 5f;  // Distancia para disparar

    private ControlArma arma;
    private GameObject objetivo;
    private NavMeshAgent agente;

    // --- VARIABLES PARA ANIMACIÓN ---
    private Animator anim;
    private bool estaMuerto = false;

    // Estados para controlar las animaciones y no lanzar Triggers repetidos
    private enum EstadoEnemigo { Reposo, Perseguidor, Atacando }
    private EstadoEnemigo estadoActual = EstadoEnemigo.Reposo;

    void Start()
    {
        arma = GetComponent<ControlArma>();
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        objetivo = GameObject.FindGameObjectWithTag("Jugador");

        // Postura inicial
        if (anim != null)
        {
            anim.SetTrigger("Gun");
            anim.SetTrigger("StandUp");
            anim.SetTrigger("NoMovement"); // Empieza quieto
        }
    }

    private void Update()
    {
        if (estaMuerto || objetivo == null) return;

        float distancia = Vector3.Distance(transform.position, objetivo.transform.position);

        if (distancia > rangoVision)
        {
            EstarQuieto(); // Fuera de todo rango
        }
        else if (distancia > rangoAtaque)
        {
            PerseguirObjetivo(); // En rango de visión, pero no de ataque
        }
        else
        {
            AtacarObjetivo(); // En rango de ataque
        }
    }

    private void EstarQuieto()
    {
        // 1. Detener al enemigo
        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }

        // 2. Cambiar a animación de estar quieto apuntando/vigilando
        if (estadoActual != EstadoEnemigo.Reposo)
        {
            if (anim != null)
            {
                anim.ResetTrigger("Run");
                anim.ResetTrigger("Shoot01");
                anim.SetTrigger("NoMovement");
            }
            estadoActual = EstadoEnemigo.Reposo;
        }
    }

    private void PerseguirObjetivo()
    {
        // 1. Mover al enemigo usando NavMesh
        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = false;
            agente.SetDestination(objetivo.transform.position);
        }

        // 2. Si no estaba persiguiendo, cambiamos la animación a correr
        if (estadoActual != EstadoEnemigo.Perseguidor)
        {
            if (anim != null)
            {
                anim.ResetTrigger("NoMovement");
                anim.ResetTrigger("Shoot01");
                anim.SetTrigger("Run");
            }
            estadoActual = EstadoEnemigo.Perseguidor;
        }
    }

    private void AtacarObjetivo()
    {
        // 1. Detener al enemigo
        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }

        // 2. Rotar para mirar al jugador
        Vector3 direccion = (objetivo.transform.position - transform.position).normalized;
        direccion.y = 0; // Evitar que mire al suelo o al cielo
        transform.rotation = Quaternion.LookRotation(direccion);

        // 3. Cambiar animación a quieto/apuntando si venía corriendo
        if (estadoActual != EstadoEnemigo.Atacando)
        {
            if (anim != null)
            {
                anim.ResetTrigger("Run");
                anim.SetTrigger("NoMovement");
            }
            estadoActual = EstadoEnemigo.Atacando;

            // Salimos este fotograma para darle tiempo al Animator de cambiar de pose
            return;
        }

        // 4. Lógica de Disparo
        if (arma != null && arma.PuedeDisparar())
        {
            arma.Disparar();

            if (anim != null)
            {
                anim.ResetTrigger("Run");
                anim.ResetTrigger("NoMovement");
                anim.SetTrigger("Shoot01"); // Hace la animación del retroceso
            }
        }
    }

    public void QuitarVidasEnemigo(int cantidad)
    {
        if (estaMuerto) return;

        vidasActual -= cantidad;

        if (vidasActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaMuerto = true;
        if (agente != null && agente.isOnNavMesh) agente.isStopped = true;

        if (ControlJuego.instancia != null)
        {
            ControlJuego.instancia.PonerPuntuacion(puntuacionEnemigo);
        }

        if (anim != null) anim.SetTrigger("Death01");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3f);
    }
}