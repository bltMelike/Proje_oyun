using UnityEngine;

public class OturmaNoktasi : MonoBehaviour
{
    [SerializeField] private bool sagaBakar = true;
    [Tooltip("Müşteri önce bu noktalardan sırayla geçer, sonra sandalyeye oturur. Engellerin arasından geçen yol çiz.")]
    [SerializeField] private Transform[] araNoktalar;

    public bool SagaBakar => sagaBakar;
    public Transform[] AraNoktalar => araNoktalar;
    public bool Dolu { get; private set; }

    public void Isgal(bool dolu)
    {
        Dolu = dolu;
    }
}