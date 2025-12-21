//using Dapper;
//using Gimzo.Infrastructure.Database;
//using Microsoft.Extensions.Logging;
//using System.Collections.Concurrent;
//using System.Data;

//namespace Gimzo.Infrastructure.Logging;

//public class LoggerProvider : ILoggerProvider
//{
//    private readonly ConcurrentQueue<Database.DataAccessObjects.Log> _logQueue;
//    private readonly ConcurrentQueue<Database.DataAccessObjects.AppEvent> _appEventQueue;

//    private bool _runQueues = true;

//    private readonly Func<string, LogLevel, bool>? _filter;

//    private readonly IDbConnection _connection;
//    public LoggerProvider(DbDef cmdDb, Func<string, LogLevel, bool>? filter = null)
//    {
//        _connection = cmdDb.GetConnection();

//        _filter = filter;

//        _logQueue = new ConcurrentQueue<Database.DataAccessObjects.Log>();
//        _appEventQueue = new ConcurrentQueue<Database.DataAccessObjects.AppEvent>();

//        _runQueues = true;

//        RunLoggerDequeue();
//    }

//    public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName, _filter);

//    internal void PreserveLog(Database.DataAccessObjects.Log? logDao)
//    {
//        if (logDao is not null && _runQueues)
//        {
//            _logQueue.Enqueue(logDao);
//        }
//    }

//    internal void PreserveLog(Database.DataAccessObjects.AppEvent appEventDao)
//    {
//        if (appEventDao is not null && _runQueues)
//        {
//            _appEventQueue.Enqueue(appEventDao);
//        }
//    }

//    private void RunLoggerDequeue()
//    {
//        Task.Run(() =>
//        {
//            while (_runQueues)
//            {
//                if (_logQueue.TryDequeue(out Database.DataAccessObjects.Log? logItem))
//                {
//                    if (logItem is not null)
//                    {
//                        _connection.Execute(SqlRepository.Logs.Insert, logItem);
//                    }
//                }
//            }
//            while (!_logQueue.IsEmpty)
//            {
//                if (_logQueue.TryDequeue(out Database.DataAccessObjects.Log? logItem))
//                {
//                    if (logItem is not null)
//                    {
//                        _connection.Execute(SqlRepository.Logs.Insert, logItem);
//                    }
//                }
//            }
//        });

//        Task.Run(() =>
//        {
//            while (_runQueues)
//            {
//                if (_appEventQueue.TryDequeue(out Database.DataAccessObjects.AppEvent? appEvent))
//                {
//                    if (appEvent is not null)
//                    {
//                        _connection.Execute(SqlRepository.AppEvents.Insert, appEvent);
//                    }
//                }
//            }
//            while (!_appEventQueue.IsEmpty)
//            {
//                if (_appEventQueue.TryDequeue(out Database.DataAccessObjects.AppEvent? appEvent))
//                {
//                    if (appEvent is not null)
//                    {
//                        _connection.Execute(SqlRepository.AppEvents.Insert, appEvent);
//                    }
//                }
//            }
//        });
//    }

//    public void Dispose()
//    {
//        const int NumberOfCycles = 10;
//        const int MsSleepTime = 20;

//        _runQueues = false;

//        int i = 0;

//        while (!_logQueue.IsEmpty && i++ < NumberOfCycles)
//        {
//            Thread.Sleep(MsSleepTime);
//        }

//        i = 0;

//        while (!_appEventQueue.IsEmpty && i++ < NumberOfCycles)
//        {
//            Thread.Sleep(MsSleepTime);
//        }
//    }
//}
