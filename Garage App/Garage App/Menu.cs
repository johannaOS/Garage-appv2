using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Garage_App
{
    public class Menu
    {
        private List<string> _menuOptions = new();
        public string? WelcomeMessage { get; set; }
        public int NrMenuOptions { get; set;  }
        public required List<string> MenuOptions { 
            get => _menuOptions;
            set
            {
                if (value == null)
                    throw new ArgumentException("MenuOptions cannot be null!");
                else
                    _menuOptions = value;
            } 
        }

        public void DisplayOptions(bool prompt = true)
        {
            if (prompt)
                Console.WriteLine("Vad vill du göra?");
            foreach (string option in MenuOptions)
            {
                Console.Write(option);
            }
        } 
    }

    public class Main : Menu
    {
        public Garage CreateNewGarage()
        {
            //Get garage name
            Console.WriteLine("(Tryck 9 för att komma tillbaka till huvudmenyn)\nNamnge ditt garage:");
            string? name = (Console.ReadLine() ?? "").Trim().Replace("  ", " ");
            while (String.IsNullOrEmpty(name)) {
                Console.WriteLine("Garaget måste ha ett namn!");
                name = Console.ReadLine();
            }
            //Cancel if user chose to get back to menue
            if (name != "9")
            {   // Get amount of spaces
                Console.WriteLine($"Ange hur många parkeringsplatser som finns i {name}");
                int parkingSpaces;
                while (!int.TryParse(Console.ReadLine(), out parkingSpaces) || parkingSpaces <= 0)
                    Console.WriteLine("Du måste ange ett giltigt antal!");
                //Confirm amount of spaces
                Console.WriteLine($"Du har angivit {parkingSpaces} parkeringsplatser. Stämmer det? (y/n)");
                string? confirmationResponse = (Console.ReadLine() ?? "").Trim();
                if (confirmationResponse == "n")
                {
                    Console.WriteLine($"Ange hur många parkeringsplatser som finns i {name}");
                    while (!int.TryParse(Console.ReadLine(), out parkingSpaces))
                        Console.WriteLine("Du måste ange ett giltigt antal!");
                }
                //Go ahead with creating a garage object
                return new Garage(name, parkingSpaces);
            }
            else
                throw new ArgumentException("Uknown error in CreateNewGarage");

        }
    }
}
