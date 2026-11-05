using UnityEngine;

public class FinalBaglayici : MonoBehaviour
{
    [SerializeField] private RifkiAmca amca;

    private RifkiDiyalog diyalog;

    private void Start()
    {
        if (amca == null)
        {
            Debug.LogWarning("FinalBaglayici: Amca kutusu boş.");
            return;
        }

        diyalog = gameObject.AddComponent<RifkiDiyalog>();
        amca.Ulasti += AmcaUlasti;

        if (WaiterFlow.Instance != null)
            amca.Dinle(WaiterFlow.Instance);
        else
            Debug.LogWarning("FinalBaglayici: WaiterFlow yok (test modu).");
    }

    private void OnDestroy()
    {
        if (amca != null) amca.Ulasti -= AmcaUlasti;
    }

    private void AmcaUlasti()
    {
        diyalog.Baslat(amca.Mutlu);
    }
}