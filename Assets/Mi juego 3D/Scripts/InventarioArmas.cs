using UnityEngine;

public class InventarioArmas : MonoBehaviour
{
    [Header("Tus Armas")]
    public GameObject[] armas; // Arrastra aquí tus objetos Arma 1, Arma 2, etc.
    public int armaActual = 0; // El arma con la que empiezas

    private ControlJugador jugador;

    void Start()
    {
        jugador = GetComponent<ControlJugador>();
        SeleccionarArma(); // Equipar el arma por defecto al iniciar
    }

    void Update()
    {
        int armaAnterior = armaActual;

        // Cambiar arma con la ruleta del ratón
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (armaActual >= armas.Length - 1) armaActual = 0;
            else armaActual++;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (armaActual <= 0) armaActual = armas.Length - 1;
            else armaActual--;
        }

        // Cambiar arma con los números 1 y 2 del teclado
        if (Input.GetKeyDown(KeyCode.Alpha1) && armas.Length >= 1) armaActual = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) && armas.Length >= 2) armaActual = 1;

        // Si el jugador ha pulsado un botón de cambio, actualizamos
        if (armaAnterior != armaActual)
        {
            SeleccionarArma();
        }
    }

    void SeleccionarArma()
    {
        int i = 0;
        foreach (GameObject arma in armas)
        {
            if (i == armaActual)
            {
                // Encendemos esta arma
                arma.SetActive(true);
                // Le pasamos el script de esta nueva arma al jugador
                jugador.EquiparArma(arma.GetComponent<ControlArma>());
            }
            else
            {
                // Apagamos las demás armas
                arma.SetActive(false);
            }
            i++;
        }
    }

    public bool AñadirMunicion(int indiceArma, int cantidad)
    {
        if (indiceArma >= armas.Length) return false;

        ControlArma armaObj = armas[indiceArma].GetComponent<ControlArma>();

        // Si ya tenemos las balas al máximo, devolvemos falso para no gastar la caja
        if (armaObj.municionActual >= armaObj.municionMax) return false;

        // Sumamos las balas sin pasarnos del máximo
        armaObj.municionActual = Mathf.Clamp(armaObj.municionActual + cantidad, 0, armaObj.municionMax);

        // Si casualmente tenemos esa arma en la mano, actualizamos la interfaz
        if (armaActual == indiceArma)
        {
            ControlHUD.instancia.actualizarBalasTexto(armaObj.municionActual, armaObj.municionMax);
        }

        return true;
    }
}