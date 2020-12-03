#define _CRT_SECURE_NO_WARNINGS
#define SIGPIPE 13
#include <string>
#include <iostream>
#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <vector>
#include <sstream>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <sys/socket.h>
#include <pthread.h>
#include <time.h>
#include <errno.h>
#include <signal.h>

using namespace std;

class Client {
private:
	int clientID;
	int roomID;
	int clientSocket;
	bool ready;
public:
	Client(int clientID, int clientSocket) {
		this->clientID = clientID;
		this->roomID = -1;
		this->clientSocket = clientSocket;
		this->ready = false;
	}
	int getClientID() {
		return clientID;
	}
	int getRoomID() {
		return roomID;
	}
	void setRoomID(int roomID) {
		this->roomID = roomID;
	}
	int getClientSocket() {
		return clientSocket;
	}
	void setReady() {
		this->ready = true;
	}
	bool getReady() {
		return ready;
	}
};

int serverSocket;
vector<Client> connected;
struct sockaddr_in serverAddr;
int nextClientID;
int check = 0;

vector<string> getTokens(string input, char delimiter) {
	vector<string> tokens;
	istringstream f(input);
	string s;
	while (getline(f, s, delimiter)) {
		tokens.push_back(s);
	}
	return tokens;
}

int clientCountInRoom(int roomID) {
	int count = 0;
	for (int i = 0; i < connected.size(); i++) {
		if (connected[i].getRoomID() == roomID) {
			count++;
		}
	}
	return count;
}

int clientReadyInRoom(int roomID) {
	int count = 0;
	for (int i = 0; i < connected.size(); i++) {
		if (connected[i].getRoomID() == roomID && connected[i].getReady() == true) {
			count++;
		}
	}
	return count;
}

string getRooms() {
	string data = "[Ask]";
	for (int i = 0; i < connected.size(); i++) {
		if (connected[i].getRoomID() != -1) {
			int room = connected[i].getRoomID();
			data += to_string(room) + ",";
		}
	}
	data += "---- access possible";
	return data;
}

