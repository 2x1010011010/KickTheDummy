using UnityEngine;
using UnityEngine.UI;

public class Tool : MonoBehaviour
{
    [SerializeField] private Image _image; 
    public void SetColor(Color color)
    {
        _image.color = color;
    }
}
