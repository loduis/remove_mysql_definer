using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace remove_mysql_definer
{
    class Program
    {

        /// <summary>
        /// Ejecuta el programa principal
        /// </summary>
        /// <param name="args">
        /// El primer parametro es el nombre del archivo
        /// Si se da el segundo parametro es el nombre del nuevo archivo
        /// </param>
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string filename = args[0];
                bool isInputGzip = isGzip(filename);

                if (isInputGzip && !File.Exists(filename))
                {
                    filename = filename.Substring(0, filename.LastIndexOf(".gz"));
                    isInputGzip = false;
                }
                
                if (File.Exists(filename))
                {
                    byte[] buffer;
                    string line;
                    string renameFile      = args.Length == 2 ? args[1] : filename;
                    string tmpFile = Guid.NewGuid().ToString("N") + ".tmp";
                    bool isOuputGzip = isGzip(renameFile);
                    
                    DateTime dts      = DateTime.Now;
                    UTF8Encoding utf8 = new UTF8Encoding(true);
                    Stream output     = new FileStream(tmpFile, FileMode.CreateNew, FileAccess.Write);
                    Stream input      = new FileStream(filename, FileMode.Open, FileAccess.Read);

                    Console.WriteLine("Start: " + dts.ToString());
                    Console.WriteLine("Procesando: " + filename + " espere...");
                    
                    if (isInputGzip)
                    {
                        input = new GZipStream(input, CompressionMode.Decompress);
                    }

                    if (isOuputGzip)
                    {
                        output = new GZipStream(output, CompressionMode.Compress);
                    }

                    using (var streamReader = new StreamReader(input))
                    {
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (!line.StartsWith("/*!50013 DEFINER="))
                            {
                                buffer = utf8.GetBytes(line + "\n");
                                output.Write(buffer, 0, buffer.Length);
                            }
                        }                        
                    }
                    input.Close();
                    output.Close();
                    File.Delete(filename);
                    File.Move(tmpFile, renameFile);
                    DateTime dte = DateTime.Now;
                    Console.WriteLine("End: " + dte.ToString());
                    Console.WriteLine("Time: " + (dte - dts).ToString());
                }
                else
                {
                    Console.WriteLine("No existe el archivo: " + filename);
                }
            }
            else
            {
                Console.WriteLine("Deme el nombre del archivo a procesar.");
            }
        }

        private static bool isGzip(string filename)
        {
            return filename.EndsWith(".gz");
        }
    }
}