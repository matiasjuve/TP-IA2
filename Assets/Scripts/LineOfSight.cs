using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    //Que tan lejos puede ver
    public float range = 10;
    //Que tan amplia es su vision
    public float angle = 90;

    //Que cosas puede ver y/o obstruyen su vision
    public LayerMask visibles = ~0;//Hack para decir "TODOS"


    //Solo para debuggear, esta linea no deberia estar aca
    private Transform debugTarget;

    public bool IsInSight(Transform target)
    {
        //Solo para debuggear, esta linea no deberia estar aca
        debugTarget = target;

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

    void OnDrawGizmos()
    {
        var position = transform.position;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, angle / 2, 0) * transform.forward * range);
        Gizmos.DrawLine(position, position + Quaternion.Euler(0, -angle / 2, 0) * transform.forward * range);

        if (debugTarget)
            Gizmos.DrawLine(position, debugTarget.position);
    }

}
