﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlubLib.Threading.Tasks;
using Dapper;
using Dapper.FastCrud;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using NeoNetsphere.API;
using NeoNetsphere.LoginAPI;
using Newtonsoft.Json;
using ProudNetSrc;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;


namespace NeoNetsphere
{
  internal class Program
  {
    private static IEventLoopGroup s_apiEventLoopGroup;
    private static IChannel s_apiHost;
    private static IChannel s_loginapiHost;
    private static readonly object s_exitMutex = new object();
    private static bool s_hasExited;
    public static ConcurrentDictionary<XBNType, byte[]> XBNdata = new ConcurrentDictionary<XBNType, byte[]>();

    class ContextEnricher : ILogEventEnricher
    {
      public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
      {
        var max = 20;
        var beginning = "";
        var empty = true;
        var val = logEvent.Properties.FirstOrDefault(x => x.Key == "SourceContext");
        if (val.Value != null)
        {
          beginning += val.Value.ToString().Replace("\"", string.Empty);
          if (beginning.Length > max - 2)
            beginning = beginning.Substring(0, max - 2);
          empty = false;
        }

        var newx = "";
        if (beginning.Length < max)
        {
          var l = (max - beginning.Length) / 2;
          for (var i = 0; i < l; i++)
          {
            newx += " ";
          }
        }

        if (empty)
          newx = new string(' ', max - 1);
        else
          newx = newx + beginning + newx;

        if (newx.Length >= max)
        {
          newx = newx.Substring(1, newx.Length - 1);
        }

        var eventType = propertyFactory.CreateProperty("SrcContext", newx);
        logEvent.AddPropertyIfAbsent(eventType);
      }
    }

    private static void Main()
    {
      JsonConvert.DefaultSettings = () => new JsonSerializerSettings
      {
        Converters = new List<JsonConverter> { new IPEndPointConverter() }
      };

      var jsonlog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AuthServer.json");
      var logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AuthServer.log");
      Log.Logger = new LoggerConfiguration()
          .WriteTo.File(new JsonFormatter(), jsonlog)
          .WriteTo.File(logfile)
          .Enrich.With<ContextEnricher>()
          .WriteTo.Console(
              outputTemplate: "[{Level:u3}] |{SrcContext}| {Message}{NewLine}{Exception}")
          .MinimumLevel.Verbose()
          .CreateLogger();

      var Logger = Log.ForContext(Constants.SourceContextPropertyName, "Initialiazor");

      AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
      TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

      AuthDatabase.Initialize();

      Logger.Information("Servidor de inicio...");

      if (!Directory.Exists("XBN"))
        throw new Exception("Falta la carpeta XBN!");

      foreach (var xbn in Enum.GetValues(typeof(XBNType)).Cast<XBNType>().ToList())
      {
        var name = $"XBN//{xbn.ToString()}.xbn";
        if (File.Exists(name))
        {
          var data = File.ReadAllBytes(name);
          XBNdata.TryAdd(xbn, data);
          Logger.Information($"Cached xbnfile: {name}");
        }
      }

      Network.AuthServer.Initialize(new Configuration());
      Network.AuthServer.Instance.Listen(Config.Instance.Listener);

      s_apiEventLoopGroup = new MultithreadEventLoopGroup(2);
      s_loginapiHost = new ServerBootstrap()
          .Group(s_apiEventLoopGroup)
          .Channel<TcpServerSocketChannel>()
          .Handler(new ActionChannelInitializer<IChannel>(ch => { }))
          .ChildHandler(new ActionChannelInitializer<IChannel>(ch =>
          {
            ch.Pipeline.AddLast(new LoginServerHandler());
          }))
          .BindAsync(Config.Instance.AuthAPI.Listener).WaitEx();
      s_apiHost = new ServerBootstrap()
          .Group(s_apiEventLoopGroup)
          .Channel<TcpServerSocketChannel>()
          .Handler(new ActionChannelInitializer<IChannel>(ch => { }))
          .ChildHandler(new ActionChannelInitializer<IChannel>(ch =>
          {
            ch.Pipeline.AddLast(new APIServerHandler());
          }))
          .BindAsync(Config.Instance.API.Listener).WaitEx();

      Logger.Information("Oscar todo Listo para conexiones!");

            if (Config.Instance.LauncherCheck)
            {
                  Logger.Information("[LauncherCheck] Activated : {0}", Config.Instance.LauncherCheckKey);
            }
            if (Config.Instance.NoobMode)
        Logger.Warning("¡EL MODO NOOB ESTÁ HABILITADO! CADA INICIO DE SESIÓN TIENE ÉXITO Y ANULA LOS DETALLES DE INICIO DE SESIÓN DE LA CUENTA !!!");

      Console.CancelKeyPress += OnCancelKeyPress;
      while (true)
      {
        var input = Console.ReadLine();
        if (input == null)
          break;

        if (input.Equals("exit", StringComparison.InvariantCultureIgnoreCase) ||
            input.Equals("quit", StringComparison.InvariantCultureIgnoreCase) ||
            input.Equals("stop", StringComparison.InvariantCultureIgnoreCase))
          break;
      }

      Exit();
    }

    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
      Exit();
    }

