using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShelterDoor : MonoBehaviour {

    public bool m_enter = true;
    public Shelter m_targetShelter = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerCharacter"))
            return;

        Player p = other.GetComponent<Player>();
        if (!p.enabled)
            return;

        if(m_enter)
        {
            p.IS_SHELTER = true;
            // 들어옴
            m_targetShelter.ShelterEnter();
        }
        else
        {
            p.IS_SHELTER = false;
            // 나감
            m_targetShelter.ShelterExit();
        }
    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }
}
