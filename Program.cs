using BanggoodGiftExchange.Models;
using BanggoodGiftExchange.Services;
using BanggoodGiftExchange.Services.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BanggoodGiftExchange
{
    class Program
    {
        private static ServiceProvider serviceProvider;
        private static BanggoodService banggoodService;

        static void Main(string[] args)
        {
            BanggoodDbInitializer dbInit = new BanggoodDbInitializer();
            dbInit.InitializeDb();


            banggoodService = new BanggoodService();

            bool running = true;
            while (running)
            {
                try
                {
                    running = HandleCommand();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }

        private static bool HandleCommand()
        {
            Console.WriteLine("Enter a command");
            Console.WriteLine("\te for a new gift exchange");
            Console.WriteLine("\to for a new gift order");
            Console.WriteLine("\ta for a gift arrival");
            Console.WriteLine("\tr for a gift recap");
            Console.WriteLine("\tq to quit");
            ConsoleKeyInfo commandInfo = Console.ReadKey();
            char command = commandInfo.KeyChar;
            Console.WriteLine();

            switch (command)
            {
                case 'e':
                    Console.WriteLine("Enter a name for this gift exchange: ");
                    string exchangeName = Console.ReadLine();
                    Console.WriteLine("Enter a date for this gift exchange (yyyy-mm-dd): ");
                    string exchangeDateValue = Console.ReadLine();
                    DateTime exchangeDate = DateTime.Parse(exchangeDateValue);

                    Console.WriteLine();
                    List<TeammateParticipation> participants = banggoodService.CreateGiftExchange(exchangeName, exchangeDate);
                    foreach (TeammateParticipation participant in participants)
                    {
                        Console.WriteLine(participant.ToString());
                    }
                    break;
                case 'o':
                    Console.WriteLine("Enter a description for this gift: ");
                    string description = Console.ReadLine();
                    Console.WriteLine("Enter a participant Id for this gift: ");
                    string particpantIdValue = Console.ReadLine();
                    int participantId = int.Parse(particpantIdValue);

                    int orderedGiftId = banggoodService.CreateOrderedGift(description, participantId);
                    Console.WriteLine($"Id for {description} is {orderedGiftId}");
                    break;
                case 'a':
                    Console.WriteLine("Enter the Id of the OrderedGift record that arrived: ");
                    string giftIdValue = Console.ReadLine();
                    int giftId = int.Parse(giftIdValue);

                    string recipientName = banggoodService.HandleGiftArrival(giftId);
                    Console.WriteLine($"{recipientName} received gift {giftId}");
                    break;
                case 'r':
                    Console.WriteLine("Enter the Id of a GiftExchange to return gifts for: ");
                    string giftExchangeIdValue = Console.ReadLine();
                    int giftExchangeId = int.Parse(giftExchangeIdValue);

                    IEnumerable<GiftRecap> recaps = banggoodService.RecapAllGiftsForExchange(giftExchangeId);
                    foreach (GiftRecap recap in recaps)
                    {
                        if (recap.ReceiverName != null)
                        {
                            string receivedStatus = recap.ReceivedDate == null ? "did not receive" : "received";
                            string receivedDate = recap.ReceivedDate == null ? "" : $" on {recap.ReceivedDate.Value.ToString("MM/dd/yyyy")}";
                            Console.WriteLine($"{recap.ReceiverName} {receivedStatus} {recap.GiftDescription} from {recap.SenderName}{receivedDate}");
                        }
                        else
                        {
                            Console.WriteLine($"{recap.GiftDescription} from {recap.SenderName} has not arrived. Receiver is TBD");
                        }

                    }
                    break;
                case 'q':
                    return false;
            }

            return true;
        }
    }
}
