/*******************************************************************************
*** NAME:       JEFF KURTZ                                                   ***
*** CLASS:      CSC354 - INTRO TO SYSTEMS PROGRAMMING                        ***
*** ASSIGNMENT: #4 - PASS TWO                                                ***
*** DUE DATE:   NOVEMBER 15th, 2018                                          ***
*** INSTRUCTOR: WERPY                                                        ***
********************************************************************************
*** DESCRIPTION: This expression file handles a passed in expression         ***
********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assembler
{
    class Expressions
    {
        //Error messages that are used throughout the class
        public string ERR_MSG_1 = "ERROR (#E1) - Symbol not found";
        public string ERR_MSG_2 = "ERROR (#E2) - Expression {0} contains an invalid symbol";
        public string ERR_MSG_4 = "ERROR (#E4) - Expression {0} contains a symbol that does not exist";
        public string ERR_MSG_5 = "ERROR (#E5) - Expression cannot add two relative addresses";
        public string ERR_MSG_6 = "ERROR (#E6) - Expression cannot subtract a relative address from an absolute address";
        public string ERR_MSG_7 = "ERROR (#E7) - Expression cannot be Indirect and Indexed Addressing";
        public string ERR_MSG_8 = "ERROR (#E8) - Expression cannot contain more than one operater";

/*******************************************************************************
***                      INSERT FUNCTION                                     ***
********************************************************************************
*** DESCRIPTION: This function takes in a symbol table and a string. Runs a  ***
*** function that return an error message if an invalid expression. If null  ***
*** then the Expression node is created and populated and returned.          ***
********************************************************************************
*** INPUT ARGS: Node_Sym table, string ex                                    ***
*** OUTPUT ARGS: N/A                                                         ***
*** IN/OUT ARGS: N/A                                                         ***
*** RETURN: Node_Exp newNode                                                 ***
********************************************************************************/

        public Node_Exp insert(Node_Sym table, string ex)
        {
            Node_Exp Expression = new Node_Exp();
            Expression.expression = ex;

            if (ex[0] != '=')
            {
                Expression.ERR_MSG = IfExpressionValid(table, ex);

                if (Expression.ERR_MSG == null)
                {
                    Expression.relocatable = retRel(table, ex);
                    Expression.value = retVal(table, ex);

                    if (ex[0] == '@') { Expression.N_bit = true; }
                    else if (ex[0] == '#') { Expression.I_bit = true; }
                    else if (ex[ex.Length - 2] == ',' && ex[ex.Length - 1] == 'X') { Expression.X_bit = true; }
                    else { Expression.N_bit = true; Expression.I_bit = true; }
                }
                else
                    Console.WriteLine(Expression.ERR_MSG);
            }
            else
            {
                Expression.N_bit = true;
                Expression.I_bit = true;
                Expression.X_bit = false;
            }    

            return Expression;
        }

/*******************************************************************************
***                      VIEW FUNCTION                                       ***
********************************************************************************
*** DESCRIPTION: This function takes in the list of expressions. And prints  ***
*** each expression. If it has an error message, then it prints the error    ***
********************************************************************************
*** INPUT ARGS: List<Node_Exp> list                                          ***
*** OUTPUT ARGS: N/A                                                         ***
*** IN/OUT ARGS: N/A                                                         ***
*** RETURN: null                                                             ***
********************************************************************************/

        public Node_Exp view(List<Node_Exp> list)
        {
            //Foreach steps through and prints all objects in the passed in list
            foreach (Node_Exp exp in list)
            {
                //If there is an Error Message, Print the expression and the Error
                if (exp.ERR_MSG != null)
                {
                    Console.WriteLine("{0,-15} {1,-55}", exp.expression, exp.ERR_MSG);
                }
                //Sets the R-Flag to Either Relative or Absolute
                //Sets the N, I, and X, to either 1 or 0
                //Prints the Expression, value, rflag, n, i, x
                else
                {
                    string R = (exp.relocatable == true) ? "RELATIVE" : "ABSOLUTE";
                    int N = (exp.N_bit == true) ? 1 : 0;
                    int I = (exp.I_bit == true) ? 1 : 0;
                    int X = (exp.X_bit == true) ? 1 : 0;
                    Console.WriteLine("{0,-15} {1,-10} {2,-15} {3,-10} {4,-10} {5,-10}", exp.expression, exp.value, R, N, I, X);
                }
            }
            return null;
        }

