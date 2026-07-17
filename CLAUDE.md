# CLAUDE.md

> 최종 업데이트: 2026-07-17

이 문서는 Claude Code로 작업할 때 공통으로 유의·협의해야 할 내용을 정리한 가이드다.

유니티 기능 학습 샌드박스 — Addressables, URP 포스트프로세싱, 2D/3D 충돌 등 기능별 데모·튜토리얼을 한 저장소에 모아 실험하는 개인 학습용 프로젝트다.

---

## 환경

| 항목 | 내용 |
|------|------|
| Unity 버전 | 6000.2.6f1 |
| 렌더 파이프라인 | URP (com.unity.render-pipelines.universal 17.2.0) |

---

## 폴더 구조

| 경로 | 내용 |
|------|------|
| `Assets/Scripts/` | 스크립트 루트 (1인 개발 표준) |
| `Assets/Scripts/Common/Attribute/` | 범용 런타임 어트리뷰트 — 마스터 `templates/code/` 에서 설치됨 |
| `Assets/Scripts/Common/Editor/` | 범용 에디터 전용 코드 — 드로어·툴 (빌드 제외) |
| `Assets/Scripts/Common/Service/` | 서비스 로케이터 (`Services` · `MonoService`) — 사용 규칙은 `feature-design` 참조 |
| `Assets/Scripts/Common/Utils/` | 범용 정적 유틸·순수 계산 헬퍼 (아직 없음 — 첫 자산이 생길 때 만든다) |
| `Assets/Project/1_Demos/` | 기능 검증 데모 (2D_3D_Collision, PostProcessing_Exception) |
| `Assets/Project/2_Tutorials/` | 학습용 튜토리얼 (Addressables, LevelUpYourCode, URP Shader Basic) |
| `Assets/Project/3_Other/` | 단발성 실험 (Cinemachine, DOTween, RandomDeck, TimePhysics 등) |
| `Assets/Plugins/` | 서드파티 (DOTween 등). 직접 수정하지 않는다 |
| `Assets/Settings/` | URP 렌더 파이프라인 애셋 (2D / 3D / PC / Mobile) |
| `Assets/AddressableAssetsData/` | Addressables 그룹·프로파일 설정 |
| `Assets/ScriptTemplates/` | 유니티 스크립트 생성 템플릿 |

- 범용 공통 코드(프로젝트를 옮겨도 그대로 쓰는 것)는 `Assets/Scripts/Common/` 아래 세 분류
  (`Attribute/` · `Editor/` · `Utils/`)로 넣는다. 세 분류에 안 맞으면 `Common/<범주>/` 를 추가한다.
- 에디터 전용 스크립트는 반드시 `Editor/` 이름의 폴더 안에 둔다 (빌드 제외).
- 새 기능 실험은 `Assets/Project/` 아래 성격에 맞는 그룹에 폴더를 만들어 격리한다.
  데모 스크립트는 씬·프리팹과 함께 각 데모 폴더에 두어 **데모 하나가 자기완결**되게 한다.
- 여러 데모에서 재사용할 코드로 승격되면 `Assets/Scripts/Common/` 으로 옮기고,
  범용이면 `/unity-skill-sync` 로 마스터 `templates/code/` 에도 반영한다.

---

## 작업 규칙

- 커밋은 `commit-convention` 규칙을 따른다.
- Unity에서 새 스크립트·에셋을 만들면 에디터를 갱신해 `.meta`를 생성한 뒤 원본과 함께 커밋한다.
  `.meta` 누락 시 GUID·참조 충돌이 발생할 수 있다.
- `.claude/settings.local.json`은 개인 설정이라 커밋하지 않는다(`.gitignore` 처리됨).
- 코드는 한글 주석을 사용한다.
- CLAUDE.md·스킬 문서를 수정하면 문서 상단의 `최종 업데이트` 날짜를 그날 날짜로 갱신한다.

---

## Skills 참조

스킬은 항상 적용하는 게 아니라, **작업 내용에 따라 필요한 경우에만** 참고한다.
클라이언트 코드 작업 시 **공용 + 클라이언트** 스킬을 함께 본다.

### 공용 (`common/`) — 모든 작업

| 스킬 | 경로 | 내용 |
|------|------|------|
| `commit-convention` | [`.claude/skills/common/commit-convention/SKILL.md`](.claude/skills/common/commit-convention/SKILL.md) | Git 커밋 메시지 규칙 |

### 클라이언트 (`client/`) — `Assets/` 코드 작업 시

| 스킬 | 경로 | 내용 |
|------|------|------|
| `clean-code-style` | [`.claude/skills/client/clean-code-style/SKILL.md`](.claude/skills/client/clean-code-style/SKILL.md) | Unity/C# 클린 코드 스타일 규칙 |
| `feature-design` | [`.claude/skills/client/feature-design/SKILL.md`](.claude/skills/client/feature-design/SKILL.md) | OOP·SOLID·디자인 패턴 기반 기능 설계 |
| `optimization` | [`.claude/skills/client/optimization/SKILL.md`](.claude/skills/client/optimization/SKILL.md) | 성능 최적화 판단 및 적용 가이드 |
| `unity-handoff` | [`.claude/skills/client/unity-handoff/SKILL.md`](.claude/skills/client/unity-handoff/SKILL.md) | 유니티 에디터 작업 핸드오프 프롬프트 생성 |
