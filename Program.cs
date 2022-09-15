using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Microsoft.Data.Sqlite;
using Telegram.Bot.Exceptions;
using System.Collections;
using NLog;
using System.IO;
using System.Data.SQLite;

namespace TelegramBotExperiments
{

    class Program
    {
        static string kod = System.IO.File.ReadAllText(@"token.txt");
        static ITelegramBotClient bot = new TelegramBotClient(kod.ToString());
        private static Logger logger = LogManager.GetCurrentClassLogger();
        

        static void Main(string[] args)
        {


            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            logger.Debug("log {0}", "Event handler");
            //Подключение SQLite
            CreateTable();


            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            //подключение событий
            {
                AllowedUpdates = { }, // получать все типы обновлений
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
            using (var connection = new SqliteConnection("Data Source=User.db"))
            {
                connection.Open();
            }
            Console.Read();






        }



        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            //Start/Info/Help
            logger.Debug("log {0}", "Start/Help/Info");

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var  message = update.Message;
                InsertData(message);
                //ненужный вывод в файл
                //System.IO.File.AppendAllText("text.txt", $"Message:{message.Text}, message_id:{message.MessageId}, FROMid:{message.From.Id}, FROMisBot:{message.From.IsBot}, date:{message.Date}, FROMusername:{message.From.Username}, FROMLastname:{message.From.LastName}\n");

                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать в Telegram-бот по подсчету БЖУ в продуктах!");
                    return;
                }
                if (message.Text.ToLower() == "/info")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Данный бот позволит рассчитать БЖУ продуктов на 100гр");
                    return;
                }
                if (message.Text.ToLower() == "/help")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Для работы с ботом, необходимо воспользоваться кнопками: Выбрать категорию продукта, затем сам продукт.");
                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat, "Не понимаю, обратитесь в /help!");


            }


        }


        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));

        }
        //Работа с БД для аналитики пользователей
        //Соединение с БД
        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source= users.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {

            }
            return sqlite_conn;
        }

        //Создание таблиц
        static void CreateTable()
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE IF NOT EXISTS Users (Text text, ID INT, FromID INT, Bot boolean, Date string(40), Username string(30), Firstname string(25), Lastname string(25))";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }

        //Вставка данных в таблицу
        static void InsertData(Message message)
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"INSERT INTO Users (Text, ID, FromID, Bot, Date, Username, Firstname, Lastname) " +
                $"VALUES( '{message.Text}', {message.MessageId}, {message.From.Id}, {message.From.IsBot}, '{message.Date}', '{message.From.Username}', '{message.From.FirstName}', '{message.From.LastName}' ); ";
            sqlite_cmd.ExecuteNonQuery();
        }



    }

}
