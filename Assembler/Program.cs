/*******************************************************************************
*** NAME:       JEFF KURTZ                                                   ***
*** CLASS:      CSC354 - INTRO TO SYSTEMS PROGRAMMING                        ***
*** ASSIGNMENT: #4 - PASS TWO                                                ***
*** DUE DATE:   NOVEMBER 15th, 2018                                          ***
*** INSTRUCTOR: WERPY                                                        ***
********************************************************************************
*** DESCRIPTION: This program takes in a file through the command line. That ***
*** file contains assembly instructions which is then parsed and interpreted ***
*** by the Pass One function. Each line is then written to a .tmp file. Then ***
*** that .tmp parsed and interpreted by the Pass Two function. Each line is  ***
*** then written to a .txt file and the object program that is compiled is   ***
*** written to a .o file. The information is then outputed to the screen.    ***
********************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

namespace Assembler
{
    class Program
    {
        static void Main(string[] args)
        {
            Node_P1 p1 = new Node_P1();
            Node_P2 p2 = new Node_P2();
            OpCodes OpCodes = new OpCodes();
            PassOne passOne = new PassOne();
            PassTwo passTwo = new PassTwo();
            Literal_Table LitTab = new Literal_Table();
            Symbol_Table SymTab = new Symbol_Table();
            List<Node_Op> op = new List<Node_Op>();

            string opCodes = "OPCODES.DAT";
            string srcProg;

            if (args.Length == 0){
                Console.Write("Please Enter File Path for Source file --> ");
                srcProg = Console.ReadLine();
            }
            else
                srcProg = args[0];


            while (true)
            {
                if (!File.Exists(opCodes))
                {
                    Console.WriteLine("OPSCODE FILE: '{0}' does not exist...", opCodes);
                    Console.Write("Please enter correct file path (Type 'x' to exit) -->  ");
                    opCodes = Console.ReadLine();
                    if (opCodes == "x")
                        break;
                    Console.WriteLine();
                }
                else if (!File.Exists(srcProg))
                {
                    Console.WriteLine("SOURCE FILE: '{0}' does not exist...", srcProg);
                    Console.Write("Please enter correct file path (Type 'x' to exit) -->  ");
                    srcProg = Console.ReadLine();
                    if (srcProg == "x")
                        break;
                    Console.WriteLine();
                }
                else
                {
                    string[] Codes = File.ReadAllLines(opCodes);
                    string[] opLine;

                    string file = string.Format(srcProg, ".tmp");

                    //Populate the Opcodes
                    foreach (string code in Codes)
                    {
                        opLine = code.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        op.Add(OpCodes.insert(opLine));
                    }

                    //PASS ONE
                    p1 = passOne.PassUno(srcProg, op);

                    //PASS ONE TO FILES
                    string tmpFile1 = srcProg.Substring(0, srcProg.LastIndexOf(".")) + ".tmp";
                    File.WriteAllLines(tmpFile1, p1.prog);

                    //PASS TWO
                    p2 = passTwo.PassDuo(op, p1.table, p1.lit, tmpFile1);

                    //PASS TWO TO FILES
                    string tmpFile2 = srcProg.Substring(0, srcProg.LastIndexOf(".")) + ".txt";
                    File.WriteAllLines(tmpFile2, p2.prog);

                    string tmpFile3 = srcProg.Substring(0, srcProg.LastIndexOf(".")) + ".o";
                    File.WriteAllLines(tmpFile3, p2.obj_prog);

                    //PRINTS PASS 2 w/ Length
                    Console.WriteLine("------------");
                    Console.WriteLine("PASS 2");
                    Console.WriteLine("------------");
                    foreach (string p in p2.prog)
                        Console.WriteLine(p);
                    Console.WriteLine("Program Length = {0}", p1.length);
                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();

                    //PRINTS OBJECT PROGRAM
                    Console.WriteLine("------------------");
                    Console.WriteLine("OBJECT PROGRAM");
                    Console.WriteLine("------------------");
                    foreach (string o in p2.obj_prog)
                        Console.WriteLine(o);
                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();

                    //PRINTS SYMBOL TABLE
                    Console.WriteLine("---------------");
                    Console.WriteLine("SYMBOL TABLE");
                    Console.WriteLine("---------------");
                    Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10}", "SYMBOL:", "VALUE:", "RFLAG:", "MFLAG:", "IFLAG:");
                    Console.WriteLine(new string('-', 50));
                    SymTab.view(p1.table);
                    Console.WriteLine("Press enter to continue...");
                    Console.ReadLine();

                    //PRINTS LITERAL TABLE
                    Console.WriteLine("-----------------");
                    Console.WriteLine("LITERAL TABEL");
                    Console.WriteLine("-----------------");
                    Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", "NAME:", "VALUE:", "LENGTH:", "ADDRESS:");
                    Console.WriteLine(new string('-', 40));
                    LitTab.view(p1.lit);
                    Console.WriteLine("Press enter to Complete Pass Two...");
                    break;

                }
            }
            Console.ReadLine();
        }
    }
}
