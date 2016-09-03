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
        static Int32 userCount = 100;
        static Int32 loops = 100;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            Trace.Listeners.Add(new ConsoleTraceListener());
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("POOR MAN's STRESS TESTING\n");
            Console.ResetColor();
            Console.WriteLine("Choose a test:");
            Console.WriteLine("1) Object test.");
            Console.Write("Select:");
            var value = (Char)Console.Read();
            Func<ITestScenario> scenario = null; ;
            switch (value)
            {
                case '1':
                    scenario = () => new ObjectOperationTest();
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
            for (int i = 0; i < 10; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nTest:");
                Console.ResetColor();

                var scenario = scenarioFactory();
                var result = scenario.Test(userCount, loops);

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
