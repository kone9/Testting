using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarcoTrigger : MonoBehaviour
{
    public GameObject fuego;
    public BoxCollider _BoxCollider;
    
    Gamehandler _Gamehandler;

    GameObject[] sound_hit;

    public BoxCollider[] overlappers;
    public LayerMask isOverlapper;

    BarcoHandler _BarcoHandler;
    Animator _Animator;

    AudioSource sonidoWinner;

    GameObject[] sonidoBarcoEnemigoDestruido;
    AudioSource musicaJugandoContraEnemigo;

    private HandlerDificultadEntreNiveles _HandlerDificultadEntreNiveles;//para referencia a la dificultad entre niveles

    
    private void Awake() {
        _BoxCollider = GetComponent<BoxCollider>();
        _Gamehandler = FindObjectOfType<Gamehandler>();
        _HandlerDificultadEntreNiveles = FindObjectOfType<HandlerDificultadEntreNiveles>();//para obtener la referencía al script


        _BarcoHandler = transform.parent.GetComponent<BarcoHandler>();
        _Animator = transform.parent.GetComponent<Animator>();
        
        sound_hit = GameObject.FindGameObjectsWithTag("hit");
        sonidoWinner = GameObject.Find("SonidoWinner").GetComponent<AudioSource>();
        sonidoBarcoEnemigoDestruido = GameObject.FindGameObjectsWithTag("SonidoBarcoEnemigoDestruido");//referencia a la sonido barcos destruidos
        musicaJugandoContraEnemigo = GameObject.Find("MusicaJugandoContraEnemigo").GetComponent<AudioSource>();//referencia a la música del juego
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnMouseDown()//si el mouse presiona el area 
    { 
        Diparar();//disparo contra el barco
    }

    /// <summary>Disparo contra los barcos y hago todo lo referenciado a disparar</summary>
    void Diparar()
    {
        if(_Gamehandler.GetPuedoPresionarBoton())//si puedo presionar boton
        {
            // mensaje acerto disparo
            if(_BarcoHandler.vidas > 1 && _Gamehandler.cantidadDeAciertosJugador < 21)//si vidas de barco es mayor a uno
            {
                sound_hit[Random.Range(0,sound_hit.Length)].GetComponent<AudioSource>().Play();
                _Gamehandler.SetPuedoPresionarBoton(false);//no puedo presionar los botones
                StartCoroutine(_Gamehandler.Mensaje_bardeadaJugadorAcertarDisparo());// mensaje bardeada acepto dipsaro
                StartCoroutine("jugarContraEnemigoDelay");//delay antes de que el enemigo dispare           }  
            }
            bool haycolision = DeshabilitarFondo();//deshabilito el fondo que esta abajo de barco
            print("hay colision" + haycolision);
            instanciarFuego();
            _Gamehandler.cantidadDeAciertosJugador += 1;//para saber cuando gano
            _BarcoHandler.vidas -= 1;//una vida menos
            // sound_hit[2].GetComponent<AudioSource>().Play();

            print("TENDRIA QUE INSTANCIAR EL FUEGO");
        }

        if(_BarcoHandler.vidas < 1 && _Gamehandler.cantidadDeAciertosJugador < 21)//si las vidas del barco es menor a uno
        {
            
            _Gamehandler.cantidadDeBarcosEnemigo -=1;
            sonidoBarcoEnemigoDestruido[Random.Range(0,sonidoBarcoEnemigoDestruido.Length)].GetComponent<AudioSource>().Play();//activo sonido barco destruido
            if(_Gamehandler.cantidadDeBarcosEnemigo != 2)//si cantidad de barcos es distinto de 2
            {
                StartCoroutine( _Gamehandler.Mensaje_bardeadaJugadorDestruyoBarco()); //mensaje bardeada destruyo barco enemigo
            }
            else//si cantidad de barcos es igual a 2
            {
                StartCoroutine(_Gamehandler.Mensaje_EstamosGanando());//cartel enemigo destruyo barco, pide ayuda el jugador    
            }
            _Animator.SetBool("barcoDestruido", true);
            _Gamehandler.SetPuedoPresionarBoton(false);//no puedo presionar los botones
            StartCoroutine("jugarContraEnemigoDelayTargetDestroy");//delay antes de que el enemigo dispare           }  
        }

        if(_Gamehandler.cantidadDeAciertosJugador == 21)//si destrui todos los barcos
        {
            //destruyo última pieza del barco
            GameObject.Find("sink_Own_end").GetComponent<AudioSource>().Play();//activo sonido barco destruido final de la partida

            _Animator.SetBool("barcoDestruido", true);

            //verifico si cambio de nivel o muestro la pantalla WinnerGameOver
            if (_HandlerDificultadEntreNiveles.nivelActual <= 3)
            {
                _Gamehandler.SetPuedoPresionarBoton(false);//ya no puedo presionar la grilla
                 StartCoroutine("PasarAlSiguienteNivelWinner");//hago las cosas de winner
            }
            else
            {
                _Gamehandler.SetPuedoPresionarBoton(false);//ya no puedo presionar la grilla
                 StartCoroutine("JugadorWinner");//hago las cosas de winner
            }
           
        }

    }

     /// <summary>hace que sea el turno del enemigo cuando acertas al disparo</summary>
    IEnumerator jugarContraEnemigoDelay()
    {
        yield return new WaitForSeconds(2f);//por defecto 2 segundos
         _Gamehandler.IsTurnoEnemigo();
    }
    
    /// <summary>hace que sea el turno del enemigo cuando destruis un barco</summary>
    IEnumerator jugarContraEnemigoDelayTargetDestroy()//hace que sea el turno del enemigo cuando destruis un barco
    {
        yield return new WaitForSeconds(4);
         _Gamehandler.IsTurnoEnemigo();
    }

    /// <summary>Hace que aparescan las cosas cuando el jugador gana</summary>
    IEnumerator JugadorWinner()
    {  
        yield return new WaitForSeconds(2);
        musicaJugandoContraEnemigo.Stop();
        sonidoWinner.Play();//sonido winner
        yield return new WaitForSeconds(2);//despues de 2 segundos 
        _Gamehandler.GameOverWinner();//cambio a nivel winner
    }

    IEnumerator PasarAlSiguienteNivelWinner()
    {  
        yield return new WaitForSeconds(2);//espero 2 segundos
        musicaJugandoContraEnemigo.Stop();//detengo la música
        sonidoWinner.Play();//sonido winner
        yield return new WaitForSeconds(1f);//despues de 5 segundos 
        _Gamehandler.UI_CambiarNivel.SetActive(true);//activo fondo
        yield return new WaitForSeconds(0.5f);//despues de 5 segundos 
        ///Para la barra de carga uso un contador
        float barraCarga = 0;
        while(barraCarga < 1)
        {
            _Gamehandler.barraCargaCambiarNivel.size = barraCarga;
            yield return new WaitForSeconds(0.05f);//espero 0.4 segundos, son 10 por lo tanto son 4 segundos
            barraCarga += 0.01f;
        }
        _Gamehandler.barraCargaCambiarNivel.size = 1;//hago que la barra llegue a 1
        yield return new WaitForSeconds(1f);//espero 0.1 segundos
        // yield return new WaitForSeconds(4);//despues de 5 segundos 
        _Gamehandler.CambiarAlProximoNivel();//cambio a nivel winner
    }    


    public bool DeshabilitarFondo()//tengo que usar una corrutina para esperar un segundo sino se presiona el boton inmediatamente y hay un error de sincronización de botones
    {
        bool estaColisionando = false;

        if(overlappers != null)
		{
			for (int i = 0; i < overlappers.Length; i++)
			{
                BoxCollider box = overlappers[i];
                Collider[] collisions = Physics.OverlapBox(box.transform.position, box.bounds.size / 2, Quaternion.identity, isOverlapper);
                if (collisions.Length > 1)
                {
                    Debug.Log("Hay Overlap");
                    // transform.localPosition = startPos;
                    print(collisions[0].name);
                    collisions[0].GetComponent<MeshRenderer>().enabled = false;
                    collisions[0].GetComponent<BoxCollider>().enabled = false;
                    estaColisionando = true;
                    break;
                }
                else
                {
                    estaColisionando = false;
                    // puedoRotar = true;
                    Debug.Log("No hay overlap");
                }
			}
		}
        return estaColisionando;
    }


    private void instanciarFuego()
    {
        GameObject fuegoInstance = Instantiate(fuego);
        //fuegoInstance.transform.SetParent(this.gameObject.transform);
        fuegoInstance.transform.position = this.transform.position;
        _BoxCollider.enabled = false;
    }


}
