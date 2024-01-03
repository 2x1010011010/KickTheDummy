using System.Collections;
using System.Collections.Generic;
using Infrastructure.Services.LoadingService;
using UnityEngine;

public class TempBootstrap : MonoBehaviour
{
  [SerializeField] private SceneLoader _loader;
  private void Awake()
  {
    _loader.LoadSceneAsync("Start");
  }
}