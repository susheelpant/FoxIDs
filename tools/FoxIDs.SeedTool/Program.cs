﻿using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using FoxIDs.SeedTool.Infrastructure;
using FoxIDs.SeedTool.SeedLogic;

namespace FoxIDs.SeedTool
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                var serviceProvider = new StartupConfigure().ConfigureServices();

                Console.WriteLine("Select seed action");
                Console.WriteLine("P: Upload risk passwords");

                var key = Console.ReadKey();
                Console.WriteLine(string.Empty);
                Console.WriteLine(string.Empty);

                switch (char.ToLower(key.KeyChar))
                {
                    case 'p':
                        await serviceProvider.GetService<RiskPasswordSeedLogic>().SeedAsync();
                        break;

                    default:
                        Console.WriteLine("Canceled");
                        break;
                }

#if !DEBUG
                Console.WriteLine(string.Empty);
                Console.WriteLine("Click any key to end...");
                Console.ReadKey();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }
    }
}
