namespace InsanityBot.Extensions.Datafixers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Serilog;

using Spectre.Console;

public class DataFixerUpper : IDatafixerService
{
    public ValueTask DiscoverDatafixers(params Assembly[] assemblies)
    {
        Log.Logger.Information("Initializing DataFixerUpper");

        List<Type> candidateTypes = new();

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                ProgressTask assemblyTypeLoading =
                    context.AddTask("Loading types from assemblies...", maxValue: assemblies.Length);

                foreach(Assembly assembly in assemblies)
                {
                    candidateTypes.AddRange(assembly.ExportedTypes);

                    assemblyTypeLoading.Increment(1);
                }

                // ensure we don't deadlock
                assemblyTypeLoading.Value = assemblies.Length;
            });

        List<Type> types = new();
        Type datafixerType = typeof(IDatafixable);

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                ProgressTask typePreprocessing =
                    context.AddTask("Preprocessing types...", maxValue: candidateTypes.Count);

                foreach(Type type in candidateTypes)
                {
                    if(type.GetInterfaces().Contains(datafixerType))
                    {
                        types.Add(type);
                    }

                    typePreprocessing.Increment(1);
                }

                typePreprocessing.Value = candidateTypes.Count;
            });
    }

    public ValueTask<Boolean> ApplyDatafixers<Datafixable>(ref Datafixable datafixable)
        where Datafixable : IDatafixable
    {
        throw new NotImplementedException();
    }

    public ValueTask<Boolean> RevertDatafixers<Datafixable>(ref Datafixable datafixable)
        where Datafixable : IDatafixable
    {
        throw new NotImplementedException();
    }
}
