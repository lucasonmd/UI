namespace HMICore.Controls;

/// <summary>
/// 상단 기능영역 라벨(<see cref="TopLabel"/>) 전용 상태. F1~F20(<see cref="SoftButton"/>/
/// <see cref="SoftButtonState"/>)과는 별개의 상태 모델이다. 이름은 기존에 쓰던 외부
/// 호출 규약을 그대로 따른다 - <c>LabelStateSeletedRight</c> 의 오탈자("Selected"가
/// 아니라 "Seleted")도 일부러 고치지 않았다. 호출부가 이 철자로 부르고 있어서,
/// 여기서 고치면 오히려 안 맞게 된다.
/// </summary>
public enum T_Button_State
{
    LabelStateEnabled,
    LabelStateDisabled,
    LabelStateSelected,

    /// <summary>트랙 바의 왼쪽 반쪽만 액센트 색으로 칠한다.</summary>
    LabelStateSelectedLeft,

    /// <summary>트랙 바의 오른쪽 반쪽만 액센트 색으로 칠한다.</summary>
    LabelStateSeletedRight,
}
