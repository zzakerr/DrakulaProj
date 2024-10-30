using UnityEngine;

[RequireComponent(typeof(InteractiveObject))]
public class Key : MonoBehaviour
{
    [SerializeField] private int keyCount;
    [SerializeField] private GameObject impactEffect;

    private InteractiveObject interactiveObject;
    
    private void Start()
    {
        interactiveObject = GetComponent<InteractiveObject>();
        interactiveObject.onUse.AddListener(PickUp);
    }
    
    private void PickUp()
    {
        Character.Instance.GetComponent<Bag>().AddKey(keyCount);

        if (impactEffect)
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        
        Destroy(gameObject);
        interactiveObject.onUse.RemoveListener(PickUp);
    }
    
    private void OnDestroy()
    {
        interactiveObject.onUse.RemoveListener(PickUp);
    }
}