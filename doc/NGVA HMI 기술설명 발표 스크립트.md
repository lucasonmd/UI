# NGVA HMI 기술설명 — 발표 스크립트

대상 자료: `NGVA HMI 기술설명.pptx` (10페이지)
예상 시간: 약 20분 + 질의응답
대상: 시스템을 처음 넘겨받는 SW 개발자 · 기술 담당자

> 표기
> - **굵은 글씨**: 반드시 짚고 넘어갈 핵심
> - `[ ]`: 발표자가 실제 내용으로 채울 부분 (코드 · 캡처 · 함수 등)
> - ▶ 전환: 다음 페이지로 넘어가는 멘트

---

## 발표 전 준비

슬라이드에 비어 있는 자리가 있으니 발표 전에 채우거나, 채우지 못했다면 말로 대신 설명할 내용을 정해 둡니다.

| 페이지 | 채울 자리 |
|---|---|
| 4 | [실제 Registry 관련 코드 삽입 영역] — Resource 등록 및 ResourceID 획득 코드 |
| 5 또는 9 | CrewRole별 JSON 예시 |
| 8 | [Application 화면 캡처 삽입 영역], [실제 Callback 코드 삽입 영역] |
| 9 | HMIJsonGenerator 화면 캡처, 주요 기능 |
| 10 | Application → Core 제어 함수 표 (함수명 / 매개변수 / 역할 / 설명) |

---

## 1. GVA HMI UI 영역 구성 (약 2분)

오늘은 우리 HMI가 어떤 구조로 만들어져 있고 어떻게 동작하는지를 설명드리겠습니다. 구조 이야기부터 하면 추상적이니까, 먼저 눈에 보이는 화면부터 시작하겠습니다.

왼쪽 그림은 UK GVA 표준, Def Stan 23-09 기준의 HMI 화면 배치입니다. 한 화면이 크게 두 영역으로 나뉩니다. 번호 1번부터 4번까지는 **HMI Core가 관리하는 공통 영역**이고, 가운데 주황색으로 표시한 5번이 **각 Application이 그리는 Content 영역**입니다.

하나씩 보겠습니다.

- **1번, Top Level Menu Buttons 8개**입니다. SA, WPN, DEF, SYS, DRV, STR, COM, BMS — 기능 영역을 고르는 버튼입니다. 여기서 중요한 점은 **기능 영역 1개에 HMI Application 1개가 1:1로 대응**한다는 것입니다. 그래서 뒤에서 Application이 8개라고 말씀드리는데, 이 8개와 같은 숫자입니다.
- **2번, Status Information Bar**입니다. 시간, 위치, 경보 같은 공통 상태 정보를 보여줍니다. 우리 시스템에서는 StatusBar라고 부릅니다.
- **3번, 좌우 F1–F6, F7–F12**입니다. 좌우 6개씩, 총 12개이고 현재 메뉴에 따라 라벨과 기능이 바뀝니다. 우리 시스템에서는 **SoftButton**이라고 부릅니다.
- **4번, 하단 F13–F20**입니다. 어떤 화면이든 항상 같은 공통 기능 — 위로 이동, 알람, 확인, 엔터 같은 것들입니다.

참고로 F1–F20과 상단 8개 버튼은 **화면 테두리, 즉 베젤에 달린 물리 버튼**입니다. 화면에 보이는 건 그 버튼의 라벨입니다.

정리하면, **하나의 HMI 화면은 Core 영역과 Application 영역으로 나뉜다** — 오늘 발표 전체가 이 구분을 기준으로 진행됩니다.

▶ 전환: 그럼 이 화면이 어떤 표준 위에서 동작하는지, NGVA부터 보겠습니다.

---

## 2. NGVA 기반 HMI 시스템 구성 (약 2분 30초)

NGVA는 차량 기능을 **모듈 단위로 정의한 데이터 모델**입니다. 그리고 각 모듈의 데이터는 **DDS 위에서 NGVA Topic이라는 단위로 주고받습니다.** DDS는 발행(Publish)과 구독(Subscribe) 방식의 통신 미들웨어라고 보시면 됩니다.

왼쪽 박스가 NGVA Reference Data Model v1.1에 있는 모듈 21개입니다. 이해하기 쉽게 플랫폼, HMI, 항법, 센서, 무장 등으로 묶어 놨는데, 이 그룹 구분은 설명을 위해 저희가 정리한 것이고 규격상 분류는 아닙니다.

이 중에 **우리 시스템이 사용하는 건 주황색 두 개**입니다.

- **Vehicle Configuration** — 차량 구성과 기동을 지원하는 모듈이고, 여기 있는 **Registry Service**를 사용합니다. 차량 안의 모든 Resource에 ResourceID를 나눠주는 역할입니다. 4페이지에서 자세히 봅니다.
- **HMI Presentation** — HMI 화면 표현에 필요한 구조를 정의한 모듈입니다. 여기서 **Menu, SoftButton, CrewRole** 개념을 가져다 씁니다. 5페이지에서 봅니다.

