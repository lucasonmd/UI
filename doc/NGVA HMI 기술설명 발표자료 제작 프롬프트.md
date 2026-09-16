NGVA 기반 HMI 시스템의 구조와 동작 방식을 설명하는 **기술설명용 PowerPoint 발표자료**를 제작해줘.

대상은 해당 시스템을 처음 전달받아 구조와 동작 원리를 이해해야 하는 **SW 개발자 및 기술 담당자**다. 단순 개념 소개가 아니라, 실제 시스템이 어떤 NGVA 모듈을 사용하고 HMI Core와 HMI Application이 어떻게 동작하는지 한눈에 이해할 수 있어야 한다.

전체 분량은 **7페이지**로 구성한다.

전체 디자인은 화려한 홍보자료가 아니라 **방산/임베디드 SW 기술설명 자료**처럼 깔끔하고 전문적으로 구성한다.

텍스트만 나열하지 말고 아키텍처 다이어그램, 모듈 블록, 시퀀스 다이어그램, UI 영역 구분 등을 적극적으로 사용한다.

다만 지나치게 AI가 만든 자료처럼 보이는 카드형 레이아웃, 과도한 그라데이션, 둥근 사각형 남용, 큰 아이콘, 모든 텍스트의 Bold 처리는 피한다.

기본 배경은 흰색 또는 아주 옅은 회색을 사용하고, 진한 남색/회색을 기본 색상으로 사용한다. 이번 시스템에서 실제 사용하는 NGVA 모듈이나 현재 설명하고 있는 요소만 포인트 컬러로 강조한다.

폰트 굵기는 제목만 Semi-Bold 정도를 사용하고 본문은 Regular 위주로 구성한다.

**중요:**
실제 C# 코드, 실제 GVA HMI 화면 캡처, 실제 시스템 화면은 내가 나중에 직접 삽입할 예정이다.
따라서 내용을 임의로 만들어 넣지 말고 아래와 같이 명확한 Placeholder 영역을 만들어둔다.

- [실제 코드 삽입 영역]
- [실제 GVA HMI 화면 삽입 영역]
- [실제 HMI Core 화면 삽입 영역]
- [Application 화면 캡처 삽입 영역]

Placeholder가 너무 크게 비어 보이지 않도록 제목, 설명, 캡션 영역까지 레이아웃에 포함한다.

---

### 1페이지 — NGVA 전체 구성과 본 시스템에서 사용하는 모듈

제목 예시:
**NGVA 기반 HMI 시스템 구성**

NGVA에서 제공하는 주요 기능/서비스들을 여러 개의 **사각형 모듈 블록**으로 표현한다.

전체 NGVA 환경 안에서 여러 모듈이 존재하고 그중 현재 HMI 시스템에서 사용하는 모듈이 무엇인지 보여주는 것이 목적이다.

모든 모듈을 동일한 회색 계열의 박스로 표현하고, 현재 시스템에서 사용하는 모듈만 포인트 컬러로 강조한다.

특히 이후 설명과 연결되는 다음 요소들이 시각적으로 드러나도록 한다.

- Registry Service
- Vehicle Configuration 관련 기능
- HMI Presentation 관련 기능
- Login 관련 기능

이 페이지에서는 세부 동작을 설명하지 말고,
**“전체 NGVA 구조 중 이번 HMI 시스템이 어떤 부분을 사용하는가”**를 한눈에 보여준다.

하단에는 다음과 같은 메시지를 짧게 표시한다.

“GVA HMI는 NGVA의 Registry 및 HMI 관련 서비스를 기반으로 Core와 Application 간 기능을 구성한다.”

2페이지와 3페이지에서 설명할  
**Vehicle Configuration → HMI Presentation** 흐름이 자연스럽게 이어지도록 시각적 연결감을 만들어준다.

---

### 2페이지 — Registry Service 및 Resource 등록 구조

제목 예시:
**Registry Service 기반 Resource 식별**

이 페이지에서는 **Vehicle Configuration 모듈을 사용하는 영역**임을 상단 또는 우측에 작은 태그 형태로 표시한다.

Registry Service의 목적과 Core/Application이 Resource로 등록되는 구조를 설명한다.

핵심 내용:

HMI Core와 각 HMI Application은 모두 각각 독립된 Resource로 동작한다.

각 Resource는 Registry Service에 등록되어야 하며, Registry Service는 등록된 Resource에 대해 **ResourceID를 할당한다.**

이 ResourceID는 이후 NGVA 통신에서 HMI Core 또는 특정 Application을 식별하는 Descriptor 역할을 한다.

화면은 크게 세 영역으로 구성한다.

왼쪽:
간단한 개념 구조도

Registry Service를 중심에 배치하고

HMI Core  
Application A  
Application B  
Application C  
…

형태의 Resource들이 Registry Service에 등록되는 구조를 표현한다.

가운데 또는 하단:
간단한 시퀀스 다이어그램을 배치한다.

예시 흐름:

