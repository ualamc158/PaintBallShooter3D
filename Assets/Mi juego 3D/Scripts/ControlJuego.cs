using System;
using UnityEngine;

public class ControlJuego : MonoBehaviour
{
    public int puntuacionActual;
    public bool juegoPausado;
    public static ControlJuego instancia;

    public void Awake()
    {
        instancia = this;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            cambiarPausa();
    }

    public void cambiarPausa()
    {
        juegoPausado = !juegoPausado;
        Time.timeScale = (juegoPausado) ? 0.0f : 1.0f;
        Cursor.lockState = (juegoPausado) ? CursorLockMode.None : CursorLockMode.Locked;

        ControlHUD.instancia.CambiarEstadoVentanaPausa(juegoPausado);
    }

    public void PonerPuntuacion(int puntuacion)
    {
        puntuacionActual += puntuacion;
        ControlHUD.instancia.actualizarPuntuacion(puntuacionActual);
    }

    // NUEVO MÉTODO PARA COMPROBAR VICTORIA
    public void ComprobarVictoria()
    {
        // Buscamos cuántos enemigos quedan con la etiqueta "Enemigo"
        int numEnemigos = GameObject.FindGameObjectsWithTag("Enemigo").Length;

        // Si ya no queda ninguno, iniciamos una cuenta atrás de 3 segundos
        if (numEnemigos <= 0)
        {
            // Invoke llama al método "MostrarPantallaVictoria" después de 3.0 segundos
            Invoke("MostrarPantallaVictoria", 3f);
        }
    }

    // Este método se ejecutará cuando termine el temporizador
    private void MostrarPantallaVictoria()
    {
        ControlHUD.instancia.establecerVentanaFinJuego(true);
    }
}