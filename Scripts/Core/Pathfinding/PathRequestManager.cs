using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PixelMiner.Core
{
    public class PathRequestManager : MonoBehaviour
    {
        public static PathRequestManager Instance { get; private set; }
        private Queue<PathRequest> _pathRequestQueue = new();


        [SerializeField] private int _maxPathRequest = 2;

        // Limit update timer
        private float _updateTimer = 0.0f;
        private float _updateTime = 0.02f;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {

        }

        private void Update()
        {
  

            _updateTimer += UnityEngine.Time.deltaTime;
            if (_updateTimer > _updateTime)
            {
                _updateTimer = 0f;


                while (_pathRequestQueue.Count > 0)
                {
                    //var request = _pathRequestQueue.Dequeue();
                    //bool foundPath = await Pathfinding.FindPathTask(request, 100);
                    //request.OnRequestComplete(foundPath);

                    HandlePathRequest(_pathRequestQueue.Dequeue());
                }
            }
        }

        private async void HandlePathRequest(PathRequest request)
        {
            bool foundPath = await Pathfinding.FindPathTask(request, 100);
            request.OnRequestComplete(foundPath);
        }

        public void RequestPath(PathRequest request)
        {
            _pathRequestQueue.Enqueue(request);
        }


        /*private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if(_requests.Length > 0)
                {
                    for(int i = 0; i < _requests.Length; i++)
                    {
                        if (_requests[i] == null) continue;
                        Vector3 offsetCenter = Vector3.one * 0.5f;
                        if (_requests[i].Path.Count > 0)
                        {
                            for (int j = 0;j < _requests[i].Path.Count - 1; j++)
                            {
                                Gizmos.DrawLine(_requests[i].Path[j] + offsetCenter, _requests[i].Path[j + 1] + offsetCenter);
                            }


                            //Gizmos.color = Color.cyan;
                            //for (int i = 0; i < request.Path.Count; i++)
                            //{
                            //    Gizmos.DrawCube(request.Path[i] + offsetCenter, new Vector3(0.5f, 0.5f, 0.5f));
                            //}
                        }
                    }
                }          
            }
        }*/
    }
}