HMI Core → Registry Service : Resource Registration  
Registry Service → HMI Core : ResourceID 할당

HMI Application → Registry Service : Resource Registration  
Registry Service → HMI Application : ResourceID 할당

이 시퀀스는 개념을 설명하기 위한 것으로 지나치게 세부적인 NGVA 메시지 명칭을 임의로 만들지 않는다.

오른쪽:
**[실제 Registry 관련 코드 삽입 영역]**

코드 영역에는 실제 코드 대신 Placeholder만 만들고,
아래에 작은 캡션으로

“Resource 등록 및 ResourceID 획득 코드”

라고 표시한다.

---

### 3페이지 — HMI Core와 HMI Application 구조

제목 예시:
**HMI Core – Application 구조**

이 페이지에서는 **HMI Presentation 모듈을 사용하는 영역**임을 명확히 표시한다.

전체 구조를 가장 중요한 메인 아키텍처 페이지처럼 구성한다.

중앙에 HMI Core를 크게 배치하고 주변에 여러 HMI Application을 배치한다.

현재 시스템에서는 총 **8개의 Application**이 존재한다.

Application은 각각 **독립 프로세스**로 실행된다.

HMI Core의 주요 역할:

- Application 실행 및 Show / Hide 관리
- 메뉴 트리 기반 페이지 전환 관리
- 좌/우측 SoftButton 관리
- Application이 전달한 Menu / SoftButton 정보를 동적으로 저장
- 현재 활성화된 메뉴와 버튼 구성 관리

Application의 주요 역할:

- 자신의 Menu 정보 생성
- 자신의 SoftButton 정보 생성
- 해당 정보를 NGVA Topic을 통해 Core에 전달
- Core로부터 전달된 사용자 입력 및 메뉴 활성화 정보 처리

Core는 로그인 계정에 따라 8개 Application 중 필요한 Application을 선택하여 실행한다.

Application 실행 시 **CrewRole을 실행 인자로 전달한다.**

각 Application은 전달받은 CrewRole에 따라 역할별 JSON 설정 파일을 읽는다.

예:

CrewRole
↓
Role별 JSON
↓
Menu 구성
+
SoftButton 구성
↓
HMI Core 전달

위 흐름은 별도의 작은 서브 다이어그램으로 표현한다.

각 Application의 Menu / SoftButton 구성은 CrewRole에 따라 달라진다는 점을 시각적으로 강조한다.

---

### 4페이지 — 시스템 전체 동작 Sequence

제목 예시:
**HMI 초기화 및 운용 Sequence**

지금까지 설명한 내용을 하나의 전체 시퀀스 다이어그램으로 연결한다.

등장 객체:

HMISimulator  
Registry Service  
Login Service  
HMI Core  
HMI Application  
User

HMISimulator는 실제 센트럴컴퓨터를 대신하여 **Registry Service와 Login Service를 제공하는 개발/시험용 환경**임을 표시한다.

전체 흐름은 다음 순서로 구성한다.

1. HMISimulator 실행

2. Registry Service / Login Service 준비

3. 사용자가 HMISimulator의
“HMI Core 시작”
버튼 선택

4. HMI Core 실행

5. Core가 Registry Service에 Resource 등록

6. 로그인 계정 확인

7. 로그인 계정 및 CrewRole에 따라 실행할 Application 결정

8. 필요한 Application 실행
   - CrewRole을 실행 인자로 전달

9. Application이 Registry Service에 Resource 등록

10. Application이 CrewRole에 맞는 JSON 파일 로드

11. Menu 정보 생성

12. SoftButton 정보 생성

13. Menu / SoftButton 정보를 NGVA Topic을 통해 Core로 전달

14. Core가 정보를 저장하고 화면 구성에 반영

15. 이후 사용자 입력에 따라 Core ↔ Application 간 이벤트 전달

Sequence Diagram이 페이지의 핵심이 되도록 화면의 60~70% 정도를 사용한다.

아래에는 아주 짧은 요약 문구를 배치한다.

“로그인 → Application 선택 → CrewRole 기반 UI 구성 → Core/Application 간 NGVA 통신”

---

### 5페이지 — GVA HMI UI 구조

제목 예시:
**GVA HMI UI 영역 구성**

이 페이지부터 소프트웨어 구조에서 실제 UI 구조로 자연스럽게 넘어간다.

페이지 중심에는 큰 GVA HMI 화면 프레임을 배치한다.

실제 화면은 내가 나중에 삽입할 예정이므로

**[실제 GVA HMI 화면 삽입 영역]**

을 만들어둔다.

화면 위에 반투명 Overlay 또는 외곽 라인을 사용하여 크게 두 영역을 구분한다.

1. HMI Core가 관리하는 공통 UI 영역
2. 각 Application이 사용하는 Content 영역

좌측/우측 SoftButton 영역,
StatusBar,
중앙 Application 영역 등을 시각적으로 구분한다.

이 페이지에서는 아직 세부 기능을 많이 설명하지 말고,

“하나의 HMI 화면이 Core 영역과 Application 영역으로 분리되어 있다.”

