# CONCEPTS — 개념 정리 · 확장 · 트러블슈팅

영상을 보며 생긴 의문과, 앞으로의 확장 방법을 정리한 문서입니다.
(실행/세팅 절차는 [README.md](./README.md) · [SETUP.md](./SETUP.md))

## 목차
- [❓ Q1. XAMPP / Apache / 서버](#-q1-xampp--apache--서버)
- [❓ Q2. 로컬 그룹 vs 그냥 프로젝트 에셋, 뭐가 다를까?](#-q2-로컬-그룹-vs-그냥-프로젝트-에셋-뭐가-다를까)
- [❓ Q3. Local / Remote 그룹 실무 흐름](#-q3-local--remote-그룹-실무-흐름)
- [❓ Q4. 로컬↔배포 경로를 자동으로 바꿀 수 있나?](#-q4-로컬배포-경로를-자동으로-바꿀-수-있나)
- [❓ Q5. CDN 이 뭐의 약자야?](#-q5-cdn-이-뭐의-약자야)
- [❓ Q6. 카탈로그 업데이트가 감지되지 않을 때 (버그 진단)](#-q6-카탈로그-업데이트가-감지되지-않을-때-버그-진단)
- [❓ Q7. 그룹이 많으면 catalog 파일도 많아지나? (부분 다운로드 기준)](#-q7-그룹이-많으면-catalog-파일도-많아지나-부분-다운로드-기준)
- [🧭 반복작업 / 테스트 / 확장 시나리오](#-반복작업--테스트--확장-시나리오)

---

## 🧠 먼저 : 핵심 멘탈 모델 3가지

아래 Q들을 읽기 전에 이 3가지만 잡으면 훨씬 쉽습니다. (실습하며 가장 헷갈리는 지점)

1. **카탈로그 = 콘텐츠 목록 + 버전.**
   업데이트 여부는 파일을 하나하나 비교하는 게 아니라 **카탈로그 해시(클라이언트 vs 서버)** 로 판단합니다.

2. **파일은 3군데에 삽니다.**
   ```
   ① ServerData        (에디터 빌드 산출물)       ──── copy_to_xampp.bat ────▶
   ② htdocs (XAMPP)    (서버 = 게임이 받는 곳)    ──── 다운로드           ────▶
   ③ 클라이언트 캐시      (게임이 받아 저장한 사본)
   ```
   실제 유저 기기엔 **① ServerData 가 없습니다**(개발용 산출물일 뿐). 유저는 ②에서 받아 ③에 저장합니다.

3. **다운로드 판단 = "이 번들이 내 캐시(③)에 있나?"**
   있으면 안 받음(`0 bytes` = 정상). 없으면 서버(②)에서 받아 ③에 저장.

> 그래서 **"다시 다운로드"** 를 보려면 ③ 캐시만 비우면 됩니다(①·②는 그대로 둠). → 상세 [Q6](#-q6-카탈로그-업데이트가-감지되지-않을-때-버그-진단)
>
> 📍 **③은 실제로 어디에 저장되나?** — 다운로드된 **번들**은 Unity 에셋번들 캐시(플랫폼별, Windows 예 : `%USERPROFILE%\AppData\LocalLow\Unity\<회사>_<제품>\` 아래), **카탈로그 사본**은 `persistentDataPath/com.unity.addressables/` 에 저장됩니다. (`Tools > Addressables > Clear Bundle Cache` = 번들 캐시 삭제)

---

## ❓ Q1. XAMPP / Apache / 서버

#### ▸ Apache 를 꼭 켜야 하나요?
**네.** `localhost` 는 "내 컴퓨터"를 가리키는 주소로 항상 존재하지만, **80번 포트에서 요청에 응답할 웹서버**가 있어야 파일이 내려옵니다. 그 웹서버가 **Apache**. 안 켜면 `http://localhost` 무응답 → Addressables 원격 로드/카탈로그 확인 실패.

#### ▸ Remote.LoadPath 와 XAMPP 는 어떻게 엮이나요?
```
http://localhost/  ─────────►  C:\xampp\htdocs\
http://localhost/Addressables/StandaloneWindows64/catalog_0.1.hash
                    ─────────►  C:\xampp\htdocs\Addressables\StandaloneWindows64\catalog_0.1.hash
```
빌드 산출물(`ServerData/StandaloneWindows64/`)을 `htdocs\Addressables\StandaloneWindows64\` 로 복사하면 게임이 그 URL 로 다운로드합니다. (복사 자동화 = `copy_to_xampp.bat`)

#### ▸ 로컬 개발 ↔ 실제 배포는?
코드는 그대로, **프로파일의 `Remote.LoadPath` 값만** 바꿉니다. → 자동화는 [Q4](#-q4-로컬배포-경로를-자동으로-바꿀-수-있나).

| 환경 | Remote.LoadPath |
|---|---|
| 로컬 개발 | `http://localhost/addressables/[BuildTarget]` |
| 실서비스 | `https://cdn.mygame.com/addressables/[BuildTarget]` |

#### ▸ XAMPP 는 유료? 실무에서 쓰나요?
- **무료 오픈소스**(로컬/리모트 모두 무료). 단 **실서비스 배포에는 잘 쓰지 않습니다** — XAMPP 는 "내 PC 에 웹서버를 빠르게 띄우는 개발/테스트 도구". 실무는 보통 CDN.

#### ▸ XAMPP 모듈은 각각 뭔가요?
| 모듈 | 역할 | 이 예제 |
|---|---|---|
| **Apache** | HTTP 웹서버 (정적 파일 서빙) | ✅ 필요 |
| **MySQL / MariaDB** | 관계형 데이터베이스 | ❌ |
| **FileZilla** | FTP 서버 (파일 전송) | ❌ |
| **Mercury** | 메일(SMTP/POP3) 서버 | ❌ |
| **Tomcat** | Java 서블릿/JSP 컨테이너 | ❌ |

#### ▸ 게임사는 어떤 서버/CDN 을 쓰나요?
- **AWS S3 + CloudFront**
- **Google Cloud Storage + Cloud CDN**
- **Azure Blob + CDN**
- **Unity CCD (Cloud Content Delivery)** — Addressables 와 직접 연동되는 유니티 공식 서비스
- 자체 **nginx · Apache**

> 공통점 : "정적 파일을 HTTP(S)로 서빙"이면 무엇이든 원격 서버가 됩니다(URL 만 교체).

---

## ❓ Q2. 로컬 그룹 vs 그냥 프로젝트 에셋, 뭐가 다를까?

> "로컬 그룹 에셋 = 그냥 프로젝트에 있는 에셋 아닌가?" — **물리적으론 맞습니다.** 둘 다 앱(플레이어)에 함께 빌드됩니다.

하지만 세 가지가 다릅니다.

#### ▸ ① 로딩 방식
- **직접 참조 에셋** (씬/프리팹에 드래그로 연결) : 씬 로드 시 **함께 즉시 로드**되고 상시 메모리에 상주.
- **로컬 어드레서블** : **주소/라벨로 필요할 때 비동기 로드**(`LoadAssetAsync`), **참조 카운팅**으로 안 쓰면 언로드. 메모리를 세밀하게 관리.

#### ▸ ② 결합도
- 직접 참조 = 강결합(에셋을 지우면 참조 깨짐).
- 어드레서블 = **주소 문자열**로 느슨하게 결합 → 에셋 교체/재구성이 유연.

#### ▸ ③ 확장성 (가장 중요)
- 로컬 그룹은 나중에 **그룹의 Build/Load Path 를 Remote 로 바꾸기만** 하면 **코드 수정 없이 원격 패치 대상으로 전환**됩니다.
- 직접 참조 에셋은 그게 불가능(원격화하려면 어드레서블로 바꿔야 함).

#### ▸ 결론
"지금 당장"은 프로젝트 에셋과 비슷해 보여도, **통일된 비동기 로딩 파이프라인 + 나중에 원격으로 넘길 수 있는 유연성**이 로컬 어드레서블의 핵심 가치입니다.

---

## ❓ Q3. Local / Remote 그룹 실무 흐름

현재 그룹 : **Packed Assets Local**(`item_sword_1`) / **Packed Assets Remote**(`item_sword_2·3·4`).

| | **Packed Assets Local** | **Packed Assets Remote** |
|---|---|---|
| 저장 위치 | 앱(플레이어) 내부 | 원격 서버(ServerData → XAMPP) |
| "변경했을 때" 할 일 | **앱 재빌드 & 재배포** | **Update 빌드 → 서버에 파일 복사** (앱 재배포 불필요) |
| 적합한 리소스 | 시작부터 항상 필요한 필수 에셋 | 나중에 추가/교체할 패치 대상 |

**판단 기준** : "첫 실행에 반드시 있고 잘 안 바뀐다" → Local / "출시 후에도 추가·수정해 패치로 내보낼 것" → Remote.

#### ▸ 보통 무엇을 Local / Remote 로 넣나요?

**Local (앱 내장) — "다운로드를 기다리는 동안에도 필요한 것"**
- 부트/스플래시/로딩 화면, **다운로드 진행 UI**, 인트로 화면
- 다운로드 중 뒤에서 재생되는 **인트로 영상 / 배경 애니메이션** ← 질문하신 예시가 정확히 이 경우
- 로고, 기본 폰트, 공용 UI 스킨, 필수 공용 셰이더/머티리얼, 시스템 효과음
- 요점 : **원격 다운로드가 끝나기 전에 화면에 보여야 하는 것은 반드시 Local.** (Remote 로 두면 "받기 전엔 보여줄 게 없는" 모순 발생)

**Remote (서버 다운로드) — "용량 크고, 나중에 추가·교체되는 것"**
- 스테이지/레벨 에셋, 캐릭터·스킨·의상
- 이벤트/시즌 한정 콘텐츠, 신규 아이템
- 고해상도 텍스처, 컷신 영상, 추가 언어팩, 사운드팩
- 밸런스/데이터 패치용 리소스
- 요점 : **출시 후에도 자주 갈아끼우거나, 초기 설치 용량을 줄이려는 것**은 Remote.

---

## ❓ Q4. 로컬↔배포 경로를 자동으로 바꿀 수 있나?

**네, 가능합니다.** 프로파일을 상황별로 두고 **활성 프로파일(activeProfileId)** 을 스크립트로 전환합니다.

#### ▸ 원리
1. `Manage Profiles` 에서 프로파일 2개 생성 : 예 `Dev`(Remote.LoadPath=localhost), `Release`(=CDN URL).
2. **Addressables "콘텐츠 빌드" 시점에 LoadPath 가 카탈로그에 구워집니다.** 따라서 "빌드 직전에 활성 프로파일을 선택"하는 것이 정석.
3. 에디터 메뉴나 빌드 프리프로세스(`IPreprocessBuildWithReport`)에서 활성 프로파일을 바꾼 뒤 Addressables 를 빌드.

#### ▸ 짧은 예시 (개념용 스니펫)
```csharp
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

var settings = AddressableAssetSettingsDefaultObject.Settings;
// "Release" 프로파일로 전환 → 이후 Addressables 빌드는 CDN 경로로 구워짐
settings.activeProfileId = settings.profileSettings.GetProfileId("Release");
```
> 사용자의 "에디터면 A, 빌드면 B" = 위처럼 **빌드 종류에 따라 프로파일을 자동 선택**하면 됩니다.
> ⚠️ 이미 빌드된 카탈로그의 경로를 **런타임에** 통째로 바꾸는 것은 제한적입니다(경로가 카탈로그에 고정됨). 정석은 "빌드 전에 프로파일 결정".
> (이 예제에는 실제 전환 스크립트 파일은 포함하지 않았습니다 — 개념만.)

---

## ❓ Q5. CDN 이 뭐의 약자야?

**CDN = Content Delivery Network** (콘텐츠 전송 네트워크)

- 정적 파일(번들·카탈로그·이미지 등)을 **전 세계 여러 엣지 서버에 분산 캐싱**해, 사용자와 가까운 서버에서 빠르게 내려주는 인프라.
- 어드레서블 관점에선 "Remote.LoadPath 를 CDN URL 로 지정" = 전 세계 유저에게 빠르고 안정적으로 패치 배포.

---

## ❓ Q6. 카탈로그 업데이트가 감지되지 않을 때 (버그 진단)

> 증상 : `Packed Assets Remote` 에 `item_sword_4` 추가 → Update 빌드 → XAMPP 복사 → 실행했는데
> ```
> [Addr] 업데이트 대상 카탈로그가 없습니다. 먼저 CheckForCatalogUpdates.
> ```

#### ▸ 왜 이런가 (핵심 원리)
`CheckForCatalogUpdates()` 는 **"앱이 마지막에 캐시한 카탈로그 해시"** vs **"서버의 최신 `.hash`"** 를 비교합니다. 방금 빌드한 새 카탈로그를 앱이 곧바로 로드하면 이미 "그게 최신"이라 **비교할 이전 상태가 없어** 감지되지 않습니다.

> 영상(24:22 구간)도 언급 : *"첫 빌드는 서버로 복사하지 말고 게임을 한 번 실행해야 한다. 실행해서 로컬 카탈로그를 갱신해 두어야 다음부터 서버 카탈로그와 정상 비교된다."*

#### ▸ 해결 : 우선순위 체크리스트
1. **⭐ 로컬 카탈로그 "프라이밍" 실행** (가장 흔함)
   1. (변경 전) 빌드 → 서버에 복사 → **게임 1회 실행** ← 현재 카탈로그 해시가 앱에 캐시됨
   2. `item_sword_4` 추가 → **Update a Previous Build**
   3. `copy_to_xampp.bat` 로 `catalog_0.1.hash/.bin` + 새 `.bundle` 서버에 복사
   4. 다시 실행 → 캐시된 이전 해시 ≠ 새 해시 → **감지 ✅**
2. **`.hash` 를 서버에 실제로 덮어썼는지** — `.bundle` 만 복사하면 안 됨. `copy_to_xampp.bat`(/MIR)로 전체 동기화.
3. **Apache 실행 + 경로 확인** — `http://localhost/Addressables/StandaloneWindows64/catalog_0.1.hash` 가 200(해시 텍스트)으로 열리는지.
4. **Allow downloads over HTTP = Always allowed** (Player Settings) — 아니면 HTTP 차단으로 조용히 실패.
5. **`item_sword_4` 를 Remote 그룹에** 넣었는지 — `Packed Assets Remote` 여야 함(현재 정상 등록됨). Local 그룹이면 원격 패치와 무관.
6. **New Build 반복 금지** — 콘텐츠 갱신은 **Update a Previous Build** 사용(New Build 를 매번 하면 Unique Bundle IDs 로 번들명이 전부 바뀌어 관리가 꼬임).

#### ▸ 반대로, 매번 `필요 다운로드 용량 : 0 bytes` 로 다운로드가 안 보인다면? (캐시)

> 증상 : 실행하면 `변경된 카탈로그 없음` + `0 bytes` 로 **바로 Game 씬으로 넘어감**. 그런데 아이템은 잘 로드됨.

**이건 버그가 아니라 정상(캐싱)입니다.** 위치를 3개로 구분해야 이해됩니다.

```
① ServerData/…      (프로젝트 내 빌드 산출물)   ─copy_to_xampp.bat─▶
② htdocs/…          (XAMPP 서버 = 게임이 받는 곳)  ─최초 다운로드─▶
③ 클라이언트 캐시    (게임이 받아서 저장하는 곳)  ← "0 bytes"의 진짜 원인
```

- Addressables 는 한 번 받은 원격 번들을 **③ 클라이언트 캐시**(`m_UseAssetBundleCache: 1`)에 저장합니다.
  캐시에는 "에셋의 위치 정보"가 아니라 **서버에서 받은 번들 파일의 사본**이 들어갑니다(해시로 식별). 그래서 같은 번들이 이미 있으면 `0 bytes`.
- 제가 안내한 **"게임 1회 실행(프라이밍)"이 바로 그 다운로드**였고, 그 뒤로는 캐시에 있으니 `0 bytes` = "받을 것 없음"이 정상입니다.
- **비교/판단 단위** : 클라이언트는 "ServerData ↔ 서버"를 비교하지 않습니다. **카탈로그 해시(클라이언트 vs 서버)** 를 비교하고, 다운로드는 **"이 번들이 내 캐시에 있나?"** 로 판단합니다. (실제 유저 기기엔 ServerData 자체가 없음 — 개발용 산출물일 뿐)
- ⚠️ **자주 하는 오해** : `① ServerData` 나 `② htdocs` 의 파일을 지워도 → **③ 클라이언트 캐시는 안 지워집니다**(재다운로드 안 됨). 런타임엔 `② htdocs` 에서 받으므로 **ServerData 는 지울 필요가 없고**, **htdocs 는 다운로드 원천이라 지우면 404 로 실패**합니다.

**다운로드 과정을 "다시" 보고 싶다면 → `ServerData`·`htdocs` 는 그대로 두고, ③ 클라이언트 캐시만 비웁니다.**
1. **캐시 비우기 (권장)** : → 이후 Play 하면 `0 bytes` 가 아니라 실제 용량이 뜨고 다운로드가 진행됩니다.
   - **원클릭 메뉴** : **`Tools > Addressables > Clear Bundle Cache`** (`Scripts/Editor/AddressablesCacheTool.cs`).
   - 코드로는 `Caching.ClearCache();`(전체) 또는 `Addressables.ClearDependencyCacheAsync(key)`(특정 키).
   - **무엇이 지워지나** :
     - ✅ 지워짐 : 내려받아 저장한 **원격 번들 사본**(③ 캐시).
     - ❌ 안 지워짐 : 카탈로그 캐시(별도 위치) · 로컬 그룹 에셋(앱 내장) · 서버 파일(① ServerData / ② htdocs).
   - ⚠️ Play 중이면 번들이 사용 중이라 `Caching.ClearCache()` 가 실패(false)할 수 있음 → **Play 를 멈춘 뒤** 실행.
2. **콘텐츠를 실제로 바꾸는 패치 흐름** : Remote 그룹 에셋 변경 → `Update a Previous Build` → `copy_to_xampp.bat` → Play. 새 번들은 캐시에 없으니 다운로드됩니다.

---

## ❓ Q7. 그룹이 많으면 catalog 파일도 많아지나? (부분 다운로드 기준)

> 궁금증 : 그룹이 늘면 `catalog_0.1.bin/.hash` 도 엄청 많아지고, 그래야 부분 다운로드가 되는 것 아닌가?

**아닙니다 — 여기 핵심 오해가 있습니다.** 쪼개지는 건 카탈로그가 아니라 **번들**입니다.

#### ▸ 카탈로그(bin/hash)는 "빌드당 1세트" — 그룹 수와 무관
- `catalog_0.1.bin`(본문) + `catalog_0.1.hash`(버전 지문)는 **전체 콘텐츠의 마스터 목록 1세트**입니다.
- 그룹이 2개든 50개든 원격 카탈로그는 **기본적으로 하나**입니다. (카탈로그 = "색인/목차"이지 데이터 자체가 아님)

#### ▸ 여러 개로 나뉘는 건 "번들(.bundle)"
- 실제 에셋 데이터는 **번들 파일**에 담기고, 번들은 **그룹(+ Bundle Mode)** 기준으로 나뉩니다.
  ```
  ServerData/StandaloneWindows64/
  ├── catalog_0.1.bin / .hash          ← 목록 (1세트)
  ├── packedassetsremote_xxxxx.bundle  ← Remote 그룹 번들
  ├── packedassetslocal_yyyyy.bundle   ← Local 그룹 번들
  └── ...(그룹/설정마다 번들 추가)
  ```
- 한 그룹을 몇 개의 번들로 쪼갤지는 그룹 인스펙터의 **Bundle Mode**(Advanced)로 정합니다 :
  - **Pack Together** : 그룹 전체를 번들 1개로 (기본).
  - **Pack Separately** : 에셋(엔트리)마다 번들 1개 → 가장 잘게 부분 다운로드.
  - **Pack Together By Label** : 라벨별로 묶어 번들.

#### ▸ 그래서 "부분 다운로드"는 이렇게 동작
1. 클라이언트가 **카탈로그 해시 1개**를 서버와 비교 → 바뀌었으면 **카탈로그(작은 파일)만** 먼저 받음.
2. 새 카탈로그가 가리키는 번들들 중 **내 캐시에 없거나 해시가 바뀐 번들만** 골라 다운로드.
3. 즉 부분 다운로드의 단위는 **카탈로그가 아니라 "번들"** 이고, 그 잘기(granularity)는 **그룹 구성 + Bundle Mode**로 조절합니다.

> 정리 : **카탈로그 = 1세트(목차)**, **번들 = 여러 개(실데이터, 그룹/설정 기준)**. 목차 하나로 "무엇이 바뀌었는지" 판단하고, 바뀐 번들만 받습니다.

![ServerData / htdocs 폴더 파일 목록](./Capture/ServerData%20%20htdocs%20폴더%20파일%20목록.png)
*▲ 실제 산출물 : `catalog_0.1.bin/.hash` 는 **1세트뿐**이고, `.bundle` 파일이 **그룹/설정만큼 여러 개** 생깁니다.*

---

## 🧭 반복작업 / 테스트 / 확장 시나리오

#### ▸ 리모트 에셋 추가/수정 패치 (가장 자주)
1. `Packed Assets Remote` 에 에셋 드래그 → 주소 키 지정
2. Groups → `Build > Update a Previous Build`
3. `copy_to_xampp.bat` 더블클릭
4. (Apache on) Intro 씬 Play → 카탈로그 업데이트 → 다운로드 → 키 입력 확인

#### ▸ 로컬 에셋 변경
`Packed Assets Local` 수정 → `Build > New Build` → 필요 시 플레이어 재빌드(서버 복사 불필요).

#### ▸ 빌드가 꼬였을 때 (초기화)
`Build > Clean Build > All Content` → `New Build` → 게임 1회 실행(프라이밍) → `copy_to_xampp.bat`.

#### ▸ 빠른 로직 테스트
Play Mode Script = `Use Asset Database` → 빌드/서버 없이 즉시 Play (다운로드/캐싱은 검증 안 됨).

#### ▸ 실배포 흐름 검증
Play Mode Script = `Use Existing Build` + Apache on + 서버 최신화 상태에서 Play.

#### ▸ 새 플랫폼 확장 (예 : Android)
빌드 타깃 전환 시 `[BuildTarget]` 이 `Android` 로 치환 → 서버에 `Addressables/Android/` 폴더 별도 준비, `copy_to_xampp.bat` 의 `DST` 를 플랫폼별로 분기(또는 bat 복제).

#### ▸ 실서버 / CDN 전환
`Manage Profiles` → `Remote.LoadPath` 를 실제 URL 로 교체(코드·에셋 변경 없음). 자동화는 [Q4](#-q4-로컬배포-경로를-자동으로-바꿀-수-있나).
