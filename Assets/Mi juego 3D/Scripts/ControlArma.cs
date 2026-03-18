using UnityEngine;

public class ControlArma : MonoBehaviour
{
    public int municionActual;
    public int municionMax;
    public bool municionInfinita;

    public float velocidadBala;
    public float frecuenciaDisparo;

    private PoolObjetos balaPool;
    public Transform puntoSalida;

    private float ultimoTiempoDisparo;
    private bool esJugador;

    private void Awake()
    {
        // NUEVO: Ahora el arma mira si su "Padre" o "Abuelo" es el Jugador
        if (GetComponentInParent<ControlJugador>() != null)
        {
            esJugador = true;
        }
        else
        {
            esJugador = false;
        }

        balaPool = GetComponent<PoolObjetos>();
    }

    public bool PuedeDisparar()
    {
        if (Time.time - ultimoTiempoDisparo >= frecuenciaDisparo)
        {
            if (municionActual > 0 || municionInfinita == true)
            {
                return true;
            }
        }
        return false;
    }

    public void Disparar()
    {
        ultimoTiempoDisparo = Time.time;
        if (!municionInfinita)
        {
            municionActual--;
        }

        GameObject bala = balaPool.getObjeto();
        bala.transform.position = puntoSalida.position;
        bala.transform.rotation = puntoSalida.rotation;

        bala.GetComponent<Rigidbody>().linearVelocity = puntoSalida.forward * velocidadBala;

        // Si es el jugador quien dispara, actualizamos el texto del HUD
        if (esJugador)
        {
            ControlHUD.instancia.actualizarBalasTexto(municionActual, municionMax);
        }

        GetComponent<AudioSource>().Play();
    }
}