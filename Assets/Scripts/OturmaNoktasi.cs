using UnityEngine;

public class OturmaNoktasi : MonoBehaviour
{
    [Tooltip("İşaretliyse müşteri oturunca sağa bakar. Masanın SAĞINDAki sandalyede kapalı olmalı.")]
    [SerializeField] private bool sagaBakar = true;

    public bool SagaBakar => sagaBakar;
    public bool Dolu { get; private set; }

    public void Isgal(bool dolu)
    {
        Dolu = dolu;
    }
}