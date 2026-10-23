using Garage_App;
using System;
using System.ComponentModel.Design;
using System.Xml.Linq;

var MainMenu = new Main()
{
    WelcomeMessage = "************** Välkommen till Epic Garage App! ***************\n",
    NrMenuOptions = 3,
    MenuOptions = new List<string> { "0: avsluta programmet\t", "1: Skapa ett nytt garage\t", "2: Hantera ett av mina garage\n" }
};

var myGarages = new List<Garage>();
bool programOn = true;
while (programOn)
{
    Console.WriteLine(MainMenu.WelcomeMessage);
    MainMenu.DisplayOptions();

    switch (Console.ReadLine())
    {
        case "0":
            Console.WriteLine("Avslutar programmet...");
            programOn = false; break;
        case "1":
            { // the braces create a block scope, meaning the newGarage instance will only excist here
                try { 
                    Garage newGarage = MainMenu.CreateNewGarage();
                    myGarages.Add(newGarage);
                    Console.WriteLine("Vill du populera garaget direkt? (y/n)");
                    string userResponse = (Console.ReadLine() ?? "").Trim().ToLower();
                    while (userResponse != "y" && userResponse != "n")
                    {
                        Console.WriteLine("Du måste ange 'y' eller 'n'!");
                        userResponse = (Console.ReadLine() ?? "").Trim().ToLower();
                    }
                    if (userResponse == "n")
                        break;
                    else
                    {
                        Console.WriteLine($"Hur många fordon vill du populera garaget med? Max: {newGarage.Spaces}!");
                        int initPop;
                        while (!int.TryParse((Console.ReadLine() ?? "").Trim(), out initPop) || initPop <= 0 || initPop > newGarage.Spaces)
                            Console.WriteLine("Du måste ange ett giltigt antal!");
                        if (newGarage != null)
                        {
                            int count = 0;
                            while (count < initPop)
                            {
                                try
                                {
                                    if (CheckIn(newGarage))
                                        count++;
                                }
                                catch (Exception ex) { Console.WriteLine(ex.Message); }
                            }
                        }
                        Console.WriteLine("Då har du populerat klart ditt garage!\n");
                    }
                    break;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); break; }
                
            }

        case "2":
            if (myGarages.Count == 0) { Console.WriteLine("!! Du har inga garage än! !!"); break; }
            var selectedGarage = SelectGarage(myGarages);
            RunGarageMenu(selectedGarage);
            break;
        default:
            Console.WriteLine("Ange ett giltigt val!");
            break;
    }
}

