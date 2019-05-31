using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IA2;
using System.Linq;

public class Agent : MonoBehaviour
{
    public enum Conditions { MOVE, SHOOT, RECHARGE, RESPAWN }
    private EventFSM<Conditions> _myFsm;
    private Rigidbody _myRb;

    public string user;
    public int bullets = 3;
    public int charger = 3;
    public int life = 2;
    public int maxLife;
    public float speed;
    public int kills = 0;
    public int Deaths = 0;
    public GameObject myWeapon;
    public Bullet bullet;
    public TextMesh nameDisplay;
    public float range;
    public int angle;
    public LayerMask visibles = ~0;
    private Transform target;
    public List<Transform> enemysOnRange;
    public List<Transform> spawnPoints;

    private void Awake()
    {
        _myRb = gameObject.GetComponent<Rigidbody>();
        DisplayName(tag);
        bullets = charger;
        life = maxLife;

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
            Debug.Log("ESTOY EN MOVE");
            if (life > 0 && target != null) SendInputToFSM(Conditions.SHOOT);
            else if (life <= 0) SendInputToFSM(Conditions.RESPAWN);
            //movimiento de cada personaje.
        };

        //Shoot
        shoot.OnUpdate += () =>
        {
            Debug.Log("ESTOY EN SHOOT");
            if (life > 0)
            {
                if (target != null)
                {
                    transform.LookAt(target); 
                }

                if (bullets > 0)
                {
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
            Debug.Log("ESTOY EN RECARGA");
            if (life <= 0)
            {
                SendInputToFSM(Conditions.RESPAWN);
            }
        };

        //Respawn
        respawn.OnEnter += x =>
        {
            transform.position = spawnPoints[Random.Range(0, spawnPoints.Count - 1)].position;
            bullets = charger;
            life = maxLife;
            target = null;
            myWeapon.GetComponent<Renderer>().material.color = Color.green;
            SendInputToFSM(Conditions.MOVE);
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

        target = SetTarget(enemysOnRange);
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
        bullets = charger;
    }

    public IEnumerator MoveToOther()
    {
        yield return new WaitForSeconds(2);
        SendInputToFSM(Conditions.SHOOT);
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

    public Transform SetTarget(List<Transform> enemys)
    {
        if (target == null && enemys != null)
        {
            foreach (var item in enemys)
            {
                if (IsInSight(item)) return item;
            }
        }
        return null;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != gameObject && other.gameObject.layer == 8)
        {
            enemysOnRange.Add(other.transform);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject != gameObject)
        {
            enemysOnRange.Remove(other.transform);
        }
    }
}
