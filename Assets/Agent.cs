using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IA2;

public class Agent : MonoBehaviour
{
    public enum Conditions { MOVE, SHOOT, RECHARGE, RESPAWN }
    private EventFSM<Conditions> _myFsm;
    private Rigidbody _myRb;
    public Renderer _myRen;

    public string user;
    public int bullets = 3;
    public int charger = 3;
    public int life = 2;
    public float speed;
    public GameObject myWeapon;
    public Bullet bullet;
    public Text nameDisplay;
    public float range;
    public int angle;
    public LayerMask visibles = ~0;
    public Agent target;

    private void Awake()
    {
        _myRb = gameObject.GetComponent<Rigidbody>();
        DisplayName(tag);

        //PARTE 1: SETEO INICIAL

        //Creo los estados
        var move = new State<Conditions>("Move");
        var shoot = new State<Conditions>("Shoot");
        var recharge = new State<Conditions>("Recharge");
        var respawn = new State<Conditions>("Respawn");

        //creo las transiciones
        StateConfigurer.Create(move)
            .SetTransition(Conditions.SHOOT, shoot)
            .SetTransition(Conditions.RESPAWN, respawn)
            .Done(); //aplico y asigno

        StateConfigurer.Create(shoot)
            .SetTransition(Conditions.MOVE, move)
            .SetTransition(Conditions.RECHARGE, recharge)
            .SetTransition(Conditions.RESPAWN, respawn)
            .Done();

        StateConfigurer.Create(recharge)
            .SetTransition(Conditions.MOVE, move)
            .SetTransition(Conditions.RESPAWN,respawn)
            .Done();

        StateConfigurer.Create(respawn)
            .SetTransition(Conditions.MOVE, move)
            .Done();

        //PARTE 2: SETEO DE LOS ESTADOS
        //Move
        move.OnUpdate += () =>
        {
            //movimiento de cada personaje.

           /*if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                SendInputToFSM(Conditions.MOVE);
            else if (Input.GetKeyDown(KeyCode.Space))
                SendInputToFSM(Conditions.SHOOT);*/
        };

        //Shoot
        shoot.OnUpdate += () =>
        {
            if (life > 0)
            {
                if (bullets > 0) {
                    var bul = Instantiate(bullet);
                    bul.transform.position = transform.position + transform.forward;
                    bul.transform.forward = transform.forward;
                    SendInputToFSM(Conditions.MOVE);
                }
                else
                {
                    myWeapon.GetComponent<Renderer>().material.color = Color.red;
                    SendInputToFSM(Conditions.RECHARGE);
                }
            }

            else
            {
                SendInputToFSM(Conditions.RESPAWN);
            }
        };

        //Recharge
        recharge.OnEnter += x =>
        {
            //tambien uso el rigidbody, pero en vez de tener una variable en cada estado, tengo una sola referencia compartida...
            StartCoroutine("Recharge");
            myWeapon.GetComponent<Renderer>().material.color = Color.green;
            SendInputToFSM(Conditions.MOVE);
        };

        recharge.OnUpdate += () =>
        {
            if (life <= 0)
            {
                SendInputToFSM(Conditions.RESPAWN);
            }
        };

        //con todo ya creado, creo la FSM y le asigno el primer estado
        _myFsm = new EventFSM<Conditions>(move);
    }

    private void SendInputToFSM(Conditions inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();

        if(target != null) IsInSight(target.transform);
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
    }

    public void DisplayName(string name)
    {
        nameDisplay.text = name;
    }

    public IEnumerator Recharge()
    {
        yield return new WaitForSeconds(1.5f);
        bullets += charger;
    }

    public bool IsInSight(Transform target)
    {
        //Solo para debuggear, esta linea no deberia estar aca
        var debugTarget = target;

        //Vector entre la posicion del target y mi posicion
        var positionDiference = target.position - transform.position;
        //Distancia al target
        var distance = positionDiference.magnitude;

        // - Si esta mas lejos que mi rango => no lo veo
        if (distance > range) return false;

        //Angulo entre mi forward y la direccion al target
        var angleToTarget = Vector3.Angle(transform.forward, positionDiference);//No hace falta normalizar, por qué?

        // - Si el angulo es mayor a la mitad de mi angulo maximo => no lo veo
        if (angleToTarget > angle / 2) return false;
        //Por qué dividimos por 2?
        //El angulo de vision lo tomamos entre extremo y extremo del rango,
        //en cabio el angulo al target entre el centro y el el extremo.

        // - Tiramos un rayo para chequear que no haya nada obstruyendo la vista.
        RaycastHit hitInfo;//En caso de haber una colision en el rayo aca se guarda la informacion

        if (Physics.Raycast(transform.position, positionDiference, out hitInfo, range, visibles))
        {
            //Si entra aca => colisiono con algo => hay algo obstruyendo
            //>>PERO<< tenemos que chequea que ese algo no sea el objeto que queremos ver
            if (hitInfo.transform != target) return false;
        }

        //Si no paso nada de lo anterior, el objeto esta a la vista
        return true;
    }

    /*public Agent SetTarget(List<Agent> enemys)
    {
        if (target == null)
        {
            foreach (var item in enemys)
            {
                if (IsInSight(item.transform)) return item;
            }
        }

        else return default(Agent);
    }*/

}
