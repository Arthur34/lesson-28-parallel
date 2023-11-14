using System.Diagnostics;

internal class Program
{
    /// <summary>
    /// Число потоков для многопоточных методов суммирования.
    /// </summary>
    public static int NumberOfThreads { get; } = 4;

    /// <summary>
    /// Простое суммирование через цикл.
    /// </summary>
    /// <param name="array">Массив целых чисел</param>
    /// <returns>Сумма</returns>
    private static long SimpleSum(int[] array)
    {
        var sum = 0L;
        foreach (var item in array)
            sum += item;
        return sum;
    }
    
    /// <summary>
    /// Метод для подсчета суммы каждого из потоков.
    /// </summary>
    /// <param name="id">Номер потока</param>
    /// <param name="array">Исходный массив</param>
    /// <param name="threadSum">Массив с суммами</param>
    static void MyThreadMethod(int id, int[] array, long[] threadSum)
    {
        for (int i = id * (array.Length / NumberOfThreads); i < (id + 1) * array.Length / NumberOfThreads; i++)
            threadSum[id] += array[i];
    }

    /// <summary>
    /// Параллельное суммирование целых чисел через Thread.
    /// </summary>
    /// <param name="array">Массив целых чисел</param>
    /// <returns></returns>
    static long ThreadSum(int[] array)
    {
        var threadSum = new long[NumberOfThreads];
        var threadsArray = new Thread[NumberOfThreads];
        for (var i = 0; i < NumberOfThreads; i++)
        {
            var id = i;
            threadsArray[i] = new Thread(() => MyThreadMethod(id, array, threadSum));
            threadsArray[i].Start();
        }
        Array.ForEach(threadsArray, x => x.Join());
        return threadSum.Sum();
    }

    /// <summary>
    /// Параллельное суммирование целых чисел через Task.
    /// </summary>
    /// <param name="array">Массив целых чисел</param>
    /// <returns></returns>
    static long TaskSum(int[] array)
    {
        var threadSum = new long[NumberOfThreads];
        var tasks = new Task[NumberOfThreads];
        for (var i = 0; i < NumberOfThreads; i++)
        {
            var id = i;
            tasks[i] = Task.Run(() => MyThreadMethod(id, array, threadSum));
        }
        Task.WaitAll(tasks);
        return threadSum.Sum();
    }

    /// <summary>
    /// Параллельное суммирование массива целых чисел с помощью LINQ.
    /// </summary>
    /// <param name="array">Исходный массив</param>
    /// <returns>Сумма</returns>
    static private long PLinqSum(int[] array) => array.Select(n => (long)n).AsParallel().Sum();

    private delegate long SumDelegate(int[] array);

    /// <summary>
    /// Вывод результата с подсчетом затраченного навычисление времени.
    /// </summary>
    /// <param name="sumMethod">Метод для вычисления</param>
    /// <param name="array">Массив с исходными значениями</param>
    static private void ShowResult(SumDelegate sumMethod, int[] array)
    {
        var watcher = new Stopwatch();
        watcher.Start();
        var sum = sumMethod(array);
        watcher.Stop();
        Console.WriteLine($"{sumMethod.Method.Name} =\t{sum} in {watcher.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Посчитать сумму всеми способами.
    /// </summary>
    /// <param name="count">Количество элементов для суммирования</param>
    private static void DoSumAllWays(int count)
    {
        Console.WriteLine($"Calculating the sum for {count} elements:\n");

        // формируем массив случайных целых чисел
        var rand = new Random();
        var randomArray = Enumerable.Repeat(0, count).Select(i => rand.Next(0, int.MaxValue)).ToArray();

        ShowResult(SimpleSum, randomArray);
        ShowResult(ThreadSum, randomArray);
        ShowResult(TaskSum,   randomArray);
        ShowResult(PLinqSum,  randomArray);

        Console.WriteLine(new string('_', 50));
    }

    /// <summary>
    /// Точка входа
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        // считаем суммы
        DoSumAllWays(   100_000);
        DoSumAllWays( 1_000_000);
        DoSumAllWays(10_000_000);

        Console.Write("Press Enter for exit: ");
        Console.ReadLine();
    }
}