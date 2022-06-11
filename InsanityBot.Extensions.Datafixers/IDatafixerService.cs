namespace InsanityBot.Extensions.Datafixers;

using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

public interface IDatafixerService
{
    public ValueTask DiscoverDatafixers(IServiceProvider services, params Assembly[] assemblies);

    public Boolean ApplyDatafixers<Datafixable>(Datafixable datafixable)
        where Datafixable : class, IDatafixable;
}
