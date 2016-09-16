using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.Redis;

namespace RedisClientStressApplication
{
    class Program
    {
        static Int32 threads = 100;
        static Int32 iterationsPerThread = 10000;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            IPEndPoint endpoint;
            try
            {
                endpoint = new IPEndPoint(IPAddress.Parse(args[0]), Int32.Parse(args[1]));
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Usage: RedisClientStressApplication <IPAddress> <Port>");
                Console.ReadKey(true);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Usage: RedisClientStressApplication <IPAddress> <Port>");
                Console.ReadKey(true);
                return;
            }

            Trace.Listeners.Add(new ConsoleTraceListener());
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("POOR MAN's STRESS TESTING on " + endpoint);
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Choose a test:");
            Console.WriteLine("1) Object test.");
            Console.WriteLine("2) Simple test.");
            Console.Write("Select:");
            var value = (Char)Console.Read();

            Func<ITestScenario> scenario = null; 
            switch (value)
            {
                case '1':
                    scenario = () => new ObjectOperationTest(endpoint);
                    break;

                case '2':
                    scenario = () => new SimpleOperationTest(endpoint);
                    break;

                default:
                    Console.WriteLine("Invalid selection");
                    break;
            }
            if(scenario != null)
                RunTestScenario(scenario);
        }

        private static void RunTestScenario(Func<ITestScenario> scenarioFactory)
        {
            var list = new List<WeakReference>();
            var results = new List<Double>();
            var total = 0L;
            for (int i = 0; i < 5; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nTest:");
                Console.ResetColor();

                var scenario = scenarioFactory();
                var result = scenario.Test(threads, iterationsPerThread);

                Console.ForegroundColor = ConsoleColor.Green;
                var opts = (result.Item1 / result.Item2.TotalSeconds);
                results.Add(opts);
                Console.WriteLine("\nDone " + result.Item1 + "\t\t" + result.Item2.TotalMilliseconds.ToString(".0") + "\t\t" + opts.ToString(".0") + " op/s.");
                Console.ResetColor();

                list.Add(new WeakReference(scenario));
                scenario = null;
                GC.Collect();
                total += result.Item1;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\nALL DONE " + total + ", " + results.OrderBy(x=>x).Skip(1).Take(results.Count -2).Average().ToString(".0"));
            Console.ResetColor();

            while (true)
            {
                Console.WriteLine("Alive " + list.Count(i => i.IsAlive));
                GC.Collect();
                Console.ReadKey(true);
            }
        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception.GetBaseException();
            LogException(ex);
        }

        private static void LogException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("TERROR: (" + ex.GetType() + ")  " + ex.Message + "\n" + ex.StackTrace);
            Console.ResetColor();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = ((Exception)e.ExceptionObject);
            LogException(ex);
        }

    }
}
