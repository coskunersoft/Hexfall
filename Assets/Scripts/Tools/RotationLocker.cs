using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLocker : MonoBehaviour
{
    public Vector3 RotationToLock;

    public void Update()
    {
        transform.eulerAngles = RotationToLock;
    }
}
