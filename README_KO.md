# IOCP 기반 멀티플레이 게임 & Unity Client

## C++ 서버와 unity 클라이언트로 제작된 실시간 멀티플레이 프로젝트
## 개인개발로 제작되었다.

### 서버(C++)
- IOCP 기반 고성능 네트워크 서버
- C++로 제작되었으며 Google의 ProtoBuf를 사용해 C#과 C++의 패킷을 자동화시켰다.
- GameObject를 상속 받는 개체들의 상태를 FSM으로 구현하였으며, 상태에 따른 update로 object를 처리한다.
- Room 분리 구조로 독립적인 플레이 환경을 구성하였다.
- Grid 기반 공간 분할 기법을 사용해 Object 간의 충돌을 계산해 불필요한 연산을 배제시킨다.

### 클라이언트(Unity)
- 서버에게 총알의 위치를 받으면 위치를 예측하며, 0.5초마다 총알의 위치를 받아서 보간한다.

## 환경
- ProtoBuf를 사용해 패킷을 편리하게 제작했으며, Python의 JinJa2, pipinstaller 등을 사용해 PacketHandler를 사용으로 작성할 수 있게 만들었다.
- Unity 2022.3.43f1 버전을 사용하였으며, Visual Studio 2022를 사용했다.

## 플레이 영상
https://youtu.be/GFeWWFQhi-I<img width="534" height="81" alt="image" src="https://github.com/user-attachments/assets/4e350f87-578e-4176-a9b2-970baf21f1f5" />
