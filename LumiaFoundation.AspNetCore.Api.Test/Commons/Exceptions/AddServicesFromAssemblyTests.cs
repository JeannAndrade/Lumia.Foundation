using System.Collections;
using LumiaFoundation.AspNetCore.Commons.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace LumiaFoundation.AspNetCore.Test.Commons.Extensions;

public interface IServicoDeExemplo { }
public class ServicoDeExemplo : IServicoDeExemplo { }

// Nome coincide com System.Collections.IEnumerator (sem o "I"): reproduz o falso positivo do bug.
internal sealed class Enumerator : IEnumerator
{
    public object Current => throw new NotSupportedException();
    public bool MoveNext() => false;
    public void Reset() { }
}

// Aninhado com par interface/implementação válido: só deve ficar de fora por ser aninhado.
public static class Contenedor
{
    public interface IAninhado { }
    public class Aninhado : IAninhado { }
}

public static class GeradorDeTipoSintetizado
{
    // Collection expression de elemento único com alvo IEnumerable<T>: o compilador sintetiza
    // <>z__ReadOnlySingleElementList<string> (e seu Enumerator) neste assembly.
    public static IEnumerable<string> UmElemento() => ["a"];
}

public class AddServicesFromAssemblyTests
{
    private static ServiceCollection Registrar()
    {
        var services = new ServiceCollection();
        services.AddServicesFromAssembly(typeof(AddServicesFromAssemblyTests).Assembly);
        return services;
    }

    [Fact]
    public void RegistraParInterfaceImplementacaoDoMesmoAssembly()
    {
        var services = Registrar();

        Assert.Contains(services, d =>
            d.ServiceType == typeof(IServicoDeExemplo) &&
            d.ImplementationType == typeof(ServicoDeExemplo) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void NaoRegistraInterfaceDeOutroAssemblyComNomeCoincidente()
    {
        var services = Registrar();

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IEnumerator));
    }

    [Fact]
    public void NaoRegistraTiposAninhados()
    {
        var services = Registrar();

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(Contenedor.IAninhado));
    }

    [Fact]
    public void NaoRegistraTiposGeradosPeloCompilador()
    {
        _ = GeradorDeTipoSintetizado.UmElemento();

        var services = Registrar();

        Assert.All(services, d =>
            Assert.False(d.ImplementationType!.IsDefined(typeof(CompilerGeneratedAttribute), false)));
    }

    [Fact]
    public void ProviderConstroiComValidateOnBuild()
    {
        var services = Registrar();

        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IServicoDeExemplo>());
    }
}
