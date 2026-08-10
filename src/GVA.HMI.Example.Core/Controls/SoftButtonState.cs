namespace HMICore.Controls;

/// <summary>
/// 소프트키(F1~F20, 기능영역 라벨)의 4상태. 이름은 기존에 쓰던 외부 규약을 그대로
/// 따른다 - 이 프로젝트가 새로 지은 이름이 아니라, UI 를 붙일 기존 프로젝트 쪽
/// 호출부가 이미 이 이름으로 부르고 있다.
/// </summary>
public enum SoftButtonState
{
    /// <summary>화면에서 사라진다(자리도 차지하지 않는다) - 현재 이 물리 버튼에
    /// 배정된 기능이 없을 때.</summary>
    SoftButtonHidden,

    /// <summary>조작 가능. 배경은 정보전시영역(#191919)보다 밝다.</summary>
    SoftButtonEnabled,

    /// <summary>조작 불가. 명도를 낮추고 대각선 해치로 "사용 불가"를 명시한다.</summary>
    SoftButtonDisabled,

    /// <summary>현재 선택됨. 채도는 이 상태에만 집중된다.</summary>
    SoftButtonSelected,
}
