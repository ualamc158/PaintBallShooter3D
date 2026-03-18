using UnityEngine;

public class ControlBala : MonoBehaviour
{
    [Header("Configuración de Bando")]
    public bool esBalaDelJugador; // NUEVO: Para saber de quién es la bala

    [Header("Atributos")]
    public GameObject particulasExplosion;
    public int cantidadVida;
    public float tiempoActivo;
    private float tiempoDisparo;

    public void OnEnable()
    {
        tiempoDisparo = Time.time;
    }

    private void Update()
    {
        if (Time.time - tiempoDisparo >= tiempoActivo)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        // 1. Si la bala es del JUGADOR y golpea a un ENEMIGO
        if (esBalaDelJugador && other.CompareTag("Enemigo"))
        {
            ControlEnemigoMejorado enemigo = other.GetComponent<ControlEnemigoMejorado>();
            if (enemigo != null) enemigo.QuitarVidasEnemigo(cantidadVida);
        }
        // 2. Si la bala es del ENEMIGO y golpea al JUGADOR
        else if (!esBalaDelJugador && other.CompareTag("Jugador"))
        {
            ControlJugador jugador = other.GetComponent<ControlJugador>();
            if (jugador != null) jugador.QuitarVidasJugador(cantidadVida);
        }
        // 3. FUEGO AMIGO: Si golpean a su propio bando, ignoramos el choque para que la bala siga de largo
        else if ((esBalaDelJugador && other.CompareTag("Jugador")) || (!esBalaDelJugador && other.CompareTag("Enemigo")))
        {
            return;
        }

        // Crear las particulas de explosión
        if (particulasExplosion != null)
        {
            GameObject particulas = Instantiate(particulasExplosion, transform.position, Quaternion.identity);
            Destroy(particulas, 1f);
        }

        gameObject.SetActive(false);
    }
}