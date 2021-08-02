using BanggoodGiftExchange.Models;
using BanggoodGiftExchange.Services.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BanggoodGiftExchange.Services
{
    public class BanggoodService
    {
        private BanggoodDbService dbService;

        public BanggoodService()
        {
            this.dbService = new BanggoodDbService();
        }

        public List<TeammateParticipation> CreateGiftExchange(string exchangeName, DateTime startDate)
        {
            int giftExchangeId = this.dbService.SaveGiftExchange(exchangeName, startDate);

            Random random = new Random();
            List<Teammate> teammates = this.dbService.GetTeammatesForDate(startDate)
                .OrderBy(t => random.Next())
                .Where(t => t.EndDate == null || t.EndDate > startDate)
                .ToList();

            var teammateParticipations = new List<TeammateParticipation>();
            var teammateId = 1;
            foreach (var teammate in teammates){
                teammateParticipations.Add(
                    new TeammateParticipation{
                    Teammate = teammate,
                    GiftExchangeId = giftExchangeId,
                    ParticipantNumber = teammateId
                    });
                teammateId++;
            }

            foreach (var teammateParticipation in teammateParticipations){
                dbService.SaveTeammateParticipation(teammateParticipation);
            }

            // TODO create participation records and assign ParticipantNumbers to each teammate. Save them to the database
            // HINT: dbService has a SaveTeammateParticipation method to save a participation record
            return teammateParticipations;
        }

        // Stores gift ordered using the current date as ordered date
        public int CreateOrderedGift(string description, int participantId)
        {
            return this.dbService.SaveOrderedGift(description, participantId);
        }

        // mark gift as received, determine who receives gift, and return name of recipient
        public string HandleGiftArrival(int giftId)
        {
            // update the received date on the OrderedGift record
            this.dbService.MarkGiftAsReceived(giftId);
            return dbService.GetGiftRecieverName(giftId);
        }

        public IEnumerable<GiftRecap> RecapAllGiftsForExchange(int giftExchangeId)
        {
            return this.dbService.RecapAllGiftsForExchange(giftExchangeId);
        }
    }
}
