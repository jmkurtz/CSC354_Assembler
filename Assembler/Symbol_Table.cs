/*******************************************************************************
*** NAME:       JEFF KURTZ                                                   ***
*** CLASS:      CSC354 - INTRO TO SYSTEMS PROGRAMMING                        ***
*** ASSIGNMENT: #4 - PASS TWO                                                ***
*** DUE DATE:   NOVEMBER 15th, 2018                                          ***
*** INSTRUCTOR: WERPY                                                        ***
********************************************************************************
*** DESCRIPTION: This Symbol Table stores passed in information as a symbol  ***
********************************************************************************/

using System;

namespace Assembler
{
    class Symbol_Table
    {
        public string ERR_MSG_1 = "**ERROR (#S1) - symbol '{0}' must contain only letters, digits, and dashes";
        public string ERR_MSG_2 = "**ERROR (#S2) - symbol '{0}' must begin with a letter";
        public string ERR_MSG_3 = "**ERROR (#S3) - symbol '{0}' has been previously defined";
        public string ERR_MSG_4 = "**ERROR (#S4) - symbol '{0}' must be less than or equal to 10 characters";
        public string ERR_MSG_5 = "**ERROR (#S5) - '{0}' is an invalid Value";
        public string ERR_MSG_6 = "**ERROR (#S6) - '{0}' is an invalid R-Flag";
        public string ERR_MSG_7 = "**ERROR (#S7) - '{0}' was not found in symbol table";
        public string ERR_MSG_8 = "**ERROR (#S8) - symbol '{0}' has missing data";

        /*******************************************************************************
        ***                      INSERT FUNCTION                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in a node and a string array. Goes and  ***
        *** tests each element in the array using the CheckIfValid functions created ***
        *** below. If all are valid, each element is added to the node and inserted  ***
        *** into the Symbol Table.                                                   ***
        ********************************************************************************
        *** INPUT ARGS: Node root, string[] v                                        ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node root                                                        ***
        ********************************************************************************/

        public Node_Sym insert(Node_Sym root, string sym, string val, bool r)
        {
            PassTwo p2 = new PassTwo();
            if (root == null)
            {
                bool MFlag = false;
                bool IFlag = true;

                root = new Node_Sym();
                root.symbol = p2.TrimOp(sym);
                root.value = val;
                root.rflag = r;
                root.iflag = IFlag;
                root.mflag = MFlag;
            }
            else
            {
                if (sym == root.symbol)
                {
                    Console.WriteLine(ERR_MSG_3, sym);
                    root.mflag = true;
                    return root;
                }
                else if (sym.CompareTo(root.symbol) < 0)
                    root.left = insert(root.left, sym, val, r);
                else
                    root.right = insert(root.right, sym, val, r);
            }
            return root;
        }

        /*******************************************************************************
        ***                      SEARCH FUNCTION                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in a node and a string. It checks to    ***
        *** see if the passed in string is a valid symbol. It then tests to see if   ***
        *** the passed in value exists in the symbol table. If not an error message  ***
        *** is displayed informing the user.                                         ***
        ********************************************************************************
        *** INPUT ARGS: Node root, string v                                          ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node root                                                        ***
        ********************************************************************************/

        public Node_Sym search(Node_Sym root, string v)
        {
            PassTwo p2 = new PassTwo();
            v = p2.TrimOp(v.ToUpper());

            if (root == null)
                return null;
            else if (v.CompareTo(root.symbol) < 0)
                return search(root.left, v);
            else if (v.CompareTo(root.symbol) > 0)
                return search(root.right, v);

            return root;
        }

        /*******************************************************************************
        ***                      INSERT FUNCTION                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in a node. It views the symbol table in ***
        *** order from left to right.                                                ***
        ********************************************************************************
        *** INPUT ARGS: Node root                                                    ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: null                                                             ***
        ********************************************************************************/

        public Node_Sym view(Node_Sym root)
        {
            PassTwo p2 = new PassTwo();
            if (root == null)
                return null;
            else
            {
                view(root.left);
                if (root.symbol != null)
                    Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10} {4,-10}", p2.TrimOp(root.symbol), root.value, (root.rflag == true) ? 1 : 0, (root.mflag == true) ? 1 : 0, (root.iflag == true) ? 1 : 0);
                view(root.right);
            }
            return null;
        }

        /*******************************************************************************
        ***                      CHECKIFSYMBOLVALID FUNCTION                         ***
        ********************************************************************************
        *** DESCRIPTION: This function Checks if the Symbol passed in is valid.      *** 
        ********************************************************************************
        *** INPUT ARGS: string sym                                                   ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: bool                                                             ***
        ********************************************************************************/

        public bool CheckIfValid(string[] sym)
        {
            //Check if symbol is valid
            foreach (char c in sym[0])
            {
                if (!char.IsLetterOrDigit(c))
                {
                    Console.WriteLine(ERR_MSG_1, sym[0]);
                    return false;
                }
            }

            if (sym.Length > 10)
            {
                Console.WriteLine(ERR_MSG_4, sym[0]);
                return false;
            }

            if (!char.IsLetter(sym[0][0]))
            {
                Console.WriteLine(ERR_MSG_2, sym[0]);
                return false;
            }

            //Check if value is valid
            foreach (char c in sym[1])
            {
                if (!char.IsDigit(c) && sym[1][0] != '+' && sym[1][0] != '-')
                {
                    Console.WriteLine(ERR_MSG_5, sym[0]);
                    return false;
                }
            }

            //Check if R-Flag valid
            sym[2] = sym[2].ToLower();

            if (sym == null)
            {
                Console.WriteLine(ERR_MSG_6, sym[0]);
                return false;
            }

            if (sym[2] != "t" && sym[2] != "f" && sym[2] != "true" &&
               sym[2] != "false" && sym[2] != "0" && sym[2] != "1")
            {
                Console.WriteLine(ERR_MSG_6, sym[0]);
                return false;
            }

            return true;
        }

        /*******************************************************************************
        ***                      STRINGTOBOOL FUNCTION                               ***
        ********************************************************************************
        *** DESCRIPTION: Converts the passed in string to a bool value.              ***
        ********************************************************************************
        *** INPUT ARGS: string sym                                                   ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: bool                                                             ***
        ********************************************************************************/

        public bool StringToBool(string v)
        {
            v = v.ToLower();
            if (v != "t" && v != "true" && v != "1")
                return false;
            return true;
        }
    }

    class Node_Sym
    {
        public string value;
        public string symbol;
        public bool rflag;
        public bool mflag;
        public bool iflag;

        public Node_Sym left = null;
        public Node_Sym right = null;
    }

}
