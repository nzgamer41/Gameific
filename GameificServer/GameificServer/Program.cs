using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace GameificServer
{
    class Program
    {
        static void Main(string[] args)
        {
            sqlTest();

            //order of operations
            /*
             *  Start http listener on port 4206 (heh)
             *  Wait for client request
             *  check account type for 0
             *  Check if salted pw matches OR if token matches
             *  if pw matches, generate token for 7 days and send back to client
             *  commit token to db with new expiry
             *  if token matches, check if expired
             *  if not, send ok message
             *  if so, send fail message
             *
             */
        }

        /// <summary>
        /// test function to make sure SQL connections are working
        /// </summary>
        static void sqlTest()
        {
            Console.WriteLine("sqltest");
            var dbCon = DBConnection.Instance();
            dbCon.Server = "nzgamer41.win";
            dbCon.DatabaseName = "gameific";
            dbCon.UserName = "gameificserver";
            dbCon.Password = "iRgBDEfVbTDI4ocs";
            if (dbCon.IsConnect())
            {
                //suppose col0 and col1 are defined as VARCHAR in the DB
                string query = "SELECT username,pass FROM gameific";
                var cmd = new MySqlCommand(query, dbCon.Connection);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string someStringFromColumnZero = reader.GetString(0);
                    string someStringFromColumnOne = reader.GetString(1);
                    Console.WriteLine(someStringFromColumnZero + "," + someStringFromColumnOne);
                }
                dbCon.Close();
            }

            Console.ReadLine();
        }
    }
}
