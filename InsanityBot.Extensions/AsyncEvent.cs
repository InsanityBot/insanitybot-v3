namespace InsanityBot.Extensions;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public delegate ValueTask AsyncEventHandler<in TSender, in TArgs>(TSender sender, TArgs args, CancellationToken token);
                
public delegate ValueTask AsyncExceptionHandler<in TSender, in TArgs>(TSender sender, TArgs args, Exception exception, CancellationToken token);

public class AsyncEvent<TSender, TArgs>
{
    private readonly SynchronizedCollection<AsyncEventHandler<TSender, TArgs>> __handlers;
    private readonly SynchronizedCollection<AsyncExceptionHandler<TSender, TArgs>> __exception_handlers;

    public event AsyncEventHandler<TSender, TArgs> Handlers
    {
        add => this.__handlers.Add(value);
        remove => this.__handlers.Remove(value);
    }

    public event AsyncExceptionHandler<TSender, TArgs> ExceptionHandlers
    {
        add => this.__exception_handlers.Add(value);
        remove => this.__exception_handlers.Remove(value);
    }

    public AsyncEvent()
    {
        this.__handlers = new();
        this.__exception_handlers = new();
    }

    public AsyncEvent
    (
        SynchronizedCollection<AsyncEventHandler<TSender, TArgs>>? handlers,
        SynchronizedCollection<AsyncExceptionHandler<TSender, TArgs>>? exceptionHandlers
    )
    {
        this.__handlers = handlers ?? new();
        this.__exception_handlers = exceptionHandlers ?? new();
    }

    public void Invoke
    (
        TSender sender,
        TArgs args,
        CancellationToken token
    )
    {
        Parallel.ForEach(this.__handlers, async handler =>
        {
            try
            {
                await handler.Invoke(sender, args, token);
            }
            catch(Exception e)
            {
                Parallel.ForEach(this.__exception_handlers, async exceptionHandler =>
                {
                    await exceptionHandler.Invoke(sender, args, e, token);
                });
            }
        });
    }
}
