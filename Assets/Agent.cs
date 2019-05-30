using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA2;

public class Agent : MonoBehaviour
{
    public enum Conditions { MOVE, SHOOT, RECHARGE, RESPAWN }
    private EventFSM<Conditions> _myFsm;
    private Rigidbody _myRb;
    public Renderer _myRen;

    public string user;
    public int bullets = 3;
    public int life = 2;
    public float speed;
    public GameObject myWeapon;
    public Bullet bullet;

    private void Awake()
    {
        _myRb = gameObject.GetComponent<Rigidbody>();

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

            //El disparo

            if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
                SendInputToFSM(Conditions.RECHARGE);
            else if (Input.GetKeyDown(KeyCode.Space))
                SendInputToFSM(Conditions.SHOOT);
        };
        shoot.OnFixedUpdate += () =>
        {
            _myRb.velocity += (transform.forward * Input.GetAxis("Vertical") * 20f + transform.right * Input.GetAxis("Horizontal") * 20f) * Time.deltaTime;
        };
        shoot.OnExit += x =>
        {
            //x es el input que recibí, por lo que puedo modificar el comportamiento según a donde estoy llendo
            if (x != Conditions.SHOOT)
                _myRb.velocity = Vector3.zero;
        };

        //Recharge
        recharge.OnEnter += x =>
        {
            //tambien uso el rigidbody, pero en vez de tener una variable en cada estado, tengo una sola referencia compartida...
            _myRb.AddForce(transform.up * 10f, ForceMode.Impulse);
        };
        recharge.OnUpdate += () =>
        {
            if (Input.GetKeyDown(KeyCode.Space))
                SendInputToFSM(Conditions.SHOOT);
        };
        recharge.GetTransition(Conditions.SHOOT).OnTransition += x =>
        {
            _myRen.material.color = Color.red;
        };
        recharge.GetTransition(Conditions.RECHARGE).OnTransition += x =>
        {
            _myRen.material.color = Color.white;
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
    }

    private void FixedUpdate()
    {
        _myFsm.FixedUpdate();
    }

    public void TakeDamage(int damage)
    {
        life -= damage;
    }
}
