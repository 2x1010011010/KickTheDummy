using System.Collections;
using System.Collections.Generic;
using Infrastructure.Services.LoadingService;
using Unity.VisualScripting;
using UnityEngine;

public class SelectSceneHUD : MonoBehaviour
{
   [SerializeField] private SceneLoader _loader;

   public void LoadNext(string scene)
   {
      _loader.LoadSceneAsync(scene);
   }
}