/*******************************************************************************
***                      RETVAL FUNCTION                                     ***
********************************************************************************
*** DESCRIPTION: This function takes in the symbol table, and finds the      ***
*** value based on the expression passed in                                  ***
********************************************************************************
*** INPUT ARGS: Node_Sym table, string ex                                    ***
*** OUTPUT ARGS: N/A                                                         ***
*** IN/OUT ARGS: N/A                                                         ***
*** RETURN: int value                                                        ***
********************************************************************************/

        private string retVal(Node_Sym table, string ex)
        {
            Symbol_Table SymTab = new Symbol_Table();
            //If and addition expression
            ex = ex.TrimStart('@', '#');
            ex = ex.TrimEnd(',', 'x', 'X');

            if (ex.Contains('+'))
            {
                if (!int.TryParse(ex, out int n))
                {
                    //Splits the Expression into two
                    string[] newEx = ex.Split('+');

                    //Bools that return false if the string is not a number, and returns true with a value if string is a number
                    bool isNum1 = int.TryParse(newEx[0], out int val1);
                    bool isNum2 = int.TryParse(newEx[1], out int val2);

                    //if both are numbers, then add them
                    if (isNum1 && isNum2)
                        return AddValue(newEx[0], newEx[1]);
                    //if num1 is a number and num2 isn't, then search and find value of symbol
                    else if (isNum1 && !isNum2)
                        return AddValue(newEx[0], SymTab.search(table, newEx[1]).value);
                    //if num1 isn't a number but num2 is, then search and find the value of the symber
                    else if (!isNum1 && isNum2)
                        return AddValue(newEx[1], SymTab.search(table, newEx[0]).value);
                    //if both are not numbers, search both and find values of both
                    else
                        return AddValue(SymTab.search(table, newEx[0]).value, SymTab.search(table, newEx[1]).value);
                }
                else
                    return ex;
            }
            //If a difference expression
            else if (ex.Contains('-'))
            {
                //If then entire expression isn't a number
                if (!int.TryParse(ex, out int n))
                {
                    string[] newEx = ex.Split('-');

                    bool isNum1 = int.TryParse(newEx[0], out int val1);
                    bool isNum2 = int.TryParse(newEx[1], out int val2);
                    //if they are both number, find the difference
                    if (isNum1 && isNum2)
                        return SubValue(newEx[0], newEx[1]);
                    //if one is a number and one isnt, search and find the difference
                    else if (isNum1 && !isNum2)
                        return SubValue(newEx[0], SymTab.search(table, newEx[1]).value);
                    //if one is a number and one isnt, search and find the difference
                    else if (!isNum1 && isNum2)
                        return SubValue(newEx[1], SymTab.search(table, newEx[0]).value);
                    // if neither are numbers, search and find the difference
                    else
                        return SubValue(SymTab.search(table, newEx[0]).value, SymTab.search(table, newEx[1]).value);
                }
                else
                    return ex;
            }
            else
            {
                if (SymTab.search(table, ex) != null)
                    return SymTab.search(table, ex).value;
                else
                    return int.Parse(ex, System.Globalization.NumberStyles.HexNumber).ToString("X");
            }
        }

