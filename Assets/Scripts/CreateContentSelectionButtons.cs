// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
//
// public class CreateContentSelectionButtons : MonoBehaviour
// {
//     public GameObject buttonPrefab;
//     public GameObject contentPlacementCamera;
//     public GameObject stateManagerGameObject;
//     public GameObject contentCubePrefab;
//     public List<Texture> textures;
//
//     private ContentPlacement contentPlacement;
//     private StateManager stateManager;
//     void Start()
//     {
//         contentPlacement = contentPlacementCamera.GetComponent<ContentPlacement>();
//         stateManager = stateManagerGameObject.GetComponent<StateManager>();
//         Populate();
//     }
//
//     private void Populate()
//     {
//         foreach (var tex in textures)
//         {
//             var button = Instantiate(buttonPrefab, transform);
//             button.GetComponent<Image>().sprite = Sprite.Create((Texture2D)tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
//             
//             
//             button.GetComponent<Button>().onClick.AddListener(() => { contentPlacement.contentTexture = tex;
//                 contentPlacement.UpdateContentToPlace();
//                 stateManager.GoToState(StateManager.MyState.Ready); });
//         }
//     }
// }
