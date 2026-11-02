using UnityEngine;
using UnityEngine.SceneManagement;

public class AnaMenu : MonoBehaviour
{
    [SerializeField] private string ilkSahne = "Sinematik";

    public void Basla()
    {
        SceneManager.LoadScene(ilkSahne);
    }
}