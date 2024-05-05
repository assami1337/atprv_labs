using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace WindowsFormsApp1
{

    
    public partial class Form1 : Form
    {
        private HttpClient _client = new HttpClient();
        private Stopwatch _stopwatch = new Stopwatch();
        private double[,] matrix;
        private double[] freeTerms;
        private double[] currentSolution;
        private double eps;
        private int size;

        public Form1()
        {
            InitializeComponent();
            button2.Click += async (sender, e) => await StartCrawling();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Деактивация GUI элементов, чтобы избежать повторного нажатия
                button1.Enabled = false;
                maskedTextBox1.Enabled = false;

                if (!int.TryParse(maskedTextBox1.Text, out int numberOfTexts))
                {
                    MessageBox.Show("Введите корректное число в поле 'Кол-во текстов'");
                    return;
                }

                var texts = GenerateTexts(numberOfTexts);
                var wordsToFind = new List<string> { "example", "text", "word" };
                var wordCounts = new ConcurrentDictionary<string, int>();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // Асинхронный запуск задачи с использованием Task.Run
                await Task.Run(() =>
                {
                    // Используем Parallel.ForEach для параллельной обработки
                    Parallel.ForEach(texts, () => new Dictionary<string, int>(),
                        (text, loopState, localWordCount) =>
                        {
                            foreach (var word in wordsToFind)
                            {
                                int count = CountWordOccurrences(text, word);
                                if (localWordCount.ContainsKey(word))
                                    localWordCount[word] += count;
                                else
                                    localWordCount.Add(word, count);
                            }
                            return localWordCount;
                        },
                        localWordCount =>
                        {
                            foreach (var pair in localWordCount)
                            {
                                wordCounts.AddOrUpdate(pair.Key, pair.Value, (key, oldValue) => oldValue + pair.Value);
                            }
                        }
                    );
                });

                stopwatch.Stop();

                // Обновление интерфейса пользователя в основном потоке
                textBox1.Text = $"Processed texts: {numberOfTexts}\r\nProcessing Time: {stopwatch.ElapsedMilliseconds} ms\r\n";
                foreach (var word in wordsToFind)
                {
                    textBox1.Text += $"Word: {word}, Count: {wordCounts[word]}\r\n";
                }
            }
            finally
            {
                // Реактивация GUI элементов
                button1.Enabled = true;
                maskedTextBox1.Enabled = true;
            }
        }

        // Ваши вспомогательные методы здесь
        private List<string> GenerateTexts(int numberOfTexts)
        {
            var baseTexts = new List<string>
    {
        "This is a simple example text containing some example words.",
        "Another text, which is used to test the occurrence count of specific words.",
        "The quick brown fox jumps over the lazy dog."
    };

            var texts = new List<string>();
            for (int i = 0; i < numberOfTexts; i++)
            {
                texts.AddRange(baseTexts);
            }
            return texts;
        }

        private int CountWordOccurrences(string text, string word)
        {
            int count = 0, startIndex = 0;
            while ((startIndex = text.IndexOf(word, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                startIndex += word.Length;
            }
            return count;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }


        private async Task StartCrawling()
        {
            string url = textBox4.Text;
            int maxDepth = int.Parse(textBox5.Text);
            Node root = new Node(url);

            _stopwatch.Restart();
            await CrawlLevel(root, maxDepth);
            _stopwatch.Stop();
        }

        private async Task CrawlLevel(Node root, int maxDepth)
        {
            List<Node> currentLevel = new List<Node> { root };
            int currentDepth = 0;

            while (currentDepth <= maxDepth && currentLevel.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                List<Node> nextLevel = new List<Node>();

                foreach (var node in currentLevel)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        Uri baseUri = new Uri(node.Url);
                        var response = await _client.GetAsync(baseUri);
                        var pageContents = await response.Content.ReadAsStringAsync();
                        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.LoadHtml(pageContents);

                        var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
                        if (links != null)
                        {
                            foreach (var link in links)
                            {
                                string hrefValue = link.GetAttributeValue("href", string.Empty);
                                Uri fullUri = new Uri(baseUri, hrefValue);
                                var childNode = new Node(fullUri.ToString());
                                node.Children.Add(childNode);
                                nextLevel.Add(childNode);
                            }
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                PrintTreeInfo(root, currentDepth);
                currentLevel = nextLevel;
                currentDepth++;
            }
        }

        private void PrintTreeInfo(Node root, int depth)
        {
            int height = GetHeight(root);
            int totalNodes = GetTotalNodes(root);
            var leaves = GetLeaves(root, 2, depth); // Получаем только два листа на текущей глубине

            // Если мы находимся на глубине 0 и список листьев пуст, используем URL корня
            if (depth == 0 && leaves.Count == 0)
            {
                leaves.Add(root.Url);
                foreach (var child in root.Children)
                {
                    leaves.Add(child.Url);
                    if (leaves.Count >= 2) break;
                }
            }

            textBox2.Invoke(new Action(() =>
            {
                textBox2.AppendText($"Глубина обработки завершена: {depth}, Время выполнения: {_stopwatch.ElapsedMilliseconds} мс, Высота дерева: {height}, Количество узлов: {totalNodes}, Пример листьев на глубине {depth}: {string.Join(", ", leaves)}\n");
            }));
            _stopwatch.Restart(); // Перезапуск таймера для следующего уровня
        }

        private int GetHeight(Node node)
        {
            int height = 1;
            foreach (var child in node.Children)
            {
                height = Math.Max(height, 1 + GetHeight(child));
            }
            return height;
        }

        private int GetTotalNodes(Node node)
        {
            int count = 1;
            foreach (var child in node.Children)
            {
                count += GetTotalNodes(child);
            }
            return count;
        }

        private List<string> GetLeaves(Node node, int maxLeaves, int targetDepth, int currentDepth = 0)
        {
            List<string> leaves = new List<string>();
            if (currentDepth == targetDepth)
            {
                if (node.Children.Count == 0) // Это лист на целевой глубине
                {
                    leaves.Add(node.Url);
                }
            }
            else
            {
                foreach (var child in node.Children)
                {
                    leaves.AddRange(GetLeaves(child, maxLeaves - leaves.Count, targetDepth, currentDepth + 1));
                    if (leaves.Count >= maxLeaves) break;
                }
            }
            return leaves.Take(maxLeaves).ToList();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            ReadInput();
            await Task.Run(() => SolveJacobi());
        }

        private void ReadInput()
        {
            // Чтение и парсинг матрицы коэффициентов из TextBox
            string[] matrixLines = textBox6.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            size = matrixLines.Length; // Размер матрицы определяется количеством строк
            matrix = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                string[] elements = matrixLines[i].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] = double.Parse(elements[j]);
                }
            }

            // Чтение вектора свободных членов
            string[] freeTermsEntries = textBox7.Text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            freeTerms = new double[size];
            for (int i = 0; i < size; i++)
            {
                freeTerms[i] = double.Parse(freeTermsEntries[i]);
            }

            // Чтение значения точности
            eps = double.Parse(textBox8.Text);

            // Инициализация вектора текущего решения
            currentSolution = new double[size];
        }


        private void SolveJacobi()
        {
            double[] newSolution = new double[size];
            Thread[] threads = new Thread[size];
            double norm;

            do
            {
                for (int i = 0; i < size; i++)
                {
                    int index = i;
                    threads[i] = new Thread(() => SolveElement(index, newSolution));
                    threads[i].Start();
                }

                // Ждем завершения всех потоков
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                norm = ComputeNorm(currentSolution, newSolution);
                Array.Copy(newSolution, currentSolution, size);
            } while (norm > eps);

            this.Invoke((MethodInvoker)delegate
            {
                textBox3.AppendText($"Решение найдено с точностью {eps}\r\n");
                foreach (var val in currentSolution)
                    textBox3.AppendText($"{val:F4}\r\n");
            });
        }

        private void SolveElement(int index, double[] newSolution)
        {
            double sum = freeTerms[index];
            for (int j = 0; j < size; j++)
            {
                if (index != j)
                {
                    sum -= matrix[index, j] * currentSolution[j];
                }
            }
            newSolution[index] = sum / matrix[index, index];
        }

        private double ComputeNorm(double[] oldSolution, double[] newSolution)
        {
            double norm = 0.0;
            for (int i = 0; i < oldSolution.Length; i++)
            {
                norm += Math.Pow(newSolution[i] - oldSolution[i], 2);
            }
            return Math.Sqrt(norm);
        }
    }

    public class Node
    {
        public string Url { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();

        public Node(string url)
        {
            Url = url;
        }
    }

    
}