오른쪽 아래 점선 박스의 **Login Service**는 NGVA 모듈 목록에는 없습니다. NGVA의 Crew_Role은 로그인 정보를 담지 않도록 정의되어 있어서, 로그인은 별도 서비스가 담당합니다. 개발 환경에서는 HMISimulator가 제공합니다.

아래쪽에 Topic 이름 예시도 넣어 뒀습니다. `Vehicle_Configuration__Registry_Service`, `HMI_Presentation__Soft_Button` — 이렇게 **모듈 이름과 클래스 이름으로 Topic 이름이 정해집니다.**

▶ 전환: 그런데 우리가 이 NGVA를 직접 다 다뤄야 하느냐 — 그렇지 않습니다. 다음 페이지가 그 이야기입니다.

---

## 3. 소프트웨어 인터페이스 구조 (약 2분 30초)

이 페이지가 개발하실 때 가장 중요한 그림입니다.

인터페이스는 **내부와 외부 두 개로 나뉩니다.**

**왼쪽, 내부 인터페이스**입니다. HMI Core와 HMI Application 8개가 **Topic Manager**와 연결됩니다. 이 Topic Manager는 **각 프로세스에 링크되는 라이브러리**입니다. Core와 Application은 Topic Manager의 **함수를 호출해서 명령을 보내고, 결과나 이벤트는 Callback으로 받습니다.**

**오른쪽, 외부 인터페이스**입니다. 외부 체계 쪽에도 Topic Manager가 있고, CC(Central Computer), FCC(Fire Control Computer) 같은 외부 체계와 연결됩니다. 가운데 화살표처럼 **양쪽 Topic Manager 사이는 NGVA Topic, 즉 DDS로 통신합니다.**

Topic Manager가 하는 일은 가운데 적어 둔 세 가지입니다.

- NGVA 모듈의 PIM 구조에 따른 **상호참조 관계 검사**
- Resource와 Topic의 **생명주기 관리**
- NGVA Topic **발행과 구독**

결론은 아래 한 줄입니다. **HMI Core와 Application은 NGVA를 직접 다루지 않습니다.** 명령은 함수 호출, 이벤트는 Callback — 이것만 알면 되고, NGVA 모듈의 내부 구조까지 깊이 파악하실 필요는 없습니다.

▶ 전환: 그래도 NGVA에서 한 가지는 알아 두셔야 하는데, 각 프로그램을 어떻게 구분하느냐, Registry입니다.

---

## 4. Registry Service 기반 Resource 식별 (약 2분)

NGVA에서는 통신에 참여하는 단위를 **Resource**라고 부릅니다. 우리 시스템에서는 **HMI Core 1개, 그리고 Application 각각이 하나의 Resource**입니다.

각 Resource는 기동하면서 **Registry Service에 등록**하고, Registry Service는 **ResourceID를 할당**해 줍니다. 가운데 시퀀스가 그 흐름입니다 — Core가 등록하면 ResourceID를 받고, Application도 똑같이 등록해서 ResourceID를 받습니다.

이렇게 받은 ResourceID가 이후 NGVA 통신에서 **"이 메시지가 Core 것인지, 어느 Application 것인지"를 구분하는 식별자**가 됩니다.

가운데 아래 참고 박스에는 NGVA 규격에 정의된 이름을 적어 뒀습니다. 다만 시퀀스는 개념도이고, 실제 사용하는 메시지는 오른쪽 코드를 기준으로 보시면 됩니다.

`[코드 설명: Resource 등록 호출 위치, ResourceID를 받아 저장하는 부분]`

▶ 전환: 이제 이렇게 식별된 Core와 Application이 어떻게 역할을 나누는지 보겠습니다.

---

## 5. HMI Core – Application 구조 (약 3분)

가운데 큰 박스가 **HMI Core 1개**, 위아래로 **HMI Application 8개**입니다. 앞에서 말씀드린 대로 Application 8개는 상단 기능 영역 8개와 1:1입니다. 그리고 **Application은 전부 독립 프로세스**로 돌아갑니다.

**HMI Core의 역할**부터 보겠습니다.

- 로그인 계정에 따라 **필요한 Application만 골라서 실행**합니다. 이때 실행 인자로 **CrewRole을 넘깁니다.** 점선으로 그린 4번, 8번이 "이 계정에서는 실행되지 않는 Application"의 예시입니다.
- Application을 Show / Hide 하고, 메뉴 트리에 따라 페이지를 전환합니다.
- 좌우 SoftButton을 관리합니다.
- Application이 보내준 Menu와 SoftButton 정보를 저장하고, 지금 활성화된 구성을 관리합니다.

