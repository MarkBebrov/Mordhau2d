using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SwordCollisionHandler : MonoBehaviour
{
    private ArcController _arcController;
    private AttackManager attackManager;

    private void Start()
    {
        //_arcController = transform.parent.Find("napr").Find("ArcObject").gameObject.GetComponent<ArcController>();
        attackManager = GetComponentInParent<AttackManager>();
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    other.transform.parent?.Find("ArcObject")?.gameObject.GetComponent<ArcController>().OnSwordHit(other, _arcController);
    //}
    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    other.transform.parent?.Find("ArcObject")?.gameObject.GetComponent<ArcController>().OnSwordExitFromTrigger(other);
    //}
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.GetComponentInParent<HitStates>()?.OnSwordHit(other, attackManager);
       // other.transform.parent?.Find("ArcObject")?.gameObject.GetComponent<ArcController>().OnSwordHit(other, _arcController);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        other.gameObject.GetComponentInParent<HitStates>()?.OnSwordExitFromTrigger(other);
    }
}