/*******************************************************************************
***                      RETREL FUNCTION                                     ***
********************************************************************************
*** DESCRIPTION: This function takes in the symbol table, and finds the      ***
*** relocatable based on the expression passed in                            ***
********************************************************************************
*** INPUT ARGS: Node_Sym table, string ex                                    ***
*** OUTPUT ARGS: N/A                                                         ***
*** IN/OUT ARGS: N/A                                                         ***
*** RETURN: int value                                                        ***
********************************************************************************/

        private bool retRel(Node_Sym table, string ex)
        {
            Symbol_Table SymTab = new Symbol_Table();
            string[] newEx = new string[] { };
            //Trim the expression
            ex = ex.TrimStart('@', '#');
            ex = ex.TrimEnd(',', 'x', 'X');
            //If it an addition expression
            if (ex.Contains('+'))
            {
                newEx = ex.Split('+');
                //If Absolute + Absolute
                if (SymTab.search(table, newEx[0]) == null && SymTab.search(table, newEx[1]) == null)
                    return false;
                //If Absolute + ?
                else if (SymTab.search(table, newEx[0]) == null && SymTab.search(table, newEx[1]) != null)
                {
                    if (!SymTab.search(table, newEx[1]).rflag)
                        return false;
                    else
                        return true;
                }
                //If ? + Absolute
                else if (SymTab.search(table, newEx[0]) != null && SymTab.search(table, newEx[1]) == null)
                {
                    if (!SymTab.search(table, newEx[0]).rflag)
                        return false;
                    else
                        return true;
                }
                //If ? + ?
                else
                {
                    //If Absolute + ?
                    if (SymTab.search(table, newEx[0]).rflag)
                    {
                        if (!SymTab.search(table, newEx[0]).rflag)
                            return false;
                        else
                            return true;
                    }
                    //If Relative + ?
                    else
                    {
                        if (!SymTab.search(table, newEx[0]).rflag)
                            return true;
                        //Else should be error
                    }
                }
            }
            else if (ex.Contains('-'))
            {
                newEx = ex.Split('-');
                //If Absolute - Absolute
                if (SymTab.search(table, newEx[0]) == null && SymTab.search(table, newEx[1]) == null)
                    return false;
                //If Absolute - ?
                else if (SymTab.search(table, newEx[0]) == null && SymTab.search(table, newEx[1]) != null)
                {
                    if (!SymTab.search(table, newEx[1]).rflag)
                        return false;
                }
                //If ? - Absolute
                else if (SymTab.search(table, newEx[0]) != null && SymTab.search(table, newEx[1]) == null)
                {
                    if (!SymTab.search(table, newEx[0]).rflag)
                        return false;
                    else
                        return true;
                }
                //If ? - ?
                else
                {
                    //If Absolute - ?
                    if (!SymTab.search(table, newEx[0]).rflag)
                    {
                        if (!SymTab.search(table, newEx[1]).rflag)
                            return false;
                        else
                            return true;
                    }
                    //If Relative - ?
                    else
                    {
                        if (SymTab.search(table, newEx[0]).rflag)
                            return false;
                    }
                }
            }
            else
            {
                //If its a number, Absolute, if a symbol, it is the rflag
                if (SymTab.search(table, ex) == null)
                    return false;
                else
                    return SymTab.search(table, ex).rflag;
            }

            return true;
        }

