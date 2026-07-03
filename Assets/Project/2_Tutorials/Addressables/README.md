# Addressables — 원격 서버 패치 (Remote Content Delivery)

Unity **Addressables** 로 원격 서버(XAMPP)에서 에셋을 다운로드/패치하는 흐름을 학습하는 예제입니다.

카탈로그 확인 → 다운로드 용량 계산 → 다운로드 → 스프라이트 로드까지의 전체 파이프라인을 다룹니다.

## 목차
- [프로젝트 개요](#-프로젝트-개요)
- [아키텍처 한눈에 보기](#-아키텍처-한눈에-보기)
- [용어 : 카탈로그(Catalog)](#-용어--카탈로그catalog)
- [핵심 흐름](#-핵심-흐름)
- [학습 자료](#-학습-자료)
- [프로젝트 구조](#-프로젝트-구조)
- [빠른 시작 (Quick Start)](#-빠른-시작-quick-start)
- [Addressables 그룹 구성](#-addressables-그룹-구성)
- [더 알아보기](#-더-알아보기)

---

## 📌 프로젝트 개요

**목표 :** 모바일/PC 게임에서 자주 쓰는 "원격 서버에서 패치 파일 다운로드" 기능을 Addressables 로 구현하고,
로컬 테스트 서버(XAMPP)를 이용해 실제 배포 흐름과 동일하게 검증한다.

- 자세한 **세팅/빌드/배포** → [SETUP.md](./SETUP.md)
- **개념 정리 / 트러블슈팅 / 확장** → [CONCEPTS.md](./CONCEPTS.md)

---

## 🏛 아키텍처 한눈에 보기

에셋이 **에디터 → 빌드 산출물 → 서버 → 게임**으로 흘러갑니다. 위에서 아래로 읽으세요.

```
[ Unity 프로젝트 ]
   · Scenes          : Intro · Game · Test
   · Scripts         : SceneIntro · SceneGame · CatalogUpdateExample · ErrorLogOverlay
   · ※ 어드레서블 그룹 : Local(앱 내장 에셋) · Remote(서버 다운로드 에셋)
        |
        |  (1) Addressables 빌드
        v
[ ServerData/StandaloneWindows64/ ]        <- 빌드 산출물 (번들 + 카탈로그)
        |
        |  (2) copy_to_xampp.bat  (미러 복사)
        v
[ C:\xampp\htdocs\Addressables\... ]       <- 원격 서버 (Apache, 포트 80)
        |
        |  (3) http://localhost/...  다운로드
        v
[ 게임 클라이언트 ]
   카탈로그 해시 비교 -> 필요한 번들만 다운로드 -> 클라이언트 캐시에 저장 -> 에셋 로드
```

한 줄 요약 : **에셋 → (빌드) → ServerData → (bat 복사) → XAMPP → (HTTP) → 게임**.

---

## 📖 용어 : 카탈로그(Catalog)

> **카탈로그 = 어드레서블 콘텐츠의 "목차/명세" 파일**

- 어떤 **주소(address)** 의 에셋이 **어느 번들/URL** 에 들어있는지, 그리고 각 콘텐츠의 **해시(버전 지문)** 를 기록합니다.
- 게임은 이 카탈로그를 읽어 **"무엇이 있고, 서버 버전이 바뀌었는지"** 를 판단합니다.
  → 버전 비교는 파일을 하나하나 대조하는 게 아니라 **카탈로그의 해시**를 비교합니다.
- 실제 파일 : `catalog_0.1.bin`(본문 = 매핑표) + `catalog_0.1.hash`(버전 지문). 파일명 `0.1` 은 PlayerSettings 의 bundleVersion.
- 즉, 흔히 "변경사항/파일 상태"라고 느끼는 것이 바로 **카탈로그(콘텐츠 목록의 버전)** 입니다.

---

## 🔄 핵심 흐름

```
[ Intro 씬 ]
   1. 카탈로그 변경 확인     (CheckForCatalogUpdates)
   2. 다운로드 용량 계산     (GetDownloadSize)
   3. (필요 시) 다운로드     (DownloadDependencies)
   4. "아무 키나 눌러 시작" 안내 -> 키 입력
        |
        v
[ Game 씬 ]
   item_sword_N 입력 -> (서버에서 받아 캐시된) 에셋 번들에서 Sprite 로드 -> 화면 표시
```

> 다운로드할 게 없거나(이미 최신·캐시됨) 다운로드가 끝나면, 자동으로 넘어가지 않고
> **"아무 키나 눌러 게임을 시작하세요"** 안내 후 키 입력을 기다립니다. (실제 게임 로딩 화면 흐름)
>
> 📌 **번들은 매번 원격에서 받는 게 아닙니다.** 처음 한 번만 서버에서 다운로드해 **로컬 캐시에 저장**하고,
> 그 뒤로는 (서버 버전이 그대로면) **캐시된 로컬 번들에서 로드**합니다. → 저장 위치 [CONCEPTS](./CONCEPTS.md)

---

## 📚 학습 자료

- **YouTube (게임코) :** https://www.youtube.com/watch?v=wYnIL09Slxg
- **예제 원본 (GitHub) :** https://github.com/GGemCo/unity-example-addressables-remote
- Addressables 패키지 : `com.unity.addressables` **3.1.0**
  - ⚠️ 초기에 `2.7.3` 으로 진행했으나, **Play Mode Script 가 `Use Existing Build` 인데도 Fast Mode 로 실행되는 desync 버그**가 있어 `3.1.0` 으로 업데이트하여 해결했습니다. (→ [SETUP 트러블슈팅](./SETUP.md))

---

## 🗂 프로젝트 구조

```
Assets/Project/2_Tutorials/Addressables/
├── README.md               # 현재 문서 (개요 · 실행)
├── SETUP.md                # 세팅 · 빌드 · 배포 · 트러블슈팅
├── CONCEPTS.md             # 개념 Q&A · 확장
├── copy_to_xampp.bat       # 빌드 산출물 → XAMPP 미러 복사 (더블클릭)
├── Scripts/
│   ├── SceneIntro.cs           # 인트로 : 카탈로그 확인 → 용량 → 다운로드 → 키 입력 → Game 이동
│   ├── SceneGame.cs            # 게임   : 입력 키로 Sprite 로드 데모
│   ├── CatalogUpdateExample.cs # 버튼 분리형 카탈로그/다운로드 데모 (Test 씬용)
│   ├── ErrorLogOverlay.cs      # 화면 내 콘솔 로그 오버레이 (F1 토글)
│   └── Editor/
│       └── AddressablesCacheTool.cs  # 메뉴 : Tools > Addressables > Clear Bundle Cache
├── Scenes/  (Intro · Game · Test)
├── Prefabs/ (ObjectItem — SpriteRenderer 데모용)
├── Sprites/ (GGemCo Data — 아이템 스프라이트)
└── Fonts/   (NanumGothicBold SDF — 한글 TMP 폰트)

Assets/AddressableAssetsData/   # 전역 Addressables 설정 (프로파일 · 그룹 · 원격 카탈로그)
ServerData/StandaloneWindows64/ # 빌드 산출물 (번들 + 원격 카탈로그) → 서버로 복사됨
```

---

## 🚀 빠른 시작 (Quick Start)

> 전제 : [XAMPP](https://www.apachefriends.org/) 설치 + `C:\xampp\htdocs\Addressables\StandaloneWindows64\` 폴더 준비.
> 세팅 상세는 → [SETUP.md](./SETUP.md)

1. **XAMPP Control Panel → Apache `Start`**
   → 브라우저에서 **`http://localhost` 접속** → **XAMPP 환영 페이지가 뜨는지 확인**. (안 뜨면 Apache 미실행 또는 80포트 충돌)
2. **(최초 1회) Addressables 빌드** : `Window > Asset Management > Addressables > Groups` → `Build > New Build > Default Build Script`
   → 이어서 **게임 1회 실행**(Intro 씬 Play) = *로컬 카탈로그 프라이밍*. 이 단계를 건너뛰면 이후 업데이트가 감지되지 않습니다. (→ [CONCEPTS Q6](./CONCEPTS.md))
3. **`copy_to_xampp.bat` 더블클릭** — ServerData → XAMPP 로 미러 복사.
4. **서버 검증** : 브라우저에서 `http://localhost/Addressables/StandaloneWindows64/catalog_0.1.hash` 접속
   → **해시 문자열(예 `97873599aab8…`)이 그대로 보이면 정상입니다.** (다운로드 창이 뜨거나 깨져 보이는 게 아니라, 텍스트가 보이는 게 맞습니다.)
5. Groups 창 상단 **Play Mode Script = `Use Existing Build`** → **Intro 씬 Play**
   → 콘솔 `[Addr]` 로그로 카탈로그 확인/다운로드 → **"아무 키나 눌러 시작"** 안내에서 키 입력 → Game 씬 진입 → InputField 에 `item_sword_1~4` 입력해 스프라이트 확인.

> 💡 **다운로드 과정을 다시 보고 싶다면** : 한 번 받은 번들은 캐시되어 다음부터 `0 bytes`(정상)입니다.
> `Tools > Addressables > Clear Bundle Cache` 로 캐시를 비우고 다시 Play 하세요. (원리 → [CONCEPTS Q6](./CONCEPTS.md))

---

## 🌐 Addressables 그룹 구성

현재 그룹은 2개입니다 (Default Local Group 은 미사용). 등록되는 에셋(주소)은 수시로 바뀌므로, **어떤 성격의 콘텐츠가 들어가는지**로 정리합니다.

| 그룹 | 종류 | 저장/로드 | 보통 넣는 콘텐츠 |
|---|---|---|---|
| **Packed Assets Local** | Local | 앱(플레이어) 내부에 포함 | 부팅/로딩 UI, 로고, 기본 폰트 등 **시작에 바로 필요한 것** |
| **Packed Assets Remote** | Remote | 원격 서버(ServerData → XAMPP) | 스테이지·캐릭터·이벤트 등 **나중에 추가/패치할 것** |

- **Local** : 앱에 함께 빌드됨. 변경하려면 앱을 다시 빌드.
- **Remote** : 서버에서 다운로드. 변경 시 `Update a Previous Build` → `copy_to_xampp.bat` 만으로 패치.
- 무엇을 Local/Remote 에 넣는지 자세한 기준 → [CONCEPTS Q3](./CONCEPTS.md), "프로젝트 에셋과 뭐가 다르지?" → [CONCEPTS Q2](./CONCEPTS.md).

![Addressables Groups 창](./Capture/Addressables%20Groups%20창.png)
*▲ `Window > Asset Management > Addressables > Groups` — Local/Remote 그룹과 우측 상단 `Play Mode Script` 드롭다운.*

---

## 🔗 더 알아보기

| 문서 | 내용 |
|---|---|
| **[SETUP.md](./SETUP.md)** | XAMPP · Addressables 설정 · 프로파일 · 빌드 옵션 · 플레이 모드 · 배포 · 트러블슈팅 |
| **[CONCEPTS.md](./CONCEPTS.md)** | 핵심 멘탈 모델, 카탈로그/CDN/로컬vs리모트 개념, 자동 프로파일 전환, 반복작업, **카탈로그 미감지·캐시 버그 진단** |
