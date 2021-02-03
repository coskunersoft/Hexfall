using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    private void Awake()
    {
        instance = this;
    }
    public GameObject particleBreak;

    public void ShowBreakParticle(Vector3 pos,Color color)
    {
        ParticleSystem created = Instantiate(particleBreak, pos, Quaternion.identity).GetComponent<ParticleSystem>();
        ParticleSystem.MainModule module = created.main;
        module.startColor = color;
        created.Play();
        Destroy(created.gameObject, 4f);
    }
}
