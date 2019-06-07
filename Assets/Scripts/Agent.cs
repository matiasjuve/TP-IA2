using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IA2;
using System;
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
    public int life;
    public int kills = 0;
    public int Deaths = 0;
    public int bullets;
    public GameObject myWeapon;
    public Bullet bullet;
    public TextMesh nameDisplay;
    public float range;
    public int angle;
    public LayerMask visibles;
    public Transform target;
    public List<GridEntity> grid;
    public Queries query;
    private List<Transform> enemysOnRange = new List<Transform>();
    public List<Transform> spawnPoints;

    public List<Tuple<string, Agent>> victims = new List<Tuple<string, Agent>>();

    private Vector3 search = Vector3.zero;

    public float shootcd;
    private bool canShoot = true;

    private int sideStepDirection;


    // COSAS PARA DIJKSTRA
    public List<GridEntity> path;
    GridEntity start;
    public GridEntity finalNode;
    public int currentIndex;

    private void Start()
    {
        grid = GameObject.FindObjectsOfType<GridEntity>().ToList();
        _myRb = gameObject.GetComponent<Rigidbody>();
        DisplayName(user);
        bullets = charger;
        life = maxLife;
        search = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count - 1)].position;
        //finalNode = query.Query().ToList()[UnityEngine.Random.Range(0, query.Query().Count())];
        //LevelManager.Instance.players.Add(this);
        //search = SpawnPoints.Instance.spawnPoints[Random.Range(0, SpawnPoints.Instance.spawnPoints.Count - 1)].position;

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
            target = null;
            //path = null;
            start = GetFirst();
            finalNode = grid[UnityEngine.Random.Range(0, grid.Count - 1)];
        };
        move.OnUpdate += () =>
        {
            path = AStar.Run(start,IsFinalNode, Expand, Heuristic);
            Move();
            if (life > 0 && target != null) SendInputToFSM(Conditions.SHOOT);
            else if (life <= 0) SendInputToFSM(Conditions.RESPAWN);

        };

        move.OnExit += x => 
        {
            currentIndex = 0;
        };

        //Shoot
        shoot.OnUpdate += () =>
        {
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
            //target = null;
            transform.position = SpawnPoints.Instance.spawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Instance.spawnPoints.Count - 1)].position;
            Deaths++;
            bullets = charger;
            life = maxLife;
            myWeapon.GetComponent<Renderer>().material.color = Color.green;
            SendInputToFSM(Conditions.MOVE);
        };

        //con todo ya creado, creo la FSM y le asigno el primer estado
        _myFsm = new EventFSM<Conditions>(move);
    }

    public void Move()
    {
        var current = path[currentIndex];
        transform.position += (current.transform.position - transform.position).normalized * speed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, (current.transform.position - transform.position).normalized, 0.1f);

        if (Vector3.Distance(transform.position, current.transform.position) < 0.5f)
        {
            currentIndex++;
            if (currentIndex == path.Count)
            {
                path = new List<GridEntity>();
                finalNode = grid[UnityEngine.Random.Range(0, grid.Count - 1)];
                start = GetFirst();
                currentIndex = 0;
            }
        }
    }

    public GridEntity GetFirst()
    {
        var nodes = query.Query();
        return nodes.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).FirstOrDefault();
    }

    public GridEntity GetLast()
    {
        var nodes = query.Query();
        return nodes.OrderBy(x => Vector3.Distance(finalNode.transform.position, x.transform.position)).FirstOrDefault();
    }

    public bool IsFinalNode(GridEntity n)
    {
        if (n == finalNode) return true;

        return false;
    }

    public Dictionary<GridEntity, float> Expand(GridEntity node)
    {
        var dictionary = new Dictionary<GridEntity, float>();
        foreach (var item in node.neighbours)
        {
            if (!dictionary.ContainsKey(item)) dictionary.Add(item, 1);
        }
        return dictionary;
    }

    float Heuristic(GridEntity node)
    {
        return Vector3.Distance(node.transform.position, finalNode.transform.position);
    }

    public IEnumerator Recharge()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        bullets = charger;
        myWeapon.GetComponent<Renderer>().material.color = Color.green;
        SendInputToFSM(Conditions.MOVE);
        StopCoroutine(Recharge());
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
        StopCoroutine(Shoot());
        
    }
    private void CheckTarget()
    {
        if (target != null && !IsInSight(target))
        {
            target = null;
        }
        if(SetTarget(enemysOnRange) != null && target == null)
        target = SetTarget(enemysOnRange);
    }

    private void SendInputToFSM(Conditions inp)
    {
        _myFsm.SendInput(inp);
    }

    private void Update()
    {
        _myFsm.Update();
        CheckTarget();
        
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



    public void SideStep(int direction)
    {
        transform.position += (transform.right * direction) * speed * Time.deltaTime ;
        if(Vector3.Distance(target.position, transform.position) > range/1.5f)transform.position += (target.position - transform.position).normalized * speed * Time.deltaTime;
    }
    public int Direction()
    {
        if (UnityEngine.Random.value > 0.5f)
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
        if (collision.gameObject.layer == 9 && collision.gameObject.GetComponent<Bullet>().shooter != this)
        {
            TakeDamage(collision.gameObject.GetComponent<Bullet>().damage);
            if (life <= 0)
            {
                collision.gameObject.GetComponent<Bullet>().shooter.kills++;
                collision.gameObject.GetComponent<Bullet>().shooter.victims.Add(Tuple.Create(collision.gameObject.GetComponent<Bullet>().shooter.user, this));
            }
            Destroy(collision.gameObject);
        }
    }
}
