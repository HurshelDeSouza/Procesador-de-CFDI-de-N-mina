using System;
using System.IO;
using CFDIProcessor.Data;
using CFDIProcessor.Services;
using Microsoft.Extensions.Configuration;

namespace CFDIProcessor
{
    /// <summary>
    /// Aplicación de consola para procesar archivos XML de CFDI de nómina
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         Procesador de CFDI - Versión Completa             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                // Cargar configuración desde appsettings.json
                var configuration = LoadConfiguration();

                // Seleccionar tipo de CFDI a procesar
                string tipoCfdi = SelectTipoCfdi();

                // Obtener la ruta de la carpeta
                string folderPath = GetFolderPath(args);

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    Console.WriteLine("Error: Debe proporcionar una ruta válida.");
                    return;
                }

                // Procesar archivos según el tipo seleccionado
                ProcessFiles(folderPath, tipoCfdi);

                Console.WriteLine();
                Console.WriteLine("Proceso completado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error crítico: {ex.Message}");
                Console.ResetColor();
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Detalle: {ex.InnerException.Message}");
                }

                #if DEBUG
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                #endif
            }

            Console.WriteLine();
            
            // Solo esperar tecla si hay consola disponible (no en modo redirigido)
            if (Environment.UserInteractive && !Console.IsInputRedirected)
            {
                Console.WriteLine("Presione cualquier tecla para salir...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Permite al usuario seleccionar el tipo de CFDI a procesar
        /// </summary>
        private static string SelectTipoCfdi()
        {
            Console.WriteLine("Seleccione el tipo de CFDI a procesar:");
            Console.WriteLine("  1. Nómina");
            Console.WriteLine("  2. Ingreso y Egreso (Facturas)");
            Console.WriteLine("  3. Pagos 2.0");
            Console.WriteLine("  4. Todos (automático según tipo)");
            Console.WriteLine();
            Console.Write("Opción (1-4): ");
            
            string opcion = Console.ReadLine();
            Console.WriteLine();

            return opcion switch
            {
                "1" => "nomina",
                "2" => "ingreso-egreso",
                "3" => "pagos",
                "4" => "todos",
                _ => "todos"
            };
        }

        /// <summary>
        /// Carga la configuración desde appsettings.json
        /// </summary>
        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        /// <summary>
        /// Obtiene la ruta de la carpeta desde argumentos o entrada del usuario
        /// </summary>
        private static string GetFolderPath(string[] args)
        {
            // Si se proporciona como argumento de línea de comandos
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                return args[0];
            }

            // Solicitar al usuario
            Console.Write("Ingrese la ruta de la carpeta con los archivos XML de nómina: ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Procesa los archivos XML de la carpeta especificada
        /// </summary>
        private static void ProcessFiles(string folderPath, string tipoCfdi)
        {
            using (var context = new DescargaCfdiGfpContext())
            {
                // Verificar conexión a la base de datos
                Console.WriteLine("Verificando conexión a la base de datos...");
                
                if (!context.Database.CanConnect())
                {
                    throw new InvalidOperationException(
                        "No se pudo conectar a la base de datos. Verifique la cadena de conexión en appsettings.json");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Conexión exitosa a la base de datos.");
                Console.ResetColor();
                Console.WriteLine();

                // Procesar según el tipo seleccionado
                switch (tipoCfdi)
                {
                    case "nomina":
                        Console.WriteLine("📋 Procesando CFDI de Nómina...");
                        Console.WriteLine();
                        var nominaProcessor = new NominaXmlProcessor(context);
                        nominaProcessor.ProcessXmlFilesFromFolder(folderPath);
                        break;

                    case "ingreso-egreso":
                        Console.WriteLine("📋 Procesando CFDI de Ingreso y Egreso...");
                        Console.WriteLine();
                        var ingresoEgresoProcessor = new IngresoEgresoXmlProcessor(context);
                        ingresoEgresoProcessor.ProcessXmlFilesFromFolder(folderPath);
                        break;

                    case "pagos":
                        Console.WriteLine("📋 Procesando CFDI de Pagos 2.0...");
                        Console.WriteLine();
                        var pagosProcessor = new PagosXmlProcessor(context);
                        pagosProcessor.ProcessXmlFilesFromFolder(folderPath);
                        break;

                    case "todos":
                        Console.WriteLine("📋 Procesando todos los tipos de CFDI...");
                        Console.WriteLine();
                        ProcessAllTypes(context, folderPath);
                        break;
                }
            }
        }

        /// <summary>
        /// Procesa todos los tipos de CFDI automáticamente
        /// </summary>
        private static void ProcessAllTypes(DescargaCfdiGfpContext context, string folderPath)
        {
            Console.WriteLine("--- Procesando Nómina ---");
            var nominaProcessor = new NominaXmlProcessor(context);
            nominaProcessor.ProcessXmlFilesFromFolder(folderPath);

            Console.WriteLine();
            Console.WriteLine("--- Procesando Ingreso y Egreso ---");
            var ingresoEgresoProcessor = new IngresoEgresoXmlProcessor(context);
            ingresoEgresoProcessor.ProcessXmlFilesFromFolder(folderPath);

            Console.WriteLine();
            Console.WriteLine("--- Procesando Pagos 2.0 ---");
            var pagosProcessor = new PagosXmlProcessor(context);
            pagosProcessor.ProcessXmlFilesFromFolder(folderPath);
        }
    }
}