라는 개념을 전달하는 데 집중한다.

6페이지와 7페이지에서 각각 두 영역을 확대 설명한다는 흐름이 느껴지게 구성한다.

---

### 6페이지 — HMI Core 관리 영역

제목 예시:
**HMI Core 공통 UI 영역**

실제 사업에서 사용하는 **GVA HMI Core가 담당하는 화면 영역**을 설명한다.

왼쪽 또는 중앙에

**[실제 HMI Core 화면 삽입 영역]**

을 크게 배치한다.

화면 외곽 영역을 강조하고 중앙 Application Content 영역은 상대적으로 흐리게 표시한다.

Core가 관리하는 대표 영역:

- StatusBar
- 좌측 SoftButton 영역
- 우측 SoftButton 영역
- Application 외부의 공통 UI Framework

특히 좌측 6개 + 우측 6개,
총 **12개의 SoftButton**을 Core가 관리한다는 점을 시각적으로 표현한다.

Application이 SoftButton 정보를 전달하면,
Core는 해당 정보를 받아 현재 화면의 버튼 구성에 반영한다.

가능하면 간단한 데이터 흐름을 함께 표현한다.

Application  
→ SoftButton Information  
→ HMI Core  
→ Left / Right SoftButton UI

StatusBar와 같은 공통 UI는 Application 화면과 독립적으로 Core가 관리한다는 점도 표시한다.

복잡한 설명보다는 실제 화면에서 **“어디까지가 Core인가”**를 바로 이해할 수 있도록 구성한다.

---

### 7페이지 — Application 영역 및 Core ↔ Application Callback

제목 예시:
**Application 화면 및 사용자 입력 처리**

Application이 실제로 사용하는 중앙 Content 영역을 강조한다.

왼쪽에는

**[Application 화면 캡처 삽입 영역]**

을 배치하고,
Application이 사용하는 영역만 명확한 테두리 또는 Highlight로 표시한다.

오른쪽에는 Core와 Application 사이의 Callback/Event 구조를 설명한다.

사용자가 SoftButton을 누른 경우:

User  
→ HMI Core  
→ NGVA Topic  
→ HMI Application  
→ **OnSoftButtonProcess**

Application은 OnSoftButtonProcess Callback을 통해
**어떤 SoftButton이 눌렸는지 확인하고 해당 기능을 수행한다.**

사용자가 메뉴 또는 페이지를 전환한 경우:

Menu / Page 변경  
→ HMI Core  
→ NGVA Topic  
→ HMI Application  
→ **OnActiveMenu**

Application은 OnActiveMenu Callback을 통해
**현재 어떤 메뉴 또는 페이지가 활성화되었는지 확인하고 필요한 처리를 수행한다.**

두 Callback 함수는 코드처럼 보이도록 별도의 작은 기술 박스로 강조한다.

OnSoftButtonProcess(...)
“SoftButton 입력 처리”

OnActiveMenu(...)
“활성 Menu / Page 변경 처리”

단 실제 함수 내부 코드를 임의로 생성하지 않는다.

페이지 하단에

**[실제 Callback 코드 삽입 영역]**

을 만들어둔다.

마지막에는 Core와 Application의 역할을 한 문장으로 정리한다.

“HMI Core는 공통 UI와 사용자 입력을 관리하고, Application은 전달받은 Menu/Page/SoftButton Event를 기반으로 실제 기능을 수행한다.”

---

### 전체 슬라이드 연결 구조

전체 발표가 아래 흐름으로 자연스럽게 이어지게 구성한다.

NGVA 전체 구조  
→ Registry를 통한 Resource 식별  
→ Core / Application 구조  
→ 시스템 실행 Sequence  
→ 실제 HMI 화면  
→ Core 담당 UI  
→ Application 영역 및 Callback

각 슬라이드는 독립적으로 보이기보다는 이전 슬라이드의 내용을 확대해서 다음 내용을 설명하는 구조로 만든다.

특히 5 → 6 → 7페이지는 동일한 HMI 화면을 기준으로

전체 화면  
→ Core 영역 강조  
→ Application 영역 강조

방식으로 이어지게 디자인한다.

기술 용어는 다음 표기를 그대로 사용한다.

HMI Core  
HMI Application  
Registry Service  
Resource  
ResourceID  
CrewRole  
Menu  
SoftButton  
NGVA Topic  
HMI Presentation  
Vehicle Configuration  
OnSoftButtonProcess  
OnActiveMenu  
HMISimulator  
Login Service

임의로 용어를 번역하거나 다른 명칭으로 바꾸지 않는다.

전체적으로 “PPT용 요약문”처럼 내용을 지나치게 축약하지 말고,
개발자가 슬라이드만 보고도 시스템 구조를 어느 정도 이해할 수 있을 정도의 기술 정보를 유지한다.

다만 긴 문단을 그대로 넣지 말고,
**다이어그램 + 짧은 설명 + 핵심 키워드** 위주로 시각화한다.