#ifndef SHELTER_H
#define SHELTER_H

class Shelter
{
public:
	int m_shelterID;
	int m_shelterHumanCounter;
	bool m_doorState;
	bool m_lightState;

public:
	Shelter();
	~Shelter();

	void ShelterEnter();
	void ShelterExit();

	void ShelterDoorStateChange(bool state);
};

#endif