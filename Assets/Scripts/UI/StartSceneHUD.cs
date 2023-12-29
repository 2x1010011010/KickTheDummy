using Infrastructure.Services.LoadingService;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartSceneHUD : MonoBehaviour
    {
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private string _nextScene;
        [SerializeField] private GameObject _settingsWindow;
        [SerializeField] private GameObject _mainWindow;
        
        private void Awake()
        {
            _settingsWindow.SetActive(false);
        }

        public void LoadNext() => 
            _sceneLoader?.LoadSceneAsync(_nextScene);

        public void OpenSettings()
        {
            _mainWindow.SetActive(false);
            _settingsWindow.SetActive(true);

        }

        
        public void CloseSettings()
        {
            _settingsWindow.SetActive(false);
            _mainWindow.SetActive(true);
        }

    }
}
