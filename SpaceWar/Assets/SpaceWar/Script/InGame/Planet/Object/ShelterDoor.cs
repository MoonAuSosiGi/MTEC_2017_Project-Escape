using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelterDoor : MonoBehaviour {

    public bool m_enter = true;
    public Shelter m_targetShelter = null;

    public Material m_colorMat = null;
    public Material m_opacityMat = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerCharacter"))
            return;

        PlayerController p = other.GetComponent<PlayerController>();
        if (!p.enabled)
            return;

        if(m_enter)
        {
            p.IS_SHELTER = true;
            // 들어옴
            m_targetShelter.ShelterEnter();
            ShelterEnterEffect();
            //transform.parent.parent.GetChild(2).GetComponent<MeshRenderer>().material = m_opacityMat;
        }
        else
        {
            p.IS_SHELTER = false;
            // 나감
            m_targetShelter.ShelterExit();
            ShelterExitEffect();
         //   transform.parent.parent.GetChild(2).GetComponent<MeshRenderer>().material = m_colorMat;
        }
    }

    void ShelterEnterEffect()
    {
        if (iTween.Count(gameObject) > 0)
            iTween.Stop(gameObject);
        float v = transform.parent.parent.
            GetChild(2).GetComponent<MeshRenderer>().material.GetFloat("_Cutoff");

        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , v , "to" , 1.0f - 0.015f ,
            "onupdatetarget" , gameObject ,
            "onupdate" , "DoorUpdate",
            "time",0.5f));
    }
    void ShelterExitEffect()
    {
        if (iTween.Count(gameObject) > 0)
            iTween.Stop(gameObject);
        float v = transform.parent.parent.
             GetChild(2).GetComponent<MeshRenderer>().material.GetFloat("_Cutoff");
        iTween.ValueTo(gameObject , iTween.Hash(
            "from" , v , "to" , 0.0f , 
            "onupdatetarget" , gameObject ,
            "onupdate" , "DoorUpdate",
            "time",0.5f));
    }

    void DoorUpdate(float v)
    {
        transform.parent.parent.
             GetChild(2).GetComponent<MeshRenderer>().material.SetFloat("_Cutoff" , v);
    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }
}
