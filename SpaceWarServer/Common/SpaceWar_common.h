#pragma once

namespace SpaceWar
{
	//Message ID that replies to each RMI method. 
               
    static const ::Proud::RmiID Rmi_RequestServerConnect = (::Proud::RmiID)(3141+1);
               
    static const ::Proud::RmiID Rmi_RequestNetworkGameTeamSelect = (::Proud::RmiID)(3141+2);
               
    static const ::Proud::RmiID Rmi_RequestGameExit = (::Proud::RmiID)(3141+3);
               
    static const ::Proud::RmiID Rmi_RequestNetworkGameReady = (::Proud::RmiID)(3141+4);
               
    static const ::Proud::RmiID Rmi_RequestLobbyConnect = (::Proud::RmiID)(3141+5);
               
    static const ::Proud::RmiID Rmi_RequestNetworkChangeMap = (::Proud::RmiID)(3141+6);
               
    static const ::Proud::RmiID Rmi_RequestNetworkPlayerCount = (::Proud::RmiID)(3141+7);
               
    static const ::Proud::RmiID Rmi_RequestNetworkGameModeChange = (::Proud::RmiID)(3141+8);
               
    static const ::Proud::RmiID Rmi_RequestNetworkGameStart = (::Proud::RmiID)(3141+9);
               
    static const ::Proud::RmiID Rmi_RequestNetworkHostOut = (::Proud::RmiID)(3141+10);
               
    static const ::Proud::RmiID Rmi_RequestGameSceneJoin = (::Proud::RmiID)(3141+11);
               
    static const ::Proud::RmiID Rmi_NotifyLoginSuccess = (::Proud::RmiID)(3141+12);
               
    static const ::Proud::RmiID Rmi_NotifyLoginFailed = (::Proud::RmiID)(3141+13);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkUserSetup = (::Proud::RmiID)(3141+14);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGameTeamChange = (::Proud::RmiID)(3141+15);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkConnectUser = (::Proud::RmiID)(3141+16);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkReady = (::Proud::RmiID)(3141+17);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGameModeChange = (::Proud::RmiID)(3141+18);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGamePlayerCountChange = (::Proud::RmiID)(3141+19);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGameChangeMap = (::Proud::RmiID)(3141+20);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGameStart = (::Proud::RmiID)(3141+21);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGameStartFailed = (::Proud::RmiID)(3141+22);
               
    static const ::Proud::RmiID Rmi_NotifyNetworkGameHostOut = (::Proud::RmiID)(3141+23);
               
    static const ::Proud::RmiID Rmi_NotifyOtherClientJoin = (::Proud::RmiID)(3141+24);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerLost = (::Proud::RmiID)(3141+25);
               
    static const ::Proud::RmiID Rmi_RequestHpUpdate = (::Proud::RmiID)(3141+26);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerChangeHP = (::Proud::RmiID)(3141+27);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerChangeOxygen = (::Proud::RmiID)(3141+28);
               
    static const ::Proud::RmiID Rmi_RequestPlayerDamage = (::Proud::RmiID)(3141+29);
               
    static const ::Proud::RmiID Rmi_RequestPlayerUseOxy = (::Proud::RmiID)(3141+30);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerMove = (::Proud::RmiID)(3141+31);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerEquipItem = (::Proud::RmiID)(3141+32);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerUnEquipItem = (::Proud::RmiID)(3141+33);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerBulletCreate = (::Proud::RmiID)(3141+34);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerBulletMove = (::Proud::RmiID)(3141+35);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerBulletDelete = (::Proud::RmiID)(3141+36);
               
    static const ::Proud::RmiID Rmi_NotifyPlayerAnimation = (::Proud::RmiID)(3141+37);
               
    static const ::Proud::RmiID Rmi_NotifyGrenadeCreate = (::Proud::RmiID)(3141+38);
               
    static const ::Proud::RmiID Rmi_NotifyGrenadeMove = (::Proud::RmiID)(3141+39);
               
    static const ::Proud::RmiID Rmi_NotifyGrenadeBoom = (::Proud::RmiID)(3141+40);
               
    static const ::Proud::RmiID Rmi_NotifyGrenadeRemove = (::Proud::RmiID)(3141+41);
               
    static const ::Proud::RmiID Rmi_RequestOxyChargerStartSetup = (::Proud::RmiID)(3141+42);
               
    static const ::Proud::RmiID Rmi_RequestUseOxyChargerStart = (::Proud::RmiID)(3141+43);
               
    static const ::Proud::RmiID Rmi_RequestUseOxyCharger = (::Proud::RmiID)(3141+44);
               
    static const ::Proud::RmiID Rmi_RequestUseOxyChargerEnd = (::Proud::RmiID)(3141+45);
               
    static const ::Proud::RmiID Rmi_NotifyUseOxyCharger = (::Proud::RmiID)(3141+46);
               
