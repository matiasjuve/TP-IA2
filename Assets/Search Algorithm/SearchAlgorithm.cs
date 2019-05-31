using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SearchAlgorithm
{
    //Time-Slicing

    //El tiempo que se le va a permitir a un programar procesar datos
    //antes de realizar un corte para seguir con la siguiente tanda de datos.

    public static IEnumerable<T> Generate<T>(T seed, Func<T, T> modify)
    {
        T acum = seed;
        while (true)
        {
            yield return acum;
            acum = modify(acum);
        }
    }

    public static T DFS<T>(T start, Func<T, bool> targetCheck,
        Func<T, IEnumerable<T>> GetNeighbours)
    {
        Stack<T> pending = new Stack<T>();
        HashSet<T> visited = new HashSet<T>();
        T current = start;
        pending.Push(current);

        while(pending.Any())
        {
            current = pending.Pop();
            visited.Add(current);

            if(targetCheck(current))
            {
                return current;
            }
            else
            {
                var n = GetNeighbours(current).Where(x => !visited.Contains(x));
                foreach (var elem in n)
                {
                    pending.Push(elem);
                }
            }
        }

        return default(T);
    }

    public static T BFS<T>(T start, Func<T, bool> targetCheck,
    Func<T, IEnumerable<T>> GetNeighbours)
    {
        Queue<T> pending = new Queue<T>();
        HashSet<T> visited = new HashSet<T>();
        T current = start;
        pending.Enqueue(current);

        while (pending.Any())
        {
            current = pending.Dequeue();
            visited.Add(current);

            if (targetCheck(current))
            {
                return current;
            }
            else
            {
                var n = GetNeighbours(current).Where(x => !visited.Contains(x));
                foreach (var elem in n)
                {
                    pending.Enqueue(elem);
                }
            }
        }

        return default(T);
    }

    public static IEnumerable<T> Dijsktra<T>(T start, Func<T, bool> targetCheck,
        Func<T, IEnumerable<Tuple<T, float>>> GetNeighbours)
    {
        HashSet<T> visited = new HashSet<T>();
        Dictionary<T, T> parents = new Dictionary<T, T>();
        Dictionary<T, float> distances = new Dictionary<T, float>();
        List<T> pending = new List<T>();
        T current = start;
        pending.Add(current);
        distances.Add(start, 0);

        while(pending.Any())
        {
            current = pending.OrderBy(x => distances[x]).First();
            pending.Remove(current);
            visited.Add(current);

            if(targetCheck(current))
            {
                return Generate(current, x => parents[x])
                    .TakeWhile(x => parents.ContainsKey(x))
                    .Reverse();
            }
            else
            {
                var n = GetNeighbours(current).Where(x => !visited.Contains(x.Item1));
                foreach (var elem in n)
                {
                    var altDist = distances[current] + elem.Item2;
                    
                    if(!distances.ContainsKey(elem.Item1) || altDist < distances[elem.Item1])
                    {
                        distances[elem.Item1] = altDist;
                        parents[elem.Item1] = current;
                        pending.Add(elem.Item1);
                    }
                }
            }
        }
        return Enumerable.Empty<T>();
    }

    void Funcion()
    {
        List<int> n = new List<int> { 1, 2 };
        BFS_TimeSlicing(0, x => x == 7, x => n, GetPath);
    }

    void GetPath(IEnumerable<int> path)
    {

    }

    public static IEnumerator BFS_TimeSlicing<T>(T start, Func<T, bool> targetCheck,
    Func<T, IEnumerable<T>> GetNeighbours, Action<IEnumerable<T>> callBack)
    {
        Queue<T> pending = new Queue<T>();
        HashSet<T> visited = new HashSet<T>();
        Dictionary<T, T> parent = new Dictionary<T, T>();
        T current = start;
        pending.Enqueue(current);

        while (pending.Any())
        {
            yield return new WaitForEndOfFrame();
            current = pending.Dequeue();
            visited.Add(current);

            if (targetCheck(current))
            {
                var path = Generate(current, x => parent[x])
                    .TakeWhile(x => parent.ContainsKey(x))
                    .Reverse();

                callBack(path);
            }
            else
            {
                var n = GetNeighbours(current).Where(x => !visited.Contains(x));
                foreach (var elem in n)
                {
                    pending.Enqueue(elem);
                }
            }
        }
    }
    //G = peso
    //H =  heuristica
    //F = G + H
    public static IEnumerable<T> AStar<T>(T start, Func<T, bool> targetCheck,
        Func<T, IEnumerable<Tuple<T, float>>> GetNeighbours, Func<T, float> GetHeuristic)
    {
        HashSet<T> visited = new HashSet<T>();
        Dictionary<T, T> parents = new Dictionary<T, T>();
        Dictionary<T, float> distances = new Dictionary<T, float>();
        Dictionary<T, float> F = new Dictionary<T, float>();
        List<T> pending = new List<T>();
        T current = start;
        pending.Add(current);
        distances.Add(start, 0);

        while (pending.Any())
        {
            current = pending.OrderBy(x => distances[x]).First();
            pending.Remove(current);
            visited.Add(current);

            if (targetCheck(current))
            {
                return Generate(current, x => parents[x])
                    .TakeWhile(x => parents.ContainsKey(x))
                    .Reverse();
            }
            else
            {
                var n = GetNeighbours(current).Where(x => !visited.Contains(x.Item1));
                foreach (var elem in n)
                {
                    var altDist = distances[current] + elem.Item2 + 
                        GetHeuristic(elem.Item1);

                    var currentDist = F.ContainsKey(elem.Item1) ? F[elem.Item1] : int.MaxValue;

                    if (altDist < currentDist)
                    {
                        F[elem.Item1] = altDist;
                        distances[elem.Item1] = distances[current] + elem.Item2;
                        parents[elem.Item1] = current;
                        pending.Add(elem.Item1);
                    }
                }
            }
        }
        return Enumerable.Empty<T>();
    }









}
