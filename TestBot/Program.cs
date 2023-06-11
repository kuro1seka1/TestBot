using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using File = System.IO.File;
using TestBot;
using System.Diagnostics;
using Telegram.Bot.Polling;


namespace TestBot
{
    class Program
    {   
        private static Quiz quiz;
        private static TelegramBotClient Bot;
        private static Dictionary<long, QuestionState> States;
        private static Dictionary<long, int> PlayerScore;
        static string pathToLog = "log.txt";
        
        
        static void Main(string[] args)
        {
            ExcelLayout excel = new();
            excel.Excel(@"C:\Users\Василий\Desktop\Kursk.xls");

            quiz = new Quiz("testKursk.txt");
            
            States = new Dictionary<long, QuestionState>();

            PlayerScore = new Dictionary<long, int>(); 

            Bot = new TelegramBotClient("5828907769:AAE0Isn4jXHtREHwTpna9sAxgCVFFM8c6aw");

            //Bot.on += BotOnCallbackQueryReceived;
            using CancellationTokenSource cts = new();
            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);
           
            StartReceivingMessagesAsync(cancellationToken: cts.Token);
            
            


            Console.ReadLine();
        }

        private static async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            var offset = 0;
            try
            {
                while (true)
                {
                    var updates = await Bot.GetUpdatesAsync(offset);

                    foreach (var update in updates)
                    {
                        if (update.Message != null)
                        {
                            var message = update.Message;

                            if (message.Type != MessageType.Text) { return; }

                            Console.WriteLine($"New message received from {message.Chat.FirstName}: {message.Text}");

                            switch (message.Text)
                            {
                                case "/start":
                                    string text =
    @"Здравствуйте, на ответ дается 6 секунд.ё
Если ответ не будет дан за это время, все зависимости от его правильности,
он будет считаться не правильным!
Итоговые результаты вы сможете увидеть в конце опроса.
Набор команд:
/quiz - Начать опрос";
                                    await Bot.SendTextMessageAsync(message.From.Id, text);
                                    break;
                                case "/quiz":
                                    var chatId = message.Chat.Id;
                                    offset = update.Id + 1;
                                    var playerId = message.From.Id;
                                    if (!States.TryGetValue(chatId, out var state))
                                    {
                                        state = new QuestionState();
                                        States[chatId] = state;
                                    }

                                    if (state.CurrentItem == null)
                                    {
                                        state.CurrentItem = quiz.NextQuestion();
                                        state.questionIndex = 0;
                                    }
                                    Stopwatch sw = new();
                                    
                                    while (state.questionIndex < quiz.Questions.Count)
                                    {
                                        var question = state.CurrentItem;
                                        await Bot.SendTextMessageAsync(chatId, question.Question);

                                        var answer = "";
                                        while (string.IsNullOrEmpty(answer))
                                        {
                                            updates = await Bot.GetUpdatesAsync(offset: offset, timeout: 100);
                                            message = updates.FirstOrDefault()?.Message;

                                            if (message != null && !string.IsNullOrEmpty(message.Text))
                                            {
                                                answer = message.Text.ToLower();
                                            }
                                        }
                                        sw.Start();

                                        offset = updates.LastOrDefault().Id + 1;




                                        if (answer == question.Answer.ToLower() && sw.ElapsedMilliseconds < 6000)
                                        {
                                            await Bot.SendTextMessageAsync(chatId, "Верно!", cancellationToken: cancellationToken);
                                            if (PlayerScore.ContainsKey(playerId))
                                            {
                                                PlayerScore[playerId]++;
                                            }
                                            else
                                            {
                                                PlayerScore[playerId] = 1;
                                            }

                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(chatId, "Не верно !", cancellationToken: cancellationToken);
                                        }

                                        state.CurrentItem = quiz.NextQuestion();
                                        state.questionIndex++;
                                        sw.Stop();

                                        Console.WriteLine(sw.ElapsedMilliseconds);
                                        sw.Restart();

                                    }

                                    await Bot.SendTextMessageAsync(chatId, "Тест окончен!", cancellationToken: cancellationToken);
                                    int score = PlayerScore[playerId];
                                    int test = quiz.Questions.Count;
                                    using (StreamWriter swr = new StreamWriter(pathToLog, true))
                                    {
                                      await swr.WriteLineAsync(($"{message.Chat.FirstName} набрал {score} очков в {DateTime.Now}"));
                                    }


                                    if (score == 0)
                                    {
                                        await Bot.SendTextMessageAsync(chatId, "Ни одного правильного ответа", cancellationToken: cancellationToken);

                                        PlayerScore[playerId] = 0;
                                        state.questionIndex = 0;
                                        break;
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(chatId, $"Вы набрали {score} из {test} очков ", cancellationToken: cancellationToken);
                                        

                                        PlayerScore[playerId] = 0;
                                        state.questionIndex = 0;


                                        break;

                                    }
                                    
                                default:
                                    
                                    break;

                            }
                                    
                                //case "/keyboard":
                                //    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                                //    {
                                //        new[]
                                //        {
                                //            new KeyboardButton("Орел"),
                                //            new KeyboardButton("Курск"),
                                //        },
                                //        new[]
                                //        {
                                //            new KeyboardButton("Белгород") { RequestContact = true},

                                //        }
                                //    });
                                //    await Bot.SendTextMessageAsync(message.Chat.Id, "Message", replyMarkup: replyKeyboard);
                                //    break;

                                
                            
                        }
                        else if (update.CallbackQuery != null)
                        {
                            var callbackQuery = update.CallbackQuery;
                            string buttonText = callbackQuery.Data;

                            Console.WriteLine($"Callback query received from {callbackQuery.From.FirstName}: {buttonText}");

                            await Bot.AnswerCallbackQueryAsync(
                                callbackQueryId: callbackQuery.Id,
                                text: $"You pressed {buttonText}");
                        }
                        offset = update.Id + 1;
                        
                    }

                    await Task.Delay(1000);
                    File.Delete("testKursk.txt");
                    cancelTokenSource.Cancel();
                    
                }



            }
            catch 
            {
              
            }
        }
    }
}