    static const ::Proud::RmiID Rmi_NotifyUseSuccessedOxyCharger = (::Proud::RmiID)(3141+47);
               
    static const ::Proud::RmiID Rmi_NotifyUseFailedOxyCharger = (::Proud::RmiID)(3141+48);
               
    static const ::Proud::RmiID Rmi_RequestUseItemBox = (::Proud::RmiID)(3141+49);
               
    static const ::Proud::RmiID Rmi_NotifyStartItemBoxState = (::Proud::RmiID)(3141+50);
               
    static const ::Proud::RmiID Rmi_NotifyUseItemBox = (::Proud::RmiID)(3141+51);
               
    static const ::Proud::RmiID Rmi_RequestShelterStartSetup = (::Proud::RmiID)(3141+52);
               
    static const ::Proud::RmiID Rmi_RequestShelterDoorControl = (::Proud::RmiID)(3141+53);
               
    static const ::Proud::RmiID Rmi_RequestShelterEnter = (::Proud::RmiID)(3141+54);
               
    static const ::Proud::RmiID Rmi_NotifyShelterInfo = (::Proud::RmiID)(3141+55);
               
    static const ::Proud::RmiID Rmi_RequestWorldCreateItem = (::Proud::RmiID)(3141+56);
               
    static const ::Proud::RmiID Rmi_NotifyCreateItem = (::Proud::RmiID)(3141+57);
               
    static const ::Proud::RmiID Rmi_RequestItemDelete = (::Proud::RmiID)(3141+58);
               
    static const ::Proud::RmiID Rmi_NotifyDeleteItem = (::Proud::RmiID)(3141+59);
               
    static const ::Proud::RmiID Rmi_NotifyMeteorCreateTime = (::Proud::RmiID)(3141+60);
               
    static const ::Proud::RmiID Rmi_NotifyMeteorCreate = (::Proud::RmiID)(3141+61);
               
    static const ::Proud::RmiID Rmi_RequestSpaceShipSetup = (::Proud::RmiID)(3141+62);
               
    static const ::Proud::RmiID Rmi_RequestSpaceShip = (::Proud::RmiID)(3141+63);
               
    static const ::Proud::RmiID Rmi_RequestUseSpaceShip = (::Proud::RmiID)(3141+64);
               
    static const ::Proud::RmiID Rmi_RequestUseSpaceShipCancel = (::Proud::RmiID)(3141+65);
               
    static const ::Proud::RmiID Rmi_NotifyUseSpaceShipSuccess = (::Proud::RmiID)(3141+66);
               
    static const ::Proud::RmiID Rmi_NotifyUseSpaceShipFailed = (::Proud::RmiID)(3141+67);
               
    static const ::Proud::RmiID Rmi_NotifySpaceShipLockTime = (::Proud::RmiID)(3141+68);
               
    static const ::Proud::RmiID Rmi_NotifySpaceShipEngineChargeFailed = (::Proud::RmiID)(3141+69);
               
    static const ::Proud::RmiID Rmi_NotifySpaceShipEngineCharge = (::Proud::RmiID)(3141+70);
               
    static const ::Proud::RmiID Rmi_NotifyDeathZoneCommingTime = (::Proud::RmiID)(3141+71);
               
    static const ::Proud::RmiID Rmi_NotifyDeathZoneCreate = (::Proud::RmiID)(3141+72);
               
    static const ::Proud::RmiID Rmi_RequestDeathZoneMoveIndex = (::Proud::RmiID)(3141+73);
               
    static const ::Proud::RmiID Rmi_NotifyDeathZoneMoveHostAndIndexSetup = (::Proud::RmiID)(3141+74);
               
    static const ::Proud::RmiID Rmi_NotifyDeathZoneMove = (::Proud::RmiID)(3141+75);
               
    static const ::Proud::RmiID Rmi_NotifyDrawGame = (::Proud::RmiID)(3141+76);
               
    static const ::Proud::RmiID Rmi_RequestDrawGameResult = (::Proud::RmiID)(3141+77);
               
    static const ::Proud::RmiID Rmi_RequestGameEnd = (::Proud::RmiID)(3141+78);
               
    static const ::Proud::RmiID Rmi_NotifyKillInfo = (::Proud::RmiID)(3141+79);
               
    static const ::Proud::RmiID Rmi_NotifyGameResultInfoMe = (::Proud::RmiID)(3141+80);
               
    static const ::Proud::RmiID Rmi_NotifyGameResultInfoOther = (::Proud::RmiID)(3141+81);
               
    static const ::Proud::RmiID Rmi_NotifyGameResultShow = (::Proud::RmiID)(3141+82);

	// List that has RMI ID.
	extern ::Proud::RmiID g_RmiIDList[];
	// RmiID List Count
	extern int g_RmiIDListCount;
}

 
