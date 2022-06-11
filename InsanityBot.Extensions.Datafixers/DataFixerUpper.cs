namespace InsanityBot.Extensions.Datafixers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using InsanityBot.Extensions.Datafixers.Attributes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

using Spectre.Console;

public class DataFixerUpper : IDatafixerService
{
    private readonly ILogger<IDatafixerService> __logger;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<IDatafixer>> __sorted_datafixers;

    public DataFixerUpper(ILogger<IDatafixerService> logger)
    {
        this.__logger = logger;
        this.__sorted_datafixers = new();
    }

    public ValueTask DiscoverDatafixers(IServiceProvider services, params Assembly[] assemblies)
    {
        this.__logger.LogInformation("Initializing DataFixerUpper");

        List<Type> types = new();
        List<Type> simpleInit = new();
        List<Type> complexInit = new();
        Type datafixerType = typeof(IDatafixable);
        List<IDatafixer> datafixers = new();

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn()
                {
                    Alignment = Justify.Right
                },
                new ProgressBarColumn()
                {
                    Width = 50,
                    CompletedStyle = new Style(Color.Purple3),
                    FinishedStyle = new Style(Color.MediumSpringGreen)
                },
                new PercentageColumn()
                {
                    Style = new Style(Color.Purple3),
                    CompletedStyle = new Style(Color.MediumSpringGreen)
                },
                new ElapsedTimeColumn(),
                new SpinnerColumn(Spinner.Known.Dots2)
            )
            .Start(context =>
            {
                ProgressTask assemblyTypeLoading =
                    context.AddTask("Loading types from assemblies", maxValue: assemblies.Length);
                ProgressTask typePreprocessing =
                    context.AddTask("Preprocessing types");
                ProgressTask typeFiltering =
                    context.AddTask("Filtering ignored datafixer types");
                ProgressTask findSimpleInit =
                    context.AddTask("Finding simple-initialization datafixers");
                ProgressTask simpleInitTask =
                    context.AddTask("Creating simple-initialization datafixers");
                ProgressTask complexInitTask =
                    context.AddTask("Creating complex-initialization datafixers");

                foreach(Assembly assembly in assemblies)
                {
                    types.AddRange(assembly.ExportedTypes);

                    assemblyTypeLoading.Increment(1);
                }

                // ensure we don't deadlock
                assemblyTypeLoading.Value = assemblies.Length;

                Double maxValue = types.Count;
                Double increment = 100.0 / maxValue;

                types = types
                    .Where(xm =>
                    {
                        typePreprocessing.Increment(increment);

                        if(xm.GetInterfaces().Contains(datafixerType))
                        {
                            return true;
                        }
                        return false;
                    })
                    .ToList();

                typePreprocessing.Value = 100.0;

                maxValue = types.Count;
                increment = 100.0 / maxValue;


                types = types
                    .Where(xm =>
                    {
                        typeFiltering.Increment(increment);

                        return xm.GetCustomAttribute<IgnoreDatafixerAttribute>() is not null;
                    })
                    .ToList();

                typeFiltering.Value = 100.0;

                maxValue = types.Count;
                increment = 100.0 / maxValue;

                simpleInit = types
                    .Where(xm =>
                    {
                        findSimpleInit.Increment(increment);

                        return xm.GetCustomAttribute<RequiresDependencyInjectionAttribute>() is null;
                    })
                    .ToList();

                findSimpleInit.Value = 100.0;

                complexInit = types.Except(simpleInit).ToList();

                maxValue = simpleInit.Count;
                increment = 100.0 / maxValue;


                foreach(Type t in simpleInit)
                {
                    datafixers.Add((IDatafixer)createInstance(t));

                    simpleInitTask.Increment(increment);
                }

                simpleInitTask.Value = 100.0;

                maxValue = complexInit.Count;
                increment = 100.0 / maxValue;


                foreach(Type t in complexInit)
                {
                    datafixers.Add((IDatafixer)createComplexInstance(t, services));

                    complexInitTask.Increment(increment);
                }

                complexInitTask.Value = 100.0;
            });

        this.__logger.LogInformation("Initialized datafixers, sorting datafixers...");

        _ = Parallel.ForEachAsync(datafixers, (datafixer, cancellationToken) =>
        {
            if(!this.__sorted_datafixers.ContainsKey(datafixer.Datafixable))
            {
                this.__sorted_datafixers.AddOrUpdate(key: datafixer.Datafixable,
                    addValue: new ConcurrentBag<IDatafixer>() { datafixer },
                    updateValueFactory: (key, old) =>
                    {
                        old.Add(datafixer);
                        return old;
                    });
            }
            else
            {
                this.__sorted_datafixers[datafixer.Datafixable].Add(datafixer);
            }

            return ValueTask.CompletedTask;
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

    public Boolean ApplyDatafixers<Datafixable>(Datafixable datafixable)
        where Datafixable : class, IDatafixable
    {
        this.__logger.LogInformation("Starting datafixer operation for {datafixableType}", typeof(Datafixable));

        if(!this.__sorted_datafixers.ContainsKey(typeof(Datafixable)))
        {
            this.__logger.LogInformation("No applicable datafixers were found for {datafixableType}, returning", typeof(Datafixable));
            return true;
        }

        String currentVersion = datafixable.DataVersion;

        do
        {
            IEnumerable<IDatafixer> candidates = this.__sorted_datafixers[typeof(Datafixable)]
                .Where(xm => xm.OldVersion == currentVersion);

            IDatafixer datafixer = candidates.FirstOrDefault()!;

            if(datafixer == default)
            {
                break;
            }

            datafixable = (Datafixable)datafixer.UpdateData(datafixable);

        } while(true);

        return true;
    }
}
