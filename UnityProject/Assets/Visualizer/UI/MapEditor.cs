using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Visualizer;
using Visualizer.UI;

public class MapEditor : MonoBehaviour
{
    private Camera _currentCamera;
    private ItemPlacer _currentPlacer = null;

    void Start()
    {
        _currentCamera = Camera.main;
    }

    private Vector3 _worldPos;

    void Update() // should work everytime we are in editing Mode                                                                                                                           
    {
        if (_currentPlacer != null) // no picker, don't do anything
        {
            if (isMouseOnMap(out _worldPos))
            {
                _currentPlacer.Update(_worldPos);
            }

            if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse)) // place an item 
            {
                _currentPlacer.PlaceItem(); // place picked item if possible
            }

            if (Input.GetMouseButtonDown((int) MouseButton.RightMouse)) // remove an item
            {
                _currentPlacer.RemoveItem(); // remove picked item if possible or if any
            }
        }
    }

    private bool isMouseOnMap(out Vector3 position)
    {
        Ray inputRay = _currentCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            position = transform.InverseTransformPoint(hit.point);
            return true;
        }

        position = Vector3.negativeInfinity;
        return false;
    }
    
    public void OnLoadMap()
    {
        // open a file chooser dialog
        var path = EditorUtility.OpenFilePanel("Select Map" , "Assets/Visualizer/Maps" , "map");

        if (path.Length != 0)
        {
            GameManager.Instance.Load(path);
        }
    }

    public void OnSaveMap()
    {
        var path = EditorUtility.SaveFilePanel("Save Map", "Assets/Visualizer/Maps", "Map", "map");

        if (path.Length != 0)
        {
            GameManager.Instance.Save(path);
        }
    }

    public void OnWallPicked()
    {
        _currentPlacer?.CleanUp();
        _currentPlacer = new WallPlacer(); // create a wall placer
    }

    public void OnDirtyTilePicked()
    {
        _currentPlacer?.CleanUp();
        _currentPlacer = new DirtPlacer(); // create a dirty tile placer
    }

    public void OnAgentPicked()
    {
        _currentPlacer?.CleanUp();
        _currentPlacer = new AgentPlacer(); // create an agent placer
    }
}
