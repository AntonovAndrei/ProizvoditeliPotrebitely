using System;
using System.Threading;

namespace ProizvoditeliPotrebitely
{
    class Program
    {
        // Количество потребителей
        private static int _consumers = 10;
        // Количество производителей
        private static int _manufacturers = 10;

        
        private static Mutex _BufferMustWait = new Mutex();
        private static object _ConsumerTakeLockObject = new object();
        
        private static readonly Random _random = new Random();

        //массив откуда рандомным образом берутся задачи
        private static string[] _allWorks = new[]
        {
            "Купить книгу", "Почистить обувь", "Погладить кота", "Прочитать книгу", "Посмотреть кино", "Напомнить об уборке", 
            "Почистить картофель", "Приготовить еду", "Погладить рубашку", "Поздравить соседа", "Купить продукты", "Покормить кота",
            "Выгулить собаку", "Почистить компьютер", "Обновить фильтр", "Поменять воду в аквариуме", "Позвонить родителям"
        };
        //наш буфер
        private static string[] _worksForConsumer = new string[_manufacturers / 2];
        
        
        static void Main(string[] args)
        {
            // Создаем потоки потребителей
            Thread[] consumers = new Thread[_consumers];
            for (int i = 0; i < _consumers; i++)
            {
                consumers[i] = new Thread(ConsumerWork);
            }
            // Создаем потоки производителей
            Thread[] manufacturers = new Thread[_manufacturers];
            for (int i = 0; i < _manufacturers; i++)
            {
                manufacturers[i] = new Thread(ManufacturerWork);
            }
            // Стартуем потоки потребителей
            foreach (var t in consumers)
            {
                t.Start();
            }
            // Стартуем потоки производителей
            foreach (var t in manufacturers)
            {
                t.Start();
            }
            // Ждем потоки потребителей
            foreach (var t in consumers)
            {
                t.Join();
            }
            // Ждем потоки производителей
            foreach (var t in manufacturers)
            {
                t.Join();
            }
        }

        static void ConsumerWork()
        {
            while (true)
            {
                _BufferMustWait.WaitOne();
                string work = null;
                bool ok = false;
                for (int i = 0; i < _worksForConsumer.Length; i++)
                {
                    if (!String.IsNullOrEmpty(_worksForConsumer[i]))
                    {
                        ConsoleHelper.WriteToConsole(
                            $"Потребитель{Thread.CurrentThread.ManagedThreadId} взял задачу: {_worksForConsumer[i]}.");
                        work = _worksForConsumer[i];
                        SetValueInBuffer(i);
                        ok = true;
                        break;
                    }
                }
                _BufferMustWait.ReleaseMutex();
                if (ok)
                {
                    //Thread.Sleep(_random.Next(500, 3500));
                    Thread.Sleep(1000);
                    ConsoleHelper.WriteToConsole(
                        $"Потребитель{Thread.CurrentThread.ManagedThreadId} выполнил задачу: {work}.");
                }

                Thread.Sleep(500);
                //Thread.Sleep(_random.Next(500, 1500));
            }
        }

        static void ManufacturerWork()
        {
            while (true)
            {
                bool workCreated = false;
                _BufferMustWait.WaitOne();
                for (int i = 0; i < _worksForConsumer.Length; i++)
                {
                    if (String.IsNullOrEmpty(_worksForConsumer[i]))
                    {
                        SetValueInBuffer(i, _allWorks[_random.Next(0, _allWorks.Length)]);
                        ConsoleHelper.WriteToConsole($"Производитель{Thread.CurrentThread.ManagedThreadId} создал задачу: {_worksForConsumer[i]}.");
                        workCreated = true;
                        
                        break;
                    }
                }
                _BufferMustWait.ReleaseMutex();
                if (workCreated)
                {
                    ConsoleHelper.WriteToConsole(
                        $"Производитель{Thread.CurrentThread.ManagedThreadId} начал думать над другой задачей.");
                    workCreated = false;
                    Thread.Sleep(1000);
                    //Thread.Sleep(_random.Next(1500, 5000));
                }
            }
        }
        
        public static void SetValueInBuffer(int i, string value = null)
        {
            lock(_ConsumerTakeLockObject)
            {
                _worksForConsumer[i] = value;
            }
        }
    }
}