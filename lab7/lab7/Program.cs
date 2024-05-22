using System;
using System.Threading;

class Program
{
    static Semaphore semaphore;
    static int honeyPot = 0;
    static int capacity = 10; // Емкость горшка меда
    static Random random = new Random();

    static void Main()
    {
        int numberOfBees = 5; // Количество пчел
        semaphore = new Semaphore(1, 1);

        Thread bearThread = new Thread(new ThreadStart(Bear));
        bearThread.Start();

        for (int i = 0; i < numberOfBees; i++)
        {
            Thread beeThread = new Thread(new ThreadStart(Bee));
            beeThread.Name = $"Пчела {i + 1}";
            beeThread.Start();
        }
    }

    static void Bee()
    {
        while (true)
        {
            semaphore.WaitOne();
            if (honeyPot < capacity)
            {
                honeyPot++;
                Console.WriteLine($"{Thread.CurrentThread.Name} добавила 1 порцию меда. Всего в горшке: {honeyPot}");
                if (honeyPot >= capacity)
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} заполнила горшок и будит медведя.");
                }
            }
            semaphore.Release();

            Thread.Sleep(random.Next(500, 2000));
        }
    }

    static void Bear()
    {
        while (true)
        {
            if (honeyPot >= capacity)
            {
                semaphore.WaitOne();
                if (honeyPot >= capacity)
                {
                    Console.WriteLine("Медведь проснулся и съел весь мед.");
                    honeyPot = 0;
                }
                semaphore.Release();
            }

            Thread.Sleep(1000);
        }
    }
}
