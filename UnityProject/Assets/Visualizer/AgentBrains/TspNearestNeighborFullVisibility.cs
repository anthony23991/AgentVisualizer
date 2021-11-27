using System;
using System.Collections;
using System.Collections.Generic;
using Visualizer.Algorithms;
using Visualizer.GameLogic;
using Visualizer.UI;

namespace Visualizer.AgentBrains
{
    public class TspNearestNeighborFullVisibility : BaseBrain
    {
        private GraphicalBoard _currentGraphicalBoard;
        private Agent actor;
        
        // for Brain Telemetry
        private List<BrainMessageEntry> _messages = new List<BrainMessageEntry>();
        private int _globalPathLength = 0;

        public int GlobalPathLength
        {
            get => _globalPathLength;
            private set
            {
                _globalPathLength = value;
                SendTelemetry(_globalPathLength);
            }
        }

        public TspNearestNeighborFullVisibility( GraphicalBoard graphicalBoard )
        {
            _currentGraphicalBoard = graphicalBoard;
            Commands = new Queue<AgentAction>();

            _messages.Add(new BrainMessageEntry( "global path length:" , "" ));
        }

        private IEnumerator GenerateGlobalPath()
        {
            Tile currentTile = actor.CurrentTile;
            var dirtyTiles = _currentGraphicalBoard.GetAllDirtyTiles();

            GlobalPathLength = 0;

            while (dirtyTiles.Count > 0)
            {
                GlobalPathLength +=
                    GetPathToNearestNeighbor(_currentGraphicalBoard, dirtyTiles, currentTile, Commands, out var closestTile);
                currentTile = closestTile; // start position for next iteration is the current closest Dirt Tile

                //TODO: use index, runs in O(N) now!!!!
                dirtyTiles.Remove(closestTile); // so it won't be picked again
            }

            yield return null;
        }

        public static int GetPathToNearestNeighbor( Board graphicalBoard , List<Tile> dirtyTiles , Tile start , Queue<AgentAction> commands , out Tile closestGraphicalTile )
        {
            // find closest tile to currentTile
            closestGraphicalTile = GetNearestDirty(graphicalBoard, dirtyTiles, start);
                
            // found the closest tile
            // get the path to it

            Bfs.DoBfs( graphicalBoard , start , closestGraphicalTile , out var path );

            path.RemoveAt(0); // agent would be on this tile already,
            // Bfs returns it for correctness
            // add all to command list
            foreach (var tile in path)
            {
                commands.Enqueue(new GoAction(tile));
            }
            
            commands.Enqueue(new CleanDirtAction(closestGraphicalTile));

            return path.Count;
        }

        public static Tile GetNearestDirty(  Board graphicalBoard , List<Tile> dirtyTiles  , Tile startGraphicalTile )
        {
            // assumes the list if not empty
            // find closest tile to currentTile
            var minIndex = 0;
            var minDistance = Int32.MaxValue;
                
            for (var i = 0; i < dirtyTiles.Count; ++i)
            {
                var temp = graphicalBoard.BfsDistance(startGraphicalTile, dirtyTiles[i]);
                if (minDistance > temp )
                {
                    minDistance = temp;
                    minIndex = i;
                }
            }

            return dirtyTiles[minIndex];
        }
        
        public override void Start(Agent actor)
        {
            // Init telemetry
            GlobalPathLength = 0; // sends telemetry
            this.actor = actor;

            // start path generation
            actor.StartCoroutine(GenerateGlobalPath());
        }

        public new void Reset()
        {
            base.Reset();
            // reset telemetry
            GlobalTelemetryHandler.Instance.DestroyBrainTelemetryFields();
        }
        
        private void SendTelemetry( int value )
        {
            _messages[0].value = "" + value;
            GlobalTelemetryHandler.Instance.UpdateBrainTelemetry(_messages);
        }
    }
}