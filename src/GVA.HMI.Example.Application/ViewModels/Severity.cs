namespace GVA.HMI.Example.Application.ViewModels;

/// <summary>
/// 규격서 §4 의 상태색. 적/황/녹은 Warning · Caution · Advisory 예약색이므로
/// 일반 값 표기에는 <see cref="None"/> 을 쓴다.
/// </summary>
public enum Severity
{
    None,
    Warning,
    Caution,
    Normal,
}
