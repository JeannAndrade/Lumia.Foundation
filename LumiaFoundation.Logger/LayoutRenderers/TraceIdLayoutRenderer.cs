using System.Diagnostics;
using System.Text;
using NLog;
using NLog.LayoutRenderers;

namespace LumiaFoundation.Logger.LayoutRenderers;

[LayoutRenderer("lumia-traceid")]
public sealed class TraceIdLayoutRenderer : LayoutRenderer
{
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var activity = Activity.Current;

        if (activity is null || activity.IdFormat != ActivityIdFormat.W3C)
        {
            return;
        }

        builder.Append(activity.TraceId.ToHexString());
    }
}
