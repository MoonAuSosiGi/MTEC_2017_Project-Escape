#ifndef SHELTER_H
#define SHELTER_H

/**
* @brief		Shelter :: ��Ʈ��ũ ������Ʈ
* @details		���Ϳ� �ִ� ��� ���� / ���� �� �� ����Ʈ ���� ���� ����
* @author		���� (MoonAuSosiGi@gmail.com)
* @date			2017-11-04
* @file			Shelter.h
* @version		0.0.1
*/
class Shelter
{
private:
	int m_shelterID;			///< ���� ��Ʈ��ũ �ε���
	int m_shelterHumanCounter;	///< ���� �ȿ� �� �ΰ��� ��
	bool m_doorState;			///< ���� �����ִ���
	bool m_lightState;			///< ���� �����ִ���

public:
	Shelter(); ///< ���� �ʱ�ȭ

#pragma region InGame Shelter Method =============================================
	void ShelterEnter(); ///< ���Ϳ� ���Դ�.
	void ShelterExit(); ///< ���Ϳ��� ������.
	void ShelterDoorStateChange(bool state); ///< ���� �� ���� ü����
#pragma endregion ================================================================

#pragma region Get / Set Method ==================================================
	void SetShelterID(int shelterID); ///< ���� �ε��� ����
	int GetShelterID(); ///< ���� �ε��� ���
	int GetShelterHuman(); ///< ���� �ȿ� �ִ� ��� �� ��� 
	bool IsDoorOpen(); ///< ���� ���� �����ִ��� ���
	bool IsLightOn(); ///< ���� ����Ʈ�� �����ִ��� ��� 
#pragma endregion ================================================================
};

#endif