using System;
using System.Collections.Generic;
using Fclp.Internals.Extensions;

namespace Ui.Console.Startup
{
    public class Help
    {
        private readonly IEnumerable<string> usage;
        private readonly IEnumerable<Tuple<string, string>> operations;
        private readonly IEnumerable<Tuple<string, string>> arguments;
        private readonly IEnumerable<string> examples;

        public Help()
        {
            string newLine = Environment.NewLine;            
            string lineBreak = $"{newLine} --------------------------------------{newLine}";
            
            usage = new List<string>
            {
                newLine,
                "Usage: cc <operation> [specifier] [arguments]",
                lineBreak
            };
            
            operations = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Operation", "--create (-c)"),
                new Tuple<string, string>("", "--verify (-v)"),
                new Tuple<string, string>("", "--convert"),
                new Tuple<string, string>("", ""),
                new Tuple<string, string>("Specifier", "[key | signature]"),
                new Tuple<string, string>("", lineBreak)
            };
            
            arguments = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Operation arguments (* = default):", newLine),
                new Tuple<string, string>("Public key path", "--publickey [path]"),
                new Tuple<string, string>("Private key path", "--privatekey [path]"),
                new Tuple<string, string>("Key type", "--keytype (-k) [*rsa | dsa | ec | elgamal]"),
                new Tuple<string, string>("Key size", "--keysize (-b) [size in bits *4096]"),
                new Tuple<string, string>("EC key curve", "--curve [curve name *curve25519]"),
                new Tuple<string, string>("Use RFC 3526 prime for ElGamal", "--fast"),
                new Tuple<string, string>("Key encryption", "--encryption (-e) [pkcs]"),
                new Tuple<string, string>("Encryption password", "--password (-p) [password]"),
                new Tuple<string, string>("Input file path", "--file (-f) [path]"),
                new Tuple<string, string>("Stdin content", "--in (-i) [content]"),
                new Tuple<string, string>("Output path", "--out (-o) [output path]"),
                new Tuple<string, string>("Key output type", "--type (-t) [pem | *der | openssh | ssh2]"),
                new Tuple<string, string>("Signature path or content", "--signature (-s) [path or content]"),
                new Tuple<string, string>("", lineBreak)
            };
            
            examples = new List<string>
            {
                "Examples:",
                "",
                "Create key: cc -c key -b 2048 -e pkcs -p mypassword --privatekey private.der --publickey public.der",
                "Create EC key: cc -c key -k ec --curve curve25519 --privatekey private.der --publickey public.der",
                "Create signature: cc -c signature --privatekey private.der -p mypassword -f foobar.file -o foobar.file.signature",
                "Verify keypair: cc -v key --privatekey private.der -p mypassword --publickey public.der",
                "Verify signature: cc -v signature --publickey public.der -f foobar.file -s foobar.file.signature",
                "Convert der to pem: cc --convert -t pem --privatekey private.der --publickey public.der",
                "Convert pem to openssh: cc --convert -t openssh --publickey public.pem"
            };
        }

        public void Show()
        {
            usage.ForEach(line => System.Console.WriteLine($"{line}"));
            operations.ForEach(pair => System.Console.WriteLine("{0,-16} {1,-8}", pair.Item1, pair.Item2));
            arguments.ForEach(pair => System.Console.WriteLine("{0,-30} {1,-15}", pair.Item1, pair.Item2));
            examples.ForEach(System.Console.WriteLine);
        }
    }
}