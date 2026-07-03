# ServerData — Addressables 원격 빌드 산출물

> ⚠️ **이 폴더와 README 는 삭제하지 마세요.** Addressables 원격(Remote) 배포의 기준 경로입니다.

---

## 이 폴더는 무엇인가

Unity **Addressables 를 빌드하면 생성되는 "원격 서버용" 산출물**이 여기에 쌓입니다.
Addressables 설정의 `Remote.BuildPath = ServerData/[BuildTarget]` 가 이 폴더로 매핑됩니다.

```
ServerData/
├── README.md                 # 현재 문서 (삭제 금지)
└── StandaloneWindows64/       # [BuildTarget] = 빌드 플랫폼명
    ├── catalog_0.1.hash       # 원격 카탈로그 해시 (변경 감지의 기준)
    ├── catalog_0.1.bin        # 원격 카탈로그 본문 (에셋 → 번들 매핑표)
    └── *.bundle               # 실제 에셋이 담긴 에셋 번들
```

- **catalog_0.1.hash / .bin** : "지금 서버에 어떤 리소스가 있는지"를 적어둔 목록.
  게임은 이 `.hash` 를 비교해 **업데이트가 필요한지** 판단합니다. (파일명 `0.1` 은 PlayerSettings 의 bundleVersion)
- ***.bundle** : 원격 그룹(`Packed Assets Remote`)에 등록한 스프라이트 등 실제 데이터.

---

## 언제 / 어떻게 갱신되는가

Addressables Groups 창의 **Build 메뉴**를 실행할 때 이 폴더 내용이 갱신됩니다.

| 빌드 방법 | 이 폴더에 일어나는 일 |
|---|---|
| **New Build \| Default Build Script** | 전체 번들 + 카탈로그를 새로 생성 |
| **Update a Previous Build** | 변경된 에셋만 새 번들로 추가, 카탈로그(.hash/.bin) 갱신 |
| **Clean Build** | 기존 번들/카탈로그 삭제 후 처음부터 다시 생성 |

> 빌드 옵션 상세는 [`../Assets/Project/2_Tutorials/Addressables/SETUP.md`](../Assets/Project/2_Tutorials/Addressables/SETUP.md) 참고.

---

## 서버(XAMPP)로 배포하기

이 폴더는 "빌드 결과물"일 뿐, 게임이 실제로 다운로드하는 곳은 **XAMPP 웹루트**입니다.
빌드 후 아래 배치파일을 실행해 복사(미러)하세요.

```
Assets/Project/2_Tutorials/Addressables/copy_to_xampp.bat  (더블클릭)
   → C:\xampp\htdocs\Addressables\StandaloneWindows64\ 로 동기화
```

배포 후 브라우저에서 `http://localhost/Addressables/StandaloneWindows64/catalog_0.1.hash` 가
열리면(= Apache 실행 중) 정상입니다.