void play(int roomID) {
	char* sent = new char[256];
	bool isBlack = true;
	for (int i = 0; i < connected.size(); i++) {
		if (connected[i].getRoomID() == roomID) {
			memset(sent, 0, 256);
			if (isBlack)
			{
				string data = "[Play]Black";
				sprintf(sent, "%s", data.c_str());
				isBlack = false;
			}
			else
			{
				string data = "[Play]White";
				sprintf(sent, "%s", data.c_str());
			}
			send(connected[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void exit(int roomID) {
	char* sent = new char[256];
	for (int i = 0; i < connected.size(); i++) {
		if (connected[i].getRoomID() == roomID) {
			string data = "[Exit]";
			memset(sent, 0, 256);
			sprintf(sent, "%s", data.c_str());
			send(connected[i].getClientSocket(), sent, 256, 0);
		}
	}
}

void put(int roomID, int x, int y, int z) {
	char* sent = new char[256];
	for (int i = 0; i < connected.size(); i++) {
		if (connected[i].getRoomID() == roomID) {
			memset(sent, 0, 256);
			string data = "[Put]" + to_string(x) + "," + to_string(y) + "," + to_string(z);
			sprintf(sent, "%s", data.c_str());
			send(connected[i].getClientSocket(), sent, 256, 0);
		}
	}
}

bool judge(int board[][11]) // 승리 판정 함수
{
	for (int i = 0; i < 11; i++) // 세로
		for (int j = 4; j < 11; j++)
			if (board[i][j] == 1 && board[i][j - 1] == 1 && board[i][j - 2] == 1 && board[i][j - 3] == 1 && board[i][j - 4] == 1)
				return true;
	for (int i = 0; i < 7; i++) // 가로
		for (int j = 0; j < 11; j++)
			if (board[i][j] == 1 && board[i + 1][j] == 1 && board[i + 2][j] == 1 && board[i + 3][j] == 1 && board[i + 4][j] == 1)
				return true;
	for (int i = 4; i < 11; i++) // Y = -X 직선
		for (int j = 0; j < 7; j++)
			if (board[i][j] == 1 && board[i - 1][j + 1] == 1 && board[i - 2][j + 2] == 1 && board[i - 3][j + 3] == 1 && board[i - 4][j + 4] == 1)
				return true;
	for (int i = 0; i < 7; i++) // Y = X 직선
		for (int j = 0; j < 7; j++)
			if (board[i][j] == 1 && board[i + 1][j + 1] == 1 && board[i + 2][j + 2] == 1 && board[i + 3][j + 3] == 1 && board[i + 4][j + 4] == 1)
				return true;
	return false;
}

void* ServerThread(void* client2) {
	struct Client* client = (struct Client*)client2;
	char* sent = new char[256];
	char* received = new char[256];
	int size = 0;
	int board[11][11];
	for (int i = 0; i < 11; i++) {
		for (int j = 0; j < 11; j++) {
			board[i][j] = 0;
		}
	}
	while (true) {
		memset(received, 0, 256);
		if ((size = recv(client->getClientSocket(), received, 256, NULL)) > 0) {
			string receivedString = string(received);
			vector<string> tokens = getTokens(receivedString, ']');
			if (receivedString.find("[Ask]") != -1) {
				for (int i = 0; i < connected.size(); i++) {
					if (connected[i].getClientSocket() == client->getClientSocket()) {
						string data = getRooms();
						memset(sent, 0, 256);
						sprintf(sent, "%s", data.c_str());
						send(connected[i].getClientSocket(), sent, 256, 0);
					}
				}
			}
			else if (receivedString.find("[Enter]") != -1) {
				/* 방에 접속 */
				for (int i = 0; i < connected.size(); i++) {
					string roomID = tokens[1];
					int roomInt = atoi(roomID.c_str());
					if (connected[i].getClientSocket() == client->getClientSocket()) {
						memset(sent, 0, 256);
						string data;
						int clientCount = clientCountInRoom(roomInt);
						cout << clientCount << endl;
						/* 방에 둘 이상 이미 차 있을 시 */
						if (clientCount >= 2) data = "[Full]";
						/* 방이 비었거나 한명만 있을시 */
						else {
							cout << "사용자 [" << client->getClientID() << "]: " << roomID << "번 방으로 접속" << endl;
							Client* newClient = new Client(*client);
							newClient->setRoomID(roomInt);
							connected[i] = *newClient;
							data = "[Enter]";
						}
						sprintf(sent, "%s", data.c_str());
						send(connected[i].getClientSocket(), sent, 256, 0);
					}
				}
			}
			else if (receivedString.find("[Ready]") != -1) {
				for (int i = 0; i < connected.size(); i++) {
					string roomID = tokens[1];
					int roomInt = atoi(roomID.c_str());
					if (connected[i].getClientSocket() == client->getClientSocket()) {
						int readyCount = clientReadyInRoom(roomInt);
						Client* newClient = new Client(*client);
						newClient->setReady();
						newClient->setRoomID(roomInt);
						connected[i] = *newClient;
						cout << "클라이언트 [" << client->getClientID() << "]: " << roomID << "번 방으로 준비 " << readyCount << endl;
						if (readyCount == 1) {
							play(roomInt);
						}
					}
				}
			}
			else if (receivedString.find("[Put]") != -1) {
				string data = tokens[1];
				vector<string> dataTokens = getTokens(data, ',');
				int roomID = atoi(dataTokens[0].c_str());
				int x = atoi(dataTokens[1].c_str());
				int y = atoi(dataTokens[2].c_str());
				int z = atoi(dataTokens[3].c_str());

				/* 시간 초과 및 범위 벗어날시 */
				if (x == 100 && y == 100) {
					put(roomID, -1, -1, z);
				}
				else {
					/* 돌 위치를 전송 */
					put(roomID, x, y, z);
					board[x][y] = 1;
					/* 승패 결정 및 전송 */
					bool win = judge(board);
					if (win) {
						put(roomID, -1, -1, z);
					}
					/* 돌의 수 50 넘었을 시 비김 */
					else {
						int count = 0;
						for (int i = 0; i < 11; i++) {
							for (int j = 0; j < 11; j++) {
								if (board[i][j] == 1) count++;
							}
						}
						if (count >= 25) {
							check++;
							if (check == 2) {
								put(roomID, -2, -2, z);
								check = 0;
							}
						}
					}
				}
			}
			else if (receivedString.find("[Play]") != -1) {
				string roomID = tokens[1];
				int roomInt = atoi(roomID.c_str());
				play(roomInt);
			}
		}
		else {
			memset(sent, 0, 256);
			sprintf(sent, "클라이언트 [%i]의 연결이 끊겼습니다.", client->getClientID());
			cout << sent << endl;
			for (int i = 0; i < connected.size(); i++) {
				if (connected[i].getClientID() == client->getClientID()) {
					if (connected[i].getRoomID() != -1 && clientCountInRoom(connected[i].getRoomID()) == 2) {
						exit(connected[i].getRoomID());
					}
					connected.erase(connected.begin() + i);
				}
			}
			delete client;
			break;
		}
	}
}


int main() {
	sigignore(SIGPIPE);
	pthread_t thread;
	int status;
	serverSocket = socket(AF_INET, SOCK_STREAM, NULL);
	serverAddr.sin_addr.s_addr = inet_addr("147.46.240.42");
	serverAddr.sin_port = htons(20385);
	serverAddr.sin_family = AF_INET;
	cout << "[ 오목 서버 가동 ]" << endl;
	bind(serverSocket, (struct sockaddr*)&serverAddr, sizeof(serverAddr));
	listen(serverSocket, 32);

	int addressLength = sizeof(serverAddr);
	while (true) {
		int clientSocket = socket(AF_INET, SOCK_STREAM, NULL);
		if (clientSocket = accept(serverSocket, (struct sockaddr*)&serverAddr, (socklen_t*)&addressLength)) {
			Client* client = new Client(nextClientID, clientSocket);
			cout << "[ 새 사용자 접속 ]" << endl;
			pthread_create(&thread, NULL, ServerThread, (void*)client);
			connected.push_back(*client);
			nextClientID++;
		}
	}
	pthread_join(thread, (void**)&status);
}