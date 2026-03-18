using UnityEngine;

// Añadimos los tipos de munición separados para cumplir con la rúbrica
public enum TipoExtra
{
    Vida,
    MunicionArma1, // Ejemplo: Balas de Pistola
    MunicionArma2  // Ejemplo: Balas de Fusil
}

public class ControlExtras : MonoBehaviour
{
    [Header("Configuración del Extra")]
    public TipoExtra tipo;
    public int cantidad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Jugador"))
        {
            // Buscamos los scripts del jugador
            ControlJugador jugador = other.GetComponent<ControlJugador>();
            InventarioArmas inventario = other.GetComponent<InventarioArmas>();

            bool recogido = false; // Variable para saber si realmente lo necesitamos

            switch (tipo)
            {
                case TipoExtra.Vida:
                    // Solo lo recoge si no tiene la vida al máximo
                    if (jugador.vidasActual < jugador.vidasMax)
                    {
                        jugador.IncrementaVida(cantidad);
                        recogido = true;
                    }
                    break;

                case TipoExtra.MunicionArma1:
                    // El índice 0 es el primer arma de tu inventario
                    if (inventario != null)
                    {
                        recogido = inventario.AñadirMunicion(0, cantidad);
                    }
                    break;

                case TipoExtra.MunicionArma2:
                    // El índice 1 es el segundo arma de tu inventario
                    if (inventario != null)
                    {
                        recogido = inventario.AñadirMunicion(1, cantidad);
                    }
                    break;
            }

            // Destruimos el objeto de la escena SOLO si el jugador lo ha recogido
            if (recogido)
            {
                Destroy(gameObject);
            }
        }
    }
}