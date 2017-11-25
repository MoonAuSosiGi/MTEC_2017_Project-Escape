﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Singletone<T> : MonoBehaviour where T : MonoBehaviour{

    private static T m_instance = default(T);
   
    public static T Instance()
    {
        if (m_instance == null)
        {
            try
            {
                m_instance = FindObjectOfType(typeof(T)) as T;
                if (m_instance == null)
                {
                    //Debug.Log("ERROR...Singletone Instance NULL");
                    return null;
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
                return null;
            }
           
                
        }
        return m_instance;
    }
}