여기서 용어 하나 정리하겠습니다. **Menu가 곧 Page입니다.** NGVA HMI Presentation의 C_Menu Topic을 사용하기 때문에 Menu라는 이름을 쓰는 것이고, 화면 입장에서는 페이지라고 생각하시면 됩니다.

**Application의 역할**은 오른쪽 위입니다. 자기 Menu 정보와 SoftButton 정보를 만들어서 Core에 보내고, Core가 넘겨주는 사용자 입력과 메뉴 활성화 정보를 처리합니다.

오른쪽 아래가 **CrewRole 흐름**입니다. CrewRole은 **Mission과 Operation 두 가지**입니다.

1. Core가 Application을 실행할 때 CrewRole을 넘기고
2. Application은 그 CrewRole에 맞는 JSON 설정 파일을 읽어서
3. Menu 구성과 SoftButton 구성을 만들고
4. NGVA Topic으로 Core에 전달합니다.

그래서 **같은 Application이라도 CrewRole이 다르면 화면의 Menu와 SoftButton이 달라집니다.**

`[JSON 예시를 보여줄 경우: 파일 구조, Menu 항목, SoftButton 항목 설명]`

▶ 전환: 지금까지 나온 걸 시간 순서로 한 번에 이어 보겠습니다.

---

## 6. HMI 초기화 및 운용 Sequence (약 3분)

전체 흐름을 다섯 구간으로 나눴습니다. 개발·시험 환경에서는 실제 센트럴컴퓨터 대신 **HMISimulator**가 Registry Service와 Login Service를 제공합니다. 회색 점선 영역이 HMISimulator가 맡는 부분입니다.

**환경 준비** — 1, 2번. HMISimulator를 실행하면 Registry Service와 Login Service가 준비됩니다.

**HMI Core 기동** — 3번부터 7번.
사용자가 HMISimulator에서 "HMI Core 시작" 버튼을 누르면 Core가 실행됩니다. Core는 먼저 Registry Service에 등록해서 ResourceID를 받고, Login Service에서 로그인 계정을 확인합니다. 그리고 계정과 CrewRole에 따라 어떤 Application을 띄울지 결정합니다.

**Application 기동** — 8, 9번.
Core가 Application을 실행하면서 **CrewRole을 실행 인자로 넘기고**, Application도 Registry Service에 등록해서 ResourceID를 받습니다.

**UI 구성** — 10번부터 14번.
Application은 CrewRole에 맞는 JSON을 읽고 Menu 정보와 SoftButton 정보를 만들어서, **NGVA Topic으로 Core에 보냅니다.** Core는 이걸 저장하고 화면에 반영합니다.

**운용** — 15번.
이후에는 사용자가 SoftButton이나 메뉴를 누를 때마다 Core와 Application 사이에 이벤트가 오갑니다.

아래 한 줄로 요약하면 — **로그인, Application 선택, CrewRole 기반 UI 구성, 그리고 Core와 Application 간 NGVA 통신**입니다.

▶ 전환: 이제 다시 화면으로 돌아가서, Core가 맡는 영역을 실제 화면으로 보겠습니다.

---

## 7. HMI Core 공통 UI 영역 (약 2분)

왼쪽은 저희가 개발한 **HMI Core 예제를 실제로 실행한 화면**입니다. 1920×1080입니다. 가운데 회색으로 가린 부분이 Application 영역이고, 주황색 테두리가 전부 Core 영역입니다.

- 1번 StatusBar — 시간, 방위각, 좌표, 경보. **Application과 상관없이 Core가 직접 갱신**합니다.
- 2번, 3번 — 좌측 F1–F6, 우측 F7–F12 SoftButton.
- 4번 — 하단 F13–F20 공통 기능 버튼.
- 5번 — 기능 영역 라벨 8개. 지금 어떤 기능 영역이 선택됐는지 보여줍니다.

화면 아래 적어 둔 것처럼, F1 DRIVE는 선택(Selected) 상태이고 F5, F9, F12는 비활성(Disabled) 상태 예시입니다. SoftButton 상태는 **Selected, Enabled, Disabled, Hidden** 네 가지입니다.

오른쪽 아래 그림이 SoftButton이 채워지는 흐름입니다. **Application이 SoftButton 정보를 만들어 보내면, Core가 받아서 좌우 12개 버튼에 반영합니다.** 버튼을 그리는 건 Core, 무엇을 넣을지 정하는 건 Application입니다.

한 줄로 — **화면에서 가운데 Application 영역을 뺀 나머지는 전부 Core 영역입니다.**

▶ 전환: 그럼 반대로 Application 쪽은 어떻게 동작하는지 보겠습니다.

---

## 8. Application 화면 및 사용자 입력 처리 (약 2분)

