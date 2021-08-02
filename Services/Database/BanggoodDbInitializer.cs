using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace BanggoodGiftExchange.Services.Database
{
	public class BanggoodDbInitializer : BaseBanggoodDbService
	{
		public void InitializeDb()
		{
			if (!File.Exists(this.DbFile))
			{
				this.CreateDatabase();
			}
		}

		private void CreateDatabase()
		{
			using (SQLiteConnection conn = this.ConnectToDb())
			{
				conn.Execute(@"CREATE TABLE Teammates(
								Id integer primary key,
								Name narchar(100) NOT NULL,
								StartDate date NOT NULL,
								EndDate date NULL
							);");

				conn.Execute(@"CREATE TABLE GiftExchanges(
								Id integer primary key,
								Name varchar(100) NOT NULL,
								StartDate date NOT NULL
							);");

				conn.Execute(@"CREATE TABLE TeammateParticipation(
								Id integer primary key,
								GiftExchangeId int NOT NULL,
								TeammateId int NOT NULL,
								ParticipantNumber int NOT NULL,
								ReceivesFromParticipationId int NULL,
								FOREIGN KEY(GiftExchangeId) REFERENCES GiftExchanges (Id),
								FOREIGN KEY(ReceivesFromParticipationId) REFERENCES TeammateParticipation (Id),
								FOREIGN KEY(TeammateId) REFERENCES Teammates (Id)
							);");

				conn.Execute(@"CREATE TABLE OrderedGifts(
								Id integer primary key,
								TeammateParticipationId int NOT NULL,
								GiftDescription nvarchar(100) NOT NULL,
								OrderedDate date NOT NULL,
								ReceivedDate date NULL,
								FOREIGN KEY(TeammateParticipationId) REFERENCES TeammateParticipation (Id)
							);");

				conn.Execute(@"INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (1, 'Corey', '2014-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (2, 'Jordan', '2014-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (3, 'Eric', '2014-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (4, 'Josh W.', '2014-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (5, 'Andy', '2014-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (6, 'Chris', '2014-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (7, 'Erin', '2014-09-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (8, 'Kyna', '2014-09-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (9, 'Kai', '2014-09-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (10, 'Lori', '2014-09-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (11, 'Josh E', '2015-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (12, 'Kyle', '2015-01-01', '2019-10-31');
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (13, 'Nikki', '2015-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (14, 'Mike A.', '2015-10-31', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (15, 'Jake', '2016-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (16, 'Cheri', '2018-01-01', '2018-12-01');
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (17, 'Nick', '2018-10-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (18, 'Dan', '2019-01-01', NULL);
							INSERT INTO Teammates (Id, Name, StartDate, EndDate) VALUES (19, 'Lisa', '2019-10-01', NULL);");

				conn.Execute(@"INSERT INTO GiftExchanges (Id, Name, StartDate) VALUES (1, '2018 Holiday Exchange', '2018-11-01');
							INSERT INTO GiftExchanges (Id, Name, StartDate) VALUES (2, '2019 Holiday Exchange', '2019-11-01');");

				// insert participants for 2018 (exclude Dan and Lisa), receives from just shifts for simplicity
				int[] teammateIds2018 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
				for (int i = 0; i < teammateIds2018.Length; i++) {
					int teammateId = teammateIds2018[i];
					int receivesFrom = i == teammateIds2018.Length - 1 ? 1 : i + 2;
					conn.Execute($"INSERT INTO TeammateParticipation (Id, GiftExchangeId, TeammateId, ParticipantNumber, ReceivesFromParticipationId) VALUES ({i + 1}, 1, {teammateId}, {i + 1}, {receivesFrom});");
				}

				// insert participants for 2018 (add Dan and Lisa, exclude Kyle and Cheri), receives from just shifts for simplicity
				int[] teammateIds2019 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 17, 18, 19 };
				for (int i = 0; i < teammateIds2019.Length; i++) {
					int teammateId = teammateIds2019[i];
					int receivesFrom = i == teammateIds2019.Length - 1 ? 1 + teammateIds2018.Length : i + teammateIds2018.Length + 2;
					conn.Execute($"INSERT INTO TeammateParticipation (Id, GiftExchangeId, TeammateId, ParticipantNumber, ReceivesFromParticipationId) VALUES ({i + teammateIds2018.Length + 1}, 2, {teammateId}, {i + 1}, {receivesFrom});");
				}



				Random giftsPerTeammateRandomizer = new Random(123);
				Random receivedDateRandomizer = new Random(456);
				int totalGifts = 0;
				totalGifts = CreateOrderedGifts(conn, giftsPerTeammateRandomizer, receivedDateRandomizer, 0, teammateIds2018.Length, 2018, totalGifts);
				totalGifts = CreateOrderedGifts(conn, giftsPerTeammateRandomizer, receivedDateRandomizer, teammateIds2018.Length, teammateIds2019.Length, 2019, totalGifts);
			}
		}

		private static int CreateOrderedGifts(SQLiteConnection conn, Random giftsPerTeammateRandomizer, Random receivedDateRandomizer,
			int totalPreviousParticipants, int currentYearParticipants, int startYear, int totalGifts)
		{
			List<string> gifts = new List<string>() {
				"Nesting Dolls", "Tropical Bathroom Decoration", "Toilet Brush", "Puzzle", "Chessboard", "Pens", "Solar Lights",
				"Remote Controlled Rat", "Electric Ab Worker", "Fidget Spinner", "Hand Feet", "Hand Soccer Set", "Chicken Keychain",
				"Magnetic Putty", "Santa Seat Cover", "Plant Lights", "Army Men", "Flashlight", "Bucky Balls", "Wearable Basketball Hoop",
				"Rubber Duck", "Knock-off Board Game", "Newtons Cradle", "Model House", "Fake Bug", "Giant Enter Key", "Finger Skateboard",
				"Dancing Wind-up Toy", "Balancing Bird", "Rubiks Cube", "3D Labyrinth Puzzle", "Dinosaur Hand Puppet", "Wind-up Chicken Toy",
				"Finger Guillotine Toy", "Rubber Chicken", "Dice"
			};

			int currentGiftIndex = 0;
			for (int i = 1; i <= currentYearParticipants; i++) {
				int participationId = i + totalPreviousParticipants;
				int giftsForTeammate = giftsPerTeammateRandomizer.Next(1, 3);
				for (int g = 0; g < giftsForTeammate; g++) {
					totalGifts++;
					string gift = gifts[currentGiftIndex];
					currentGiftIndex++;
					if (currentGiftIndex == gifts.Count) {
						currentGiftIndex = 0;
					}

					string receivedDate = "NULL";
					int recievedDateRandom = receivedDateRandomizer.Next(1, 100);
					if (recievedDateRandom < 97)
					{
						receivedDate = $"'{DateTime.Parse($"{startYear}-11-20").AddDays(recievedDateRandom).ToString("yyyy-MM-dd")}'";
					} else
					{
						// keep received date as null
					}
					conn.Execute($"INSERT INTO OrderedGifts (Id, TeammateParticipationId, GiftDescription, OrderedDate, ReceivedDate) VALUES ({totalGifts}, {participationId}, '{gift}', '{startYear}-11-02', {receivedDate});");
				}
			}

			return totalGifts;
		}
	}
}
