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
            Console.WriteLine("=== Procesador de CFDI de Nómina ===");
            Console.WriteLine();

            try
            {
                // Cargar configuración desde appsettings.json
                var configuration = LoadConfiguration();

                // Obtener la ruta de la carpeta
                string folderPath = GetFolderPath(args);

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    Console.WriteLine("Error: Debe proporcionar una ruta válida.");
                    return;
                }

                // Procesar archivos
                ProcessFiles(folderPath);

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
            Console.WriteLine("Presione cualquier tecla para salir...");
            Console.ReadKey();
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
        private static void ProcessFiles(string folderPath)
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

                // Procesar los archivos XML
                var processor = new NominaXmlProcessor(context);
                processor.ProcessXmlFilesFromFolder(folderPath);
            }
        }
    }
}
