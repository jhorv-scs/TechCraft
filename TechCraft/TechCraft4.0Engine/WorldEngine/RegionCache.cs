using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Threading;

namespace TechCraftEngine.WorldEngine
{
    public class RegionCache
    {
        private const int CACHE_SIZE = 25;

        private Region[] _regions;
        private bool[] _availability;
        private Queue<Region> _available;
        private World _world;
        private TechCraftGame _game;

        private Queue<Vector3> _toLoad;
        private Thread _loadingThread;

        private Queue<Region> _toBuild;
        private Thread _buildingThread;
       // private ThreadManager _threadManager;

        private bool _running = true;
        private Vector3 _playerPosition;

        public RegionCache(TechCraftGame game)
        {
            _game = game;
        }

        public Game Game
        {
            get { return _game; }
        }

        public void Initialize(World world)
        {
            _world = world;
            _regions = new Region[CACHE_SIZE];
            _availability = new bool[CACHE_SIZE];
            _available = new Queue<Region>(CACHE_SIZE);

            _toLoad = new Queue<Vector3>();
            _toBuild = new Queue<Region>();

            Clear();

           // _threadManager = (ThreadManager)Game.Services.GetService(typeof(ThreadManager));

            _loadingThread = new Thread(new ThreadStart(LoadingThread));
           // _threadManager.Add(_loadingThread);
            _loadingThread.Start();

            _buildingThread = new Thread(new ThreadStart(BuildingThread));
         //   _threadManager.Add(_buildingThread);
            _buildingThread.Start();
        }

        public Vector3 PlayerPosition
        {
            get { return _playerPosition; }
            set { _playerPosition = value; }
        }

        public void Clear()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                _availability[i] = true;
                _regions[i] = new Region(Game);
                //_regions[i].Initialize(_world, _world.RegionSize);
                _regions[i].InitRegion();
                _available.Enqueue(_regions[i]);
                _regions[i].NodeIndex = i;
            }
        }

        public void Flush(Vector3 position, float radius)
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i])
                {

                    float distance = (_regions[i].Center - position).Length();
                    if (distance < 0) distance = 0 - distance;
                    if (distance > radius)
                    {
                        //Debug.WriteLine(string.Format("Flushing {0} : {1},{2},{3}", _regions[i].GetFilename(), _regions[i].Center, position, distance)); 
                        _regions[i].Flush(true);
                        _availability[i] = true;
                        _available.Enqueue(_regions[i]);
                    }
                }
            }
        }

        public void Save()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i])
                {
                    _regions[i].Flush(false);
                }
            }
        }

        public Region FindRegion(Vector3 regionPosition)
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i] && _regions[i].RegionPosition == regionPosition)
                {
                    return _regions[i];
                }
            }
            return null;
        }

        public void UpdateDirtySceneObjects()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i] && _regions[i].Dirty)
                {
                    _regions[i].UpdateSceneObject();
                }
            }
        }

        public void SubmitModifiedRegionsForBuild()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                if (!_availability[i] && _regions[i].Modified)
                {
                    QueueBuild(_regions[i]);
                    _regions[i].Modified = false;
                }
            }
        }

        public void QueueBuild(Region region)
        {
            //Debug.WriteLine(string.Format("Queue Build {0}-{1}-{2}", (int) region.RegionPosition.X, (int) region.RegionPosition.Y, (int) region.RegionPosition.Z));
            lock (_toBuild)
            {
                _toBuild.Enqueue(region);
            }
        }

        public void QueueLoad(Vector3 position)
        {
            //Debug.WriteLine(string.Format("Queue Load {0}-{1}-{2}", (int) position.X, (int) position.Y, (int) position.Z));
            lock (_toLoad)
            {
                foreach (Vector3 check in _toLoad)
                {
                    if (position == check)
                    {
                        //Debug.WriteLine("Already queued");
                        return;
                    }
                }
                if (IsLoaded(position)) return;
                _toLoad.Enqueue(position);
            }
        }

        public bool IsLoaded(Vector3 position)
        {
            if (FindRegion(position) == null) return false;
            //Debug.WriteLine("Already loaded");
            return true;
        }

        public void BuildingThread()
        {
#if XBOX
            _buildingThread.SetProcessorAffinity(4);
#endif
            while (_running)
            {
                Region buildRegion = null;
                bool doBuild = false;
                lock (_toBuild)
                {
                    if (_toBuild.Count > 0)
                    {
                        buildRegion = _toBuild.Dequeue();
                        doBuild = true;
                    }
                }
                if (doBuild)
                {
                    DoBuild(buildRegion);
                }
                Thread.Sleep(1);
            }
        }

        public void DoBuild(Region region)
        {
            region.Build();
        }

        public void LoadingThread()
        {
#if XBOX
            _loadingThread.SetProcessorAffinity(4);
#endif
            while (_running)
            {
                Vector3 loadPosition = Vector3.Zero;
                bool doLoad = false;
                lock (_toLoad)
                {
                    if (_toLoad.Count > 0)
                    {
                        loadPosition = _toLoad.Dequeue();
                        doLoad = true;
                    }
                }
                if (doLoad)
                {
                    DoLoad(loadPosition);
                }
                Thread.Sleep(1);
            }
        }

        public void DoLoad(Vector3 regionPosition)
        {
            if (FindRegion(regionPosition) == null)
            {
                QueueBuild(LoadRegion(regionPosition));
            }
        }

        private Region LoadRegion(Vector3 position)
        {
            Flush(_playerPosition, 250);
            if (_available.Count > 0)
            {
                Region region = _available.Dequeue();
                _availability[region.NodeIndex] = false;

                region.Load(position);

                return region;
            }
            else
            {
                throw new Exception("No available regions");
            }
        }

        public Region GetRegion(Vector3 position)
        {
            Region region = FindRegion(position);
            if (region != null)
            {
                //Debug.WriteLine(string.Format("Found : {0},{0},{0}", position.x, position.y, position.z));
                return region;
            }
            else
            {
                //return LoadRegion(position);
                //StartLoading(position);
                return null;
            }
        }
    }
    
}
