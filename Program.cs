using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Input;

/*
TODO:
Create validity checker
Separate user entered numbers
HInt system?
Check if text can be changed (size, font, color, etc)
Full screen mode? (clicking enabled)
*/
namespace independent_project
{
    internal class Program
    {
        //Source for board generation: https://stackoverflow.com/questions/45471152/how-to-create-a-sudoku-puzzle-in-python

        static int pattern(int r, int c, int square, int size)
        {
            return (square * (r % square) + r / square + c) % size;
        }



        static int[] shuffle(Random rand, ref int[] rowBase) //reference
        {
            //Fisher-Yates algorithm: https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
            int n = rowBase.Length;
            while (n > 1)
            {
                int k = rand.Next(n--);
                int temp = rowBase[n];
                rowBase[n] = rowBase[k];
                rowBase[k] = temp;
            }
            return rowBase;
        }
        static int[,] generateBoard(int square, int size = 9)
        {
            int[,] board = new int[size, size];
            int[] rows = new int[size];
            int[] cols = new int[size];
            int[] nums = Enumerable.Range(1, size).ToArray();

            int[] gArray = new int[square];
            int[] rArray = new int[square];

            //randomize rows, columns and numbers (of valid base pattern)
            Random rand = new Random();

            int[] rowBase = Enumerable.Range(0, square).ToArray();
            rowBase.CopyTo(gArray, 0);
            rowBase.CopyTo(rArray, 0);
            int iter = 0;
            shuffle(rand, ref gArray);
            shuffle(rand, ref rArray);
            foreach (int g in gArray)
                foreach (int r in rArray)
                    rows[iter++] = g * square + r;

            iter = 0;
            shuffle(rand, ref gArray);
            shuffle(rand, ref rArray);
            foreach (int g in gArray)
                foreach (int r in rArray)
                    cols[iter++] = g * square + r;

            shuffle(rand, ref nums);

            //produce board using randomized baseline pattern
            for (int r = 0; r < rows.Length; r++)
                for (int c = 0; c < cols.Length; c++)
                    board[r, c] = nums[pattern(rows[r], cols[c], square, size)];
            return board;
        }

        static string[,] generateBoard(int remove, int[,] ans, ref string[,] board, out int[] change)
        {
            for (int i = 0; i < ans.GetLength(0); i++)
                for (int j = 0; j < ans.GetLength(1); j++)
                    board[i, j] = Convert.ToString(ans[i, j]);
            int[] removed = Enumerable.Range(0, board.GetLength(0) * board.GetLength(1)).ToArray();
            Random rand = new Random();
            shuffle(rand, ref removed);
            for (int i = 0; i < remove; i++)
            {
                board[removed[i] / board.GetLength(0), removed[i] % board.GetLength(0)] = " ";
            }

            change = new int[remove];
            Array.Copy(removed, change, remove);
            Array.Sort(change);
            return board;
        }

        static void printBoard(int[,] board)
        {
            string dashes = String.Concat(Enumerable.Repeat(" --" + String.Concat(Enumerable.Repeat("-", Convert.ToString(board.GetLength(0)).Length)), board.GetLength(0)));
            for (int i = 0; i < board.GetLength(0); i++)
            {
                Console.WriteLine(dashes);
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Console.Write($"| {Convert.ToString(board[i, j]).PadLeft(Convert.ToString(board.GetLength(0)).Length + 1 - Convert.ToString(board[i, j]).Length)} ");
                }
                Console.WriteLine("|");
            }
            Console.WriteLine(dashes);
        }

