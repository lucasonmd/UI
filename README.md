# GVA HMI — WPF (.NET 8), Core / Application 2-프로세스

`gva-hmi-core-spec.md` 규격에서 출발해 사용자 피드백으로 여러 차례 개편한 결과물.
1920 × 1080 논리 좌표로 그린 뒤 `Viewbox` 가 창 크기에 맞춰 균일 축소한다.

**현재 디자인 방향** : "premium embedded HMI" — 고가 차세대 차량/방산 장비의 임베디드
운영 콘솔. 네온 글로우·유리 효과·강한 그라데이션 없이, 짙은 차콜 메탈 톤 + 미세한
인셋 + 정돈된 분리선으로 신뢰감을 만든다. **Application(중앙 대시보드)은 재설계
대상이 아니다** — 카드·수치·메뉴 구조는 처음 그대로이고, Core UI(상단 바·좌우
레일·하단 바·외곽 프레임)만 개편했다.

**두 개의 독립된 프로세스다** — 실제 장비에서 Core(베젤 펌웨어급 UI)와
Application(교체 가능한 미션 소프트웨어)이 분리돼 있는 구조를 그대로 반영했다.
자세한 내용은 [`## 프로세스 분리`](#프로세스-분리-core--application) 참고.

**"기존 프로젝트에 UI 만 옮겨 붙인다"는 전제로 설계했다** — 이 저장소만의
Window 서브클래스·ViewModel·설정 파일에 종속되지 않는다. `CoreUiBuilder` 는
`Window` 하나(그 안의 `<Grid x:Name="gd_main"/>`)만 있으면 동작하고, 위치/라벨/
상태는 전부 "기본 형태"로 시작해서 명령형 메서드(`SetLabel`/`SetState`/
`SetLabelState`)로 외부에서 채운다. 자세한 내용은
[`## Core UI 붙이기 (CoreUiBuilder)`](#core-ui-붙이기-coreuibuilder) 참고.

## 실행

```
dotnet run --project src/GVA.HMI.Example.Core
```

Core 를 실행하면 Application 을 알아서 띄운다 — 따로 실행할 필요 없다(자세한
내용은 아래 프로세스 분리 참고). 창에 타이틀바가 없다(`WindowStyle="None"`,
베젤 내장형 HMI 가정).

| 입력 | 동작 |
|---|---|
| `Alt+Enter` / `Esc` | 창 최대화 토글 / 최대화 해제 |
| 빈 배경 드래그 | 창 이동 (타이틀바가 없어 대신 처리) |
| `Alt+F4` | 종료 (Application 프로세스도 같이 닫힌다) |

F1~F20/기능영역 라벨 클릭·키보드 단축키는 이 프로젝트가 등록하지 않는다 —
아래 [`## Core UI 붙이기`](#core-ui-붙이기-coreuibuilder) 참고. 지금 실행하면
`MainWindow.xaml.cs` 의 `ApplyPreviewContent()` 가 채워 넣은 데모 문구·상태만
보이고, 눌러도 아무 반응이 없는 게 정상이다.

## 구성

```
src/
├─ GVA.HMI.Example.Core/          베젤 UI (상단 바 + 좌우 레일 + 하단 바 + 프레임)
│  ├─ Controls/
│  │  ├─ CoreUiBuilder.cs 실제 UI 를 통째로 만들어서 호스트 Window 에 꽂는다
│  │  ├─ SoftKey.cs / SoftButtonState.cs      F1~F20
│  │  └─ TopLabel.cs / T_Button_State.cs      기능영역 라벨 8개
│  ├─ Themes/
│  │  ├─ GvaColors.xaml   색상·치수·폰트. 값은 여기서만 정의한다
│  │  └─ GvaStyles.xaml   소프트키 스타일 4종 + 전시 텍스트 스타일
│  │     (아이콘은 없다 — "유지보수가 안 될 것 같다" 피드백으로 전부 뺐다)
│  ├─ Models/StatusBarCustomData.cs   StatusBar 커스텀 전시정보 JSON 모델 + 전용 컨버터
│  ├─ Views/StatusBarView.xaml(.cs)   1줄:시간/방위각/커스텀정보, 2줄:좌표/알람/집계
│  ├─ ViewModels/
│  │  ├─ StatusBarViewModel.cs    SystemData/AlarmData/CustomItems + LoadCustomDisplayData
│  │  ├─ SystemDataModel.cs / AlarmDataModel.cs   기존 바인딩 경로 그대로(SystemData.*, AlarmData.*)
│  │  └─ CustomStatusItemViewModel.cs
│  └─ MainWindow.xaml(.cs)   <Grid x:Name="gd_main"/> 하나뿐인 껍데기 창.
│                              CoreUiBuilder 호출 + Application 프로세스 실행만 한다
│
└─ GVA.HMI.Example.Application/   중앙 대시보드 (재설계 대상 아님, 별도 프로세스)
   ├─ Themes/             Application 전용 팔레트/카드 스타일 (Core 와 완전히 독립)
   ├─ Interop/OwnerWindowSync.cs  Core 창 위에 정확히 겹쳐 앉는 로직
   ├─ Views/ApplicationAreaView.xaml   카드 2×2 레이아웃 (그대로)
   ├─ ViewModels/         예시 데이터. 실제 연동 시 ApplicationViewModel 만 교체
   └─ MainWindow.xaml     Viewbox 하나로 ApplicationAreaView 를 보여줄 뿐
```

## Core UI 붙이기 (CoreUiBuilder)

**"기존 프로젝트에 UI 만 바꿀 예정이라 종속적인 설계는 피해야 한다"** 는 요구를
그대로 반영한 설계다 - `CoreUiBuilder` 는 이 저장소의 `MainWindow` 를 몰라도
된다. 필요한 건 딱 하나, 호스트 `Window` 안에 이름이 `gd_main` 인 `Grid` 가
있다는 것뿐이다(기존 프로젝트가 이미 쓰던 이름을 그대로 따랐다):

```xml
<Window x:Class="...">
    <Grid x:Name="gd_main"/>
</Window>
```

```csharp
var ui = new CoreUiBuilder(window);   // gd_main 을 찾아서 그 안에 전부 만든다
```

**위치·크기는 "기본 형태"로 `CoreUiBuilder` 상단 상수에 고정돼 있다** — 예전엔
JSON 설정 파일로 뺐었지만, 라벨/상태는 결국 전부 외부(기존 프로젝트)가 그때그때
동적으로 채울 것이라 설정 파일을 따로 둘 이유가 없어서 코드 상수로 되돌렸다.
버튼 개수·간격을 바꾸고 싶으면 `CoreUiBuilder.cs` 상단 상수와 `Build*()` 호출의
개수 인자를 고치면 된다.

**기본 콘텐츠** :

| 대상 | 기본 라벨 | 기본 상태 |
|---|---|---|
| F1~F20 (`SoftKey`) | `"ngva.f1"` ~ `"ngva.f20"` | `SoftButtonEnabled` |
| 기능영역 라벨 8개 (`TopLabel`) | (텍스트 없음) | `LabelStateDisabled` |

**클릭/키보드 등 입력 이벤트는 여기서 등록하지 않는다** — 호출부가 나중에 직접
등록할 것이므로, `CoreUiBuilder` 는 컨트롤을 만들고 `SoftKeys`/`TopLabels` 로
꺼내 줄 뿐이다(둘 다 `ButtonBase` 라 `Click` 이벤트는 기본으로 갖고 있다).

```csharp
public IReadOnlyList<SoftKey> SoftKeys { get; }     // F1..F20, 생성 순서 그대로
public IReadOnlyList<TopLabel> TopLabels { get; }   // 왼쪽부터 8개
```

`MainWindow.xaml.cs` 의 `ApplyPreviewContent()` 는 이 저장소를 단독 실행했을 때
보기 편하라고 예전 데모 문구·상태로 한 번 덮어쓰는 것뿐인 테스트 코드다 -
`SoftKeys`/`TopLabels` 에서 `KeyId` 로 찾아 `SetLabel`/`SetState` 를 부르는 게
전부라, 지워도 레이아웃/기능엔 전혀 영향이 없다.

### F1~F20 : `SoftKey` / `SoftButtonState`

```csharp
softKey.SetLabel("MAP");
softKey.SetState(SoftButtonState.SoftButtonSelected);
```

```csharp
public enum SoftButtonState
{
    SoftButtonHidden,    // 화면에서 통째로 사라진다(자리도 차지하지 않는다)
    SoftButtonEnabled,
    SoftButtonDisabled,
    SoftButtonSelected,
}
```

`SetLabel`/`SetState` 는 결국 `Label`/`State` DependencyProperty 를 그대로
바꾸는 것이라 호출 즉시 화면(배경 명도·해치·표시 여부)에 반영된다 — 별도의
새로고침/커밋 호출이 필요 없다. `SoftButtonHidden` 은 `ControlTemplate.Trigger`
로 `Visibility="Collapsed"` 를 거는 것뿐이라 F1~F20 두 스타일(좌/우 레일) +
하단 열 스타일 전부에서 동일하게 동작한다.

| 스타일 | 대상 | 크기 |
|---|---|---|
| `SideSoftKeyStyle` | F1~F6 | 153 × 90 |
| `SideSoftKeyRightStyle` | F7~F12 (라벨·Selected 엣지만 우측 미러링) | 153 × 90 |
| `BottomSoftKeyStyle` | F13~F20 | 214 × 48 |

**F13~F20 은 규격서 §5 물리 버튼 8개 고정 배치다** (모드 탭이 아니다). 참고용
매핑(실제 라벨은 `ApplyPreviewContent()` 참고, 라벨/상태는 어차피 외부에서 다시
채워진다):

| 버튼 | 참고 라벨 | 규격서 기능 (Function / PPT 영문) |
|---|---|---|
| F13 | UP | 메뉴 구조에서 한 단계 위로 이동 — Navigate Up |
| F14 | ALARMS | 현재 모든 System Alert 표시 — Display Alerts |
| F15 | THREAT | 현재 모든 Threat 표시 — Display Threats |
| F16 | ACK | Warning/Alarm 확인 — Acknowledge Alerts |
| F17 | ▲ | 메뉴 스크롤 위 / 숫자값 증가 — Scroll Up / Increase Value |
| F18 | ▼ | 메뉴 스크롤 아래 / 숫자값 감소 — Scroll Down / Decrease Value |
| F19 | LABELS | Bezel 버튼 라벨·오버레이 표시/숨김 — Show / Hide Labels |
| F20 | ENTER | 데이터 입력 또는 선택된 메뉴 실행 — Enter / Select |

F17/F18 참고 라벨만 "SCROLL UP/DOWN" 문구 대신 화살표 글리프 한 글자(`▲`/`▼`)를
쓴다 — 나머지와 폭·정렬 규칙은 동일하게 유지된다.

### 기능영역 라벨 8개 : `TopLabel` / `T_Button_State`

F1~F20 과 **다른 컨트롤·다른 상태 모델**이다 — 기존에 쓰던 별개의 호출 규약을
그대로 따른다:

```csharp
topLabel.SetLabelState(T_Button_State.LabelStateSelectedLeft, "");
```

```csharp
public enum T_Button_State
{
    LabelStateEnabled,
    LabelStateDisabled,
    LabelStateSelected,
    LabelStateSelectedLeft,     // 트랙 바 왼쪽 반쪽만 색칠
    LabelStateSeletedRight,     // 트랙 바 오른쪽 반쪽만 색칠 (오탈자 그대로 - 호출부 철자와 맞춰야 한다)
}
```

`SetLabelState(state, multiState)` 의 두 번째 인자는 `TopLabel.MultiState`
(string)에 그대로 보관된다 - 이 라벨엔 텍스트가 없어서(규격서 §8, 상단 하드웨어
버튼 좌표 미확정) 지금은 화면에 쓰이지 않지만, 기존 호출부 시그니처를 그대로
받기 위해 값은 저장해 둔다.

**트랙 바가 반으로 나뉘어 있다** — `TopLabelStyle` 은 6px 트랙 바를
`LeftHalf`/`RightHalf` 두 `Border` 로 쪼개서, `LabelStateSelectedLeft`/
`LabelStateSeletedRight` 일 때 한쪽만 액센트 색을 칠하고 반대쪽은 Enable
기본색 그대로 둔다. 둘 다 같은 색이면(Enabled/Disabled/Selected) 이음매 없는
바 하나로 보인다(가운데만 각지고 바깥쪽만 둥글게 잘라 붙였다 -
`TopLabelLeftCornerRadius`/`TopLabelRightCornerRadius`, `GvaColors.xaml`).

기능영역 라벨 8개는 히트타겟(183×20)과 실제로 칠하는 영역도 분리돼 있다 —
"두껍고 무거워 보인다" 피드백 때문에, 텍스트/아이콘이 없는데 20px 전체를 색으로
꽉 채우면 정보량 없이 크기만 큰 블록으로 읽혔다. 가운데의 얇은 6px 트랙 바만
칠하고, 간격 계산에 쓰이는 183×20 히트타겟 자체는 그대로 둔다(투명 `Border` 로
클릭 영역만 유지).

### 아이콘은 전부 뺐다

F1~F12 좌측 상단에 있던 24×24 SVG(`StreamGeometry` 12개)와 StatusBar 우선 경고
줄의 삼각형 아이콘까지 전부. 아이콘 하나마다 좌표를 손으로 그린 `StreamGeometry`
라 늘어날수록 유지보수가 버거워질 거라는 피드백으로, 라벨 텍스트(F1~F12)나
색상 하나(StatusBar 심각도 바)만으로 표현하도록 정리했다. F각인(`KeyIdText`)은
원래도 우측 상단이었고, 아이콘이 차지하던 위 칸이 없어지면서 라벨이 버튼 전체
높이를 그대로 쓰게 됐다.

## StatusBar

기존 프로젝트가 이미 쓰고 있는 바인딩 경로/컨트롤 이름에 맞춰 전면 재설계했다
(VEH/MSN/MODE 같은 예전 항목은 없다). 76px 2줄:

| 줄 | 구성 |
|---|---|
| 1줄 | 시간 · 방위각 · 커스텀 전시정보(JSON, 폭 가변) |
| 2줄 | 좌표 · 알람(시간+메세지+카운트 뱃지) · 경고/주의/무시 집계 |

**바인딩 경로는 기존 그대로 맞췄다** :

| 표시 | 바인딩 |
|---|---|
| 시간 | `SystemData.CurrentTime` |
| 방위각 | `SystemData.HeadingMil` |
| 좌표 | `SystemData.Coordinate` |
| 알람 시간 (`x:Name="AlarmMessageTime"`) | `AlarmData.AlarmTime` |
| 알람 메세지 (`x:Name="AlarmMessageText"`) | `AlarmData.AlarmMessage` |
| 알람 카운트 뱃지 | `AlarmData.AlarmCount` |
| 경고 / 주의 / 무시 카운트 | `AlarmData.WarningCount` / `CautionCount` / `IgnoreCount` |

알람 시간/메세지/뱃지 색은 `AlarmData.AlarmLevel`("경고"/"주의" 문자열) 을 보는
`DataTrigger` 로 정해진다(`StatusBarView.xaml` 의 `AlarmLevelTextStyle`/
`AlarmBadgeStyle`) — 경고=`WarningBrush`(빨강), 주의=`CautionBrush`(주황). 경고/
주의/무시 집계는 항상 고정색(각각 빨강/주황/회색)이다. `SystemData`/`AlarmData`
는 `StatusBarViewModel` 이 노출하는 서브 객체(`SystemDataModel`/`AlarmDataModel`)
다 — 이름 그대로 바인딩 경로와 일치한다.

### 커스텀 전시정보 (JSON)

1줄 우측은 기존 프로젝트가 JSON 으로 밀어주는 데이터다. `StatusBarViewModel.
LoadCustomDisplayData(string json)` 하나가 진입점이고, 어디서 온 문자열인지는
몰라도 된다:

```jsonc
{
  "status_bar": {
    "itemWithLayout": [
      ["MSN", "임무", "MSN", 0.3],
      ["OPMODE", "모드", "MODE", 0.35],
      ["GPS", "위성항법", "GPS", 0.35]
    ],
    "itemWithValue": {
      "MSN":    [["RECON", "정찰", "RECON", "#FF32D74B"]],
      "OPMODE": [["ACTIVE", "활성", "ACTIVE", "#FF00C8FF"]],
      "GPS":    [["FIX3D", "3D 고정", "3D FIX", "#FF32D74B"]]
    }
  }
}
```

`itemWithLayout` 은 `[Key, KorName, EngName, Ratio]` 목록 - `Ratio` 는 커스텀
영역 안에서 그 Key 가 차지하는 가로 비율(Grid Star 크기로 그대로 쓴다).
`itemWithValue[Key]` 는 `[Value, KorValue, EngValue, Color]` 목록이다 - "실측
코드값 → 표시 문자열/색" 매핑 테이블(`Value` 가 라이브 데이터의 코드와 매칭되는
구조)이다. `LoadCustomDisplayData` 직후엔 목록의 첫 항목이 "현재값"이고, 그
뒤로는 아래 메서드로 Key 단위 실측값을 계속 밀어 넣는다 — **외부에서 Key 로
갱신 가능하다**:

```csharp
statusBar.UpdateCustomValue("GPS", "FIX2D");
```

`itemWithValue["GPS"]` 에서 `Value == "FIX2D"` 인 항목을 찾아 그 KorValue/
EngValue/Color 로 즉시 바꾼다 — 레이아웃(`itemWithLayout`)이나 값 테이블
(`itemWithValue`) 전체를 다시 보낼 필요 없이 Key 와 새 코드값만 넘기면 된다.
언어 전환(`SetLanguage`)이나 재조립이 일어나도 마지막으로 반영된 코드값은
유지된다(`StatusBarViewModel._currentValueCodes`).

Key 마다 `KorName : KorValue`(한글 모드) 또는 `EngName : EngValue`(영어 모드)
가 표시된다 — `StatusBarViewModel.SetLanguage(bool isEnglish)` 로 전환한다.
JSON 배열은 위치 기반 튜플(`[[...], [...]]`)이라 일반적인 System.Text.Json
객체 매핑이 안 통해서 `Models/StatusBarCustomData.cs` 에 전용 `JsonConverter`
를 만들었다.

Key 개수·비율이 실행 중에 바뀔 수 있어서(JSON 이 다시 로드될 때마다) 정적
`ColumnDefinition` 으로는 표현이 안 된다 - `StatusBarView.xaml.cs` 가
`CustomItems` 가 바뀔 때마다(`ObservableCollection.CollectionChanged`) 빈
`<Grid x:Name="CustomInfoHost"/>` 의 열을 통째로 다시 만든다(`CoreUiBuilder` 와
같은 이유로 코드에서 조립한다).

## 프로세스 분리 (Core / Application)

`ApplicationAreaView.xaml` 원래 헤더에 "실제 시스템에서는 이 영역이 별도 프로세스
창으로 대체된다" 라고 적혀 있었다 — 그 방향을 실제로 구현했다.

**왜 분리하나** : 실기에서는 Core(베젤 물리 버튼·StatusBar 를 다루는 저수준 UI)와
Application(미션에 따라 바뀌는 소프트웨어)이 서로 다른 배포 주기·신뢰 수준을 갖는
별개 소프트웨어인 경우가 많다. 한 프로세스가 죽어도 다른 쪽은 살아있어야 하고,
Application 은 통째로 교체 가능해야 한다 — 그래서 진짜로 두 개의 `.exe` 로 나눴다
(같은 WPF 앱 안에서 `UserControl` 두 개로 흉내낸 게 아니라).

**어떻게 겹쳐 보이나(핵심)** : Application 은 Core 의 자식 창이 아니라 완전히
별개의 최상위 창이다. 다만 —

1. Core 가 `SourceInitialized`(자기 HWND 가 막 생긴 시점)에서
   `GVA.HMI.Example.Application.exe` 를 `--owner=<HWND 값>` 인자로 실행한다
   (`MainWindow.xaml.cs` 의 `OnSourceInitialized`). exe 경로는 자기 실행 파일 경로의
   `"...Core"` 를 `"...Application"` 으로 문자열 치환해서 찾는다 — 두 프로젝트가
   같은 빌드 구성(Debug/Release)으로 나란히 빌드된다는 전제.
2. Application 은 그 HWND 값을 받아 `SetWindowLongPtr(GWL_HWNDPARENT)` 로
   Win32 **owner**(부모 아님) 관계를 맺는다 — Core 위에 항상 뜨고, Core 가
   최소화되면 같이 숨고, Core 가 `DestroyWindow` 되면(정상 종료든 강제 종료든)
   시스템이 이 창도 같이 닫아준다(`Interop/OwnerWindowSync.cs`).
3. Application 은 ~60Hz 로 `GetWindowRect(coreHwnd)` 를 폴링해서, Core 의
   `Viewbox`(Stretch=Uniform) 가 하는 것과 **똑같은 산수**(균일 축소 + 레터박스
   중앙정렬)를 스스로 반복해 "Application 영역"의 화면 좌표를 계산하고,
   `SetWindowPos` 로 자기 자신을 그 자리에 맞춘다. 기준 좌표(캔버스 크기,
   Application 영역 x/y/width/height)는 `OwnerWindowSync.cs` 상단 상수다 - Core
   쪽 `CoreUiBuilder.cs` 상단의 같은 상수와 **값이 반드시 같아야 한다**(설정
   파일을 공유하지 않기로 했으므로, 좌표를 바꿀 땐 두 파일을 각각 고친다).
4. Core 는 그 자리를 **실제로 투명하게 비워둔다**(아래 "Application 영역을 비우는 법").
5. 이중 안전장치 : Core 의 `Closing` 에서도 Application 프로세스를 명시적으로
   `CloseMainWindow`/`Kill` 하고, Application 쪽도 매 폴링마다 `IsWindow(coreHwnd)`
   로 Core 생존 여부를 확인해 죽었으면 스스로 종료한다.

**리소스는 공유하지 않는다** — `Severity`, `SeverityToBrushConverter`,
`ObservableObject` 처럼 양쪽이 다 쓰는 작은 타입은 두 프로젝트에 각각 파일을
복제해뒀다(어셈블리 참조를 걸지 않았다). Warning/Caution/Normal 색상 값도 두
`GvaColors.xaml` 에 각각 정의돼 있다 — 디자인 토큰이 같은 값을 공유할 뿐, 코드나
리소스 자체를 묶지는 않는다. 완전히 독립된 배포 단위로 두고 싶어서 일부러 이렇게
했다(둘 중 하나만 있어도 컴파일된다 - `GVA.HMI.Example.Application.exe` 를
`--owner` 없이 단독 실행하면 그냥 가운데 정렬된 보통 창으로 뜬다, 디버그용).

## Application 영역을 비우는 법

Application 영역(1590×915)만 투명하고 나머지는 불투명해야 한다. **순수 WPF 로만**
처리하며 P/Invoke 는 쓰지 않는다.

1. **창을 통째로 투명하게 한다** — `MainWindow.xaml` 의 `AllowsTransparency="True"` +
   `Background="Transparent"`.
2. **비워둘 자리만 남기고 다시 칠한다** — `CoreUiBuilder.BuildVoidBackdrop()` 이
   `CombinedGeometry(Exclude)` 로 "캔버스 − Application 영역" 모양의 `Path` 를 만들어
   맨 아래에 깐다. `Grid.Background` 를 쓰지 않는 이유가 이것이다 - 사각형 하나라
   가운데를 비울 수가 없다.
3. **그 자리에 얹을 자리를 하나 둔다** — `CoreUiBuilder.ApplicationHost`
   (`ContentControl`, 배경 없음). 비워두면 완전 투명이라 뒤가 비치고 클릭도
   통과하며, `Content` 를 넣으면 그 엘리먼트만 그 위에 그려진다.

**왜 `SetWindowRgn` 이 아닌가** : 윈도우 리전은 그 자리 픽셀을 아예 잘라내므로,
Application 영역 위에 Core 가 WPF 엘리먼트를 얹으면 그것까지 같이 사라진다.
오버레이를 얹어야 한다는 요구를 리전으로는 만족시킬 수 없다.

**대가** : `AllowsTransparency` 는 창 전체를 소프트웨어 렌더링으로 떨어뜨린다.
1920×1080 전면 UI 에서 싸지 않지만 위 요구와 맞바꾼 값이다.

**얻은 것** : 투명 영역이 나머지 UI 와 똑같이 `Viewbox` 를 타고 스케일되므로
좌표를 다시 계산할 일이 없다. 창 크기·DPI 가 바뀌어도 손댈 게 없고,
`SizeChanged` 훅도 필요 없다.

**배경판이 캔버스보다 큰 이유** : 창 비율이 16:9 가 아니면 `Viewbox` 가 레터박스
여백을 남기는데, 창 배경이 투명이라 그 여백까지 뚫려 보인다. 그래서 배경판을
`BackdropOverscan`(2000) 만큼 넘치게 그려 덮는다. 이게 성립하려면 `root` 와
`Viewbox` 의 `ClipToBounds` 가 `false` 여야 한다(코드에 명시해 뒀다).

## Core UI 리디자인 (premium embedded HMI)

- **테두리 없음** — 버튼·카드에 `BorderBrush` 를 쓰지 않는다. 상태 구분은 배경
  명도차로만 한다 (`SurfaceHi/LoColor`, Application 의 `PanelColor` 보다 한 단
  밝게 잡은 `ChassisHi/LoColor` 위에 얹힌 형태).
- **Selected = 3px 솔리드 액센트 엣지 하나** — 블러/글로우를 완전히 뺐다. 좌우
  키는 좌측, 하단 키는 상단, 기능영역 라벨은 트랙 바 색 자체가 바뀐다(위
  TopLabel 절 참고). Effect 를 전혀 안 쓰므로 글자가 흐려질 여지 자체가 없다.
- **Disable = 명도 저하 + 대각선 해치** — `DisabledHatchBrush`(`GvaColors.xaml`,
  8px 타일 대각선 1px)를 오버레이로 겹쳐 "그냥 어둡게 숨긴 것"이 아니라 "사용
  불가"임을 명시한다. 언어에 의존하지 않는 표기라 라벨과 별개로 항상 보인다.
- **뒤판은 순검정** — 좌/우 레일과 하단 바 뒤에 `VoidBrush`(#000000) 뒤판을
  깔아서, 개별 키가 "패널에 박힌 모듈"처럼 보이게 한다(`CoreUiBuilder.BuildChassisPlates()`,
  `Canvas` 안에서 버튼보다 먼저 그려 뒤에 깔리게 했다). 원래 `ChassisBrush`
  그라디언트였는데 "버튼 사이에 보이는 배경을 아예 검은색으로" 피드백으로
  바꿨다 — 버튼과 버튼 사이 틈에서 미세하게 밝은 색이 비쳤었다. StatusBar 자체
  배경은 그대로 Chassis 톤을 유지한다(그 요청은 레일/하단 열 한정이었다).
- **Application 보호 프레임** — Application 뒤에 1px 심선 Border 를 4px 여백을
  두고 둘러서 "보호된 캔버스"로 읽히게 한다. Application 자체의 위치·크기·스타일은
  건드리지 않는다 (`CoreUiBuilder` 의 `applicationFrame` 만 별도 레이어).
- **외곽 베젤 심선** — 화면 전체를 감싸는 1px 라인 하나(`FrameSeamBrush`, 흰색
  12%), 클릭은 통과한다. 두껍거나 장식적이지 않게 최소로.
- **StatusBar 그룹핑** — 항목을 흩뿌리지 않고 의미 단위로 묶은 뒤 그룹 사이에만
  짧은 세로 심선을 둔다. 구체적인 항목 구성은 [`## StatusBar`](#statusbar) 참고
  (전면 재설계됨). 알람 카운트만 작은 필(뱃지)에 심각도 색을 채우고, 나머지는
  여전히 "배경 전체를 물들이지 않는다" 원칙을 지킨다.
- **8px 그리드** — 버튼 내부 여백(8), 라벨 줄간격(22=8×2.75… 시각적으로 맞춘
  근사치), 해치 타일(8) 등 내부 치수를 8 의 배수에 맞췄다. 다만 F1~F20/StatusBar
  의 외곽 크기 자체는 규격서 §2 의 물리 버튼 대응 좌표라 그리드에 강제로 맞추지
  않았다(§3 참조).
- **라운드 축소** — 소프트키 10→6, 기능영역 라벨 5→2 (과하게 둥글면 장난감처럼
  보인다는 피드백). Application 카드의 12 는 그대로(재설계 대상 아님).

## 렌더링 관련 유의사항 (직접 겪은 문제)

- **버튼 모서리가 계단처럼 깨져 보였던 원인** — `Window.UseLayoutRounding="True"` 와
  각 소프트키 스타일의 `SnapsToDevicePixels="True"` 가 `Viewbox` 의 비정수 축소 배율
  (예: 1280/1920 ≈ 0.667)과 충돌해서 `CornerRadius` 호가 축소 전 격자에 스냅됐다.
  두 속성 모두 제거했다. 다시 켜지 말 것.
- **Selected 상태에서 글자가 흐려 보였던 원인** — `DropShadowEffect` 를 텍스트가 든
  `Border`(Root)에 직접 걸면 WPF 가 그 서브트리 전체(텍스트 포함)를 비트맵으로
  래스터라이즈한 뒤 효과를 입힌다. 그 비트맵이 다시 `Viewbox` 로 축소되며 글자가
  흐려진다. 최종적으로는 Effect 자체를 버튼에서 완전히 없앴다(위 "Selected = 3px
  솔리드 액센트 엣지" 참고) — 미봉책(별도 Glow 레이어)이 아니라 근본 해결.

## Application 영역과의 여백

Application(165, 105 / **1590 × 915, 고정**) 은 움직이지 않는다. 원래 좌표대로면
좌우 레일과 하단 열이 Application 에 그대로 맞닿아서(0~2px), 주변 소프트키 쪽을
안쪽으로 들여 여백을 만들었다 — 바깥쪽(화면 가장자리)은 그대로 붙어있고 안쪽에만 틈이 생긴다.

여러 차례 조정을 거쳤다 : 8px/4px/5px 대(1차) → "Core 버튼이 App 에 거의
붙어보인다" 피드백으로 16~20px 대(2차, 과했다) → "너무 넓다, 하단 버튼 높이가
너무 작다" 피드백으로 12px/8px/6px 대로 다시 줄이고 하단 열 높이도 40→48 로
키웠다(지금 값, `CoreUiBuilder.cs` 상수).

| 요소 | 값(현재) | 여백 |
|---|---|---|
| 좌우 레일 폭 | 153 (X 는 그대로 0 / 1767) | Application 과 12px |
| 기능영역 라벨 Y | 79 | Application 과 6px (StatusBar 와 낀 좁은 틈이라 이게 한계) |
| 하단 열 Y / 높이 | 1028 / 48 | Application 과 8px, 화면 하단은 4px |

## 규격서와의 차이 / 남은 사항

- **폰트** — Segoe UI (Windows 기본, 영문 전용 전환 이후). `GvaColors.xaml` 의
  `GvaFont` 한 곳에서 정의한다.
- **최소 폰트 18px** — `GvaStyles.xaml` 의 모든 `FontSize` 는 18 이상이다
  (원 규격서는 11~16px 대였음, 가독성 피드백으로 상향).
- **Border 없음** — 규격서 §4 는 "Border 가 히트 타겟을 정의, 2px 권장" 이라 되어
  있으나 이 프로젝트는 채택하지 않는다(사용자 피드백).
- **`word-break: keep-all`** — WPF `TextBlock` 에는 대응 속성이 없어 한글이 단어
  중간에서 접힐 수 있다. 현재는 라벨이 전부 영문이라 해당 없음.
- **그라디언트** — 규격서에 색상값 1개만 기재해야 하는 경우
  `SurfaceEnableFlatBrush` / `SurfaceSelectedFlatBrush` / `LabelSelectedFlatBrush`
  로 교체한다 (§8).
- **미해결(§8)** — 상단 하드웨어 버튼 8개 좌표, 터치 지원 여부는 규격서와
  동일하게 열려 있다. (F12 시험 아이콘 이슈는 아이콘 자체를 없애면서 해소됐다.)
- **레이아웃 밸리데이션 없음** — F1~F20/기능영역 라벨 개수를 `CoreUiBuilder.cs`
  상수에서 바꿨을 때 레일이 Application 영역과 겹치거나 화면을 벗어나는지는
  검증하지 않는다(데모 범위 밖).