/*******************************************************************************
***                     IFEXPRESSIONVALID FUNCTION                           ***
********************************************************************************
*** DESCRIPTION: This function takes in the symbol table and test the        ***
*** expression to make sure that it is a valid expression                    ***
********************************************************************************
*** INPUT ARGS: Node_Sym table, string ex                                    ***
*** OUTPUT ARGS: N/A                                                         ***
*** IN/OUT ARGS: N/A                                                         ***
*** RETURN: string ERR_MSG                                                   ***
********************************************************************************/

        public string IfExpressionValid(Node_Sym table, string ex)
        {
            PassTwo p2 = new PassTwo();
            Symbol_Table SymTab = new Symbol_Table();
            Literal_Table litTab = new Literal_Table();

            if (ex[0] == '@')
            {
                //Cant have Indirect and Indexing Addressing
                if (ex[ex.Length - 2] == ',' && ex[ex.Length - 1] == 'X')
                    return ERR_MSG_7;
            }

            //Trim expression from valid characters
            ex = ex.TrimStart('#', '@');
            ex = ex.TrimEnd(',', 'X');

            if (ex.Contains('+'))
            {
                if (!int.TryParse(ex, out int n))
                {
                    string[] newExp = ex.Split('+');

                    //Cannot have more than one operator
                    if (newExp.Length > 2) { return ERR_MSG_8; }

                    //newExp[0] = p2.TrimOp(newExp[0]);
                    //newExp[1] = p2.TrimOp(newExp[1]);

                    //If both arn't symbols, then they have to both be numbers
                    if (SymTab.search(table, newExp[0]) == null && SymTab.search(table, newExp[1]) == null)
                    {
                        if (!int.TryParse(newExp[0], out int val1))
                            return string.Format(ERR_MSG_2, ex);
                        if (!int.TryParse(newExp[1], out int val2))
                            return string.Format(ERR_MSG_2, ex);
                    }
                    //If one is a symbol, then the other has to a number
                    else if (SymTab.search(table, newExp[0]) != null && SymTab.search(table, newExp[1]) == null)
                    {
                        if (!int.TryParse(newExp[1], out int val1))
                            return string.Format(ERR_MSG_2, ex);
                    }
                    //If one is a symbol, then the other has to a number
                    else if (SymTab.search(table, newExp[0]) == null && SymTab.search(table, newExp[1]) != null)
                    {
                        if (!int.TryParse(newExp[0], out int val1))
                            return string.Format(ERR_MSG_2, ex);
                    }
                    //If they are both symbols, they cannot both be relative
                    else
                    {
                        if (!SymTab.search(table, newExp[0]).rflag && !SymTab.search(table, newExp[1]).rflag)
                            return ERR_MSG_5;
                    }
                }
            }
            else if (ex.Contains('-'))
            {
                //Check if just a negative number
                if (!int.TryParse(ex, out int n))
                {
                    string[] newExp = ex.Split('-');

                    //Cannot contain more than one operand
                    if (newExp.Length > 2) { return ERR_MSG_8; }

                    //If both are not symbols, they both must be numbers
                    if (SymTab.search(table, newExp[0]) == null && SymTab.search(table, newExp[1]) == null)
                    {
                        if (!int.TryParse(newExp[0], out int val1))
                            return string.Format(ERR_MSG_2, ex);
                        if (!int.TryParse(newExp[1], out int val2))
                            return string.Format(ERR_MSG_2, ex);
                    }
                    //If one is a symbol, the other must be a number
                    else if (SymTab.search(table, newExp[0]) != null && SymTab.search(table, newExp[1]) == null)
                    {
                        if (!int.TryParse(newExp[1], out int val2))
                            return string.Format(ERR_MSG_2, ex);
                    }
                    //If one is a symbol, the other must be a number
                    else if (SymTab.search(table, newExp[0]) == null && SymTab.search(table, newExp[1]) != null)
                    {
                        if (!int.TryParse(newExp[1], out int val2))
                            return string.Format(ERR_MSG_2, ex);
                        else if (SymTab.search(table, newExp[1]).rflag)
                            return ERR_MSG_6;
                    }
                    //If both are found, then both should have rflags, and you cannot sub a rel with abs
                    else if (SymTab.search(table, newExp[0]) != null && SymTab.search(table, newExp[1]) != null)
                    {
                        if (!SymTab.search(table, newExp[0]).rflag && SymTab.search(table, newExp[1]).rflag)
                            return ERR_MSG_6;
                    }
                }
            }
            //If there is no expression, see if a symbol, if not a symbol, then a number
            else if (SymTab.search(table, ex) == null)
            {
                foreach (char x in ex)
                    if (!Char.IsDigit(x)) { return string.Format(ERR_MSG_4, ex); }
            }
            return null;
        }

        public string AddValue(string str1, string str2)
        {
            int val1 = int.Parse(str1, System.Globalization.NumberStyles.HexNumber);
            int val2 = int.Parse(str2, System.Globalization.NumberStyles.HexNumber);

            int retVal = val1 + val2;

            return retVal.ToString("X");
        }

        public string SubValue(string str1, string str2)
        {
            int val1 = int.Parse(str1, System.Globalization.NumberStyles.HexNumber);
            int val2 = int.Parse(str2, System.Globalization.NumberStyles.HexNumber);

            int retVal = val1 - val2;

            return retVal.ToString("X");
        }

    }

    class Node_Exp
    {
        public string expression;
        public string value;
        public bool relocatable = false;
        public bool N_bit = false;
        public bool I_bit = false;
        public bool X_bit = false;
        //ERR_Bit created, if set, there is an error
        public string ERR_MSG = null;
    }

    class Literal_Table
    {
        //Error messages that are used throughout the class
        public string ERR_MSG_1 = "ERROR (#L1) - Expression consist of an invalid literal";
        public string ERR_MSG_2 = "ERROR (#L2) - Expression consists of an invalid hex value";

        /*******************************************************************************
        ***                      INSERT FUNCTION                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in a Literal Node and a string. This    ***
        *** function runs a check function to test if the literal entered is valid   ***
        *** and if valid, returns null, and populates the literal and return the     ***
        *** linked list.                                                             ***
        ********************************************************************************
        *** INPUT ARGS: Node_Lit list, string lit                                    ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: Node_Lit newNode                                                 ***
        ********************************************************************************/

        public Node_Lit insert(Node_Lit list, string lit, string add)
        {
            Node_Lit newNode = new Node_Lit();
            newNode.ERR_MSG = IfLiteralValid(list, lit);

            if (newNode.ERR_MSG == "AD")
                return list;

            newNode.head = newNode;
            newNode.next = list.head;
            newNode.address = add;
            newNode.name = lit;
            newNode.value = FindVal(lit);
            newNode.length = FindLen(lit);

            return newNode;
        }

        /*******************************************************************************
        ***                      VIEW FUNCTION                                       ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in the linked list of literals. And     ***
        *** prints each expression. If it has an error message, then it prints the   ***
        *** error.                                                                   ***
        ********************************************************************************
        *** INPUT ARGS: Node_Lit list                                                ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: null                                                             ***
        ********************************************************************************/

        public Node_Lit view(Node_Lit list)
        {
            if (list != null)
            {
                if (list.ERR_MSG != null && list.ERR_MSG != "AD")
                {
                    view(list.next);
                    Console.WriteLine("{0,-10} {1,-30}", list.name, list.ERR_MSG);
                }
                else if (list.ERR_MSG == null)
                {
                    view(list.next);
                    Console.WriteLine("{0,-10} {1,-10} {2,-10} {3,-10}", list.name, list.value, list.length, list.address);
                }
                return null;
            }
            else
                return null;
        }

        /*******************************************************************************
        ***                  SEARCH FUNCTION                                         ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in the linked list and a literal and    ***
        *** searches the linked list to see if the literal exsists                   ***
        ********************************************************************************
        *** INPUT ARGS: Node_Lit list, string lit                                    ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: null or list                                                     ***
        ********************************************************************************/

        public Node_Lit search(Node_Lit list, string lit)
        {
            if (list == null)
                return null;
            else if (list.name != lit)
                return search(list.next, lit);
            else
                return list;
        }

        /*******************************************************************************
        ***                     FINDVAL FUNCTION                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in a literal, and outputs its value     ***
        ********************************************************************************
        *** INPUT ARGS: string lit                                                   ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: int value                                                        ***
        ********************************************************************************/

        public string FindVal(string lit)
        {
            if (lit[1] == 'C' || lit[1] == 'c')
                return BitConverter.ToString(Encoding.Default.GetBytes(lit.TrimStart('=', '\'', 'C', 'c').TrimEnd('\''))).Replace("-", "");
            else if (lit[1] == 'X' || lit[1] == 'x')
                return lit.TrimStart('=', '\'', 'X', 'x').TrimEnd('\'');
            return null;
        }

        /*******************************************************************************
        ***                     FINDLEN FUNCTION                                     ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in a literal, and returns its length    ***
        ********************************************************************************
        *** INPUT ARGS: string lit                                                   ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: int value                                                        ***
        ********************************************************************************/

        public int FindLen(string lit)
        {
            if (lit[1] == 'C')
            {
                lit = lit.TrimStart('=', '\'', 'C');
                lit = lit.TrimEnd('\'');
                return lit.Length;
            }
            else if (lit[1] == 'X')
            {
                lit = lit.TrimStart('=', '\'', 'X');
                lit = lit.TrimEnd('\'');
                return lit.Length / 2;
            }
            else
                return 0;
        }

        /*******************************************************************************
        ***                     IFLITERALVALID FUNCTION                              ***
        ********************************************************************************
        *** DESCRIPTION: This function takes in the linked list and test the         ***
        *** literal to make sure that it is a valid literal                          ***
        ********************************************************************************
        *** INPUT ARGS: Node_Lit list, string ex                                     ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: string ERR_MSG                                                   ***
        ********************************************************************************/

        public string IfLiteralValid(Node_Lit list, string ex)
        {
            //Search if previously defined
            if (search(list, ex) != null)
                return "AD";
            //If the first character is not correct
            if (ex[0] != '=')
                return ERR_MSG_1;
            //If the second character is not correct
            if (ex[1] != 'C' && ex[1] != 'c' && ex[1] != 'X' && ex[1] != 'x')
                return ERR_MSG_1;
            //If the third and last character are '
            if (ex[2] != (char)39 || ex[ex.Length - 1] != (char)39)
                return ERR_MSG_1;

            if (ex[1] == 'X' || ex[1] == 'x')
            {
                ex = ex.TrimStart('X', 'x', '=', '\'');
                ex = ex.TrimEnd('\'');

                if (!isStringHex(ex))
                    return ERR_MSG_2;
            }
            return null;
        }

        /*******************************************************************************
        ***                     ISSTRINGHEX FUNCTION                                 ***
        ********************************************************************************
        *** DESCRIPTION: A function to test if a string is a hex                     ***
        ********************************************************************************
        *** INPUT ARGS: string ex                                                    ***
        *** OUTPUT ARGS: N/A                                                         ***
        *** IN/OUT ARGS: N/A                                                         ***
        *** RETURN: bool                                                             ***
        ********************************************************************************/

        public bool isStringHex(string ex)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(ex, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }

    class Node_Lit
    {
        public Node_Lit next;
        public Node_Lit head;
        public string name;
        public string value;
        public int length;
        public string address;
        public string ERR_MSG = null;
    }
}
