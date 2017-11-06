#ifndef SHELTER_H
#define SHELTER_H

/**
* @brief		Shelter :: 네트워크 오브젝트
* @details		쉘터에 있는 사람 정보 / 쉘터 문 및 라이트 상태 정보 제어
* @author		이훈 (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			Shelter.h
* @version		0.0.1
*/
class Shelter
{
private:
	int m_shelterID;			///< 쉘터 네트워크 인덱스
	int m_shelterHumanCounter;	///< 쉘터 안에 들어간 인간의 수
	bool m_doorState;			///< 문이 열려있는지
	bool m_lightState;			///< 불이 켜져있는지

public:
	Shelter(); ///< 변수 초기화

#pragma region InGame Shelter Method =============================================
	void ShelterEnter(); ///< 쉘터에 들어왔다.
	void ShelterExit(); ///< 쉘터에서 나갔다.
	void ShelterDoorStateChange(bool state); ///< 쉘터 문 상태 체인지
#pragma endregion ================================================================

#pragma region Get / Set Method ==================================================
	void SetShelterID(int shelterID); ///< 쉘터 인덱스 세팅
	int GetShelterID(); ///< 쉘터 인덱스 얻기
	int GetShelterHuman(); ///< 쉘터 안에 있는 사람 수 얻기 
	bool IsDoorOpen(); ///< 쉘터 문이 열려있는지 얻기
	bool IsLightOn(); ///< 쉘터 라이트가 켜져있는지 얻기 
#pragma endregion ================================================================
};

#endif