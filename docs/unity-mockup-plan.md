# Unity mockup 환경 구성안

## 현재 확인한 범위

- 기존 저장소: CapstoneDesign2026. 시작 시 README만 존재했다.
- 작업 브랜치: mockup. 원격 push와 앱 구현은 아직 수행하지 않는다.
- 받은 계획: Proxmox LXC / Unity / Android / Visual CI handoff.
  사용자가 2026-09-12 같은 문서를 다시 전달해 작업 기준임을 확인했다.
- 첫 mockup 범위는 handoff 23번의 sky / floating island / tree / wind shader /
  directional light / camera / rigidbody sphere / sensor debug overlay다.
- 센서는 mock 데이터로 시작한다. 첫 목표는 제품 완성보다 Scene 수정부터
  preview / APK / A7 artifact 회수까지 자동화 루프를 검증하는 것이다.
- 별도 앱 기획서 없이 이 vertical slice의 환경 구상을 진행할 수 있다.
  상세 화면 흐름, 센서 규격, 최종 아트 방향은 이후 제품 설계에서 구체화한다.

## 설치 환경과 실행 조건

- LXC 777, Ubuntu 24.04, 6 vCPU / RAM 12GB / swap 4GB / rootfs 100GB.
- 내부 10.0.0.77/24, 외부 vmbr0 DHCP.
- 설치 Editor: 6000.3.24f1, changeset 4e7b9b5b6244.
- 라이선스 조회: Unity Pro (ULF), 활성 상태. 계정/시리얼은 저장하지 않는다.
- Android SDK Build Tools 36.0.0, NDK r27c, OpenJDK 17.0.18.
- /opt/unity/current/Editor/Unity 및 unity-editor 래퍼 사용.
- 일반 개발/빌드 사용자: codex. 현재 checkout은 /root/CapstoneDesign2026에
  있어 codex가 접근할 수 없다. 구현 시작 전 기존 변경사항을 보존하며
  /workspace/CapstoneDesign2026에 작업 checkout을 준비해야 한다.
  /root 권한을 넓히거나 Unity를 root로 실행하는 방식은 사용하지 않는다.
- 결과 경로: /artifacts/{build,unity,visual,a7}; Gradle 캐시 /cache/gradle.
- Intel UHD 630 Vulkan 접근은 검증됐다. 실제 프로젝트 APK/preview 검증은
  프로젝트가 구성된 뒤 별도로 수행한다.

## 프로젝트 구조 제안

기존 Git 저장소의 루트를 Unity 프로젝트 루트로 사용한다. 중첩 프로젝트나
별도 Git 저장소는 만들지 않는다. Editor가 실제 초기화할 때 생성한 버전,
패키지 manifest/lock 및 .meta를 추적한다. 존재하지 않는 패키지 버전이나
Scene YAML을 수작업으로 만들어 초기화하지 않는다.

```text
Assets/
  _Project/
    Scenes/             Bootstrap, Mockup, Tests/VisualSmoke
    Scripts/
      Runtime/          앱 상태, 입력 인터페이스, mock 데이터
      UI/               센서 디버그 표시와 화면 상태
    Art/                Materials, Shaders, Textures, Models
    Prefabs/
    Settings/           URP 및 입력 설정
    Tests/              EditMode, PlayMode
  Editor/Automation/    빌드, 씬 구성, preview 캡처
Packages/
ProjectSettings/
scripts/                독립 실행 가능한 CLI 래퍼
docs/
```

## 최소 패키지와 씬 설계

1. 설치 Editor에서 호환되는 Universal 3D 템플릿/URP 구성을 선택한다.
   선택된 manifest와 packages-lock.json을 커밋한다.
2. Unity Test Framework를 설정한다. 입력 패키지는 실제 상호작용 요구를
   확인한 뒤 추가한다. 초기 단계에 Addressables 등은 강제로 도입하지 않는다.
3. 앱 입력은 인터페이스로 분리하고 mock provider를 먼저 연결한다.
   실제 센서, Android native plugin, 권한은 이후 구현 범위로 둔다.
4. 첫 VisualSmoke 씬: 하늘/섬/나무/바람 재질/조명/카메라/구체 물리와
   센서 디버그 overlay. 카메라, 시드, 캡처 시점을 고정한다.
