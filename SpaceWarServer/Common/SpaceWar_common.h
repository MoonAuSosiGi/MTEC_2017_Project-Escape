#pragma once

namespace SpaceWar
{
	//Message ID that replies to each RMI method. 
               
    static const ::Proud::RmiID Rmi_RequestServerConnect = (::Proud::RmiID)(3141+1);
               
    static const ::Proud::RmiID Rmi_RequestClientJoin = (::Proud::RmiID)(3141+2);
               
    static const ::Proud::RmiID Rmi_RequestWorldCreateItem = (::Proud::RmiID)(3141+3);
               
    static const ::Proud::RmiID Rmi_RequestPlayerDamage = (::Proud::RmiID)(3141+4);
               
    static const ::Proud::RmiID Rmi_RequestPlayerUseOxy = (::Proud::RmiID)(3141+5);
               
    static const ::Proud::RmiID Rmi_RequestUseOxyCharger = (::Proud::RmiID)(3141+6);
               
    static const ::Proud::RmiID Rmi_RequestUseItemBox = (::Proud::RmiID)(3141+7);
               
    static const ::Proud::RmiID Rmi_RequestShelterStartSetup = (::Proud::RmiID)(3141+8);
               
    static const ::Proud::RmiID Rmi_RequestShelterDoorControl = (::Proud::RmiID)(3141+9);
               
    static const ::Proud::RmiID Rmi_RequestShelterEnter = (::Proud::RmiID)(3141+10);
               
    static const ::Proud::RmiID Rmi_NotifyLoginSuccess = (::Proud::RmiID)(3141+11);
               
    static const ::Proud::RmiID Rmi_NotifyLoginFailed = (::Proud::RmiID)(3141+12);
               
    static const ::Proud::RmiID Rmi_NotifyOtherClientJoin = (::Proud::RmiID)(3141+13);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerLost = (::Proud::RmiID)(3141+14);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerMove = (::Proud::RmiID)(3141+15);
               
    static const ::Proud::RmiID Rmi_NotifyDeleteItem = (::Proud::RmiID)(3141+16);
               
    static const ::Proud::RmiID Rmi_NotifyCreateItem = (::Proud::RmiID)(3141+17);
               
    static const ::Proud::RmiID Rmi_NotifyStartOxyChargerState = (::Proud::RmiID)(3141+18);
               
    static const ::Proud::RmiID Rmi_NotifyStartItemBoxState = (::Proud::RmiID)(3141+19);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerEquipItem = (::Proud::RmiID)(3141+20);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerUnEquipItem = (::Proud::RmiID)(3141+21);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerBulletCreate = (::Proud::RmiID)(3141+22);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerBulletMove = (::Proud::RmiID)(3141+23);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerBulletDelete = (::Proud::RmiID)(3141+24);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerAnimation = (::Proud::RmiID)(3141+25);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerChangeHP = (::Proud::RmiID)(3141+26);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerChangeOxygen = (::Proud::RmiID)(3141+27);
               
    static const ::Proud::RmiID Rmi_NotifyUseOxyCharger = (::Proud::RmiID)(3141+28);
               
    static const ::Proud::RmiID Rmi_NotifyUseItemBox = (::Proud::RmiID)(3141+29);
               
    static const ::Proud::RmiID Rmi_NotifyShelterInfo = (::Proud::RmiID)(3141+30);
               
    static const ::Proud::RmiID Rmi_NotifyMeteorCreateTime = (::Proud::RmiID)(3141+31);
               
    static const ::Proud::RmiID Rmi_NotifyMeteorCreate = (::Proud::RmiID)(3141+32);

	// List that has RMI ID.
	extern ::Proud::RmiID g_RmiIDList[];
	// RmiID List Count
	extern int g_RmiIDListCount;
}

 
