/*******************************************************************************
*** NAME:       JEFF KURTZ                                                   ***
*** CLASS:      CSC354 - INTRO TO SYSTEMS PROGRAMMING                        ***
*** ASSIGNMENT: #4 - PASS TWO                                                ***
*** DUE DATE:   NOVEMBER 15th, 2018                                          ***
*** INSTRUCTOR: WERPY                                                        ***
********************************************************************************
*** DESCRIPTION: This Pass 1 funciton takes in information from the file     ***
*** passed in. In the file contains assembly instructions and pass one       ***
*** produces line numbers and load counters. Pass 1 also stores the labels   ***
*** as symbols and also store literals in the literal table.                 ***
********************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Assembler
{

    class PassOne
    {

        /*******************************************************************************
        ***                      PASSUNO function                                    ***
        ********************************************************************************
        *** DESCRIPTION: This Pass 1 funciton takes in information from the file     ***
        *** passed in. In the file contains assembly instructions and pass one       ***
        *** produces line numbers and load counters. Pass 1 also stores the labels   ***
        *** as symbols and also store literals in the literal table.                 ***
        ********************************************************************************
        *** INPUT ARGS: string program, List<Node_Op> OpCodes                        ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node_P1 p1                                                       ***
        ********************************************************************************/

        public Node_P1 PassUno(string program, List<Node_Op> OpCodes)
        {
            Node_P1 passOne = new Node_P1();
            PassTwo p2 = new PassTwo();

            List<string> theFile = new List<string>();
            List<int> valLits = new List<int>();

            OpCodes opCode = new OpCodes();
            Symbol_Table SymTab = new Symbol_Table();
            Expressions Exp = new Expressions();
            Literal_Table LitTable = new Literal_Table();

            Node_Lit lit = new Node_Lit();
            Node_Sym table = new Node_Sym();
            Node_Op op = new Node_Op();
            Node_Exp ex = new Node_Exp();

            List<string> Literals = new List<string>();
            string[] Prog = File.ReadAllLines(program);
            string[] splitLine;

            string str_Count = null;
            string tmpCounter = null;
            string label = null;
            string operation = null;
            string operand = null;
            int l = 0;
            string Counter = "00000";
            string EquCount = null;
            string tmpStr;
            int val;
            int tmp;
            bool r = true;

            foreach (string line in Prog)
            {
                //HANDLE IF EMPTY LINE
                if (line != null)
                {
                    //COMMMENT HANDLING
                    if (line[0] != '.')
                    {
                        tmpStr = line;

                        //SPLITS THE LINE
                        splitLine = tmpStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        //SETS LABEL
                        if (splitLine.Length >= 3)
                        {
                            label = splitLine[0].ToUpper();
                            splitLine = splitLine.Where(w => w != splitLine[0]).ToArray();
                        }
                        else
                        {
                            tmpStr = splitLine[0];
                            if (tmpStr[0] != '+' && opCode.search(OpCodes, tmpStr) == null && tmpStr != "BASE" && tmpStr != "BYTE" && tmpStr != "END" && tmpStr != "EQU" && tmpStr != "EXTDEF" && tmpStr != "EXTREF" && tmpStr != "RESB" && tmpStr != "RESW" && tmpStr != "START" && tmpStr != "WORD")
                            {
                                label = splitLine[0].ToUpper();
                                splitLine = splitLine.Where(w => w != splitLine[0]).ToArray();
                            }
                        }

                        //SETS OPERTATION
                        operation = splitLine[0].ToUpper();

                        //SETS OPERAND
                        if(splitLine.Length > 1)
                            operand = splitLine[1];

                        //SETS LINE NUMBER
                        l++;

                        if (operation != "EQU")
                            tmpCounter = Counter;

                        //IF START, THEN START; IF NOT END, THEN DO STUFF
                        if (operation == "START")
                        {
                            Counter = operand.PadLeft(5, '0');
                            str_Count = Counter;
                        }
                        else if (operation != "END")
                        {
                            //FORMAT 4 CHECK
                            if (operation[0] != '+')
                                op = opCode.search(OpCodes, operation);
                            else
                            {
                                tmpStr = operation.TrimStart('+');
                                op = opCode.search(OpCodes, tmpStr);
                                op.format = "4";
                            }

                            if (op == null)
                            {
                                if (operation == "WORD")
                                {
                                    val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + 3;
                                    Counter = val.ToString("X").PadLeft(5, '0');
                                }
                                else if (operation == "RESW")
                                {
                                    if (int.TryParse(operand, out tmp))
                                    {
                                        val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + (tmp * 3);
                                        Counter = val.ToString("X").PadLeft(5, '0');
                                    }
                                }
                                else if (operation == "RESB")
                                {
                                    if (int.TryParse(operand, out tmp))
                                    {
                                        val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + tmp;
                                        Counter = val.ToString("X").PadLeft(5, '0');
                                    }
                                }
                                else if (operation == "BYTE")
                                {
                                    tmpStr = operand;
                                    if (tmpStr[0] == 'C')
                                    {
                                        tmpStr = tmpStr.TrimStart('C', '\'');
                                        tmpStr = tmpStr.TrimEnd('\'');
                                        tmp = tmpStr.Length;
                                    }
                                    else
                                    {
                                        tmpStr = tmpStr.TrimStart('X', '\'');
                                        tmpStr = tmpStr.TrimEnd('\'');
                                        tmp = tmpStr.Length / 2;
                                    }

                                    val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + tmp;
                                    Counter = val.ToString("X").PadLeft(5, '0');
                                }
                                else if (operation == "BASE")
                                {

                                }
                                else if (operation == "EQU")
                                {
                                    if (operand == "*")
                                        EquCount = Counter;
                                    //IF OPERAND IS A DECIMAL VALUE
                                    else if (int.TryParse(operand, out tmp))
                                    {
                                        EquCount = tmp.ToString("X").PadLeft(5, '0');
                                        r = false;
                                    }
                                    //IF OPERAND IS AN EXPRESSION
                                    else
                                    {
                                        ex = Exp.insert(table, operand);
                                        if (int.TryParse(ex.value, out tmp))
                                        {
                                            EquCount = tmp.ToString("X").PadLeft(5, '0');
                                            r = ex.relocatable;
                                        }
                                    }
                                }
                                else if (operation == "EXTDEF")
                                {
                                    r = false;
                                }
                                else if (operation == "EXTREF")
                                {
                                    r = false;
                                }
                            }
                            else
                            {
                                if (operation[0] != '+')
                                {
                                    if (int.TryParse(op.format, out tmp))
                                    {
                                        val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + tmp;
                                        Counter = val.ToString("X").PadLeft(5, '0');
                                    }
                                }
                                else
                                {
                                    val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + 4;
                                    Counter = val.ToString("X").PadLeft(5, '0');
                                }
                            }
                        }

                        if (operand != null)
                        {
                            if (operand[0] == '=')
                                Literals.Add(operand);
                        }

                        if (EquCount == null)
                        {
                            theFile.Add(string.Format("{0,-4} {1,-10} {2,-10} {3,-10} {4,-15}", l, tmpCounter, label, operation, operand));
                            if (label != null)
                                table = SymTab.insert(table, label, tmpCounter, r);
                        }
                        else
                        {
                            theFile.Add(string.Format("{0,-4} {1,-10} {2,-10} {3,-10} {4,-15}", l, EquCount, label, operation, operand));
                            if (label != null)
                                table = SymTab.insert(table, label, EquCount, r);
                        }

                        EquCount = null;
                        r = true;
                        label = null;
                        operand = null;
                    }
                }
            }

            if (Literals != null)
            {
                foreach (string oper in Literals)
                {
                    l++;
                    tmpCounter = Counter;

                    lit = LitTable.insert(lit, oper, Counter);

                    val = int.Parse(Counter, System.Globalization.NumberStyles.HexNumber) + lit.length;
                    Counter = val.ToString("X").PadLeft(5, '0');
                    valLits.Add(lit.length);

                    theFile.Add(string.Format("{0,-4} {1,-10} {2,-10} {3,-10} {4,-15}", l, tmpCounter, "*", oper, ""));
                }
            }

            passOne.length = p2.SubHex(Counter, str_Count);
            theFile.Add(passOne.length);

            passOne.table = table;
            passOne.lit = lit;
            passOne.prog = theFile;
            
            return passOne;
        }
    }

    //THIS CLASS STORES THE SYM_TABLE, LIT_TABLE, THE PROGRAM, AND THE LENGTH TO RETURN
    class Node_P1
    {
        public Node_Sym table;
        public Node_Lit lit;
        public List<string> prog;
        public string length;
    }


    class OpCodes
    {
        /*******************************************************************************
        ***                      INSERT function                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function inserts each op code into a Node              ***
        ********************************************************************************
        *** INPUT ARGS: string[]                                                     ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node_Op                                                          ***
        ********************************************************************************/

        public Node_Op insert(string[] op)
        {
            Node_Op opCode = new Node_Op();
            opCode.name = op[0].ToUpper();
            opCode.format = op[2];
            opCode.code = op[1];

            return opCode;
        }

        /*******************************************************************************
        ***                      SEARCH function                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function searches through the opcodes to return the op ***
        ********************************************************************************
        *** INPUT ARGS: List<Node_Op>, string                                        ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node_Op                                                          ***
        ********************************************************************************/

        public Node_Op search(List<Node_Op> OpCode, string src)
        {
            foreach (Node_Op op in OpCode)
            {
                if (src == op.name)
                    return op;
            }
            return null;
        }

    }

    //THIS CLASS STORES THE INFORMAITON NEEDED FOR THE OPCODES
    class Node_Op
    {
        public string name;
        public string format;
        public string code;
    }

}