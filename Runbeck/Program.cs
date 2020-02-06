using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Runbeck
{
    class Program
    {
        static void Main(string[] args)
        {
            var parameters = GetParameters();
            var inputFile = ReadFile(parameters.InputFilePath, parameters.InputFormat);
            var desiredOutput = inputFile.Skip(1).Where(f => f.Length == parameters.FieldCount);
            var otherOutput = inputFile.Skip(1).Where(f => f.Length != parameters.FieldCount);
            WriteFile($@"{parameters.OutputDirectory}\outputFileDesired.txt", desiredOutput);
            WriteFile($@"{parameters.OutputDirectory}\outputFileOther.txt", otherOutput);
        }

        static Parameters GetParameters()
        {
            // *** Get input file path
            var parameters = new Parameters();
            Console.WriteLine("Where is the file located?");
            var inputFilePath = "";
            while (!File.Exists(inputFilePath))
            {
                if (inputFilePath !="")
                    Console.WriteLine("File does not exist.  Please check the path.");

                inputFilePath = Console.ReadLine();
            }
            parameters.InputFilePath = inputFilePath;


            // *** Get file format ***
            Console.WriteLine("Is the file format CSV (comma-separated values) or TSV (tab-separated values)?");

            var fileFormatInput = "";
            FileFormat fileFormat;

            while (!Enum.TryParse(fileFormatInput, out fileFormat)) // Validate input
            {
                if (fileFormatInput != "") // Present valid options.
                    Console.WriteLine($"Valid formats: {parameters.GetValidFileFormats()}");

                fileFormatInput = Console.ReadLine().Trim().ToUpper();
            }
            parameters.InputFormat = fileFormat;


            // *** Get field count ***
            Console.WriteLine("How many fields should each record contain?");
            byte fieldCount = 0;
            while (!byte.TryParse(Console.ReadLine(), out fieldCount) | fieldCount == 0 | fieldCount >byte.MaxValue)
            {
                Console.WriteLine("Please enter a valid number (1-255).");
            }
            parameters.FieldCount = fieldCount;

            // *** Get output directory ***
            //Console.WriteLine("Where is the output directory?");
            //var outputDirectory = "";
            //while (!Directory.Exists(outputDirectory))
            //{
            //    if(outputDirectory!="")
            //        Console.WriteLine("Directory not found.  Please provide a valid directory.");
            //    outputDirectory = Console.ReadLine();
            //}
            //parameters.OutputDirectory = outputDirectory;
            parameters.OutputDirectory = Path.GetDirectoryName(parameters.InputFilePath);
            return parameters;
        }
        static List<string[]> ReadFile(string filePath, FileFormat format)
        {
            var results = new List<string[]>();
            var file = File.ReadAllLines(filePath);
            var delimeter = GetDescription(format);
            foreach (var line in file) // Parse each line
            {
                var fields = line.Split(char.Parse(delimeter));
                results.Add(fields);
            };
            return results;
        }
        static void WriteFile(string filePath, IEnumerable<string[]> lines, string delimeter = ",")
        {
            using(var writer = new StreamWriter(filePath))
            {
                foreach(var line in lines)
                {
                    writer.WriteLine(string.Join(delimeter,line));
                }
            }
        }
        public static string GetDescription(Enum value)
        {
            return
                value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description
                ?? value.ToString();
        }
    }
    public class Line
    {
        List<string> Fields { get; set; } = new List<string>();
    }
    
    public struct Parameters
    {
        public string InputFilePath { get; set; }
        public string OutputDirectory { get; set; }
        public FileFormat InputFormat { get; set; }
        public byte FieldCount { get;set; }
        public string GetValidFileFormats(string delimiter = ",")
        {
            var list = Enum.GetNames(typeof(FileFormat)).ToList();
            var delimited = string.Join(delimiter + " ", list);
            return delimited;
        }
    }
    public enum FileFormat
    {   [Description(",")]
        CSV,
        [Description("\t")]
        TSV
    }
}
