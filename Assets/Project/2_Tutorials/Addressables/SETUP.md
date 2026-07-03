# SETUP — 세팅 · 빌드 · 배포 · 트러블슈팅

이 예제를 처음부터 돌리기 위한 **모든 세팅**을 모았습니다. (개요는 [README.md](./README.md), 개념은 [CONCEPTS.md](./CONCEPTS.md))

## 목차
- [1. XAMPP (로컬 원격 서버)](#-1-xampp-로컬-원격-서버)
- [2. Addressables 설정 (Before → After)](#-2-addressables-설정-before--after)
- [3. 프로파일 (Remote.LoadPath)](#-3-프로파일-remoteloadpath)
- [4. 빌드 옵션 3종 & content_state.bin](#-4-빌드-옵션-3종--content_statebin)
- [5. 플레이 모드 스크립트](#-5-플레이-모드-스크립트)
- [6. 서버 배포 (copy_to_xampp.bat)](#-6-서버-배포-copy_to_xamppbat)
- [7. 트러블슈팅](#-7-트러블슈팅)

---

## 🖥 1. XAMPP (로컬 원격 서버)

1. [apachefriends.org](https://www.apachefriends.org/) 에서 Windows 버전 설치 (기본 경로 `C:\xampp`).
2. **XAMPP Control Panel → Apache `Start`**.

   ![XAMPP Control Panel — Apache 실행](./Capture/XAMPP%20Control%20Panel%20실행%20캡쳐.png)
   *▲ XAMPP Control Panel 에서 Apache 를 Start 한 모습.*

3. 브라우저에서 **`http://localhost`** 접속 → **XAMPP 환영 페이지가 뜨면 성공**. (안 뜨면 Apache 미실행 / 80포트 충돌 — Skype·IIS 등이 80포트를 쓰면 충돌)

   ![http://localhost 정상 화면](./Capture/httplocalhost%20알맞은%20화면%20캡쳐.png)
   *▲ `http://localhost` 접속 시 이 XAMPP 대시보드가 뜨면 웹서버 정상.*

4. 웹 루트는 `C:\xampp\htdocs`. 아래 폴더를 미리 만들어 둡니다 :
   ```
   C:\xampp\htdocs\Addressables\StandaloneWindows64\
   ```
   → URL `http://localhost/Addressables/StandaloneWindows64/` 로 접근됩니다.

---

## ⚙️ 2. Addressables 설정 (Before → After)

`Assets/AddressableAssetsData/AddressableAssetSettings` 선택 → 인스펙터. (영상 07:25~)

| 설정 | Before (기본/로컬) | After (원격) | 켜면 달라지는 점 |
|---|---|---|---|
| **Build Remote Catalog** | off | ✅ on | 카탈로그(.hash/.bin)를 서버에 배포 → 앱이 원격 변경을 감지 가능 |
| **그룹 Build & Load Paths** | Local | **Remote** | 번들 저장/로드 경로가 서버(ServerData → LoadPath URL)로 바뀜 |
| **Only update catalogs manually** | off | ✅ on | 앱 시작 시 자동 갱신 대신 **유저 동의 후** 수동 업데이트 |
| **Unique Bundle IDs** | off | ✅ on | 빌드마다 번들에 고유 ID → CDN 캐시 충돌 방지(대신 배포 용량↑) |

> 그룹별 Build & Load Path 는 각 그룹 인스펙터의 **BundledAssetGroupSchema** 에서 지정합니다.
> - `Packed Assets Local` → Local / `Packed Assets Remote` → Remote.

---

## 🌐 3. 프로파일 (Remote.LoadPath)

`AddressableAssetSettings` 인스펙터 상단 **`Manage Profiles`** → `Default` 프로파일.

| 변수 | 값 |
|---|---|
| **Remote.LoadPath** | `http://localhost/addressables/[BuildTarget]` |
| **Remote.BuildPath** | `ServerData/[BuildTarget]` |

- `[BuildTarget]` 은 빌드 플랫폼명으로 **자동 치환**됩니다(PC 빌드 = `StandaloneWindows64`).
- 최종 경로는 인스펙터 `Catalog > Paths > Load Path` 프리뷰에서 확인 가능.
- **배포(실서버/CDN) 전환은 이 LoadPath 값만** 바꾸면 됩니다. 코드/에셋 변경 불필요. (자동 전환 → [CONCEPTS](./CONCEPTS.md))

---

## 🔨 4. 빌드 옵션 3종 & content_state.bin

Groups 창 `Build` 메뉴. (영상 09:58~)

| 옵션 | 언제 | 동작 | 기준 파일 |
|---|---|---|---|
| **New Build \| Default Build Script** | 첫 배포, 그룹/경로 구조 변경 | 전체 번들 + 카탈로그를 새로 생성 | 새로 `content_state.bin` 기록 |
| **Update a Previous Build** | 리소스 일부만 패치 | 변경된 에셋만 새 번들로 추가, 카탈로그 갱신 (빠름) | 기존 `content_state.bin` 참조 |
| **Clean Build** | 빌드 꼬임/초기화 | 모든 번들·카탈로그 삭제 후 재빌드 | — |

**`addressables_content_state.bin`**
- 위치 : `Assets/AddressableAssetsData/Windows/` (`[BuildTarget]` 별)
- 역할 : **마지막 빌드 상태 기록**. 이 파일이 있어야 *Update a Previous Build* 로 변경분만 빌드 가능.
- 없거나 삭제되면 → *Update* 불가, `New Build` 부터 다시.
- 저장 경로 옵션(인스펙터 *Update a Previous Build* 프리뷰) :
  - **Local** : 프로젝트 내부(빠름, 관리 불필요 / 다른 PC·클린 시 소실)
  - **Editor Settings** : 프로젝트 폴더 저장 → **버전 관리 포함**, 협업/QA 기준 공유
  - **Remote** : 원격 저장 → 빌드 서버/CI 파이프라인

> 참고 : 이 프로젝트는 `.gitignore` 의 `*.bin*` 규칙으로 `content_state.bin` 이 git 제외됩니다. 협업 시엔 Editor Settings 경로 + git 추적 고려.

**산출물 위치** : 빌드하면 `ServerData/StandaloneWindows64/` 에 번들 + 카탈로그 생성 → 6번(배포)으로 서버에 올림.

---

## ▶️ 5. 플레이 모드 스크립트

Groups 창 상단 `Play Mode Script`. (영상 18:00~)

| 모드 | 용도 | 검증 범위 |
|---|---|---|
| **Use Asset Database** | 빌드 없이 가장 빠른 개발/디버깅 | 로직만. 번들 다운로드·캐싱·카탈로그는 **검증 안 됨** |
| **Use Existing Build** | 실배포 흐름 그대로 검증 (이 예제 사용) | 로드·캐싱·카탈로그 업데이트까지 **모두 검증** |

정리 : **개발 중 = Asset Database**, **원격 패치 검증 = Use Existing Build**.

---

## 📦 6. 서버 배포 (copy_to_xampp.bat)

빌드 산출물(`ServerData/StandaloneWindows64/`)을 XAMPP 웹루트로 복사해야 게임이 다운로드할 수 있습니다.

```
Assets/Project/2_Tutorials/Addressables/copy_to_xampp.bat   (더블클릭)
   → robocopy /MIR 로 C:\xampp\htdocs\Addressables\StandaloneWindows64 에 미러 복사
```

- **/MIR(미러)** : 대상 폴더를 소스와 정확히 동일하게 맞춤(잉여 파일 삭제) → 서버 = 항상 최신 빌드.
- 경로가 다르면 bat 상단 `SRC` / `DST` 두 줄만 수정.
- 배포 후 검증 : `http://localhost/Addressables/StandaloneWindows64/catalog_0.1.hash` 접속 → **해시 문자열이 보이면 정상**.

---

## 🛠 7. 트러블슈팅

### ▸ bat 실행 시 `'…' is not recognized as an internal command` (한글 깨짐)
- **원인** : 배치 파일에 한글이 들어가면 한국어 Windows(cp949)가 UTF-8 한글 바이트를 명령으로 오해해 파싱이 깨짐.
- **해결** : 현재 `copy_to_xampp.bat` 는 **영문(ASCII) 전용**으로 작성되어 이 문제가 없습니다. (직접 .bat 을 만들 땐 한글 대신 영문 사용)

### ▸ `catalog_0.1.hash` 를 열면 해시 문자열만 뜬다 — 정상인가요?
- **정상입니다.** `http://localhost/…/catalog_0.1.hash` 는 원래 **텍스트(해시값)** 파일입니다. 예 : `97873599aab8fb80fc383bf0ff383afe`.
- 브라우저에 이 값이 그대로 보이면 = Apache 가 파일을 잘 서빙하는 것 = **성공 신호**. (다운로드가 시작되거나 페이지가 렌더되는 게 아닙니다.)

![catalog_0.1.hash 정상 화면](./Capture/httplocalhostAddressablesStandaloneWindows64catalog_0.1.hash%20알맞은%20화면%20캡쳐.png)
*▲ 해시 문자열(예 `97873599…`)이 텍스트로 그대로 보이면 서버 배포 정상.*

### ▸ 원격 서버 접속이 안 됨 / 다운로드 실패
- **Apache** 가 켜져 있는지(`http://localhost` 확인).
- **Player Settings → Player → Other Settings → Configuration → `Allow downloads over HTTP` = Always allowed**. HTTP(비 HTTPS) 차단이 원인일 수 있음. (실서비스 빌드 전엔 다시 Not allowed 권장)
- 파일이 `htdocs\Addressables\StandaloneWindows64\` 경로에 실제로 있는지(대소문자 포함).

### ▸ 카탈로그 업데이트가 감지되지 않음 (`업데이트 대상 카탈로그가 없습니다`)
- 가장 흔한 원인은 **프라이밍 실행 누락** 또는 **catalog `.hash` 미복사** 입니다.
- 상세 진단 6가지 → **[CONCEPTS.md → Q6](./CONCEPTS.md)**.

### ▸ Play Mode Script 가 `Use Existing Build` 인데도 계속 로컬(Fast Mode)로 실행됨
- **증상** : 드롭다운 체크는 `Use Existing Build` 인데, 실행하면 항상 `0 bytes` + 에셋이 원격이 아니라 프로젝트에서 직접 로드됨.
  진단 시 리소스 provider 가 `AssetDatabaseProvider`(= Fast Mode)로 찍힘.
- **원인** : **Addressables `2.7.x` 의 Play Mode 인덱스 desync 버그.** 빌드·클린을 한 세션에서 여러 번 반복하면 체크 표시와 실제 실행 모드가 어긋납니다. (사용자 설정 문제 아님)
- **해결** : **패키지를 `3.1.0`(또는 그 이상)으로 업데이트.** Package Manager → 좌측 상단 `+` → `Install package by name` → `com.unity.addressables` 최신 설치. (이 프로젝트가 겪고 해결한 실제 사례)
- 임시 회피 : Play Mode Script 재선택(Fast ↔ Existing 토글) → Unity 재시작 → `Library/com.unity.addressables` 삭제 후 재빌드. (버전 업데이트가 근본 해결)

### ▸ 매번 `필요 다운로드 용량 : 0 bytes` 로 다운로드가 안 보임
- **정상(캐싱)입니다.** 한 번 받은 번들은 **클라이언트 캐시**에 저장되어, 같은 콘텐츠면 다시 안 받습니다.
- ⚠️ `ServerData` / `htdocs` 의 파일을 지워도 **클라이언트 캐시는 안 지워집니다**(재다운로드 안 됨). `ServerData` 는 지울 필요 없고, `htdocs` 는 다운로드 원천이라 지우면 404.
- 다운로드를 다시 보려면 → **캐시만 비우기** : 메뉴 **`Tools > Addressables > Clear Bundle Cache`**(또는 `Caching.ClearCache()`) → 다시 Play. 아니면 **콘텐츠 변경 후 Update 빌드**.
  - 이 메뉴가 지우는 것 : 내려받은 원격 번들 사본. 안 지우는 것 : 카탈로그 캐시 · 로컬 그룹 · 서버 파일.
- 원리/그림 → **[CONCEPTS.md → Q6 하단(캐시)](./CONCEPTS.md)**.
