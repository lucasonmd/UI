using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace HMICore;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 한국어 화면이므로 FlowDirection / 숫자 서식이 ko-KR 을 따르게 한다.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
    }
}
