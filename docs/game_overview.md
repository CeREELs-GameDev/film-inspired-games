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
- 버닝 기본 진행: `C01 → C02 → C03 → C04 → C08 → C09 → C10 → C11 → C12`
- C04 완료 후 클릭 시 제작 중인 C05～C07을 건너뛰고 C08로 이동
- 장면 이동은 마우스 클릭 기준. 드래그 중 클릭은 장면 이동으로 처리하지 않음

## 버닝 1막 실행

- 통합 플레이 씬: `Assets/Game/Burning/Scenes/Burning_Act1_Playable.unity`
- 씬 재생성: `Tools > Burning > Build Act 1 Playable Scene`
- C02 완료 전 C03 이동 차단
- C01·C02·C03 전환 시점별 연출 신호 제공

## 버닝 C08-C12 실행

- 단독 플레이 씬: `Assets/Game/Burning/C08/Scenes/Burning_C08_C12_Playable.unity`
- 씬 재생성: `Tools > Burning > Build C08-C12 Playable Scene`
- C08: 어두운 배경에서 밝은 배경으로 느린 디졸브 후 블랙 전환
- C09: 블랙 화면의 불빛 점멸 후 종수 접근, 담배불 접촉 시 해미 이미지 디졸브
- C10: 종수의 눈 감기와 힐끔 동작 반복 후 해미에게 시선 집중
- C11: 화면 터치 시 종수가 시계를 내미는 동작 재생
- C12: 시계를 받은 해미의 미소에서 웃는 표정으로 디졸브
- 재생 중 `C08ToC12Sequence` 인스펙터에서 현재 챕터와 장면 확인 가능

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
