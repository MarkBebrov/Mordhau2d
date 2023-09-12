using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pr : MonoBehaviour
{
    public Transform parentTransform;

    void LateUpdate()
    {
        transform.position = parentTransform.position;
        transform.rotation = parentTransform.rotation;
    }

}
