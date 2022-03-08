using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSub : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3f);
        if (IAPManager.Instance.HadPurchased(IAPManager.ProductSubscription))
        {
            Debug.Log("±¸µ¶ Áß");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
