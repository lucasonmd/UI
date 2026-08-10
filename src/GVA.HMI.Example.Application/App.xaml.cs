using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace GVA.HMI.Example.Application;

public partial class App : System.Windows.Application
{
    /// <summary>
    /// Core 가 "--owner=&lt;hwnd&gt;" 로 자기 창 핸들을 넘겨준다. 없으면(단독 실행)
    /// IntPtr.Zero - MainWindow 가 이 경우 일반 창처럼(가운데 정렬, 크기 조절 가능)
    /// 뜬다. 이 프로젝트만 따로 실행해서 화면을 확인할 때 쓰라고 일부러 이렇게 뒀다.
    /// </summary>
    public static IntPtr OwnerHandle { get; private set; } = IntPtr.Zero;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 한국어 화면이므로 FlowDirection / 숫자 서식이 ko-KR 을 따르게 한다.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        foreach (var arg in e.Args)
        {
            if (arg.StartsWith("--owner=", StringComparison.OrdinalIgnoreCase)
                && long.TryParse(arg.AsSpan(8), out var handle))
            {
                OwnerHandle = new IntPtr(handle);
            }
        }
    }
}
