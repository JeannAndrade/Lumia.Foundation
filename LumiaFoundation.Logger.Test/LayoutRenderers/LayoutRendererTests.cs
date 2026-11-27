using System.Diagnostics;
using LumiaFoundation.Logger;
using LumiaFoundation.Logger.LayoutRenderers;
using NLog;

namespace LumiaFoundation.Logger.Test.LayoutRenderers;

public class TraceIdLayoutRendererTests
{
    [Fact]
    public void Render_WhenSemActivity_RetornaVazio()
    {
        using var activity = StartActivity();
        activity.Stop();

        Assert.Equal(string.Empty, new TraceIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent()));
    }

    [Fact]
    public void Render_WhenActivityW3C_RetornaTraceIdEmHexaDe32Caracteres()
    {
        using var activity = StartActivity();

        var result = new TraceIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent());

        Assert.Equal(activity.TraceId.ToHexString(), result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void Render_WhenActivityHierarquica_RetornaVazio()
    {
        using var activity = StartActivity(ActivityIdFormat.Hierarchical);

        Assert.Equal(string.Empty, new TraceIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent()));
    }

    [Fact]
    public void Render_WhenActivityFilha_RetornaOTraceIdDaRaiz()
    {
        using var root = StartActivity();
        using var child = StartActivity();

        Assert.Equal(root.TraceId.ToHexString(), new TraceIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent()));
    }

    [Fact]
    public async Task Render_WhenLogDepoisDeAwaitEmOutraThread_PropagaOTraceId()
    {
        using var activity = StartActivity();
        var traceId = activity.TraceId.ToHexString();

        await Task.Run(async () =>
        {
            await Task.Yield();
            Assert.Equal(traceId, new TraceIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent()));
        });
    }

    private static Activity StartActivity(ActivityIdFormat format = ActivityIdFormat.W3C)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;
        return new Activity("teste").SetIdFormat(format).Start();
    }
}

public class OwnerIdLayoutRendererTests
{
    [Fact]
    public void Render_WhenSemActivity_RetornaVazio() =>
        Assert.Equal(string.Empty, new OwnerIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent()));

    [Fact]
    public void Render_WhenTagNaActivityAtual_RetornaOGuid()
    {
        var ownerId = Guid.NewGuid();
        using var activity = StartActivity().SetTag(ActivityTagNames.OwnerId, ownerId);

        Assert.Equal(ownerId.ToString("D"), Render());
    }

    [Fact]
    public void Render_WhenTagSoNaActivityPai_RetornaOGuidDoPai()
    {
        var ownerId = Guid.NewGuid();
        using var parent = StartActivity().SetTag(ActivityTagNames.OwnerId, ownerId);
        using var child = StartActivity();

        Assert.Equal(ownerId.ToString("D"), Render());
    }

    [Fact]
    public void Render_WhenTagComStringDeGuid_RetornaOGuidNormalizado()
    {
        var ownerId = Guid.NewGuid();
        using var activity = StartActivity().SetTag(ActivityTagNames.OwnerId, ownerId.ToString("N").ToUpperInvariant());

        Assert.Equal(ownerId.ToString("D"), Render());
    }

    [Fact]
    public void Render_WhenTagAusente_RetornaVazio()
    {
        using var activity = StartActivity();

        Assert.Equal(string.Empty, Render());
    }

    [Fact]
    public void Render_WhenTagComTextoQueNaoEhGuid_RetornaVazio()
    {
        using var activity = StartActivity().SetTag(ActivityTagNames.OwnerId, "valor-invalido\nforjado");

        Assert.Equal(string.Empty, Render());
    }

    [Fact]
    public void Render_WhenTagDeOutroTipo_RetornaVazio()
    {
        using var activity = StartActivity().SetTag(ActivityTagNames.OwnerId, 42);

        Assert.Equal(string.Empty, Render());
    }

    [Fact]
    public void Render_WhenTagInvalidaNaFilhaEValidaNoPai_RetornaVazio()
    {
        var ownerId = Guid.NewGuid();
        using var parent = StartActivity().SetTag(ActivityTagNames.OwnerId, ownerId);
        using var child = StartActivity().SetTag(ActivityTagNames.OwnerId, "invalido");

        Assert.Equal(string.Empty, Render());
    }

    private static string Render() => new OwnerIdLayoutRenderer().Render(LogEventInfo.CreateNullEvent());

    private static Activity StartActivity()
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;
        return new Activity("teste").Start();
    }
}
