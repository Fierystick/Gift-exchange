using System;
using System.Data.SQLite;

namespace BanggoodGiftExchange.Services.Database
{
    public abstract class BaseBanggoodDbService
    {
        protected string DbFile
        {
            get { return $"{Environment.CurrentDirectory}\\BanggoodDb.sqlite"; }
        }

        protected SQLiteConnection ConnectToDb()
        {
            return new SQLiteConnection($"Data Source={this.DbFile}");
        }
    }
}
