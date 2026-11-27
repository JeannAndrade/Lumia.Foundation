using System;
using System.Diagnostics;
using System.Text;
using LumiaFoundation.Logger;
using NLog;
using NLog.LayoutRenderers;

namespace LumiaFoundation.Logger.LayoutRenderers;

[LayoutRenderer("lumia-ownerid")]
public sealed class OwnerIdLayoutRenderer : LayoutRenderer
{
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        for (var activity = Activity.Current; activity is not null; activity = activity.Parent)
        {
            foreach (var tag in activity.TagObjects)
            {
                if (tag.Key != ActivityTagNames.OwnerId)
                {
                    continue;
                }

                if (tag.Value is Guid ownerId)
                {
                    builder.Append(ownerId.ToString("D"));
                }
                else if (tag.Value is string ownerIdText && Guid.TryParse(ownerIdText, out ownerId))
                {
                    builder.Append(ownerId.ToString("D"));
                }

                return;
            }
        }
    }
}
