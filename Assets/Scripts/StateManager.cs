using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlacementCriteria;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{
    [Serializable]
    public struct Content
    {
        public GameObject Obj;
        public Texture Preview;
        public PositionCriterion positionCriterion;
    }

    // GameObjects
    [SerializeField] private GameObject contentPlacementCamera;
    [SerializeField] private GameObject addButton;
    [SerializeField] private GameObject smartPlaceButton;
    [SerializeField] private GameObject selectMenu;
    [SerializeField] private GameObject selectMenuContentParent;
    
    [SerializeField] private List<Content> contentList;

    // Prefabs
    [SerializeField] private GameObject buttonPrefab;
    
    private ContentPlacement _contentPlacement;


    public enum MyState
    {
        Ready,
        SelectContent,
        PlaceContent
    }

    [HideInInspector] public MyState myState = MyState.Ready;

    private void Start()
    {
        addButton.GetComponent<Button>().onClick.AddListener(() => {GoToState(MyState.SelectContent); });
        _contentPlacement = contentPlacementCamera.GetComponent<ContentPlacement>();
        smartPlaceButton.GetComponent<Button>().onClick.AddListener(() => {_contentPlacement.AutoPlace(); });
        PopulateSelectMenu();
        GoToState(MyState.Ready);
    }
    
    
    public void GoToState(MyState state)
    {
        LeaveState(state);

        switch (state)
        {
            case MyState.Ready:
            {
                addButton.SetActive(true);
                selectMenu.SetActive(false);
                break;
            }
            case MyState.SelectContent:
            {
                addButton.SetActive(false);
                selectMenu.SetActive(true);
                break;
            }
            case MyState.PlaceContent:
            {
                addButton.SetActive(false);
                selectMenu.SetActive(false);
                break;
            }
        }

        myState = state;

    }

    private void LeaveState(MyState state)
    {
        switch (state)
        {
            case MyState.Ready:
            {
                break;
            }
            case MyState.SelectContent:
            {
                break;
            }
            case MyState.PlaceContent:
            {
                break;
            }
        }
    }

    private void PopulateSelectMenu()
    {
        foreach (Content c in contentList)
        {
            GameObject button = Instantiate(buttonPrefab, selectMenuContentParent.transform);
            
            var text = button.transform.GetChild(0).GetComponent<Text>();
            text.fontSize = 60;

            if (c.Preview != null)
            {
                button.GetComponent<Image>().sprite = Sprite.Create((Texture2D)c.Preview, new Rect(0.0f, 0.0f, c.Preview.width, c.Preview.height), new Vector2(0.5f, 0.5f), 100.0f);
                text.text = "";
            }
            else
            {
                text.text = c.Obj.name;
            }
        
            button.GetComponent<Button>().onClick.AddListener(() => { 
                _contentPlacement.PlaceGameObject(c.Obj);
                _contentPlacement.AutoPlace();
                GoToState(MyState.Ready); });
        }

    }
}
