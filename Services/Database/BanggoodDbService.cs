using BanggoodGiftExchange.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BanggoodGiftExchange.Services.Database
{
    public class BanggoodDbService : BaseBanggoodDbService
    {
        public IEnumerable<Teammate> GetTeammatesForDate(DateTime dateToCheck)
        {
            using (IDbConnection connection = this.ConnectToDb())
            {
                string selectQuery = @"SELECT Id, Name
                               FROM Teammates
                               WHERE StartDate <= @dateToCheck";

                return connection.Query<Teammate>(selectQuery, new { dateToCheck = dateToCheck });
            }
        }

        public int SaveGiftExchange(string exchangeName, DateTime startDate)
        {
            using (IDbConnection connection = this.ConnectToDb())
            {
                string insertQuery = @"INSERT INTO GiftExchanges (Name, StartDate)
                                       VALUES (@Name, @StartDate);
                                       SELECT last_insert_rowid() AS Id";

                return connection.ExecuteScalar<int>(insertQuery, new { Name = exchangeName, StartDate = startDate });
            }
        }

        public int SaveTeammateParticipation(TeammateParticipation teammateParticipation)
        {
            using (IDbConnection connection = this.ConnectToDb())
            {
                string insertQuery = @"INSERT INTO TeammateParticipation (GiftExchangeId, TeammateId, ParticipantNumber)
                                       VALUES (@GiftExchangeId, @TeammateId, @ParticipantNumber);
                                       SELECT last_insert_rowid() AS Id";

                int id = connection.ExecuteScalar<int>(insertQuery, teammateParticipation);
                teammateParticipation.Id = id;

                return id;
            }
        }

        public int SaveOrderedGift(string description, int participantId)
        {
            using (IDbConnection connection = this.ConnectToDb())
            {
                string insertQuery = @"INSERT INTO OrderedGifts (GiftDescription, TeammateParticipationId, OrderedDate)
                                       VALUES (@description, @participantId, DATE('now'));
                                       SELECT last_insert_rowid() AS Id";

                return connection.ExecuteScalar<int>(insertQuery, new { description = description, participantId = participantId });
            }
        }

        public void MarkGiftAsReceived(int orderedGiftId)
        {
            using (IDbConnection connection = this.ConnectToDb())
            {
                // date('now') in SQLite returns the current day
                string updateQuery = @"UPDATE OrderedGifts SET ReceivedDate = DATE('now') WHERE Id = @orderedGiftId";

                // if filtering by id, use @orderedGiftId. The Dapper framework will correctly map the variable
                // value into the query via SQL parameters
                connection.Execute(updateQuery, new { orderedGiftId = orderedGiftId });

            }
        }

        public int GetOrderedGiftSender(int orderedGiftId){
            using (IDbConnection connection = this.ConnectToDb()){
                var getParticipant = @"SELECT TeammateParticipationId FROM OrderedGifts WHERE Id = @orderedGiftId";

                return connection.ExecuteScalar<int>(getParticipant, new{orderedGiftId = orderedGiftId});
            }
        }

        public string GetGiftRecieverName(int giftId){
            var senderId = GetOrderedGiftSender(giftId);
            using (IDbConnection connection = this.ConnectToDb()){
                var getRecieverPnumb = @"SELECT ParticipantNumber FROM TeammateParticipation WHERE ReceivesFromParticipationId = @senderId";

                var recPNum = connection.ExecuteScalar<int>(getRecieverPnumb, new{senderId = senderId});
                if (recPNum != 0) return GetParticipantName(recPNum);
                
                var getNextRecever = @"SELECT MIN(ParticipantNumber) FROM TeammateParticipation WHERE ReceivesFromParticipationId = null";
                var nextRec = connection.ExecuteScalar<int>(getNextRecever);
                
                
                var updateParticipation = @"UPDATE TeammateParticipation SET ReceivesFromParticipationId = @senderId WHERE TeammateId = @nextRec";
                connection.Execute(updateParticipation,new{senderId = senderId,nextRec = nextRec});
                return GetParticipantName(recPNum);
            }
        }

        public string GetParticipantName(int id){
            using (IDbConnection connection = this.ConnectToDb()){
                var getParticipant = @"SELECT Name FROM Teammates WHERE Id = @id";

                return connection.ExecuteScalar<string>(getParticipant, new{id = id});
            }
        }
        
        public IEnumerable<GiftRecap> RecapAllGiftsForExchange(int giftExchangeId)
        {
            using (IDbConnection connection = this.ConnectToDb())
            {
                // Alias columns (e.g. tm.Name AS SenderName, tm2.Name AS ReceiverName to match properties on
                // the GiftRecap object. The Dapper framework will map the query results to those properties
                var selectQuery = @"";
                
                // if filtering by id, use @giftExchangeId. The Dapper framework will correctly map the variable
                // value into the query via SQL parameters
                return connection.Query<GiftRecap>(selectQuery, new { giftExchangeId = giftExchangeId });
            }
        }
    }
}