static Garage SelectGarage(List<Garage> myGarages)
{
    Console.WriteLine("Vilka av dina garage vill du hantera?");
    foreach (var garage in myGarages)
        Console.Write($"{myGarages.IndexOf(garage)}: {garage.Name}\t");
    Console.WriteLine();
    int userSelection = -1;
    while (true)
    {
        try
        {
            if (int.TryParse(Console.ReadLine(), out userSelection) && userSelection >= 0 && userSelection < myGarages.Count)
                return myGarages[userSelection];
            else
                throw new ArgumentException("Du måste ange ett giltigt val!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
static void RunGarageMenu(Garage selectedGarage)
{
    var garageMenu = new Menu()
    {
        WelcomeMessage = $"************** Hantera ditt garage \"{selectedGarage.Name}\"! **************\n",
        NrMenuOptions = 4,
        MenuOptions = new List<string> { "0: Gå tillbaka\t", "1: Checka in\t", "2: Checka ut\t", "3: Lista alla fordon\t", "4: Sök bland dina fordon\n" }
    };

    bool MenuOn = true;
    while (MenuOn)
    {
        Console.WriteLine(garageMenu.WelcomeMessage);
        garageMenu.DisplayOptions();

        switch (Console.ReadLine())
        {
            case "0":
                MenuOn = false;
                break;
            case "1":
                try
                {
                    CheckIn(selectedGarage);
                }                
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                break;
            case "2":
                selectedGarage.CheckOut();
                break;
            case "3":
                selectedGarage.ListParkedVehicles();
                break;
            case "4":
                try { RunSearchMenu(selectedGarage); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                break;
            default:
                Console.WriteLine("Ange ett giltigt alternativ!");
                break;
        }
    }

}
static void RunSearchMenu(Garage selectedGarage)
{
    //Kolla att garaget inte är tomt
    if (selectedGarage.GetAvailableSpaces(false) == selectedGarage.Spaces)
        throw new ArgumentException($"{selectedGarage.Name} är tomt!");

    var searchMenu = new Menu()
    {
        WelcomeMessage = $"************** Vad vill du söka efter i \"{selectedGarage.Name}\"? **************\n",
        NrMenuOptions = 3,
        MenuOptions = new List<string> { "0: Gå tillbaka\t", "1: Sök på registreringsskylt\t", "2: Övrigt sök\n" }
    };

    bool MenuOn = true;
    while (MenuOn)
    {
        Console.WriteLine(searchMenu.WelcomeMessage);
        searchMenu.DisplayOptions(false);

        switch (Console.ReadLine())
        {
            case "0":
                MenuOn = false;
                break;
            case "1":
                Console.WriteLine("Ange registreringsnumret du vill söka på:");
                string regNr = GetRegNr();
                int plats = -1;
                if (selectedGarage.SearchByRegNr(regNr, ref plats))
                    Console.WriteLine($"Fordonet finns på plats {plats}\n");
                else
                    Console.WriteLine("Det här fordonet finns inte i garaget!\n");
                //Sök på registreringsskylt
                break;
            case "2":
                RunSearchInVehiclesMenu(selectedGarage);
                break;
            default:
                Console.WriteLine("Ange ett giltigt alternativ!");
                break;
        }
    }
}
static void RunSearchInVehiclesMenu(Garage selectedGarage)
{
    var searchMenu = new Menu()
    {
        WelcomeMessage = $"************** Välj vad du vill söka på i \"{selectedGarage.Name}\" **************\n",
        NrMenuOptions = 3,
        MenuOptions = new List<string> { "0: Gå tillbaka\t", "1: Registreringsnummer\t", "2: Färg\t", "3: Parkeringsrutor\t", "4: Drivmedel\t", "9: Rensa sökfras\n" }
    };

    var searchResult = new List<Vehicle>();
    bool MenuOn = true;
    string searchPhrase = "";
    while (MenuOn)
    {
        Console.WriteLine(searchMenu.WelcomeMessage);
        searchMenu.DisplayOptions(false);

        switch (Console.ReadLine())
        {
            case "0":
                MenuOn = false;
                break;
            case "1":
                // T ex alla registreringsnummer som innehåller "f"
                Console.Write("Skriv din sökfras: ");
                string searchRegNr = GetRegNr();
                List<Vehicle> searchRegResult = selectedGarage.SearchInRegNr(searchRegNr);
                AddToSearchResult(searchResult, searchRegResult);
                searchPhrase += " + " + searchRegNr;
                Console.WriteLine($"Resultatet visar sök på: {searchPhrase}");
                break;
            case "2":
                Console.Write("Skriv din sökfras: ");
                string userInput = GetSearchPhrase();
                var searchColorResult = selectedGarage.SearchColor(userInput);
                AddToSearchResult(searchResult, searchColorResult);
                searchPhrase += userInput;
                Console.WriteLine($"Resultatet visar sök på: {searchPhrase}");
                break;
            case "3":
                Console.WriteLine("Den här funktionen finns inte än!");
                //Metod för att få färg
                break;
            case "4":
                Console.WriteLine("Den här funktionen finns inte än!");
                //Metod för att få Parkeringsrutor
                break;
            case "9":
                searchPhrase = "";
                searchResult.Clear();
                break;
            default:
                Console.WriteLine("Ange ett giltigt alternativ!");
                break;
        }
    }
}
static bool CheckIn(Garage garage)
{
    int availableSpaces = garage.GetAvailableSpaces();
    if (availableSpaces > 0)
    {
        Console.WriteLine("Vilket typ av fordon vill du checka in?\n1: Motorcykel\t2: Flygplan\t3: Bil\t4: Buss\t5: Båt\t0: Avbryt"); //fyll på här när nya Vehicleklasser skapats
        string userChoice;
        while (true)
        {
            userChoice = (Console.ReadLine() ?? "").Trim();
            if (userChoice == "0")
                return false;
            else if (new string[] {"1", "2", "3", "4", "5"}.Contains(userChoice))
            {
                try
                {
                    garage.CheckInVehicle(
                    CreateVehicle(userChoice, garage)
                    );
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            else
                Console.WriteLine("Ange ett giltigt val!");            
        }
    }
    else
        throw new ArgumentException("Det här garaget är fullt!");
}

static Vehicle CreateVehicle(string choice, Garage garage)
{
    switch (choice)
    {
        case "1": //motorcykel
            {
                string regNr;
                try
                {
                    regNr = ValidateRegNr(garage);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
                string color = ValidateColor();
                int? nrOfWheels = ValidateNrOfWheels();

                //instanciera en ny motorcykel
                try
                {
                    var motorcycle = new MotorCycle(regNr, color, nrOfWheels);
                    return motorcycle;
                }
                catch (Exception ex) 
                { 
                    throw new ArgumentException(ex.Message); 
                }
            }
        case "2": //flygplan
            {
                string regNr;
                try
                {
                    regNr = ValidateRegNr(garage);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
                string color = ValidateColor();
                double length = ValidateLength();

                try
                {
                    var airplane = new Airplane(regNr, color, length);
                    return airplane;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
        case "3": //bil
            {
                string regNr;
                try
                {
                    regNr = ValidateRegNr(garage);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
                string color = ValidateColor();

                try
                {
                    var car = new Car(regNr, color);
                    return car;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
        case "4": //buss
            {
                string regNr;
                try
                {
                    regNr = ValidateRegNr(garage);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
                string color = ValidateColor();

                try
                {
                    var bus = new Bus(regNr, color);
                    return bus;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
        case "5": //båt
            {
                string regNr;
                try
                {
                    regNr = ValidateRegNr(garage);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
                string color = ValidateColor();

                try
                {
                    var boat = new Boat(regNr, color);
                    return boat;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
        default:
            break;
    }
    throw new ArgumentException("Det här fordonet finns redan i garaget!");
}

static string ValidateRegNr(Garage garage)
{
    Console.Write("Ange registreringsnummer (obligatoriskt): ");
    string regNr = GetRegNr();
    //Check that _regNr is unique (in garage)
    int dummyIndex = -1;
    if (garage.SearchByRegNr(regNr, ref dummyIndex))
        throw new ArgumentException("Det här fordonet finns redan i garaget!");
    else
        return regNr;

}
static string GetRegNr()
{
    string _regNr = (Console.ReadLine() ?? "").Trim().Replace(" ", "").ToUpper();
    while (String.IsNullOrEmpty(_regNr) || _regNr.Length > 12)
    {
        Console.WriteLine("Du måste ange ett giltigt registreringsnummer!");
        _regNr = (Console.ReadLine() ?? "").Trim().Replace(" ", "").ToUpper();
    }
    return _regNr;
}

static string ValidateColor()
{
    Console.Write("Ange färg (obligatoriskt): ");
    while (true)
    {
        string color = (Console.ReadLine() ?? "").Trim().Replace(" ", "").ToLower();
        try
        {
            if (String.IsNullOrEmpty(color) || color.Length > 200)
                throw new ArgumentException("Du måste ange en giltig färg!");
            return color;
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
}
static int? ValidateNrOfWheels()
{
    Console.Write("Ange antal hjul (tryck 0 för att hoppa över denna): ");
    int nrOfwheels;
    while (true)
    {
        string wheels = (Console.ReadLine() ?? "").Trim().Replace(" ", "");
        if (wheels == "0")
            return null;
        if (int.TryParse(wheels, out nrOfwheels) && nrOfwheels > 1 && nrOfwheels < 11)
            return nrOfwheels;
        else
            Console.WriteLine("Du måste ange ett giltigt antal hjul!");  
    }
}
static double ValidateLength()
{
    Console.Write("Ange längd på fordonet (obligatorisk): ");
    double length;
    while (true)
    {
        string input = (Console.ReadLine() ?? "").Trim().Replace(" ", "");
        if (double.TryParse(input, out length) && length > 1 && length <100 )
            return length;
        else
            Console.WriteLine("Du måste ange en giltig längd!");
    }
}

static void AddToSearchResult(List<Vehicle> oldSearchResult, List<Vehicle> newSearchResult)
{
    if (newSearchResult.Count == 0)
        Console.WriteLine("Din sökning gav ingen match!\n");
    else
    {
        bool duplicate = false;
        if (oldSearchResult.Count > 0) //kolla om searchResult har Vehicles i sig
        {
            foreach (var v in newSearchResult)
            {
                duplicate = false; //reset if true
                foreach(var v2 in oldSearchResult)
                {
                        if (v.RegNr == v2.RegNr) //Check if vehicle isn't already there
                            duplicate = true;
                }
                if (duplicate == false)
                    oldSearchResult.Add(v);
            }
        }
        else //oldSearchResult är tom
            oldSearchResult.AddRange(newSearchResult);
        
        Console.WriteLine("Dina matchningar:");
        foreach (var v in oldSearchResult)
        {
            Console.WriteLine(v.Type + ": " + v.RegNr);
        }
        Console.WriteLine();
    }
}

static string GetSearchPhrase()
{
    //Hämta sökfras
    Console.Write("Ange din sökfras: ");
    string userInput = (Console.ReadLine() ?? "").Trim().ToLower().Replace(" ", "");
    while (String.IsNullOrEmpty(userInput))
    {
        Console.WriteLine("Ange giltig sökfras!");
        userInput = (Console.ReadLine() ?? "").Trim().ToLower().Replace(" ", "");
    }
    return userInput;
}


