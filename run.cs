using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


class HotelCapacity
{
    static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        // TODO: реализация алгоритма
        var roomCheckouts = new PriorityQueue<DateTime, DateTime>(maxCapacity + 1);

        foreach(var guest in guests.OrderBy(x => x.CheckIn))
        {
            if (roomCheckouts.TryPeek(out var minCheckOut, out var _) && minCheckOut <= guest.CheckIn)
                roomCheckouts.Dequeue();

            roomCheckouts.Enqueue(guest.CheckOut, guest.CheckOut);

            if (roomCheckouts.Count > maxCapacity)
                return false;
        }

        return true;
    }

    class Guest
    {
        public string Name { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }


    static void Main()
    {
        int maxCapacity = int.Parse(Console.ReadLine());
        int n = int.Parse(Console.ReadLine());


        List<Guest> guests = new List<Guest>();


        for (int i = 0; i < n; i++)
        {
            string line = Console.ReadLine();
            Guest guest = ParseGuest(line);
            guests.Add(guest);
        }


        bool result = CheckCapacity(maxCapacity, guests);


        Console.WriteLine(result ? "True" : "False");
    }


    // Простой парсер JSON-строки для объекта Guest
    static Guest ParseGuest(string json)
    {
        var guest = new Guest();


        // Извлекаем имя
        Match nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;


        // Извлекаем дату заезда
        Match checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = DateTime.ParseExact(checkInMatch.Groups[1].Value, "yyyy-MM-dd", null);


        // Извлекаем дату выезда
        Match checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = DateTime.ParseExact(checkOutMatch.Groups[1].Value, "yyyy-MM-dd", null);


        return guest;
    }
}