    private static void Exit()
    {
      lock (s_exitMutex)
      {
        if (s_hasExited)
          return;

        Log.Information("Clausura...");
        s_apiHost.CloseAsync().WaitEx(TimeSpan.FromSeconds(1));
        s_apiEventLoopGroup.ShutdownGracefullyAsync().WaitEx(TimeSpan.FromSeconds(1));
        Network.AuthServer.Instance.Dispose();
        s_hasExited = true;
      }
    }

    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
      e.SetObserved();

      if (e.Exception.InnerException is ClosedChannelException)
        return;

      Log.Error(e.Exception, "UnobservedTaskException");
    }

    private static void OnUnhandledException(object s, UnhandledExceptionEventArgs e)
    {
      Log.Error((Exception)e.ExceptionObject, "UnhandledException");
    }
  }

  internal static class AuthDatabase
  {
    // ReSharper disable once InconsistentNaming
    private static readonly ILogger Logger =
        Log.ForContext(Constants.SourceContextPropertyName, nameof(AuthDatabase));

    private static string s_connectionString;

    public static void Initialize()
    {
      Logger.Information("Iniciando...");

      var config = Config.Instance.Database;

      switch (config.Engine)
      {
        case DatabaseEngine.MySQL:
          s_connectionString =
              $"SslMode=none;Server={config.Auth.Host};Port={config.Auth.Port};Database={config.Auth.Database};Uid={config.Auth.Username};Pwd={config.Auth.Password};Pooling=true;";
          OrmConfiguration.DefaultDialect = SqlDialect.MySql;

          using (var con = Open())
          {
            if (con.QueryFirstOrDefault($"SHOW DATABASES LIKE \"{config.Auth.Database}\"") == null)
            {
              Logger.Error($"Database '{config.Auth.Database}' not found");
              Environment.Exit(0);
            }
          }

          break;

        case DatabaseEngine.SQLite:
          s_connectionString = $"Data Source={config.Auth.Filename};Pooling=true;";
          OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

          if (!File.Exists(config.Auth.Filename))
          {
            Logger.Error($"Database '{config.Auth.Filename}' not found");
            Environment.Exit(0);
          }

          break;

        default:
          Logger.Error($"Invalid database engine {config.Engine}");
          Environment.Exit(0);
          return;
      }
    }

    public static IDbConnection Open()
    {
      var engine = Config.Instance.Database.Engine;
      IDbConnection connection;
      switch (engine)
      {
        case DatabaseEngine.MySQL:
          connection = new MySqlConnection(s_connectionString);
          break;

        case DatabaseEngine.SQLite:
          connection = new SqliteConnection(s_connectionString);
          break;

        default:
          Logger.Error($"Invalid database engine {engine}");
          Environment.Exit(0);
          return null;
      }

      connection.Open();
      return connection;
    }
  }
}