5. 씬/Prefab 구성은 Editor API automation으로 생성하고 검증한다.
   생성된 .meta와 serialized asset은 저장소에 포함한다.
6. Android 패키지 ID, 화면 방향, 최소 OS와 최종 UI 구성을 확정한 뒤
   Player Settings를 설정한다. 최초 빌드는 개발용 APK/디버그 서명으로 한다.

## 구현 단계와 각 단계의 완료 기준

1. Phase 1 — Fast / Android: 기존 저장소에 Unity 구성을 준비하고
   import/compile → EditMode → 가능한 PlayMode tests → 개발용 APK를 검증한다.
   GPU가 필요 없는 작업에만 -batchmode -nographics 사용.
   build-android.sh는 Editor executeMethod를 호출하고 종료코드와 실제 APK를
   검사한다. 설치/인증 완료와 실제 프로젝트 빌드 성공은 별도로 기록한다.
2. Phase 2 — Visual: Xvfb DISPLAY=:99, -batchmode -force-vulkan 사용.
   -nographics는 사용하지 않는다. Unity 로그에서 Intel GPU 사용을 확인하고
   Camera → RenderTexture → PNG를 /artifacts/visual에 생성한다.
   Xvfb의 GLX llvmpipe 출력은 Unity Vulkan 성공 증거가 아니다.
3. Phase 3 — A7: 기기 확인/권한 승인 후 install → 실행 → screenshot/logcat
   수집. 미연결 상태는 별도 skipped로 기록하고 Fast를 막지 않는다.
   수동 기기 승인 전에는 준비된 스크립트와 offline 처리까지만 검증한다.
4. Phase 4 — Test Mode: Development Build 전용 Android Intent 진입점과
   반복 가능한 테스트 상태, screenrecord, metadata.json을 구현한다.
5. Phase 5 — CI: 검증된 독립 스크립트를 호출하는 trigger와 Codex 검토 루프를
   연결한다. 기존 CI가 없다면 제품/연동 권한을 확인한 뒤 선택한다.

각 단계가 실제 성공한 뒤 다음 단계로 진행한다. iGPU FPS는 Android 성능
지표로 사용하지 않는다. Unity 6 splash 설정은 프로젝트에서 관리한다.

## Git 관리

- 추적: Assets와 .meta, Packages/manifest.json, packages-lock.json,
  ProjectSettings, scripts, docs.
- 제외: Library/Temp/Obj/Logs/UserSettings, 빌드/기기 캡처/캐시,
  IDE 생성 파일, 로컬 인증정보, Android keystore.
- .gitattributes로 Unity 텍스트 자산과 스크립트 줄바꿈을 정규화한다.
- Git LFS 대상은 실제 대형 에셋 도입 시 확정한다. 인증정보와 시리얼은
  gitignore에만 의존하지 않고 애초에 프로젝트 파일에 작성하지 않는다.
- 현재 단계에서는 Unity 프로젝트나 앱 코드, Scene을 생성하지 않았다.

## 실행 스크립트 계약

- unity-compile.sh: 프로젝트 import/compile 결과와 Editor log.
- unity-test.sh: EditMode/PlayMode 선택, 실패 종료코드, XML 결과.
- build-android.sh: Development APK와 빌드 로그. 키/시리얼을 인수에 내장하지 않음.
- visual-preview.sh: Intel Vulkan 확인과 Unity 내부 카메라 PNG.
- test-a7.sh: 명시된 SERIAL과 APK/패키지 대상으로 배포/실행/수집.
  기기가 없으면 성공으로 위장하지 않고 skipped 상태를 기록.
- 각 실행의 artifacts는 commit 또는 실행 ID별로 분리한다. 동일 프로젝트에서
  Unity Editor가 동시에 실행되지 않도록 잠금을 사용한다.

## 이후 구체화할 제품 설정

- 첫 mockup 이후의 화면/기능 설계와 시각 레퍼런스
- Android application identifier, 화면 방향, A7 모델/OS
- 센서 mock 데이터 항목과 초기 사용자 흐름

기반 자료: 사용자 제공 handoff 및 CT 777의 실제 설치 상태.
