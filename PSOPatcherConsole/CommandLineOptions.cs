using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSOPatcherConsole
{
    public class CommandLineOptions
    {
        [OptionArray('i', "input", Required = true, HelpText = "Input file(s) to be processed. (separated by a space)")]
        public string[] InputFiles { get; set; }

        [Option('p', "port", DefaultValue = 9200, HelpText = "The port to use. Default value is 9200.")]
        public int Port { get; set; }

        [Option('c', "check", DefaultValue = false, HelpText = "Check the patches for errors and warnings.")]
        public bool CheckPatches { get; set; }

        [Option('v', "verbose", DefaultValue = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

    }
}
