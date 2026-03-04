# 3D_RPG (Unity)

L자 경로를 따라 스폰된 몬스터가 이동하고, 플레이어는 사거리 내 적을 자동 공격하는 **3D RPG 디펜스 데모**입니다.  
Life가 0이 되는 순간 **GameOver 팝업**을 표시하고 게임이 즉시 중단됩니다.

---

## 🔥 핵심 기능
- **전투 루프**: 이동 → 타겟팅 → 공격/피격 → 사망 처리 → GameOver
- **몬스터 스폰**: 일정 주기 자동 생성 + 유지 수량 기반 관리(SpawningPool)
- **UI/HUD**: HP / MP / EXP 갱신 + 인벤토리 토글
- **구조화된 매니저 아키텍처**: Managers 기반 싱글톤/역할 분리
- **오브젝트 풀링**: 반복 생성/파괴 비용을 줄여 안정적인 프레임 유지

---

## 🧩 구현 상세 (기술 요소)
### 1) 입력 / 이동 / 카메라
- 마우스 클릭 기반 이동 목적지 설정
- 캐릭터 이동/회전 제어(Transform/Vector3 기반)
- 카메라 추적 및 시점 제어(3rd Person 구성)

### 2) 전투 / 상태 관리
- 플레이어/몬스터 상태 전환(Idle/Move/Attack/Die 등)
- 공격 판정 및 타겟팅(락온/사거리 기반)
- 애니메이션 전환(CrossFade/BlendTree 등)과 전투 로직 동기화

### 3) 충돌 / Raycast
- 클릭/타겟팅에 Raycasting 활용
- Collider / Trigger를 통한 근접 판정 및 이벤트 처리
- LayerMask로 상호작용 대상 분리

### 4) UI 시스템
- HUD(HP/MP/EXP) 실시간 반영
- 인벤토리 UI 토글 및 갱신
- UI 자동화(바인딩/관리 패턴) + UI Manager 구조

### 5) 데이터 / 스탯
- 스탯(HP/MP/EXP/Level 등) 구조화
- Data Manager로 스탯 데이터 로드/참조(StatDict 등)

### 6) 리소스 / 프리팹 / 씬 관리
- Prefab 기반 오브젝트 관리
- Resource Manager로 로드/생성 흐름 통일
- Scene Manager로 씬 전환/초기화 역할 분리

### 7) 사운드 / 코루틴 / 풀링
- Sound Manager로 BGM/SFX 재생 정책 분리
- Coroutine으로 시간 기반 처리(예: 스폰 주기/지연 처리)
- Pool Manager로 몬스터/이펙트 재사용(Instantiate/Destroy 최소화)

---

## 🏗️ 아키텍처 요약
이 프로젝트는 Unity 기본 기능을 “기능 단위로 분리”하여 확장 가능한 구조를 목표로 했습니다.

- **Managers(싱글톤 루트)**  
  - UI / Scene / Sound / Resource / Pool / Data 등 매니저 분리
- **Controller 계층**  
  - Player / Monster 행동 로직 분리 + 상태 기반 업데이트
- **SpawningPool**  
  - 유지 수량(keep count) + 주기(spawn time) 기반 자동 스폰 관리

## 🎮 조작 방법
- **이동**: 마우스 클릭
- **전투**: (자동 공격 / 또는 스킬키: Q/W/E 등 프로젝트 기준으로 수정)
- **인벤토리**: (예: I)

---

## ▶️ 실행 방법
1. Unity Hub에서 프로젝트 열기
2. 시작 씬 실행 (예: `Login` 또는 `Game`)
3. Play

---

## 📸 스크린샷 / 영상
- 시연 영상: (링크)
- 스크린샷: `Docs/` 폴더 참고
