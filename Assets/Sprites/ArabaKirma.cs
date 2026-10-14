using System.Collections;
using UnityEngine;

public class ArabaKirma : MonoBehaviour
{
    [SerializeField] private Transform hedef;
    [SerializeField] private float hiz = 10f;

    public IEnumerator Gel()
    {
        while (Vector2.Distance(transform.position, hedef.position) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, hedef.position, hiz * Time.deltaTime);
            yield return null;
        }
    }
}