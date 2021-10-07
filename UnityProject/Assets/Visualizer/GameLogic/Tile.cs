using System;
using UnityEngine;
using UnityEngine.Scripting;
using Visualizer.UI;

namespace Visualizer
{
    [Serializable()]
    public enum TILE_EDGE // assuming z is looking up and x to the right and we are looking down in 2D
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2 ,
        LEFT = 3
    }

    static class TileEdgeExtension
    {
        public static TILE_EDGE getOpposite( this TILE_EDGE edge)
        {
            return (TILE_EDGE) (((int) edge + 2) % 4); // get opposite direction or edge
        }
    }
    
    public class Tile : MonoBehaviour
    {
        private GameObject _tilePrefab;
        private GameObject _wallPrefab = Epoch.Instance._wallPrefab;
        private GameObject _upperWall;
        private GameObject _rightWall;

        private const int PlaneSize = 10;
        public int x { get; private set; }
        public int z { get; private set; }

        //TODO: data is stored in a separate non Monobehavior class so that we can serialize it
        //TODO: maybe there exists a cleaner way to do it
        private TileState data;
        
        public void Init(int x, int z, TileState state)
        {
            gameObject.transform.localPosition = new Vector3(x*PlaneSize, 0, z*PlaneSize);
            this.x = x;
            this.z = z;
            data = state; // assign state
        }

        public void Update()
        {
            // do something here
        }
        
        public static Tile CreateTile(GameObject prefab , Transform parent ,  int x , int y , TileState state = null )
        {
            var plane = Instantiate(prefab , parent);
            var component = plane.AddComponent<Tile>();

            state ??= new TileState();
            
            component.Init(x,y , state);
            return component;
        }

        public bool IsDirty
        {
            get
            {
                return data.isDirty;
            }
            set
            {
                data.isDirty = value;
                Refresh(); // refresh tile
            }
        }

        public void setWall ( TILE_EDGE edge , bool present )
        {
            // place a wall on that edge
            data.hasWallOnEdge[(int)edge] = present;
            Refresh();
        }

        public bool hasWall( TILE_EDGE edge )
        {
            return data.hasWallOnEdge[(int) edge]; 
        }

        public TileState getTileState()
        {
            return data;
        }

        public Vector3 getTileWorldPos()
        {
            return gameObject.transform.position;
        }

        public TILE_EDGE GetClosestEdge(Vector3 pointOnTile)
        {
            // both pointOnTile and the return are in global coordinates
            
            // transform point to origin
            Vector3 toOrigin = pointOnTile - gameObject.transform.position;

            // get min of the 4 perpendiculars to the edges
            // TODO: very ugly, find better way to write this section
            
            float distanceUp = Math.Abs(5 - toOrigin.z);
            float distanceDown = Math.Abs(-5 - toOrigin.z);

            float distanceRight = Math.Abs(5 - toOrigin.x);
            float distanceLeft = Math.Abs(-5 - toOrigin.x);

            float min = Math.Min(distanceUp, Math.Min(distanceDown, Math.Min(distanceRight, distanceLeft)));
            
            // return closest edge
            if (min == distanceUp)
            {
                return TILE_EDGE.UP;
            }
            if ( min == distanceDown )
            {
                return TILE_EDGE.DOWN;
            }
            if (min == distanceRight)
            {
                return TILE_EDGE.RIGHT;
            }

            return TILE_EDGE.LEFT;
        }

        public Vector3 GetClosestEdgeWorldPos(Vector3 pointOnTile)
        {
            // first find out where the closest edge is
            var closest = GetClosestEdge(pointOnTile);

            return gameObject.transform.position + new Vector3(
                closest == TILE_EDGE.RIGHT ? 5 : (closest == TILE_EDGE.LEFT) ? -5 : 0,
                0,
                closest == TILE_EDGE.UP ? 5 : (closest == TILE_EDGE.DOWN) ? -5 : 0);
        }
    
        //TODO: remove magic numbers
        public void Refresh()
        {
            // Refresh the Tile graphics ( Dirt + Tiles )
            // Tile is only responsible for checking its right and upper walls
            
            // create the up wall
            if (hasWall(TILE_EDGE.UP) && _upperWall == null )
            {
                _upperWall = Instantiate(_wallPrefab , gameObject.transform);
                _upperWall.transform.localPosition = new Vector3(0, 0, 5);
            }
            
            // remove the up wall
            if (!hasWall(TILE_EDGE.UP) && _upperWall != null )
            {
                Destroy(_upperWall);
            }
            
            // create the right wall
            if (hasWall(TILE_EDGE.RIGHT) && _rightWall == null)
            {
                _rightWall = Instantiate(_wallPrefab, gameObject.transform);
                _rightWall.transform.localPosition = new Vector3(5, 0, 0);
                _rightWall.transform.rotation = Quaternion.Euler(0,90,0);
            }
            
            // remove the right wall
            if (!hasWall(TILE_EDGE.RIGHT) && _rightWall != null)
            {
                Destroy(_rightWall);
            }
            
            // // is it has any dirt assigned
            var rend = gameObject.GetComponent<Renderer>();
            // control the second detail albedo map to show or hide the dirt
            //TODO: maybe this is not so efficient ? 
            rend.material.EnableKeyword("_DETAIL_MULX2");
            rend.material.SetTexture("_DetailAlbedoMap" , IsDirty ? Epoch.Instance._dirtTexture : null );
        }
        
        
    }
}