Application은 **가운데 Content 영역만 사용합니다.** 크기는 1590×915이고, Core 화면 위에 **별도 프로세스의 창으로 겹쳐서** 표시됩니다.

`[Application 화면 캡처 설명]`

사용자 입력이 Application까지 오는 경로는 두 가지입니다.

**첫째, SoftButton을 누른 경우.**
사용자 → HMI Core → NGVA Topic → HMI Application 순서로 전달되고, Application에서는 **OnSoftButtonProcess** Callback이 호출됩니다. 여기서 어떤 버튼이 눌렸는지 확인하고 해당 기능을 수행합니다.

**둘째, 메뉴, 즉 페이지를 전환한 경우.**
같은 경로로 전달되고 **OnActiveMenu** Callback이 호출됩니다. 지금 어떤 Menu, 어떤 Page가 활성화됐는지 확인하고 필요한 처리를 합니다.

`[Callback 코드 설명: 두 함수의 매개변수, 버튼 식별 방법, 메뉴 식별 방법]`

정리하면 — **Core는 공통 UI와 사용자 입력을 관리하고, Application은 전달받은 이벤트로 실제 기능을 수행합니다.**

▶ 전환: 여기까지가 구조 설명이고, 이어서 개발할 때 참고하실 자료 두 가지를 보여드리겠습니다.

---

## 9. HMIJsonGenerator (약 1분)

앞에서 Application이 CrewRole별 JSON을 읽어서 Menu와 SoftButton을 만든다고 말씀드렸는데, 그 **JSON을 UI로 편하게 작성하는 도구가 HMIJsonGenerator**입니다.

오른쪽 아래 흐름처럼, HMIJsonGenerator로 만든 JSON을 Application이 읽고, 그 결과가 Core 화면에 반영됩니다.

`[도구 화면 설명]`
`[주요 기능 설명]`

▶ 전환: 마지막으로 Application이 Core를 제어할 때 쓰는 함수 목록입니다.

---

## 10. Application → Core 제어 함수 (약 1분 30초)

Application이 Core에 뭔가를 요청할 때는 **함수를 호출**합니다. 이 표의 함수들은 **모두 Application에서 Core 방향**입니다. 반대로 Core에서 Application으로 오는 건 8페이지에서 본 **Callback**으로 처리됩니다.

`[함수별 설명: 함수명, 매개변수와 각 역할, 설명]`
`[자주 쓰는 함수 두세 개는 사용 예를 곁들여 설명]`

정리하면 — **Application → Core는 함수 호출, Core → Application은 Callback.** 이 두 방향만 기억하시면 됩니다.

---

## 마무리 (약 30초)

오늘 내용을 세 줄로 정리하겠습니다.

1. **화면은 Core 영역과 Application 영역으로 나뉩니다.** 기능 영역 8개와 Application 8개는 1:1입니다.
2. **NGVA 관련 처리는 Topic Manager가 맡습니다.** 우리는 함수 호출과 Callback만 다루면 됩니다.
3. **Application은 CrewRole에 맞는 JSON으로 Menu와 SoftButton을 만들어 Core에 보내고,** Core는 그걸 화면에 그리고 입력을 돌려줍니다.

질문 받겠습니다.

---

## 예상 질문

| 질문 | 답변 |
|---|---|
| 상단 기능 영역 8개와 Application 8개는 같은 건가요? | 1:1로 대응합니다. 기능 영역 하나에 Application 하나입니다. |
| Topic Manager는 별도 프로세스인가요? | 아닙니다. Core와 Application 각 프로세스에 링크되는 라이브러리입니다. |
| Menu와 Page는 다른 건가요? | 같은 것입니다. NGVA HMI Presentation의 C_Menu Topic을 사용해서 Menu라고 부릅니다. |
| CrewRole은 몇 종류인가요? | Mission과 Operation 두 가지입니다. |
| Application이 Core를 제어할 때는요? | 10페이지의 함수를 호출합니다. 전부 Application → Core 방향입니다. |
| Core가 Application에 알려줄 때는요? | Callback입니다. SoftButton 입력은 OnSoftButtonProcess, 메뉴 전환은 OnActiveMenu입니다. |
| NGVA 규격을 다 알아야 하나요? | 아닙니다. 상호참조 검사, 생명주기 관리, 발행·구독은 Topic Manager가 처리합니다. 사용하는 모듈이 Vehicle Configuration과 HMI Presentation이라는 정도만 알면 됩니다. |
| 실제 장비 없이 어떻게 시험하나요? | HMISimulator가 센트럴컴퓨터 대신 Registry Service와 Login Service를 제공합니다. |
| Menu나 SoftButton 구성을 바꾸려면요? | CrewRole별 JSON을 수정합니다. HMIJsonGenerator로 UI에서 작성할 수 있습니다. |