        static void printBoard(string[,] board, int[] change) //THIS IS YOUR FAULT THAT THE CODE IS 350+ LINES LONG MS. SARGSYAN
        {
            string dashes = "╔";
            for(int i = 1; i <= board.GetLength(0); i++)
            {
                dashes += "══" + String.Concat(Enumerable.Repeat("═", Convert.ToString(board.GetLength(0)).Length));
                if (i == board.GetLength(0))
                    dashes += "╗";
                else if (i % Math.Sqrt(board.GetLength(0)) == 0)
                    dashes += "╦";
                else
                    dashes += "╤";
            }
            //string dashes = String.Concat(Enumerable.Repeat(" --" + String.Concat(Enumerable.Repeat("-", Convert.ToString(board.GetLength(0)).Length)), board.GetLength(0)));
            foreach (int i in Enumerable.Range(1, board.GetLength(0)).ToArray()) Console.Write($" {Convert.ToString(i).PadLeft(Convert.ToString(board.GetLength(0)).Length + 1)} ");
            Console.WriteLine();
            for (int i = 0; i < board.GetLength(0); i++)
            {
                if (i == 0)
                { }
                else if (i % Math.Sqrt(board.GetLength(0)) == 0)
                    dashes = dashes.Replace("┼", "╪").Replace("─", "═").Replace("╫", "╬").Replace("╢", "╣").Replace("╟", "╠");
                else
                    dashes = dashes.Replace("╔", "╟").Replace("╗", "╢").Replace("╤", "┼")
                        .Replace("╦", "╫").Replace("═", "─").Replace("╬", "╫")
                        .Replace("╪", "┼").Replace("╣", "╢").Replace("╠", "╟");
                Console.WriteLine(dashes);
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    Console.Write(j % Math.Sqrt(board.GetLength(1)) == 0 ? "║ " : "│ ");
                    if (Array.Exists(change, element => element == i * board.GetLength(0) + j))
                        Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(board[i, j].PadLeft(Convert.ToString(board.GetLength(0)).Length + 1 - board[i, j].Length) + " ");
                    Console.ResetColor();
                }
                Console.WriteLine($"║ {i + 1}");
            }

