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
    public int charger;
    public int maxLife;
    public float speed;
    private int life;
    public int kills = 0;
    public int Deaths = 0;
    private int bullets;
    public GameObject myWeapon;
    public Bullet bullet;
    public TextMesh nameDisplay;
    public float range;
    public int angle;
    public LayerMask visibles;
    private Transform target = null;
    private List<Transform> enemysOnRange = new List<Transform>();

    private Vector3 search;

    public float shootcd;
    private bool canShoot = true;

    private int sideStepDirection;

    private void Start()
    {
        _myRb = gameObject.GetComponent<Rigidbody>();
        DisplayName(user);
        bullets = charger;
        life = maxLife;
        search = SpawnPoints.Instance.spawnPoints[Random.Range(0, SpawnPoints.Instance.spawnPoints.Count - 1)].position;

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
            .SetTransition(Conditions.RESPAWN, respawn)
            .SetTransition(Conditions.RECHARGE, recharge)
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
        move.OnEnter += x =>
        {
            search = SpawnPoints.Instance.spawnPoints[Random.Range(0, SpawnPoints.Instance.spawnPoints.Count - 1)].position;
        };
        move.OnUpdate += () =>
        {
            Debug.Log("ESTOY EN MOVE");
            if (life > 0 && target != null) SendInputToFSM(Conditions.SHOOT);
            else if (life <= 0) SendInputToFSM(Conditions.RESPAWN);

            if (Vector3.Distance(search, transform.position) > 0.5f) transform.position += (search - transform.position).normalized * speed * Time.deltaTime;
            else { search = SpawnPoints.Instance.spawnPoints[Random.Range(0, SpawnPoints.Instance.spawnPoints.Count - 1)].position; }
            
            //CODIGO DE MOVIMIENTO.
        };

        //Shoot
        shoot.OnUpdate += () =>
        {
            Debug.Log("ESTOY EN SHOOT");
            CheckTarget();
            if (life <= 0) SendInputToFSM(Conditions.RESPAWN);

            if (target != null)
            {
                transform.LookAt(target); 

                if (bullets > 0)
                {
                    if (canShoot)
                    {
                        StartCoroutine(Shoot());
                    }

                    else
                    {
                        SideStep(sideStepDirection);
                    }
                }

                else
                {
                    SendInputToFSM(Conditions.RECHARGE);
                }
            }

            else
            {
                SendInputToFSM(Conditions.MOVE);
            }
        };
        shoot.OnExit += x => 
        {
            StopCoroutine(Shoot());
        };

        //Recharge
        recharge.OnEnter += x =>
        {
            myWeapon.gameObject.GetComponent<Renderer>().material.color = Color.red;
            StartCoroutine(Recharge());            
        };
        recharge.OnUpdate += () =>
        {
            Debug.Log("ESTOY EN RECARGA");
            if (life <= 0)
            {
                SendInputToFSM(Conditions.RESPAWN);
            }
        };
        recharge.OnExit += x =>
         {
             StopCoroutine(Recharge());
         };

        //Respawn
        respawn.OnEnter += x =>
        {
            target = null;
            transform.position = SpawnPoints.Instance.spawnPoints[Random.Range(0, SpawnPoints.Instance.spawnPoints.Count - 1)].position;
            Deaths++;
            bullets = charger;
            life = maxLife;
            myWeapon.GetComponent<Renderer>().material.color = Color.green;
            SendInputToFSM(Conditions.MOVE);
        };

        //con todo ya creado, creo la FSM y le asigno el primer estado
        _myFsm = new EventFSM<Conditions>(move);
    }

    private void CheckTarget()
    {
        if (target != null && !IsInSight(target))
        {
            target = null;
        }
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
        yield return new WaitForSeconds(5);
        bullets = charger;
        myWeapon.GetComponent<Renderer>().material.color = Color.green;
        SendInputToFSM(Conditions.MOVE);
    }

    public IEnumerator MoveToOther()
    {
        yield return new WaitForSeconds(2);
        SendInputToFSM(Conditions.SHOOT);
    }

    public IEnumerator Shoot()
    {
        var bul = Instantiate(bullet);
        bul.shooter = this;
        bul.transform.position = transform.position + transform.forward * 1.3f + transform.up;
        bul.transform.forward = transform.forward;
        bullets--;
        sideStepDirection = Direction();
        canShoot = false;

        yield return new WaitForSeconds(1);
        canShoot = true;
        
    }

    public void SideStep(int direction)
    {
        transform.position += (transform.right * direction) * speed * Time.deltaTime ;
        if(Vector3.Distance(target.position, transform.position) > range/1.5f)transform.position += (target.position - transform.position).normalized * speed * Time.deltaTime;
    }

    public int Direction()
    {
        if (Random.value > 0.5f)
        {
            return -1;
        }

        else
        {
            return 1;
        }
    }

    public bool IsInSight(Transform target)
    {
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
        foreach (var item in enemys)
        {
            if (IsInSight(item))
            {
                return item;
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

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9)
        {
            TakeDamage(collision.gameObject.GetComponent<Bullet>().damage);
            Destroy(collision.gameObject);
        }
    }
}
