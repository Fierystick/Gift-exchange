using System;
using System.Collections.Generic;
using System.Text;

namespace BanggoodGiftExchange.Models
{
    public class TeammateParticipation
    {
        public int Id { get; set; }

        public int GiftExchangeId { get; set; }

        public Teammate Teammate { get; set; }

        public int? TeammateId
        {
            get
            {
                if (this.Teammate != null)
                {
                    return this.Teammate.Id;
                }

                return null;
            }
        }

        private string teammateName = null;
        public string TeammateName
        {
            set { this.teammateName = value; }
            get
            {
                if (this.Teammate != null)
                {
                    return this.Teammate.Name;
                }

                return this.teammateName;
            }
        }

        public int ParticipantNumber { get; set; }

        public int? ReceivesFromParticipationId { get; set; }

        public override string ToString()
        {
            if (this.Teammate != null)
            {
                return $"{this.TeammateName}: {this.ParticipantNumber}";
            }

            return base.ToString();
        }
    }
}
