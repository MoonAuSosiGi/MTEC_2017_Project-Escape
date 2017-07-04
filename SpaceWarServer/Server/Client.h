#ifndef CLIENT_H
#define CLIENT_H
#include "stdafx.h"

class Client
{
public:
	HostID m_hostID;
	string m_userName;
	float x;
	float y;
	float z;
public:
	Client();
	~Client();
};

#endif