            dashes = "╚";
            for (int i = 1; i <= board.GetLength(0); i++)
            {
                dashes += "══" + String.Concat(Enumerable.Repeat("═", Convert.ToString(board.GetLength(0)).Length));
                if (i == board.GetLength(0))
                    dashes += "╝";
                else if (i % Math.Sqrt(board.GetLength(0)) == 0)
                    dashes += "╩";
                else
                    dashes += "╧";
            }
            Console.WriteLine(dashes);
        }

        //check for win
        static bool winCondition(string[,] board, int[,] ans)
        {
            //check if matches
            //might add validity function?
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == " " || Int32.Parse(board[i, j]) != ans[i, j])
                        return false;
                }
            }
            return true;
        }

        //continuous input
        static int contInput(string prompt, params int[] options)
        {
            int output;
            Console.Write(prompt);
            while (!(int.TryParse(Console.ReadLine(), out output) && Array.Exists(options, element => element == output)))
                Console.Write("Invalid value. Please try again.\n\n" + prompt);
            return output;
        }

        static string contInput(string prompt, params string[] options)
        {
            Console.Write(prompt);
            string output = Console.ReadLine().ToLower();
            while (!Array.Exists(options, element => element == output))
            {
                Console.Write("Invalid value. Please try again.\n\n" + prompt);
                output = Console.ReadLine().ToLower();
            }
            return output;
        }

        //main function
        static void Main(string[] args)
        {
            while (true)
            {
                //ALTERNATIVE CLICKING METHOD: https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.applicationview.isfullscreenmode?view=winrt-22621#windows-ui-viewmanagement-applicationview-isfullscreenmode

                Console.Clear();

                //menu
                string output;
                bool flag; //flag variable for later ( ͡° ͜ʖ ͡°)
                Console.WriteLine("Sudoku!"); //user "interface"
                Console.WriteLine("Play");
                Console.WriteLine("Quit");
                output = contInput("What would you like to do? ", "play", "quit");
                if (output == "quit")
                {
                    Console.Write("Are you sure? ");
                    if (Console.ReadLine().ToLower() == "yes")
                        return;
                }

                Console.Clear();

                //difficulty selection
                int removed = 40, square = 3; //dimensions
                Console.WriteLine("Difficulty");
                Console.WriteLine("Easy");
                Console.WriteLine("Medium");
                Console.WriteLine("Hard");
                Console.WriteLine("Custom");
                switch (contInput("Choose your difficulty: ", "easy", "medium", "hard", "custom"))
                {
                    case "easy":
                        {
                            removed = 40;
                            break;
                        }
                    case "medium":
                        {
                            removed = 50;
                            break;
                        }
                    case "hard":
                        {
                            removed = 60;
                            break;
                        }
                    case "custom": //working
                        {
                            flag = true; //flag variable to leave when apply changes
                            while (flag)
                            {
                                Console.Clear();
                                Console.WriteLine("Customize");
                                Console.WriteLine("Board size: " + square);
                                Console.WriteLine("Squares removed: " + removed);
                                Console.WriteLine("Apply Changes");
                                switch (contInput("Enter option to modify or apply changes: ", "board size", "squares removed", "apply changes"))
                                {
                                    case "board size":
                                        {
                                            square = contInput("Enter new board size: ", Enumerable.Range(1, 100).ToArray());
                                            break;
                                        }
                                    case "squares removed":
                                        {
                                            removed = contInput("Enter new number of squares removed: ", Enumerable.Range(1, square * square * (square * square - 1)).ToArray());
                                            break;
                                        }
                                    case "apply changes": //debugging (flag variable)
                                        {
                                            Console.Write("Are you sure? ");
                                            if (Console.ReadLine().ToLower() == "yes")
                                            {
                                                flag = false;
                                                if (removed > square * square * (square * square - 2))
                                                {
                                                    foreach (int i in Enumerable.Range(1, 3).ToArray())
                                                    {
                                                        Console.Clear();
                                                        Console.WriteLine($"Too many squares removed. Adjusting to {1 + square * square * (square * square - 1)}{String.Concat(Enumerable.Repeat(".", i))}");
                                                        Thread.Sleep(500);
                                                    }
                                                    removed = 1 + square * square * (square * square - 1);
                                                }
                                            }
                                            break;
                                        }
                                }
                            }

                            break;
                        }
                }

                Console.Clear();

                //board generation
                int size = square * square;
                int[,] ans;
                if (square != 3) //this is your fault that the code is 300+ lines long, ms. sargsyan
                    ans = generateBoard(square, size);
                else
                    ans = generateBoard(square); //optional argument
                string[,] board = new string[size, size];
                int[] change;
                generateBoard(remove: removed /* amt removed */, ans, ref board, out change); //named argument (by value, reference, and out)
                Console.WriteLine();
                printBoard(board, change);

                //game logic
                int col, row, num;
                flag = true;
                while (flag)
                {
                    row = contInput("Please enter row number: ", Enumerable.Range(1, size).ToArray()); //error handling
                    col = contInput("Please enter column number: ", Enumerable.Range(1, size).ToArray());
                    row--;
                    col--;
                    if (Array.Exists(change, element => element == row * size + col))
                    {
                        num = contInput("Enter preferred number: ", Enumerable.Range(1, size).ToArray());
                        //change color of user-inputted number? https://www.geeksforgeeks.org/c-sharp-how-to-change-foreground-color-of-text-in-console/
                        board[row, col] = Convert.ToString(num);
                        Console.Clear();
                        Console.WriteLine();
                        printBoard(board, change);
                        if (winCondition(board, ans))
                        {
                            Console.WriteLine("Congratulations!");
                            Thread.Sleep(2000);
                            Console.WriteLine("Press enter to exit to menu");
                            Console.ReadLine();
                            flag = false;
                            continue;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid space. Please try again.");
                        continue;
                    }
                }
            }
        }
    }
}







