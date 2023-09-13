using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SwordCollisionHandler : MonoBehaviour
{
    private ArcController _arcController;

    private void Start()
    {
        _arcController = transform.parent.Find("napr").Find("ArcObject").gameObject.GetComponent<ArcController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.transform.parent?.Find("ArcObject")?.gameObject.GetComponent<ArcController>().OnSwordHit(other, _arcController);
    }
}
