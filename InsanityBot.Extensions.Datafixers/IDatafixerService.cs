namespace InsanityBot.Extensions.Datafixers;

using System;
using System.Reflection;
using System.Threading.Tasks;

public interface IDatafixerService
{
    public ValueTask DiscoverDatafixers(params Assembly[] assemblies);

    public ValueTask<Boolean> ApplyDatafixers<Datafixable>(ref Datafixable datafixable)
        where Datafixable : IDatafixable;

    public ValueTask<Boolean> RevertDatafixers<Datafixable>(ref Datafixable datafixable)
        where Datafixable : IDatafixable;
}
