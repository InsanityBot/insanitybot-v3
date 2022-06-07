namespace InsanityBot.Extensions.Datafixers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using InsanityBot.Extensions.Datafixers.Attributes;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

using Spectre.Console;

public class DataFixerUpper : IDatafixerService
{
    private readonly Dictionary<Type, IEnumerable<ITypelessDatafixer>> __sorted_datafixers;

    public DataFixerUpper()
    {
        this.__sorted_datafixers = new();
    }

    public ValueTask DiscoverDatafixers(IServiceProvider services, params Assembly[] assemblies)
    {
        Log.Logger.Information("Initializing DataFixerUpper");

        List<Type> types = new();

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                ProgressTask assemblyTypeLoading =
                    context.AddTask("Loading types from assemblies...", maxValue: assemblies.Length);

                foreach(Assembly assembly in assemblies)
                {
                    types.AddRange(assembly.ExportedTypes);

                    assemblyTypeLoading.Increment(1);
                }

                // ensure we don't deadlock
                assemblyTypeLoading.Value = assemblies.Length;
            });

        Type datafixerType = typeof(IDatafixable);

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                Double maxValue = types.Count;

                ProgressTask typePreprocessing =
                    context.AddTask("Preprocessing types...", maxValue: maxValue);

                types = types
                    .Where(xm =>
                    {
                        typePreprocessing.Increment(1);

                        if(xm.GetInterfaces().Contains(datafixerType))
                        {
                            return true;
                        }
                        return false;
                    })
                    .ToList();

                typePreprocessing.Value = maxValue;
            });

        Log.Logger.Information("Datafixer types discovered, filtering and processing datafixers.");

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                Double maxValue = types.Count;

                ProgressTask typeFiltering =
                    context.AddTask("Filtering ignored datafixer types...", maxValue: maxValue);

                types = types
                    .Where(xm =>
                    {
                        typeFiltering.Increment(1);

                        return xm.GetCustomAttribute<IgnoreDatafixerAttribute>() is not null;
                    })
                    .ToList();

                typeFiltering.Value = maxValue;
            });

        List<Type> simpleInit = new();
        List<Type> complexInit = new();

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                Double maxValue = types.Count;

                ProgressTask findSimpleInit =
                    context.AddTask("Finding simple-initialization datafixers...", maxValue: maxValue);

                simpleInit = types
                    .Where(xm =>
                    {
                        findSimpleInit.Increment(1);

                        return xm.GetCustomAttribute<RequiresDependencyInjectionAttribute>() is null;
                    })
                    .ToList();

                findSimpleInit.Value = maxValue;
            });

        complexInit = types.Except(simpleInit).ToList();

        List<ITypelessDatafixer> datafixers = new();

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                Double maxValue = simpleInit.Count;

                ProgressTask simpleInitTask =
                    context.AddTask("Creating simple-initialization datafixers...", maxValue: maxValue);

                foreach(Type t in simpleInit)
                {
                    datafixers.Add((ITypelessDatafixer)createInstance(t));

                    simpleInitTask.Increment(1);
                }

                simpleInitTask.Value = maxValue;
            });

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Start(context =>
            {
                Double maxValue = complexInit.Count;

                ProgressTask complexInitTask =
                    context.AddTask("Creating complex-initialization datafixers...", maxValue: maxValue);

                foreach(Type t in complexInit)
                {
                    datafixers.Add((ITypelessDatafixer)createComplexInstance(t, services));

                    complexInitTask.Increment(1);
                }

                complexInitTask.Value = maxValue;
            });

        return ValueTask.CompletedTask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Object createInstance(Type type)
    {
        ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes)!;

        return ctor.Invoke(null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Object createComplexInstance(Type type, IServiceProvider services)
    {
        return ActivatorUtilities.CreateInstance(services, type);
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
