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
            
            transform.parent.parent.GetChild(2).GetComponent<MeshRenderer>().material = m_opacityMat;
        }
        else
        {
            p.IS_SHELTER = false;
            // 나감
            m_targetShelter.ShelterExit();
            transform.parent.parent.GetChild(2).GetComponent<MeshRenderer>().material = m_colorMat;
        }
    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }
}
