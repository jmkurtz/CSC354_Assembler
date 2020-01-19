/*******************************************************************************
*** NAME:       JEFF KURTZ                                                   ***
*** CLASS:      CSC354 - INTRO TO SYSTEMS PROGRAMMING                        ***
*** ASSIGNMENT: #4 - PASS TWO                                                ***
*** DUE DATE:   NOVEMBER 15th, 2018                                          ***
*** INSTRUCTOR: WERPY                                                        ***
********************************************************************************
*** DESCRIPTION: This Pass 2 funciton takes in information from the .tmp     ***
*** file and outputs the Object Code for each line that applies, and then    ***
*** creates the object program to the assembly code that was given intially. ***
********************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assembler
{
    class PassTwo
    {

        /*******************************************************************************
        ***                      PASSDUO function                                    ***
        ********************************************************************************
        *** DESCRIPTION: This Pass 2 funciton takes in information from the .tmp     ***
        *** file and outputs the Object Code for each line that applies, and then    ***
        *** creates the object program to the assembly code that was given intially. ***
        ********************************************************************************
        *** INPUT ARGS: List<Node_Op> ops, Node_Sym table, Node_Lit lit, string file ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node_P2 p2                                                       ***
        ********************************************************************************/

        public Node_P2 PassDuo(List<Node_Op> ops, Node_Sym table, Node_Lit lit, string file)
        {
            Node_P2 p2 = new Node_P2();
            Node_Op op = new Node_Op();
            NIXBPE nixbpe = new NIXBPE();
            Node_Sym sym = new Node_Sym();
            Node_Exp ex = new Node_Exp();
            Registers reg = new Registers();

            Literal_Table litTab = new Literal_Table();
            Expressions Exp = new Expressions();
            OpCodes opCode = new OpCodes();
            Symbol_Table syTab = new Symbol_Table();
            List<string> codes = new List<string>();
            List<string> mrec = new List<string>();
            string[] Prog = File.ReadAllLines(file);

            int lnum;
            string counter;
            string label = null;
            string operation;
            string operand = null;
            string rrec = null;
            string drec = null;
            string prog_len = "00000";
            string prog_start = "00000";
            string prog_name = "00000";

            string Obj_Code = null;
            string[] splitLine;
            string tmpStr;
            string tmpFmt = null;
            bool Base_bit = false;
            string Base_Label = null;

            foreach(string line in Prog)
            {
                splitLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitLine.Length != 1)
                {
                    //SETS LINE NUMBER
                    if (Int32.TryParse(splitLine[0], out lnum)) { };

                    //SETS LOAD COUNTER
                    counter = splitLine[1];

                    //SETS LABEL
                    if (splitLine.Length == 5)
                    {
                        label = splitLine[2].ToUpper();
                        splitLine = splitLine.Where(w => w != splitLine[0]).ToArray();
                    }
                    else
                    {
                        tmpStr = splitLine[2];
                        if (tmpStr[0] != '+' && opCode.search(ops, tmpStr) == null)
                        {
                            label = tmpStr.ToUpper();
                            splitLine = splitLine.Where(w => w != splitLine[2]).ToArray();
                        }
                    }

                    //SETS OPERATION
                    operation = splitLine[2].ToUpper();

                    //SETS OPERAND
                    if (splitLine.Length > 3)
                        operand = splitLine[3];

                    if (label == Base_Label && int.Parse(counter, System.Globalization.NumberStyles.HexNumber) > int.Parse("7FF", System.Globalization.NumberStyles.HexNumber))
                        syTab.search(table, label).value = SubHex(counter, "7FF").TrimStart('0').PadLeft(5, '0');
                    else
                    {
                        if (operation == "START")
                        {
                            prog_start = splitLine[3]; //FOR THE HEADER
                            prog_name = label; //FOR THE HEADER
                        }
                        else if (operation != "END")
                        {
                            //FORMAT 4 CHECK
                            if (operation[0] != '+')
                                op = opCode.search(ops, operation);
                            else
                            {
                                tmpStr = operation.TrimStart('+');
                                op = opCode.search(ops, tmpStr);
                                tmpFmt = "4";
                            }

                            if (op == null)
                            {
                                if (operation == "BASE") { Base_Label = operand; }
                                else if (operation == "BYTE")
                                {
                                    if (operand[0] == 'X')
                                        Obj_Code = operand.TrimStart('X', '\'').TrimEnd('\'').PadLeft(6, '0');
                                    else
                                        Obj_Code = BitConverter.ToString(Encoding.Default.GetBytes(operand.TrimStart('\'', 'C', 'c').TrimEnd('\''))).Replace("-", "").PadLeft(6, '0');
                                }
                                else if (operation == "EQU") { }
                                else if (operation == "EXTDEF")
                                {
                                    foreach (string n in operand.Split(','))
                                        drec = drec + "^" + n + "^" + syTab.search(table, TrimOp(n)).value;
                                }
                                else if (operation == "EXTREF")
                                {
                                    foreach (string n in operand.Split(','))
                                        rrec = rrec + "^" + n;
                                }
                                else if (operation == "RESB") { }
                                else if (operation == "RESW") { }
                                else if (operation == "WORD")
                                {
                                    Obj_Code = int.Parse(operand, System.Globalization.NumberStyles.HexNumber).ToString("X").PadLeft(6, '0');
                                    if (operand.Contains('+') || operand.Contains('-'))
                                        mrec.Add("M^" + AddHex(counter.PadLeft(6, '0'), "1") + "^06^");
                                }
                            }
                            else if (tmpFmt != null)
                            {
                                nixbpe = setNIXBPE(table, operation, operand, Base_bit);
                                if (operand[0] == '#') { Obj_Code = ObjPtOne(op.code, nixbpe) + counter; }
                                else if (operand[0] == '@') { Obj_Code = ObjPtOne(op.code, nixbpe) + Exp.insert(table, operand).value; }
                                else { Obj_Code = ObjPtOne(op.code, nixbpe) + Exp.insert(table, operand).value; }
                                mrec.Add("M^" + AddHex(counter, "1").PadLeft(6, '0') + "^05^+" + operand.TrimStart('@', '#').TrimEnd(',', 'X'));
                            }
                            else
                            {
                                if (op.format == "1") { Obj_Code = op.code; }
                                else if (op.format == "2") { Obj_Code = op.code + fmt2(operand); }
                                else if (op.format == "3")
                                {
                                    nixbpe = setNIXBPE(table, operation, operand, Base_bit);
                                    if (nixbpe.n == true && nixbpe.i == false && nixbpe.x == false)  //IF INDIRECT '@'
                                        Obj_Code = ObjPtOne(op.code, nixbpe) + SubHex(Exp.insert(table, operand).value, getNext(op, counter)).TrimStart('0').PadLeft(3, '0');
                                    else if (nixbpe.n == false && nixbpe.i == true && nixbpe.x == false)  //IF IMMEDIATE '#'
                                        Obj_Code = ObjPtOne(op.code, nixbpe) + Exp.insert(table, operand).value.TrimStart('0').PadLeft(3, '0');
                                    else if (nixbpe.n == false && nixbpe.i == false && nixbpe.x == true)  //IF INDEXED ',X'
                                        Obj_Code = ObjPtOne(op.code, nixbpe) + SubHex(Exp.insert(table, operand).value, getNext(op, counter)).TrimStart('0').PadLeft(3, '0');
                                    else
                                    {
                                        if (operand[0] == '=')
                                            Obj_Code = ObjPtOne(op.code, nixbpe) + SubHex(litTab.search(lit, operand).address, getNext(op, counter)).TrimStart('0').PadLeft(3, '0');
                                        else
                                            Obj_Code = ObjPtOne(op.code, nixbpe) + SubHex(Exp.insert(table, operand).value, getNext(op, counter)).TrimStart('0').PadLeft(3, '0');
                                    }
                                }
                            }
                        }
                    }

                    if(operation == "*")
                        Obj_Code = litTab.search(lit, operand).value;
                }
                else
                    prog_len = splitLine[0];

                //ADD THE OBJ CODE TO THE P2 NODE
                if(splitLine.Length != 1)
                    p2.prog.Add(line + string.Format("{0,-10}", Obj_Code));

                if(Obj_Code != null)
                    codes.Add(Obj_Code);

                //SET VARIABLES BACK TO NULL
                tmpFmt = null;
                tmpStr = null;
                label = null;
                Obj_Code = null;
            }

            p2.obj_prog.Add("H^" + prog_name + "^" + prog_start.PadLeft(5, '0') + "^" + prog_len.PadLeft(5, '0'));
            if (drec != null) { p2.obj_prog.Add("D" + drec); }
            if (rrec != null) { p2.obj_prog.Add("R" + rrec); }
            p2.obj_prog = T_Records(p2.obj_prog, codes);
            foreach(string m in mrec)
                p2.obj_prog.Add(m);
            p2.obj_prog.Add("E^" + prog_start.PadLeft(6, '0'));
            return p2;
        }

        /*******************************************************************************
        ***                      OBJPTONE function                                   ***
        ********************************************************************************
        *** DESCRIPTION: This function handles the first three digits of the Object  ***
        *** code.                                                                    ***
        ********************************************************************************
        *** INPUT ARGS: string op, NIXBPE bits                                       ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: First three digits of the object code                            ***
        ********************************************************************************/

        public string ObjPtOne(string op, NIXBPE bits)
        {
            int N = 0, I = 0, X = 0, B = 0, P = 0, E = 0;

            if (bits.n == true) { N = 32; }
            if (bits.i == true) { I = 16; }
            if (bits.x == true) { X = 8; }
            if (bits.b == true) { B = 4; }
            if (bits.p == true) { P = 2; }
            if (bits.e == true) { E = 1; }

            return (int.Parse(op.PadRight(3, '0'), System.Globalization.NumberStyles.HexNumber) + (N + I + X + B + P + E)).ToString("X").PadLeft(3, '0');
        }

        /*******************************************************************************
        ***                      setNIXBPE function                                  ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in line and outputs the bits            ***
        ********************************************************************************
        *** INPUT ARGS: Node_Sym table, string opt, string opd, bool b               ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: NIXBPE bit                                                       ***
        ********************************************************************************/

        public NIXBPE setNIXBPE(Node_Sym table, string opt, string opd, bool b)
        {
            NIXBPE bit = new NIXBPE();
            Expressions ex = new Expressions();

            if (opt[0] == '+') { bit.e = true; }
            else if (b == true) { bit.b = true; }
            else { bit.p = true; }

            bit.n = ex.insert(table, opd).N_bit;
            bit.i = ex.insert(table, opd).I_bit;
            bit.x = ex.insert(table, opd).X_bit;

            return bit;
        }

        /*******************************************************************************
        ***                      getNext function                                    ***
        ********************************************************************************
        *** DESCRIPTION: gets the next location in the counter                       ***
        ********************************************************************************
        *** INPUT ARGS: Node_Op, string count                                        ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: The next location in the counter                                 ***
        ********************************************************************************/

        public string getNext(Node_Op op, string count)
        {
            return AddHex(count, op.format);
        }

        /*******************************************************************************
        ***                      fmt2 function                                       ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in and handles the last digits in       ***
        *** format two instructions.                                                 ***
        ********************************************************************************
        *** INPUT ARGS: string op                                                    ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: string                                                           ***
        ********************************************************************************/

        public string fmt2(string op)
        {
            Registers reg = new Registers();
            string[] newop = null;
            if (op.Contains(','))
                newop = op.Split(',');
            string theString = null;

            if (newop != null)
            {
                if (newop[0].ToUpper() == "TWO")
                {
                    Console.WriteLine("NICE");
                } 
                else if (newop[0].ToUpper() == "A") { theString = reg.A; }
                else if (newop[0].ToUpper() == "X") { theString += reg.X; }
                else if (newop[0].ToUpper() == "L") { theString += reg.L; }
                else if (newop[0].ToUpper() == "B") { theString += reg.B; }
                else if (newop[0].ToUpper() == "S") { theString += reg.S; }
                else if (newop[0].ToUpper() == "T") { theString += reg.T; }
                else if (newop[0].ToUpper() == "F") { theString += reg.F; }
                else if (newop[0].ToUpper() == "PC") { theString += reg.PC; }
                else if (newop[0].ToUpper() == "SW") { theString += reg.SW; }

                if (newop[1].ToUpper() == "A") { theString += reg.A; }
                else if (newop[1].ToUpper() == "X") { theString += reg.X; }
                else if (newop[1].ToUpper() == "L") { theString += reg.L; }
                else if (newop[1].ToUpper() == "B") { theString += reg.B; }
                else if (newop[1].ToUpper() == "S") { theString += reg.S; }
                else if (newop[1].ToUpper() == "T") { theString += reg.T; }
                else if (newop[1].ToUpper() == "F") { theString += reg.F; }
                else if (newop[1].ToUpper() == "PC") { theString += reg.PC; }
                else if (newop[1].ToUpper() == "SW") { theString += reg.SW; }
            }
            else
            {
                if (op.ToUpper() == "A") { theString = reg.A; }
                else if (op.ToUpper() == "X") { theString += reg.X; }
                else if (op.ToUpper() == "L") { theString += reg.L; }
                else if (op.ToUpper() == "B") { theString += reg.B; }
                else if (op.ToUpper() == "S") { theString += reg.S; }
                else if (op.ToUpper() == "T") { theString += reg.T; }
                else if (op.ToUpper() == "F") { theString += reg.F; }
                else if (op.ToUpper() == "PC") { theString += reg.PC; }
                else if (op.ToUpper() == "SW") { theString += reg.SW; }
            }

            return theString;
        }

        /*******************************************************************************
        ***                      TrimOp function                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in labels and trims them in needed      ***
        ********************************************************************************
        *** INPUT ARGS: string                                                       ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: string                                                           ***
        ********************************************************************************/

        public string TrimOp(string op)
        {
            if (op.Length > 4)
                op = op.Remove(4);
            return op;
        }

        /*******************************************************************************
        ***                      AddHex function                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes two hex strings and adds them           ***
        ********************************************************************************
        *** INPUT ARGS: string, string                                               ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: string                                                           ***
        ********************************************************************************/

        public string AddHex(string h1, string h2)
        {
            if (h1 != null && h2 != null)
                return (int.Parse(h1, System.Globalization.NumberStyles.HexNumber) + int.Parse(h2, System.Globalization.NumberStyles.HexNumber)).ToString("X");
            else if (h1 == null && h2 != null)
                return (0 + int.Parse(h2, System.Globalization.NumberStyles.HexNumber)).ToString("X");
            else if (h1 != null && h2 == null)
                return (int.Parse(h1, System.Globalization.NumberStyles.HexNumber) + 0).ToString("X");
            else
                return "0";
        }

        /*******************************************************************************
        ***                      SubHex function                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes two hex strings and subtracts them      ***
        ********************************************************************************
        *** INPUT ARGS: string, string                                               ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: string                                                           ***
        ********************************************************************************/

        public string SubHex(string h1, string h2)
        {
            if (h1 != null && h2 != null)
                return (int.Parse(h1, System.Globalization.NumberStyles.HexNumber) - int.Parse(h2, System.Globalization.NumberStyles.HexNumber)).ToString("X");
            else if (h1 == null && h2 != null)
                return (0 - int.Parse(h2, System.Globalization.NumberStyles.HexNumber)).ToString("X");
            else if (h1 != null && h2 == null)
                return (int.Parse(h1, System.Globalization.NumberStyles.HexNumber) - 0).ToString("X");
            else
                return "0";
        }

        /*******************************************************************************
        ***                      T_Records function                                  ***
        ********************************************************************************
        *** DESCRIPTION: This function takes the current records and add the T_Recs  ***
        ********************************************************************************
        *** INPUT ARGS: List<string>, List<string>                                   ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: List<string>                                                     ***
        ********************************************************************************/

        public List<string> T_Records(List<string> recs, List<string> codes)
        {
            string str = "";
            string start = "0";
            int num = 0;
            int length = 0;

            foreach (string c in codes)
            {
                length += c.Length / 2;
                num++;

                if (length <= 30 && num <= 10)
                    str += "^" + c;
                else
                {
                    length -= c.Length / 2;
                    recs.Add("T^" + start.PadLeft(6, '0') + "^" + length.ToString("X").PadLeft(2, '0') + str);
                    start = length.ToString("X");
                    length = 0; num = 0; str = "";
                }
            }

            recs.Add("T^" + start.PadLeft(6, '0') + "^" + length.ToString("X").PadLeft(2, '0') + str);

            return recs;
        }

    }

    //THIS CLASS STORES THE NIXBPE BITS
    class NIXBPE
    {
        public bool n = false;
        public bool i = false;
        public bool x = false;
        public bool b = false;
        public bool p = false;
        public bool e = false;
    }

    //THIS CLASS STORES THE OBJECT PROGRAM AND THE OUTPUT TO RETURN
    class Node_P2
    {
        public List<string> obj_prog = new List<string>();
        public List<string> prog = new List<string>();
    }

    //THIS CLASS HAS THE REGISTERS THAT ARE USED IN FORMAT TWO INSTRUCTION
    class Registers
    {
        public string A = "0";
        public string X = "1";
        public string L = "2";
        public string B = "3";
        public string S = "4";
        public string T = "5";
        public string F = "6";
        public string PC = "8";
        public string SW = "9";
    }
}