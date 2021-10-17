using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    #region Fields

    [SerializeField] GameObject _grenadePrefab;
    [SerializeField] GameObject _lineRendererPrefab;

    Scene _currentScene;
    Scene _predictionScene;

    PhysicsScene _currentPhysicsScene;
    PhysicsScene _predictionPhysicsScene;

    LineRenderer _lineRenderer;
    List<GrenadeSim> _grenades = new List<GrenadeSim>();

    // trajectories cache
    struct TrajectoryInfo
    {
        public bool available;
        public Vector3[] trajectory;
        public Vector3 origin;
        public Vector3 impulse;
    }
    Dictionary<Vector3, TrajectoryInfo> _trajectoryCache = new Dictionary<Vector3, TrajectoryInfo>();
    Coroutine _CoTrajectoryToTarget;

    #endregion

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Physics.autoSimulation = false;

        _currentScene = SceneManager.GetActiveScene();
        _currentPhysicsScene = _currentScene.GetPhysicsScene();

        CreateSceneParameters sceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        _predictionScene = SceneManager.CreateScene("PredictionScene", sceneParameters);
        _predictionPhysicsScene = _predictionScene.GetPhysicsScene();
        CloneColliders();

        // create the line renderer
        GameObject obj = Instantiate(_lineRendererPrefab, transform);
        _lineRenderer = obj.GetComponent<LineRenderer>();
        _lineRenderer.material.color = Color.red;
        _lineRenderer.loop = false;

        MatchManager.Instance.OnCharacterActivated += Instance_OnCharacterActivated;

    }

    private void Instance_OnCharacterActivated(Character character)
    {
        if (_CoTrajectoryToTarget != null)
            StopCoroutine(_CoTrajectoryToTarget);
        ClearGrenades();
        _trajectoryCache.Clear();
        return;
        GridEntity gridEntity = character.GetComponent<GridEntity>();
        List<GridNode> targets = new List<GridNode>();
        foreach (GridNode target in GridManager.Instance.GetGrid())
        {
            if (target.HasFloor && GridNode.EuclideanDistance(gridEntity.CurrentNode, target) < 10)
            {
                targets.Add(target);
            }
        }        
        _CoTrajectoryToTarget = StartCoroutine(CoTrajectoryToTarget(gridEntity, targets));
    }

    private void CloneColliders()
    {
        GameObject level = GameObject.Find("Level");
        GameObject clone = Instantiate(level.gameObject);
        SceneManager.MoveGameObjectToScene(clone, _predictionScene);
        foreach (Transform child in clone.GetComponentsInChildren<Transform>())
        {
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr)
            {
                Destroy(mr);
            }
        }
    }

    private void FixedUpdate()
    {
        if (_currentPhysicsScene.IsValid())
        {
            _currentPhysicsScene.Simulate(Time.fixedDeltaTime);
        }
    }

    public void ShowTrajectory(Vector3[] trajectory)
    {
        _lineRenderer.positionCount = trajectory.Length;
        _lineRenderer.SetPositions(trajectory);
    }

    public void ClearTrajectory()
    {
        _lineRenderer.positionCount = 0;
    }

    public bool TrajectoryToTarget(List<Vector3> origins, Vector3 target, out Vector3 impulse, out Vector3[] trajectory, out Vector3 origin)
    {
        // initialize out parameters
        impulse = Vector3.zero;
        trajectory = new Vector3[0];
        origin = Vector3.zero;

        if (_trajectoryCache.ContainsKey(target))
        {
            impulse = _trajectoryCache[target].impulse;
            trajectory = _trajectoryCache[target].trajectory;
            origin = _trajectoryCache[target].origin;
            return _trajectoryCache[target].available;
        }
        else
        {
            //Debug.Log("Stop");
            if (_CoTrajectoryToTarget != null)
                StopCoroutine(_CoTrajectoryToTarget);
            ClearGrenades();
        }

        UnityEngine.Profiling.Profiler.BeginSample("TrajectoryToTarget1");
        bool hit = false;
        
        //
        for (int o = 0; o < origins.Count; o++)
        {
            Vector3 direction = target - origins[o];
            direction.y = 0;
            Vector3 imp = direction.normalized;
            trajectory = new Vector3[0];
            if (direction.magnitude > 0)
            {
                Vector3 rotationAxis = Quaternion.AngleAxis(90, Vector3.up) * direction;
                for (int a = 75; a > 0; a -= 15)
                {
                    for (int f = 20; f > 0; f -= 1)
                    {
                        imp = Quaternion.AngleAxis(-a, rotationAxis) * (f * direction.normalized);
                        // instantiate the grenade
                        GameObject grenadeObject = Instantiate(_grenadePrefab, Vector3.zero, Quaternion.identity);
                        GrenadeSim grenade = grenadeObject.GetComponent<GrenadeSim>();
                        grenade.Impulse = imp;
                        grenade.Init(150);
                        SceneManager.MoveGameObjectToScene(grenadeObject, _predictionScene);
                        grenadeObject.transform.position = origins[o];
                        grenade.Origin = origins[o];
                        grenadeObject.GetComponent<Rigidbody>().AddForce(imp, ForceMode.Impulse);
                        //
                        _grenades.Add(grenade);
                    }
                }
            }
        }
        // simulate
        for (int i = 0; i < 150; i++)
        {
            if (hit)
            {
                break;
            }
            _predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
            foreach (var grenade in _grenades)
            {
                grenade.AddPoint(grenade.transform.position);
                if ((grenade.transform.position - target).magnitude < 0.5f)
                {
                    impulse = grenade.Impulse;
                    trajectory = grenade.Trajectory;
                    origin = grenade.Origin;
                    hit = true;
                    break;
                }
            }
        }
        ClearGrenades();
        CacheTrajectory(target, hit, impulse, trajectory, origin);
        UnityEngine.Profiling.Profiler.EndSample();

        return hit;
    }

    IEnumerator CoTrajectoryToTarget(GridEntity gridEntity, List<GridNode> targets)
    {
        foreach (GridNode target in targets)
        {
            List<Vector3> origins = new List<Vector3>() { gridEntity.CurrentNode.FloorPosition + 2 * Vector3.up };
            foreach (var sidestep in GridCoverManager.Instance.SideSteps(gridEntity, target))
            {
                origins.Add(sidestep.FloorPosition + 2 * Vector3.up);
            }
            for (int o = 0; o < origins.Count; o++)
            {
                Vector3 direction = target.FloorPosition - origins[o];
                direction.y = 0;
                Vector3 impulse = direction.normalized;
                if (direction.magnitude > 0)
                {
                    Vector3 rotationAxis = Quaternion.AngleAxis(90, Vector3.up) * direction;
                    for (int a = 75; a > 0; a -= 15)
                    {
                        for (int f = 20; f > 0; f -= 5)
                        {
                            impulse = Quaternion.AngleAxis(-a, rotationAxis) * (f * direction.normalized);
                            // instantiate the grenade
                            GameObject grenadeObject = Instantiate(_grenadePrefab, Vector3.zero, Quaternion.identity);
                            GrenadeSim grenade = grenadeObject.GetComponent<GrenadeSim>();
                            grenade.enabled = false;
                            grenade.Impulse = impulse;
                            grenade.Origin = origins[o];
                            grenade.Target = target.FloorPosition;
                            grenade.Init(150);
                            SceneManager.MoveGameObjectToScene(grenadeObject, _predictionScene);
                            grenadeObject.transform.position = origins[o];
                            grenadeObject.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
                            //
                            _grenades.Add(grenade);
                        }
                    }
                }
            }
        }
        // simulate
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                _predictionPhysicsScene.Simulate(Time.fixedDeltaTime);
                foreach (var grenade in _grenades)
                {
                    grenade.AddPoint(grenade.transform.position);
                    if ((grenade.transform.position - grenade.Target).magnitude < 0.5f)
                    {
                        Debug.Log($"Trajectory found!");
                        CacheTrajectory(grenade.Target, true, grenade.Impulse, grenade.Trajectory, grenade.Origin);
                        break;
                    }
                }
            }
            yield return null;
        }
        ClearGrenades();
    }

    private void CacheTrajectory(Vector3 target, bool hit, Vector3 impulse, Vector3[] trajectory, Vector3 origin)
    {
        if (!_trajectoryCache.ContainsKey(target))
        {
            if (hit)
            {
                TrajectoryInfo trajectoryInfo = new TrajectoryInfo();
                trajectoryInfo.trajectory = trajectory;
                trajectoryInfo.origin = origin;
                trajectoryInfo.impulse = impulse;
                trajectoryInfo.available = true;
                _trajectoryCache.Add(target, trajectoryInfo);
            }
            else
            {
                TrajectoryInfo trajectoryInfo = new TrajectoryInfo();
                trajectoryInfo.available = false;
                _trajectoryCache.Add(target, trajectoryInfo);
            }
        }
    }

    void ClearGrenades()
    {
        foreach (var grenade in _grenades)
        {
            Destroy(grenade.gameObject);
        }
        _grenades.Clear();
    }

    #region Singleton

    private static TrajectoryPredictor _instance;

    public static TrajectoryPredictor Instance { get { return _instance; } }

    #endregion
}
