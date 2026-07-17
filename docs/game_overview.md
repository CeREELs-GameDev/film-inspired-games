# 게임 개요

## 상태

- 생성일: 2026-07-13
- 프로젝트명: `film-inspired-games`
- Unity: `6000.0.67f1`
- 템플릿: Universal 2D

## 현재 방향

영화에서 출발한 짧은 2D 인터랙션 게임 실험 모음.

첫 실험은 영화 `버닝`, `양들의 침묵`을 모티브로 삼고, `Florence`처럼 짧은 터치 조작과 감정 변화 중심으로 전개.

## 현재 정해진 것

- 기존 `odie-in-wtf-company` 추리게임과 별도 프로젝트로 진행
- 세로형 2D 화면 기준
- 강한 추리 판정보다 장면, 감정, 손가락 조작, 대화 긴장감 우선
- 첫 목표는 한 장면 안에서 짧은 상호작용 2～3개를 플레이해보는 프로토타입
- 버닝 1막 기본 진행: `C01 클릭 → C02 상자 7개 배치 → 클릭 → C03 첫 화면 → 클릭 → C03 두 번째 화면`
- 장면 이동은 마우스 클릭 기준. 드래그 중 클릭은 장면 이동으로 처리하지 않음

## 버닝 1막 실행

- 통합 플레이 씬: `Assets/Game/Burning/Scenes/Burning_Act1_Playable.unity`
- 씬 재생성: `Tools > Burning > Build Act 1 Playable Scene`
- C02 완료 전 C03 이동 차단
- C01·C02·C03 전환 시점별 연출 신호 제공

## 아직 정할 것

- 첫 실험의 가제
- 플레이어가 맡는 인물
- 상대 인물과의 관계
- 첫 장면의 장소
- 핵심 조작 2～3개
- 실패와 재시도의 처리 방식

## 참고

- `버닝`: 모호한 실종, 계급 감각, 불안, 타고 남은 흔적
- `양들의 침묵`: 심문, 심리전, 답을 아는 인물과의 거래
- `Florence`: 짧은 터치 조작으로 관계와 감정을 표현
- Florence 챕터별 연출 분석: `docs/references/florence_interaction_analysis.md`
- Florence 장면 모음 이미지: `assets-src/reference/florence_chapters/contact_sheets/`
- 버닝 1막 장면 설계와 C02 연결 방법: `docs/references/burning_act1_scene_plan.md`
