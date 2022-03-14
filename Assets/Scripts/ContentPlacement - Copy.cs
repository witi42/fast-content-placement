// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// /// <summary>
// /// Must be added to the camera
// /// Let's user place content using the mouse
// /// </summary>
// public class ContentPlacement : MonoBehaviour
// {
//
//     [SerializeField] private Slider scaleSlider;
//     
//     // Content
//     private GameObject _contentToPlace;
//     
//     // Instantiated Content
//     private GameObject _contentToModify;
//     private Vector3 originalScale;
//
//     // only RayCast for planes
//     private const int PlaneLayerMask = 1 << 7;
//     private Camera _camera; 
//
//     private void Awake()
//     {
//         _camera = gameObject.GetComponent<Camera>();
//     }
//
//     public void PlaceGameObject(GameObject gameObjectToPlace)
//     {
//         _contentToPlace = gameObjectToPlace;
//         _contentToModify = null;
//         originalScale = gameObjectToPlace.transform.localScale;
//         scaleSlider.value = 1f;
//     }
//
//     public void PlaceAutomatically()
//     {
//         RaycastHit hit;
//         if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity,
//             PlaneLayerMask))
//         {
//             // hit.transform.GetComponent<Renderer>().material.color = Color.green;
//             if (_contentToModify == null)
//             {
//                 _contentToModify = Instantiate(_contentToPlace);
//                 _contentToModify.SetActive(true);
//             }
//
//             _contentToModify.transform.position = hit.transform.position;
//             _contentToModify.transform.rotation = hit.transform.rotation;
//             _contentToModify.transform.Rotate(new Vector3(0f, 0f, -_contentToModify.transform.eulerAngles.z));
//             
//         }
//
//     }
//     
//     private void Update()
//     {
//
//         // skip if there is no content
//         if (_contentToPlace == null && _contentToModify == null)
//         {
//             scaleSlider.gameObject.SetActive(false);
//             return;
//         }
//         scaleSlider.gameObject.SetActive(true);
//             
//         
//         // check if mouse is on a plane
//         RaycastHit hit;
//         Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
//         if (!EventSystem.current.IsPointerOverGameObject (-1) && Input.GetMouseButton(0) && Physics.Raycast(ray,out hit, Mathf.Infinity, PlaneLayerMask))
//         {
//             // hit.transform.GetComponent<Renderer>().material.color = Color.green;
//             if (_contentToModify == null)
//             {
//                 _contentToModify = Instantiate(_contentToPlace);
//                 _contentToModify.SetActive(true);
//             }
//
//             _contentToModify.transform.position = hit.point;
//             _contentToModify.transform.rotation = hit.transform.rotation;
//             _contentToModify.transform.Rotate(new Vector3(0f, 0f, -_contentToModify.transform.eulerAngles.z));
//         }
//
//         _contentToModify.transform.localScale = originalScale * scaleSlider.value;
//
//     }
// }
