using System;
using UnityEngine;
using Visualizer.AgentBrains;
using Visualizer.UI;

namespace Visualizer.GameLogic
{
    public enum AGENT_STATE // assuming z is looking up and x to the right and we are looking down in 2D
    {
        NOT_RUNNING = 0, // was never running 
        RUNNING, // is running right now
        PAUSED, // is pause, but can be resumed
    }

    public class Agent : MonoBehaviour
    {
        private BaseBrain _currentBrain;
        private Map _currentMap;
        private Tile _currentTile;
        public Tile CurrentTile
        {
            get => _currentTile;
            set
            {
                _currentTile = value;
                OnTileChange?.Invoke();
            }
        }
        
        // agent initial position
        private Tile _initialTile;
        
        // state variables

        private int _steps;
        private int _turns;
        
        // delegates

        public event Action OnTileChange; // called when the agent moves a Tile

        public int Steps
        {
            get => _steps;
            set { _steps = value; SendTelemetry(); }
        }

        public int Turns
        {
            get => _turns;
            set { _turns = value; SendTelemetry(); }
        }

        
        // telemetry object, reused every time
        private AgentTelemetry _telemetry = new AgentTelemetry();

        private AGENT_STATE _state = AGENT_STATE.NOT_RUNNING; // created as not running, needs to be initialized 
        


        private AgentAction _lastAction = null;
        
        void Init( Map map , int x , int z )
        {
            _currentMap = map;
            _currentMap.SetActiveAgent(this);

            _initialTile = _currentTile = _currentMap.GetTile(x, z);
            gameObject.transform.transform.position = _currentTile.GetWorldPosition();
            
            // hook the needed events
            GameStateManager.Instance.OnSceneReset += ResetAgent;
            GameStateManager.Instance.OnScenePause += PauseAgent;
            GameStateManager.Instance.OnSceneStart += StartAgent;
            GameStateManager.Instance.OnSceneResume += StartAgent;
            
            // init telemetry with start values
            SendTelemetry();
        }

        void Init( Map map, AgentState state)
        {
            Init( map , state.tileX , state.tileZ );
        }

        void FixedUpdate()
        {
            if (_state == AGENT_STATE.RUNNING /*&& _currentBrain.IsReady*/ )
            {
                Move();
            }
        }
        
        public static void SetSpeed(int speedMultiplier) // sets the multiplier globally for all agents
        {
            GoAction.SetMultiplier(speedMultiplier); // set it for all future GoActions
        }

        private void Move()
        {
            if (_lastAction == null || _lastAction.IsDone())
            {
                _lastAction = _currentBrain.GetNextAction();
                _lastAction?.Do(this);
            }
        }
        
        public static Agent CreateAgent(BaseBrain brain, Map map, AgentState state)
        {
            var gameObject = Instantiate(PrefabContainer.Instance.agentPrefab);
            var component = gameObject.AddComponent<Agent>();

            if (state.valid) // was the agent position saved with the Map ? 
            {
                component.Init( map , state );
            }
            else
            {
                component.Init( map , 0 , 0 ); // 0 0 as defaults
            }
            return component;
        }
        public static Agent CreateAgent( Map map ,  int x , int z )
        {
            var gameObject = Instantiate(PrefabContainer.Instance.agentPrefab , PrefabContainer.Instance.mapReference.transform ); //TODO: transform shouldn't be used here
            var component = gameObject.AddComponent<Agent>();

            component.Init( map , x, z );
            return component;
        }

        public void SetBrain(BaseBrain brain)
        {
            _currentBrain = brain;
            brain.SetAttachedAgent(this);
        }

        public Vector2 GetGridPosition()
        {
            return new Vector2( _currentTile.GridX , _currentTile.GridZ );
        }

        public void SendTelemetry()
        {
            _telemetry.Steps = this._steps;
            _telemetry.Turns = this._turns;
            
            GlobalTelemetryHandler.Instance.UpdateAgentTelemetry(_telemetry);
        }

        public void HookToEvent( Action callBack )
        {
            OnTileChange += callBack;
        }

        public void UnHookEvent(Action callback)
        {
            OnTileChange -= callback;
        }

        // forward to the brain

        public void StartAgent()
        {
            if ( _state == AGENT_STATE.NOT_RUNNING )
            {
                _currentBrain.Start( this ); // if he was not running before
            }

            _state = AGENT_STATE.RUNNING;
        }

        public void PauseAgent()
        {
            //TODO: check again, is this state really needed ?
            // _currentBrain.Pause();
            _state = AGENT_STATE.PAUSED;
        }

        public void ResetAgent()
        {
            _state = AGENT_STATE.NOT_RUNNING;
            // reset the agents position
            _currentTile = _initialTile;
            gameObject.transform.position = _currentTile.GetWorldPosition();

            // reset brain before removing it
            _currentBrain?.Reset();
            _currentBrain = null;

            // clear telemetry data
            Steps = Turns = 0;

            if (OnTileChange != null)
            {
                // unhook all events
                foreach (var eventHandler in OnTileChange?.GetInvocationList())
                {
                    OnTileChange -= (Action) eventHandler;
                }

            }
        }

        public void Destroy()
        {
            // unhook all events
            GameStateManager.Instance.OnSceneReset -= ResetAgent;
            GameStateManager.Instance.OnScenePause -= PauseAgent;
            GameStateManager.Instance.OnSceneStart -= StartAgent;
            GameStateManager.Instance.OnSceneResume -= StartAgent;
            
            Destroy(gameObject); // byebye!
        }
        
    }
}
